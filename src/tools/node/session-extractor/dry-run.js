#!/usr/bin/env node

/**
 * Dry-run Investigation Script
 *
 * Purpose: Understand the full scope of problems before implementing complete recovery.
 *
 * What it does:
 * 1. Parse all operations (using existing jsonl-parser.js)
 * 2. Apply toolUseId deduplication and log removal count
 * 3. Group by file
 * 4. Replay operations IN MEMORY ONLY (no file writes)
 * 5. Track skipped operations with classification:
 *    - duplicate: Removed by toolUseId deduplication
 *    - chain_gap: old_string not found in current state
 *    - concurrent_edit: Same file, same line, different session, close timestamp (<1s)
 *
 * Output: .tmp/dry-run-result.json with:
 * - Pre/post deduplication operation counts
 * - Per-file applied/skipped counts
 * - Skip reason classification
 * - Candidate list for "truly concurrent same-line edits"
 *
 * Usage:
 *   node tools/session-extractor/dry-run.js <session-dir> [base-path-prefix]
 *
 * Example:
 *   node tools/session-extractor/dry-run.js C:\Users\siihe\.ccs\shared\projects\C--Era-erakoumakanNTR
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';
import { discoverSessions } from './session-discovery.js';
import { parseSession } from './jsonl-parser.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * @typedef {Object} Operation
 * @property {'edit'|'write'} type
 * @property {number} timestamp
 * @property {string} filePath
 * @property {string} [oldString]
 * @property {string} [newString]
 * @property {boolean} [replaceAll]
 * @property {string} [content]
 * @property {string} [toolUseId]
 * @property {string} [sessionPath]
 * @property {Array} [structuredPatch]
 */

/**
 * @typedef {Object} SkipRecord
 * @property {number} index - Index in operations array
 * @property {string} toolUseId
 * @property {number} timestamp
 * @property {'duplicate'|'chain_gap'|'concurrent_edit'} reason
 * @property {string} detail - Specific detail about the skip
 */

/**
 * @typedef {Object} ConcurrentEdit
 * @property {string} filePath
 * @property {Object} op1 - First operation
 * @property {Object} op2 - Second operation
 * @property {number} timeDiffMs - Time difference in milliseconds
 */

/**
 * Calculate line range from content and old_string position
 * @param {string} content - Current file content
 * @param {string} oldString - Old string to find
 * @returns {{startLine: number, endLine: number}|null} - Line range or null if not found
 */
function getLineRange(content, oldString) {
  const index = content.indexOf(oldString);
  if (index === -1) return null;

  const beforeText = content.substring(0, index);
  const startLine = beforeText.split('\n').length;
  const endLine = startLine + oldString.split('\n').length - 1;

  return { startLine, endLine };
}

/**
 * Check if two line ranges overlap
 * @param {{startLine: number, endLine: number}|null} range1
 * @param {{startLine: number, endLine: number}|null} range2
 * @returns {boolean}
 */
function linesOverlap(range1, range2) {
  if (!range1 || !range2) return false;
  return range1.startLine <= range2.endLine && range2.startLine <= range1.endLine;
}

/**
 * Find the git repository root for a given file path
 * @param {string} filePath - Absolute file path (normalized with forward slashes)
 * @param {string} basePath - Main repository root path
 * @returns {{ gitRoot: string, relativePath: string }} - Git root and relative path from that root
 */
