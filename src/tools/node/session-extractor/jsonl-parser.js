import readline from 'readline';
import fs from 'fs';
import path from 'path';

/**
 * Parses a single JSONL file and extracts tool_use records
 * @param {string} sessionPath - Absolute path to .jsonl file
 * @param {number} sessionMtime - Session file mtime timestamp
 * @param {string} repoRoot - Repository root path for normalizing relative paths
 * @returns {Promise<Operation[]>} - Array of operations
 */
export async function parseSession(sessionPath, sessionMtime, repoRoot = '') {
  const operations = [];
  const erroredToolUseIds = new Set();
  const structuredPatchMap = new Map(); // tool_use_id -> structuredPatch

  const fileStream = fs.createReadStream(sessionPath);
  const rl = readline.createInterface({
    input: fileStream,
    crlfDelay: Infinity
  });

  for await (const line of rl) {
    // Skip empty lines
    if (!line.trim()) {
      continue;
    }

    let messageObj;
    try {
      messageObj = JSON.parse(line);
    } catch (error) {
      console.error(`[jsonl-parser] Malformed JSON in ${sessionPath}: ${error.message}`);
      continue;
    }

    // Extract content array from either messageObj.message.content or messageObj.content
    const contentArray = (messageObj.message && Array.isArray(messageObj.message.content))
      ? messageObj.message.content
      : (Array.isArray(messageObj.content) ? messageObj.content : null);

    // Handle messages with content array
    if (contentArray) {
      for (const contentBlock of contentArray) {
        // Extract tool_use records
        if (contentBlock.type === 'tool_use') {
          const { name, input, id } = contentBlock;

          if (name === 'Edit' && input) {
            const { file_path, old_string, new_string, replace_all } = input;

            // Skip if missing required fields
            if (!file_path || old_string === undefined || new_string === undefined) {
              continue;
            }

            const normalizedPath = normalizePath(file_path, repoRoot);

            operations.push({
              type: 'edit',
              timestamp: new Date(messageObj.timestamp).getTime(),
              filePath: normalizedPath,
              oldString: old_string,
              newString: new_string,
              replaceAll: replace_all || false,
              toolUseId: id,
              sessionPath: sessionPath
            });
          } else if (name === "Write" && input) {
            const { file_path } = input;

            // Skip if missing required fields
            if (!file_path || input.content === undefined) {
              continue;
            }

            const normalizedPath = normalizePath(file_path, repoRoot);

            operations.push({
              type: 'write',
              timestamp: new Date(messageObj.timestamp).getTime(),
              filePath: normalizedPath,
              content: input.content,
              toolUseId: id,
              sessionPath: sessionPath
            });
          }
        }

        // Extract tool_result records to identify errors
        if (contentBlock.type === 'tool_result') {
          const { tool_use_id, is_error } = contentBlock;

          if (is_error === true && tool_use_id) {
            erroredToolUseIds.add(tool_use_id);
          }
        }
      }

      // Extract structuredPatch from messageObj.toolUseResult (P0-2: top-level, not in contentBlock)
      if (messageObj.toolUseResult && messageObj.toolUseResult.structuredPatch) {
        // Get tool_use_id from contentArray
        for (const contentBlock of contentArray) {
          if (contentBlock.type === 'tool_result' && contentBlock.tool_use_id) {
            structuredPatchMap.set(contentBlock.tool_use_id, messageObj.toolUseResult.structuredPatch);
          }
        }
      }
    }

    // Handle type:progress messages (skill_progress and agent_progress only)
    if (messageObj.type === 'progress') {
      const dataType = messageObj.data?.type;
      if (dataType === 'skill_progress' || dataType === 'agent_progress') {
        const content = messageObj.data?.message?.message?.content;
        const timestamp = messageObj.data?.message?.timestamp || messageObj.timestamp;

        if (Array.isArray(content)) {
          for (const block of content) {
            if (block.type === 'tool_use' && block.input) {
              const { name, input, id } = block;

              if (name === 'Edit') {
                const { file_path, old_string, new_string, replace_all } = input;

                // Skip if missing required fields
                if (!file_path || old_string === undefined || new_string === undefined) {
                  continue;
                }

                const normalizedPath = normalizePath(file_path, repoRoot);

                operations.push({
                  type: 'edit',
                  timestamp: new Date(timestamp).getTime(),
                  filePath: normalizedPath,
                  oldString: old_string,
                  newString: new_string,
                  replaceAll: replace_all || false,
                  toolUseId: id,
                  sessionPath: sessionPath
                });
              } else if (name === 'Write') {
                const { file_path } = input;

                // Skip if missing required fields
                if (!file_path || input.content === undefined) {
                  continue;
                }

                const normalizedPath = normalizePath(file_path, repoRoot);

                operations.push({
                  type: 'write',
                  timestamp: new Date(timestamp).getTime(),
                  filePath: normalizedPath,
                  content: input.content,
                  toolUseId: id,
                  sessionPath: sessionPath
                });
              }
            }

            // Extract tool_result from progress messages to identify errors and structuredPatches
            if (block.type === 'tool_result') {
              const { tool_use_id, is_error } = block;

              if (is_error === true && tool_use_id) {
                erroredToolUseIds.add(tool_use_id);
              }
            }
          }
        }

        // Extract structuredPatch from progress messages
        // Path: messageObj.data.structuredPatch (NOT messageObj.toolUseResult)
        if (messageObj.data?.structuredPatch) {
          // Get tool_use_id from the content blocks (tool_result)
          if (Array.isArray(content)) {
            for (const block of content) {
              if (block.type === 'tool_result' && block.tool_use_id) {
                structuredPatchMap.set(block.tool_use_id, messageObj.data.structuredPatch);
              }
            }
          }
        }
      }
    }
  }

  // Filter out operations that had errors, but keep those with structuredPatch
  const validOperations = operations.filter(op => {
    if (op.toolUseId && erroredToolUseIds.has(op.toolUseId)) {
      // Check if operation has structuredPatch (indicates file was modified)
      if (structuredPatchMap.has(op.toolUseId)) {
        console.log(`[jsonl-parser] Keeping operation ${op.toolUseId} despite is_error (has structuredPatch)`);
        return true;  // Apply - file was actually modified
      }
      console.warn(`[jsonl-parser] Skipping operation ${op.toolUseId} due to is_error: true (no structuredPatch)`);
      return false;  // Skip - true pre-validation error
    }
    return true;
  });

  // Add structuredPatch to operations
  const operationsWithPatch = validOperations.map(op => {
    if (op.toolUseId && structuredPatchMap.has(op.toolUseId)) {
      return { ...op, structuredPatch: structuredPatchMap.get(op.toolUseId) };
    }
    return op;
  });

  return operationsWithPatch;
}

/**
 * Normalizes file paths for consistent comparison
 * @param {string} filePath - Raw file path
 * @param {string} repoRoot - Repository root path for normalizing relative paths
 * @returns {string} - Normalized absolute path with forward slashes
 */
function normalizePath(filePath, repoRoot = '') {
  let normalized = path.normalize(filePath).replace(/\\/g, '/');

  // Convert relative paths to absolute paths using repoRoot
  if (!path.isAbsolute(filePath) && repoRoot) {
    normalized = path.join(repoRoot, filePath).replace(/\\/g, '/');
  }

  return normalized;
}
