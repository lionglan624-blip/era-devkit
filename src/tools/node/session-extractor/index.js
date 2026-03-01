#!/usr/bin/env node

/**
 * Session JSONL Extractor CLI
 *
 * Thin entry point that coordinates session discovery, parsing, replay, and reporting.
 * Reconstructs lost file state from Claude Code session JSONL files.
 *
 * Usage:
 *   node index.js <session-dir> [base-path-prefix]
 *
 * Example:
 *   node index.js C:\Users\siihe\.ccs\shared\projects\C--Era-erakoumakanNTR
 *   node index.js /path/to/sessions "C:/Era/erakoumakanNTR/"
 *
 * Output:
 *   .tmp/recovery/<relative-path>  (reconstructed files)
 *   .tmp/recovery/summary.json     (metadata report)
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';
import { discoverSessions } from './session-discovery.js';
import { parseSession } from './jsonl-parser.js';
import { replayOperations } from './edit-replayer.js';
import { generateSummary } from './summary-reporter.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Constants
const OUTPUT_DIR = '.tmp/recovery';

/**
 * Normalize path to forward slashes and strip base path prefix
 * @param {string} filePath - Absolute file path
 * @param {string} basePrefix - Base path prefix to strip
 * @returns {string} - Relative path with forward slashes
 */
function getRelativePath(filePath, basePrefix) {
  // Normalize to forward slashes
  const normalized = filePath.replace(/\\/g, '/');

  // Strip base prefix if present
  if (basePrefix && normalized.startsWith(basePrefix)) {
    return normalized.slice(basePrefix.length);
  }

  return normalized;
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
      // Ensure trailing slash
      return currentDir.endsWith('/') ? currentDir : currentDir + '/';
    }

    const parentDir = path.dirname(currentDir).replace(/\\/g, '/');
    if (parentDir === currentDir) {
      break; // Reached root
    }
    currentDir = parentDir;
  }

  return null;
}

/**
 * Main CLI entry point
 */