function findGitRoot(filePath, basePath) {
  // Start from the file's directory
  let dir = path.dirname(filePath).replace(/\\/g, '/');
  const basePathNorm = basePath.replace(/\\/g, '/').replace(/\/$/, '');

  // Search for .git directory
  while (dir.length >= basePathNorm.length) {
    const gitPath = path.join(dir, '.git');
    if (fs.existsSync(gitPath)) {
      // Found a git repository
      const gitRoot = dir.endsWith('/') ? dir : dir + '/';
      const relativePath = filePath.startsWith(gitRoot) ? filePath.slice(gitRoot.length) : filePath;
      return { gitRoot, relativePath };
    }

    const parentDir = path.dirname(dir).replace(/\\/g, '/');
    if (parentDir === dir) {
      break; // Reached filesystem root
    }
    dir = parentDir;
  }

  // No git repository found, use base path
  const getRelativePath = (fp, bp) => {
    const normalized = fp.replace(/\\/g, '/');
    if (bp && normalized.startsWith(bp)) {
      return normalized.slice(bp.length);
    }
    return normalized;
  };

  return {
    gitRoot: basePath,
    relativePath: getRelativePath(filePath, basePath)
  };
}

/**
 * Get the commit hash at or before a given timestamp
 * @param {string} gitRoot - Git repository root
 * @param {number} timestamp - Unix timestamp in milliseconds
 * @returns {string|null} - Commit hash or null if no commits before that time
 */
function getCommitAtTime(gitRoot, timestamp) {
  const isoTime = new Date(timestamp).toISOString();
  try {
    const result = execSync(
      `git rev-list -1 --before="${isoTime}" HEAD`,
      { encoding: 'utf8', cwd: gitRoot }
    ).trim();
    return result || null;
  } catch (e) {
    return null;
  }
}

/**
 * Get file content at a specific commit
 * @param {string} gitRoot - Git repository root
 * @param {string} gitPath - File path relative to git root
 * @param {string|null} commitHash - Commit hash or null
 * @returns {string|null} - File content or null if file doesn't exist at commit
 */
function getFileAtCommit(gitRoot, gitPath, commitHash) {
  if (!commitHash) return null;
  try {
    return execSync(
      `git -c core.quotepath=false show ${commitHash}:"${gitPath}"`,
      { encoding: 'utf8', cwd: gitRoot }
    );
  } catch (e) {
    return null; // File doesn't exist at this commit
  }
}

/**
 * Detect repository root by searching for .git directory
 * @returns {string|null} - Normalized repository root path or null if not found
 */
function detectRepoRoot() {
  let currentDir = process.cwd().replace(/\\/g, '/');

  while (currentDir) {
    const gitPath = path.join(currentDir, '.git');
    if (fs.existsSync(gitPath)) {
      return currentDir.endsWith('/') ? currentDir : currentDir + '/';
    }

    const parentDir = path.dirname(currentDir).replace(/\\/g, '/');
    if (parentDir === currentDir) {
      break;
    }
    currentDir = parentDir;
  }

  return null;
}

/**
 * Apply toolUseId deduplication (F742 P0-3 algorithm)
 * @param {Operation[]} operations - Operations sorted by timestamp
 * @returns {{ deduplicated: Operation[], duplicates: Operation[] }}
 */
function deduplicateOperations(operations) {
  const seenToolUseIds = new Set();
  const deduplicated = [];
  const duplicates = [];

  for (const op of operations) {
    if (op.toolUseId) {
      if (seenToolUseIds.has(op.toolUseId)) {
        duplicates.push(op);
        continue;
      }
      seenToolUseIds.add(op.toolUseId);
    }
    deduplicated.push(op);
  }

  return { deduplicated, duplicates };
}

/**
 * Replay operations in memory and track skip reasons
 * @param {Operation[]} operations - Operations for a single file
 * @param {string|null} baseContent - Base content from git at time of first operation
 * @param {ConcurrentEdit[]} concurrentEdits - List of detected concurrent edits for this file
 * @returns {{ applied: number, skipped: SkipRecord[] }}
 */
