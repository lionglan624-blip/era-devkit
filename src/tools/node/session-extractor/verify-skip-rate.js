#!/usr/bin/env node

/**
 * verify-skip-rate.js
 *
 * Verifies skip rate from session-extractor output.
 * Reads .tmp/recovery/summary.json and validates skip rates.
 *
 * Usage:
 *   node verify-skip-rate.js claudeService.js  # Verify specific file has zero skips
 *   node verify-skip-rate.js App.jsx           # Verify specific file has zero skips
 *   node verify-skip-rate.js overall           # Verify overall skip rate < 1%
 *
 * Exit codes:
 *   0 - Success (validation passed)
 *   1 - Failure (validation failed or error)
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Summary.json location relative to repository root
const SUMMARY_PATH = '.tmp/recovery/summary.json';

/**
 * Reads and parses summary.json
 * @returns {Object} - Parsed summary object
 */
function readSummary() {
  // Try relative to current working directory first
  let summaryPath = path.resolve(process.cwd(), SUMMARY_PATH);

  if (!fs.existsSync(summaryPath)) {
    // Try relative to script directory (tools/session-extractor)
    const repoRoot = path.resolve(__dirname, '../..');
    summaryPath = path.join(repoRoot, SUMMARY_PATH);
  }

  if (!fs.existsSync(summaryPath)) {
    console.error(`Error: summary.json not found at ${SUMMARY_PATH}`);
    console.error('Run session-extractor first to generate summary.json');
    process.exit(1);
  }

  try {
    const content = fs.readFileSync(summaryPath, 'utf8');
    return JSON.parse(content);
  } catch (error) {
    console.error(`Error: Failed to read/parse summary.json: ${error.message}`);
    process.exit(1);
  }
}

/**
 * Verifies skip rate for a specific file
 * @param {Object} summary - Parsed summary object
 * @param {string} fileName - File name to verify (e.g., "claudeService.js")
 * @returns {boolean} - True if validation passed
 */
function verifyFileSkipRate(summary, fileName) {
  // Find file in summary
  const fileResult = summary.files.find(f => f.path.endsWith(fileName));

  if (!fileResult) {
    console.error(`Error: File "${fileName}" not found in summary.json`);
    console.error('Available files:');
    summary.files.forEach(f => console.error(`  - ${f.path}`));
    return false;
  }

  console.log(`Verifying skip rate for: ${fileResult.path}`);
  console.log(`  Applied operations: ${fileResult.applied}`);
  console.log(`  Skipped operations: ${fileResult.skipped}`);

  if (fileResult.skipped === 0) {
    console.log(`✓ PASS: Zero skips for ${fileName}`);
    return true;
  } else {
    console.error(`✗ FAIL: Expected 0 skips, but found ${fileResult.skipped} skips for ${fileName}`);
    return false;
  }
}

/**
 * Verifies overall skip rate is under 1%
 * @param {Object} summary - Parsed summary object
 * @returns {boolean} - True if validation passed
 */
function verifyOverallSkipRate(summary) {
  // Calculate totals from all files
  let totalApplied = 0;
  let totalSkipped = 0;

  for (const file of summary.files) {
    totalApplied += file.applied;
    totalSkipped += file.skipped;
  }

  const totalOps = totalApplied + totalSkipped;
  const skipRate = totalOps > 0 ? (totalSkipped / totalOps) : 0;
  const skipRatePercent = (skipRate * 100).toFixed(2);

  console.log('Verifying overall skip rate:');
  console.log(`  Total applied operations: ${totalApplied}`);
  console.log(`  Total skipped operations: ${totalSkipped}`);
  console.log(`  Total operations: ${totalOps}`);
  console.log(`  Skip rate: ${skipRatePercent}%`);

  if (skipRate < 0.01) {
    console.log(`✓ PASS: Skip rate (${skipRatePercent}%) is under 1%`);
    return true;
  } else {
    console.error(`✗ FAIL: Skip rate (${skipRatePercent}%) exceeds 1% threshold`);
    return false;
  }
}

/**
 * Main entry point
 */
function main() {
  const args = process.argv.slice(2);

  if (args.length === 0) {
    console.error('Usage:');
    console.error('  node verify-skip-rate.js claudeService.js  # Verify specific file');
    console.error('  node verify-skip-rate.js App.jsx           # Verify specific file');
    console.error('  node verify-skip-rate.js overall           # Verify overall skip rate');
    process.exit(1);
  }

  const mode = args[0];
  const summary = readSummary();

  let success = false;

  if (mode === 'overall') {
    success = verifyOverallSkipRate(summary);
  } else {
    // Assume it's a file name
    success = verifyFileSkipRate(summary, mode);
  }

  process.exit(success ? 0 : 1);
}

main();