async function main() {
  // Parse CLI arguments
  const sessionDir = process.argv[2];
  let basePath = process.argv[3];

  if (!sessionDir) {
    console.error('Usage: node index.js <session-dir> [base-path-prefix]');
    console.error('');
    console.error('Example:');
    console.error('  node index.js C:\\Users\\user\\.ccs\\shared\\projects\\repo');
    console.error('  node index.js /path/to/sessions "C:/Era/erakoumakanNTR/"');
    process.exit(1);
  }

  // If base path not provided, attempt to detect repository root
  if (!basePath) {
    basePath = detectRepoRoot();
    if (basePath) {
      console.log(`Detected repository root: ${basePath}`);
    } else {
      console.log('Warning: Could not detect repository root. Output paths will be absolute.');
      basePath = '';
    }
  } else {
    // Normalize user-provided base path
    basePath = basePath.replace(/\\/g, '/');
    // Ensure trailing slash
    if (!basePath.endsWith('/')) {
      basePath += '/';
    }
  }

  console.log(`Session directory: ${sessionDir}`);
  console.log(`Base path prefix: ${basePath || '(none)'}`);
  console.log(`Output directory: ${OUTPUT_DIR}`);
  console.log('');

  // Step 1: Discover sessions
  console.log('Step 1: Discovering session files...');
  const sessions = await discoverSessions(sessionDir);

  if (sessions.length === 0) {
    console.log('No session files found. Nothing to do.');
    process.exit(0);
  }

  console.log(`Found ${sessions.length} session files (sorted chronologically by mtime)`);
  console.log('');

  // Step 2: Parse all sessions and collect operations
  console.log('Step 2: Parsing sessions and extracting operations...');
  const allOperations = [];
  let totalOperations = 0;

  for (const session of sessions) {
    const operations = await parseSession(session.path, session.mtime, basePath);
    allOperations.push(...operations);
    totalOperations += operations.length;
  }

  if (totalOperations === 0) {
    console.log('No operations extracted from sessions. Nothing to reconstruct.');
    process.exit(0);
  }

  console.log(`Extracted ${totalOperations} operations from ${sessions.length} sessions`);
  console.log('');

  // Global sort by timestamp
  allOperations.sort((a, b) => a.timestamp - b.timestamp);

  // Step 2.5: Deduplicate by toolUseId
  // Same operation appears in both subagent JSONL and parent progress messages
  const seenToolUseIds = new Set();
  const beforeDedup = allOperations.length;
  const dedupedOperations = allOperations.filter(op => {
    if (op.toolUseId) {
      if (seenToolUseIds.has(op.toolUseId)) {
        return false; // Duplicate
      }
      seenToolUseIds.add(op.toolUseId);
    }
    return true;
  });
  console.log(`Deduplicated: ${beforeDedup} -> ${dedupedOperations.length} operations (removed ${beforeDedup - dedupedOperations.length} duplicates)`);

  // Replace allOperations with deduplicated version
  allOperations.length = 0;
  allOperations.push(...dedupedOperations);

  // Step 3: Group operations by file path
  console.log('Step 3: Grouping operations by file path...');
  const operationsByFile = new Map();

  for (const op of allOperations) {
    if (!operationsByFile.has(op.filePath)) {
      operationsByFile.set(op.filePath, []);
    }
    operationsByFile.get(op.filePath).push(op);
  }

  console.log(`Operations affect ${operationsByFile.size} unique files`);
  console.log('');

  // Step 4: Replay operations for each file
  console.log('Step 4: Replaying operations to reconstruct files...');
  const fileResults = [];

  for (const [filePath, operations] of operationsByFile.entries()) {
    // F740: Skip files with no valid operations (defensive guard)
    if (operations.length === 0) {
      continue;
    }

    // Detect the correct git repository for this file
    const { gitRoot, relativePath: gitPath } = findGitRoot(filePath, basePath);

    // F740: Get base content at the time of FIRST operation on this file
    // NOT HEAD (which is post-session state)
    const earliestOpTimestamp = operations[0].timestamp; // Already sorted by timestamp
    const commitAtStart = getCommitAtTime(gitRoot, earliestOpTimestamp);

    let baseContent = null;
    if (commitAtStart) {
      baseContent = getFileAtCommit(gitRoot, gitPath, commitAtStart);
      if (baseContent === null) {
        console.log(`[git] File not in commit ${commitAtStart.slice(0, 7)}: ${gitPath}`);
      }
    } else {
      console.log(`[git] No commits before ${new Date(earliestOpTimestamp).toISOString()}: ${gitPath}`);
    }

    // F740: Apply ALL operations - NO timestamp filtering
    const result = replayOperations(operations, baseContent);

    // Get relative path for output (relative to main basePath, not gitRoot)
    const relativePath = getRelativePath(filePath, basePath);

    // Skip files outside the repository (absolute paths that couldn't be relativized)
    if (relativePath.includes(':') || relativePath.startsWith('/')) {
      console.warn(`  ⚠ Skipping external file: ${relativePath}`);
      continue;
    }

    const outputPath = path.join(OUTPUT_DIR, relativePath);

    // Create parent directories
    const outputDir = path.dirname(outputPath);
    fs.mkdirSync(outputDir, { recursive: true });

    // Write reconstructed content
    fs.writeFileSync(outputPath, result.content, 'utf8');

    // Count write/edit operations
    const writeOps = operations.filter(op => op.type === 'write').length;
    const editOps = operations.filter(op => op.type === 'edit').length;

    // Collect result for summary
    fileResults.push({
      path: relativePath,
      outputPath,
      operations: { write: writeOps, edit: editOps },
      applied: result.stats.appliedOps,
      skipped: result.stats.skippedOps,
      warnings: result.stats.warnings
    });

    console.log(`  ✓ ${relativePath} (${operations.length} ops, ${result.stats.appliedOps} applied, ${result.stats.skippedOps} skipped)`);
  }

  console.log('');

  // Step 5: Generate summary report
  console.log('Step 5: Generating summary report...');
  const summary = generateSummary(fileResults, sessions.length, totalOperations);

  // Write summary to .tmp/recovery/summary.json
  const summaryPath = path.join(OUTPUT_DIR, 'summary.json');
  fs.writeFileSync(summaryPath, JSON.stringify(summary, null, 2), 'utf8');

  console.log(`Summary report written to: ${summaryPath}`);
  console.log('');

  // Step 6: Print summary to console
  console.log('=== Recovery Summary ===');
  console.log(`Total files: ${summary.files.length}`);
  console.log(`Total operations: ${summary.metadata.totalOperations}`);
  console.log(`Session count: ${summary.metadata.sessionCount}`);
  const confidences = summary.files.map(f => f.confidence);
  const overallConfidence = confidences.includes('low') ? 'low' : confidences.includes('medium') ? 'medium' : 'high';
  console.log(`Overall confidence: ${overallConfidence}`);
  console.log('');
  console.log('Recovery complete.');
}

// Run main and handle errors
main().catch(error => {
  console.error('Fatal error during recovery:');
  console.error(error);
  process.exit(1);
});
