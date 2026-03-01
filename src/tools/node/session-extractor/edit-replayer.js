/**
 * edit-replayer.js
 *
 * Simple string-based replacement engine that applies Edit operations sequentially.
 */

/**
 * Applies a sequence of operations to reconstruct file state
 * @param {Operation[]} operations - Operations sorted chronologically
 * @param {string|null} baseContent - Initial file content (null if file is new)
 * @returns {{content: string, stats: {appliedOps: number, skippedOps: number, warnings: string[]}}}
 */
export function replayOperations(operations, baseContent) {
  let content = baseContent || '';
  const stats = {
    appliedOps: 0,
    skippedOps: 0,
    warnings: []
  };

  // If baseContent is null and first operation is edit, skip it (chain gap)
  if (baseContent === null && operations.length > 0 && operations[0].type === 'edit') {
    stats.warnings.push('First operation is edit but file is new (baseContent is null) - skipping operation');
    stats.skippedOps++;
    operations = operations.slice(1);
  }

  for (const operation of operations) {
    if (operation.type === 'write') {
      // Write operation: replace entire content
      content = operation.content || '';
      stats.appliedOps++;
    } else if (operation.type === 'edit') {
      const { oldString, newString, replaceAll } = operation;

      // Skip if oldString is empty
      if (!oldString || oldString === '') {
        stats.warnings.push('Edit operation skipped: empty oldString');
        stats.skippedOps++;
        continue;
      }

      // Check if oldString exists in current content
      const firstIndex = content.indexOf(oldString);
      if (firstIndex === -1) {
        stats.warnings.push(`Edit operation skipped: old_string not found (chain gap)`);
        stats.skippedOps++;
        continue;
      }

      if (replaceAll) {
        // Replace all occurrences using split-join (safe from infinite loops)
        content = content.split(oldString).join(newString);
        stats.appliedOps++;
      } else {
        // Replace first occurrence only using .replace(
        content = content.replace(oldString, newString);
        stats.appliedOps++;
      }
    }
  }

  return { content, stats };
}