function replayInMemory(operations, baseContent = null, concurrentEdits = []) {
  let content = baseContent || '';
  let applied = 0;
  const skipped = [];

  // Track if we've seen a write operation to establish base content
  let hasBaseContent = baseContent !== null;

  for (let i = 0; i < operations.length; i++) {
    const op = operations[i];

    if (op.type === 'write') {
      content = op.content || '';
      hasBaseContent = true;
      applied++;
      continue;
    }

    if (op.type === 'edit') {
      const { oldString, newString, replaceAll } = op;

      // Skip empty oldString
      if (!oldString || oldString === '') {
        skipped.push({
          index: i,
          toolUseId: op.toolUseId,
          timestamp: op.timestamp,
          reason: 'chain_gap',
          detail: 'empty oldString'
        });
        continue;
      }

      // Check if this is the first operation and we have no base content
      if (!hasBaseContent && i === 0) {
        skipped.push({
          index: i,
          toolUseId: op.toolUseId,
          timestamp: op.timestamp,
          reason: 'chain_gap',
          detail: 'first-edit-on-new-file'
        });
        continue;
      }

      // Check if oldString exists
      const firstIndex = content.indexOf(oldString);
      if (firstIndex === -1) {
        // Check if this operation is involved in a concurrent edit
        const isConcurrent = concurrentEdits.some(ce =>
          (ce.op1.toolUseId === op.toolUseId || ce.op2.toolUseId === op.toolUseId)
        );

        skipped.push({
          index: i,
          toolUseId: op.toolUseId,
          timestamp: op.timestamp,
          reason: isConcurrent ? 'concurrent_edit' : 'chain_gap',
          detail: isConcurrent
            ? 'concurrent same-line edit from different session'
            : 'old_string not found'
        });
        continue;
      }

      // Apply the edit
      if (replaceAll) {
        content = content.split(oldString).join(newString);
      } else {
        content = content.replace(oldString, newString);
      }
      applied++;
    }
  }

  return { applied, skipped };
}

/**
 * Analyze concurrent edit patterns
 * @param {Map<string, Operation[]>} operationsByFile
 * @param {string} gitRoot - Git repository root
 * @param {string} basePath - Base path for relative path calculation
 * @returns {ConcurrentEdit[]}
 */
function analyzeConcurrentEdits(operationsByFile, gitRoot, basePath) {
  const concurrent = [];

  for (const [filePath, operations] of operationsByFile.entries()) {
    // Get base content from git at time of first operation
    const { gitRoot: fileGitRoot, relativePath: gitPath } = findGitRoot(filePath, basePath);
    const earliestOpTimestamp = operations[0].timestamp;
    const commitAtStart = getCommitAtTime(fileGitRoot, earliestOpTimestamp);
    const baseContent = commitAtStart ? getFileAtCommit(fileGitRoot, gitPath, commitAtStart) : null;

    // Replay to track content state at each operation
    let content = baseContent || '';
    let hasBaseContent = baseContent !== null;

    for (let i = 0; i < operations.length; i++) {
      const op1 = operations[i];

      // Update content state
      if (op1.type === 'write') {
        content = op1.content || '';
        hasBaseContent = true;
      } else if (op1.type === 'edit' && hasBaseContent && op1.oldString) {
        const firstIndex = content.indexOf(op1.oldString);
        if (firstIndex !== -1) {
          // Get line range for op1
          const range1 = getLineRange(content, op1.oldString);

          // Apply the edit to update content
          if (op1.replaceAll) {
            content = content.split(op1.oldString).join(op1.newString);
          } else {
            content = content.replace(op1.oldString, op1.newString);
          }

          // Check subsequent operations
          for (let j = i + 1; j < operations.length; j++) {
            const op2 = operations[j];

            // Check: different session, close timestamp (<1s), edit type
            const timeDiff = Math.abs(op2.timestamp - op1.timestamp);
            if (op2.sessionPath !== op1.sessionPath && timeDiff < 1000 && op2.type === 'edit') {
              // Calculate line range for op2 (using content BEFORE op2 is applied)
              const range2 = op2.oldString ? getLineRange(content, op2.oldString) : null;

              // Check if lines overlap
              if (range1 && range2 && linesOverlap(range1, range2)) {
                concurrent.push({
                  filePath,
                  op1: {
                    toolUseId: op1.toolUseId,
                    timestamp: op1.timestamp,
                    type: op1.type,
                    sessionPath: op1.sessionPath,
                    lineRange: range1
                  },
                  op2: {
                    toolUseId: op2.toolUseId,
                    timestamp: op2.timestamp,
                    type: op2.type,
                    sessionPath: op2.sessionPath,
                    lineRange: range2
                  },
                  timeDiffMs: timeDiff
                });
              }
            }

            // Break if timestamp difference too large (optimization)
            if (timeDiff > 1000) break;
          }
        }
      }
    }
  }

  return concurrent;
}

