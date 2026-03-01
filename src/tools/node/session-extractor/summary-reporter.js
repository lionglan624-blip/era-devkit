/**
 * summary-reporter.js
 *
 * Generates summary report for session extraction results.
 * Collects per-file metadata, calculates confidence metrics, outputs JSON.
 */

/**
 * Calculates confidence level for a file based on applied operation percentage
 * @param {number} applied - Number of applied operations
 * @param {number} total - Total operations attempted
 * @returns {'high'|'medium'|'low'} - Confidence level
 */
export function calculateConfidence(applied, total) {
  if (total === 0) return 'high'; // No operations = nothing to fail

  const percentage = (applied / total) * 100;

  if (percentage >= 95) return 'high';
  if (percentage >= 75) return 'medium';
  return 'low';
}

/**
 * Generates summary report for all processed files
 * @param {Array} fileResults - Array of {path, operations, applied, skipped, warnings}
 * @param {number} sessionCount - Total number of sessions processed
 * @param {number} totalOperations - Total operations extracted
 * @returns {Object} - Summary report object
 */
export function generateSummary(fileResults, sessionCount, totalOperations) {
  const files = fileResults.map(result => {
    const total = result.applied + result.skipped;
    const confidence = calculateConfidence(result.applied, total);

    return {
      path: result.path,
      sourceSessions: sessionCount,
      operations: result.operations,
      applied: result.applied,
      skipped: result.skipped,
      warnings: result.warnings || [],
      confidence
    };
  });

  return {
    metadata: {
      timestamp: new Date().toISOString(),
      sessionCount,
      totalOperations
    },
    files
  };
}

/**
 * Generates summary report as JSON string
 * @param {Array} fileResults - Array of file result objects
 * @param {number} sessionCount - Total sessions processed
 * @param {number} totalOperations - Total operations extracted
 * @returns {string} - JSON string representation of summary report
 */
export function generateSummaryJSON(fileResults, sessionCount, totalOperations) {
  const summary = generateSummary(fileResults, sessionCount, totalOperations);
  return JSON.stringify(summary, null, 2);
}