/**
 * Main dry-run execution
 */
async function main() {
  const sessionDir = process.argv[2];
  let basePath = process.argv[3];

  if (!sessionDir || sessionDir === '--help' || sessionDir === '-h') {
    console.error('Usage: node tools/session-extractor/dry-run.js <session-dir> [base-path-prefix]');
    console.error('');
    console.error('Example:');
    console.error('  node tools/session-extractor/dry-run.js C:\\Users\\user\\.ccs\\shared\\projects\\repo');
    console.error('');
    console.error('Purpose:');
    console.error('  Dry-run investigation to understand the full scope of recovery problems');
    console.error('  before implementing complete recovery. No files are written.');
    console.error('');
    console.error('Output:');
    console.error('  .tmp/dry-run-result.json - Detailed results with skip classifications');
    process.exit(1);
  }

  if (!basePath) {
    basePath = detectRepoRoot();
    if (basePath) {
      console.log(`Detected repository root: ${basePath}`);
    } else {
      basePath = '';
    }
  }

  console.log('=== Dry-Run Investigation ===');
  console.log(`Session directory: ${sessionDir}`);
  console.log(`Base path: ${basePath || '(none)'}`);
  console.log('');

  // Step 1: Discover sessions
  console.log('[1/5] Discovering sessions...');
  const sessions = await discoverSessions(sessionDir);
  console.log(`Found ${sessions.length} session files`);
  console.log('');

  // Step 2: Parse all operations
  console.log('[2/5] Parsing operations...');
  const allOperations = [];
  for (const session of sessions) {
    const operations = await parseSession(session.path, session.mtime, basePath);
    allOperations.push(...operations);
  }
  const beforeDedup = allOperations.length;
  console.log(`Extracted ${beforeDedup} operations`);
  console.log('');

  // Step 3: Apply toolUseId deduplication
  console.log('[3/5] Applying toolUseId deduplication...');
  allOperations.sort((a, b) => a.timestamp - b.timestamp);
  const { deduplicated, duplicates } = deduplicateOperations(allOperations);
  console.log(`Before deduplication: ${beforeDedup} operations`);
  console.log(`After deduplication: ${deduplicated.length} operations`);
  console.log(`Removed: ${duplicates.length} duplicate operations`);
  console.log('');

  // Step 4: Group by file
  console.log('[4/5] Grouping by file...');
  const operationsByFile = new Map();
  for (const op of deduplicated) {
    if (!operationsByFile.has(op.filePath)) {
      operationsByFile.set(op.filePath, []);
    }
    operationsByFile.get(op.filePath).push(op);
  }
  console.log(`Operations affect ${operationsByFile.size} unique files`);
  console.log('');

  // Step 5: Analyze concurrent edits (before replay)
  console.log('[5/6] Analyzing concurrent edits...');
  const concurrentEdits = analyzeConcurrentEdits(operationsByFile, basePath, basePath);
  console.log(`Detected ${concurrentEdits.length} potential concurrent edits`);
  console.log('');

  // Create a map for quick lookup
  const concurrentEditsByFile = new Map();
  for (const ce of concurrentEdits) {
    if (!concurrentEditsByFile.has(ce.filePath)) {
      concurrentEditsByFile.set(ce.filePath, []);
    }
    concurrentEditsByFile.get(ce.filePath).push(ce);
  }

  // Step 6: Replay operations and collect skip statistics
  console.log('[6/6] Replaying operations in memory...');
  const fileResults = [];
  let totalApplied = 0;
  let totalSkipped = 0;
  const skipReasons = {
    duplicate: duplicates.length,
    chain_gap: 0,
    concurrent_edit: 0
  };

  for (const [filePath, operations] of operationsByFile.entries()) {
    // Get base content from git at time of first operation
    const { gitRoot, relativePath: gitPath } = findGitRoot(filePath, basePath);
    const earliestOpTimestamp = operations[0].timestamp;
    const commitAtStart = getCommitAtTime(gitRoot, earliestOpTimestamp);
    const baseContent = commitAtStart ? getFileAtCommit(gitRoot, gitPath, commitAtStart) : null;

    const fileConcurrentEdits = concurrentEditsByFile.get(filePath) || [];
    const result = replayInMemory(operations, baseContent, fileConcurrentEdits);
    totalApplied += result.applied;
    totalSkipped += result.skipped.length;

    // Count skip reasons
    for (const skip of result.skipped) {
      if (skip.reason === 'chain_gap') {
        skipReasons.chain_gap++;
      } else if (skip.reason === 'concurrent_edit') {
        skipReasons.concurrent_edit++;
      }
    }

    fileResults.push({
      filePath,
      totalOps: operations.length,
      applied: result.applied,
      skipped: result.skipped.length,
      skipDetails: result.skipped
    });
  }

  console.log('Replay complete.');
  console.log(`Total applied: ${totalApplied}`);
  console.log(`Total skipped: ${totalSkipped}`);
  console.log('');

  // Step 7: Generate output
  const output = {
    summary: {
      sessionCount: sessions.length,
      beforeDeduplication: beforeDedup,
      afterDeduplication: deduplicated.length,
      duplicatesRemoved: duplicates.length,
      totalFilesAffected: operationsByFile.size,
      totalApplied,
      totalSkipped,
      skipRate: ((totalSkipped / (totalApplied + totalSkipped)) * 100).toFixed(2) + '%'
    },
    skipReasons,
    fileResults,
    concurrentEdits
  };

  // Write to .tmp/dry-run-result.json
  const repoRoot = detectRepoRoot();
  if (!repoRoot) {
    console.error('Error: Could not detect repository root');
    process.exit(1);
  }

  const outputPath = path.join(repoRoot, '.tmp', 'dry-run-result.json');
  const outputDir = path.dirname(outputPath);
  fs.mkdirSync(outputDir, { recursive: true });
  fs.writeFileSync(outputPath, JSON.stringify(output, null, 2), 'utf8');

  console.log('=== Results ===');
  console.log(`Operations before deduplication: ${beforeDedup}`);
  console.log(`Operations after deduplication: ${deduplicated.length}`);
  console.log(`Duplicates removed: ${duplicates.length}`);
  console.log('');
  console.log(`Total operations applied: ${totalApplied}`);
  console.log(`Total operations skipped: ${totalSkipped}`);
  console.log(`Skip rate: ${output.summary.skipRate}`);
  console.log('');
  console.log('Skip reason breakdown:');
  console.log(`  duplicate (toolUseId): ${skipReasons.duplicate}`);
  console.log(`  chain_gap: ${skipReasons.chain_gap}`);
  console.log(`  concurrent_edit: ${skipReasons.concurrent_edit}`);
  console.log('');
  console.log(`Potential concurrent edits detected: ${concurrentEdits.length}`);
  console.log('');
  console.log(`Results written to: ${outputPath}`);
}

main().catch(error => {
  console.error('Error during dry-run:');
  console.error(error);
  process.exit(1);
});
