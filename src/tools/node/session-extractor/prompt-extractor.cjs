#!/usr/bin/env node

/**
 * User Prompt Extractor for Claude Code Session Files
 *
 * Extracts user prompts and Claude responses from session JSONL files
 * to understand conversation flow during dashboard/handoff development.
 *
 * Usage:
 *   node prompt-extractor.cjs [--start YYYY-MM-DD] [--end YYYY-MM-DD] [--design-only]
 *
 * Options:
 *   --start YYYY-MM-DD    Start date for filtering (default: 2026-01-01)
 *   --end YYYY-MM-DD      End date for filtering (default: 2026-02-01)
 *   --design-only         Filter for design discussions only (excludes implementation sessions)
 *
 * Examples:
 *   node prompt-extractor.cjs --start 2026-01-01 --end 2026-02-01
 *   node prompt-extractor.cjs --start 2026-01-01 --end 2026-02-01 --design-only
 */

const fs = require('fs');
const path = require('path');
const readline = require('readline');

// Configuration
const SESSION_DIR = 'C:\\Users\\siihe\\.ccs\\shared\\context-groups\\default\\projects\\C--Era-devkit';
const OUTPUT_FILE = 'C:\\Era\\devkit\\_out\\tmp\\user-prompts-dashboard-v2.md';
const OUTPUT_FILE_DESIGN_ONLY = 'C:\\Era\\devkit\\_out\\tmp\\user-prompts-design-only.md';
const RELEVANCE_KEYWORDS = ['dashboard', 'handoff', 'HANDOFF', 'feature-dashboard'];

// Default date range (pre-incident period)
const DEFAULT_START = '2026-01-01';
const DEFAULT_END = '2026-02-01';

// Design-only filtering patterns
const EXCLUDE_SUMMARY_PATTERNS = [
	// Feature implementation sessions
	/F\d{3}.*(?:implementation|FL review|Progressive|workflow|ERB|Engine|Batch|Complete|Resume|TDD|AC validation)/i,
	/Feature \d{3}/i,
	/(?:\/fl|\/run|\/fc|\/next).*(?:Feature|F\d{3})/i,

	// Rate limit/error sessions
	/You've hit your limit/i,
	/You're out of extra usage/i,
	/resets \d+am/i,
	/API Error/i,

	// Automated queue/status
	/Queue (?:status|analysis|generation)/i,
	/Phase \d+ (?:queue|Kojo|Post-Review)/i,

	// Pure implementation
	/TDD|Batch Converter|YAML (?:Migration|Conversion)|ERB Implementation/i,
	/(?:Bug Fix|fix|Fix)(?!.*Dashboard)/i,
];

const EXCLUDE_PROMPT_PATTERNS = [
	/^<local-command-stdout><\/local-command-stdout>$/,
	/^(?:続けて|continue|y|n|Y|N|yes|no|OK|ok|commit|finalize|LGTM|完了|done)$/i,
];

const KEEP_SUMMARY_PATTERNS = [
	// Dashboard design
	/Dashboard.*(?:UI|UX|設計|design|Architecture|Stream|Integration|Layout|Code Review)/i,

	// Workflow design
	/(?:workflow|Workflow).*(?:design|改善|improvement|設計)/i,
	/(?:\/fc|FL|Progressive Disclosure).*(?:design|改善|設計)/i,

	// Infrastructure
	/(?:CCS|PM2|proxy|プロキシ|setup|Setup|Install|設定)/i,
	/Claude Code設定|ハッカソン|比較|Multi-account|switching/i,

	// Architecture
	/アーキテクチャ|Architecture|Documentation|Handoff|HANDOFF/i,
	/Feature Creator|Finalizer|Orchestrator|subagent/i,
];

const KEEP_PROMPT_PATTERNS = [
	// Design questions (Japanese)
	/どうすれば|なぜ|ワークフロー改善|設計|アーキテクチャ|検討|提案|推奨/,

	// Design questions (English)
	/how to|why.*design|architecture|workflow improvement|recommend/i,

	// Dashboard-specific
	/dashboard|タイル|tile|UI|UX|レイアウト/i,

	// HANDOFF
	/HANDOFF|引継ぎ|handoff|引き継ぎ/i,

	// Infrastructure
	/PM2|proxy|CCS|profile|switch|設定|config/i,
];

// Parse command line arguments
const args = process.argv.slice(2);
let startDate = DEFAULT_START;
let endDate = DEFAULT_END;
let designOnly = false;

for (let i = 0; i < args.length; i++) {
	if (args[i] === '--start' && i + 1 < args.length) {
		startDate = args[i + 1];
		i++;
	} else if (args[i] === '--end' && i + 1 < args.length) {
		endDate = args[i + 1];
		i++;
	} else if (args[i] === '--design-only') {
		designOnly = true;
	}
}

console.log(`Date range filter: ${startDate} to ${endDate}`);
if (designOnly) {
	console.log('Design-only filtering enabled');
}

/**
 * Check if content is a genuine user prompt
 * @param {object} entry - JSONL entry
 * @returns {boolean}
 */
function isGenuineUserPrompt(entry) {
	if (entry.type !== 'user') return false;
	if (entry.userType !== 'external') return false;
	if (entry.isMeta === true) return false;

	const content = getContent(entry);
	if (!content) return false;

	// Apply noise filters
	if (isNoiseContent(content)) return false;

	return true;
}

/**
 * Check if content is noise (should be filtered out)
 * @param {string} content - Message content
 * @returns {boolean}
 */
function isNoiseContent(content) {
	const trimmed = content.trim();

	// Single char responses (including "N" in example)
	if (/^[yYnN123ABCab]$/.test(trimmed)) {
		return true;
	}

	// Very short responses (2-3 chars) that are likely noise
	if (trimmed.length <= 3 && /^[yYnN]+$/i.test(trimmed)) {
		return true;
	}

	// Continuation phrases
	if (/^続けて$/.test(trimmed) || /^continue$/i.test(trimmed)) {
		return true;
	}

	// Interruption markers
	if (/^\[Request interrupted by user/.test(trimmed)) {
		return true;
	}

	// Strip command tags and check if remaining content is too short
	const stripped = content
		.replace(/<command-message>[\s\S]*?<\/command-message>/g, '')
		.replace(/<command-name>[\s\S]*?<\/command-name>/g, '')
		.replace(/<command-args>[\s\S]*?<\/command-args>/g, '')
		.replace(/<skill-format>[\s\S]*?<\/skill-format>/g, '')
		.trim();

	// If only command tags (stripped is empty or very short), filter out
	if (stripped.length < 10) {
		return true;
	}

	return false;
}

/**
 * Check if content is an assistant response
 * @param {object} entry - JSONL entry
 * @returns {boolean}
 */
function isAssistantResponse(entry) {
	if (entry.type !== 'assistant') return false;

	const content = getContent(entry);
	if (!content) return false;

	return true;
}

/**
 * Get content from message (handle string or array)
 * @param {object} entry - JSONL entry
 * @returns {string|null}
 */
function getContent(entry) {
	if (!entry.message || !entry.message.content) return null;

	const content = entry.message.content;

	// Handle string
	if (typeof content === 'string') {
		return content;
	}

	// Handle array (concatenate text parts)
	if (Array.isArray(content)) {
		return content
			.filter(part => part.type === 'text')
			.map(part => part.text)
			.join('\n');
	}

	return null;
}

/**
 * Check if session is relevant to dashboard/handoff
 * @param {string} filePath - JSONL file path
 * @returns {Promise<boolean>}
 */
async function isRelevantSession(filePath) {
	return new Promise((resolve, reject) => {
		const stream = fs.createReadStream(filePath);
		const rl = readline.createInterface({
			input: stream,
			crlfDelay: Infinity
		});

		let foundRelevance = false;

		rl.on('line', (line) => {
			if (foundRelevance) return;

			// Check if line contains any keyword
			for (const keyword of RELEVANCE_KEYWORDS) {
				if (line.includes(keyword)) {
					foundRelevance = true;
					rl.close();
					stream.destroy();
					break;
				}
			}
		});

		rl.on('close', () => {
			resolve(foundRelevance);
		});

		rl.on('error', (err) => {
			reject(err);
		});
	});
}

/**
 * Check if session should be kept for design-only filtering
 * @param {object} session - Session data
 * @returns {object} Result with keep decision and reason
 */
function shouldKeepSessionDesignOnly(session) {
	// Check summary against EXCLUDE patterns
	if (session.summary) {
		for (const pattern of EXCLUDE_SUMMARY_PATTERNS) {
			if (pattern.test(session.summary)) {
				return { keep: false, reason: 'summary_exclude' };
			}
		}

		// Check summary against KEEP patterns
		for (const pattern of KEEP_SUMMARY_PATTERNS) {
			if (pattern.test(session.summary)) {
				return { keep: true, reason: 'summary_keep' };
			}
		}
	}

	// For unmatched sessions, check if any prompt matches KEEP patterns
	for (const conv of session.conversations) {
		// Skip excluded prompts
		let isExcludedPrompt = false;
		for (const pattern of EXCLUDE_PROMPT_PATTERNS) {
			if (pattern.test(conv.content)) {
				isExcludedPrompt = true;
				break;
			}
		}
		if (isExcludedPrompt) continue;

		// Check against KEEP patterns
		for (const pattern of KEEP_PROMPT_PATTERNS) {
			if (pattern.test(conv.content)) {
				return { keep: true, reason: 'prompt_keep' };
			}
		}
	}

	// Otherwise exclude
	return { keep: false, reason: 'no_match' };
}

/**
 * Check if timestamp is within date range
 * @param {string} timestamp - ISO timestamp
 * @param {string} start - Start date (YYYY-MM-DD)
 * @param {string} end - End date (YYYY-MM-DD)
 * @returns {boolean}
 */
function isWithinDateRange(timestamp, start, end) {
	try {
		const date = new Date(timestamp);
		const startDate = new Date(start);
		const endDate = new Date(end);
		return date >= startDate && date <= endDate;
	} catch (err) {
		// If timestamp parsing fails, exclude the entry
		return false;
	}
}

/**
 * Extract user prompts and Claude responses from a session file
 * @param {string} filePath - JSONL file path
 * @param {string} startDate - Start date filter
 * @param {string} endDate - End date filter
 * @returns {Promise<object>} Session data with conversations
 */
async function extractPromptsFromSession(filePath, startDate, endDate) {
	return new Promise((resolve, reject) => {
		const stream = fs.createReadStream(filePath);
		const rl = readline.createInterface({
			input: stream,
			crlfDelay: Infinity
		});

		const conversations = [];
		let summary = null;
		let sessionId = null;
		let firstTimestamp = null;
		let lastTimestamp = null;
		let lastUserPrompt = null;

		rl.on('line', (line) => {
			try {
				const entry = JSON.parse(line);

				// Extract session ID
				if (!sessionId && entry.sessionId) {
					sessionId = entry.sessionId;
				}

				// Extract summary if available
				if (!summary && entry.summary) {
					summary = entry.summary;
				}

				// Track first/last timestamps
				if (entry.timestamp) {
					if (!firstTimestamp) firstTimestamp = entry.timestamp;
					lastTimestamp = entry.timestamp;
				}

				// Extract user prompts (with date filter)
				if (isGenuineUserPrompt(entry)) {
					if (!isWithinDateRange(entry.timestamp, startDate, endDate)) {
						return;
					}

					const content = getContent(entry);
					if (content) {
						lastUserPrompt = {
							timestamp: entry.timestamp,
							content: content,
							response: null
						};
						conversations.push(lastUserPrompt);
					}
				}

				// Extract assistant responses (pair with last user prompt)
				if (isAssistantResponse(entry) && lastUserPrompt && !lastUserPrompt.response) {
					if (!isWithinDateRange(entry.timestamp, startDate, endDate)) {
						return;
					}

					const content = getContent(entry);
					if (content) {
						lastUserPrompt.response = {
							timestamp: entry.timestamp,
							content: content
						};
					}
				}
			} catch (err) {
				// Skip invalid JSON lines
			}
		});

		rl.on('close', () => {
			resolve({
				sessionId: sessionId || path.basename(filePath, '.jsonl'),
				summary,
				firstTimestamp,
				lastTimestamp,
				conversations
			});
		});

		rl.on('error', (err) => {
			reject(err);
		});
	});
}

/**
 * Truncate text to specified length
 * @param {string} text - Text to truncate
 * @param {number} maxLength - Maximum length
 * @returns {string}
 */
function truncate(text, maxLength) {
	// Keep newlines for better readability in conversation format
	if (text.length <= maxLength) {
		return text;
	}

	return text.substring(0, maxLength) + '...';
}

/**
 * Escape markdown special characters
 * @param {string} text - Text to escape
 * @returns {string}
 */
function escapeMarkdown(text) {
	return text
		.replace(/\|/g, '\\|')
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;');
}

/**
 * Format timestamp for display
 * @param {string} timestamp - ISO timestamp
 * @returns {string}
 */
function formatTimestamp(timestamp) {
	try {
		const date = new Date(timestamp);
		return date.toISOString().replace('T', ' ').substring(0, 16);
	} catch (err) {
		return 'unknown';
	}
}

/**
 * Generate markdown output
 * @param {Array<object>} sessions - Session data
 * @param {string} startDate - Start date filter
 * @param {string} endDate - End date filter
 * @param {object} stats - Optional statistics for design-only mode
 * @returns {string}
 */
function generateMarkdown(sessions, startDate, endDate, stats = null) {
	let md = '# User Prompts & Claude Responses from Dashboard/Handoff Sessions\n\n';
	md += `Generated: ${new Date().toISOString()}\n`;
	md += `Date range: ${startDate} to ${endDate}\n\n`;

	if (stats) {
		md += '## Filtering Statistics\n\n';
		md += `- Sessions excluded by summary pattern: ${stats.excludedBySummary}\n`;
		md += `- Sessions kept by summary pattern: ${stats.keptBySummary}\n`;
		md += `- Sessions kept by prompt pattern: ${stats.keptByPrompt}\n`;
		md += `- Final count: ${stats.finalCount}\n\n`;
	}

	// Sort sessions by first timestamp
	sessions.sort((a, b) => {
		const aTime = a.firstTimestamp || '';
		const bTime = b.firstTimestamp || '';
		return aTime.localeCompare(bTime);
	});

	for (const session of sessions) {
		md += `## Session: ${session.sessionId}\n`;

		if (session.summary) {
			md += `Summary: ${session.summary}\n`;
		}

		if (session.firstTimestamp && session.lastTimestamp) {
			md += `Period: ${formatTimestamp(session.firstTimestamp)} to ${formatTimestamp(session.lastTimestamp)}\n`;
		}

		md += '\n';

		// Generate conversation format
		for (let i = 0; i < session.conversations.length; i++) {
			const conv = session.conversations[i];

			md += `### Conversation ${i + 1}\n`;
			md += `**User** (${formatTimestamp(conv.timestamp)}):\n`;
			md += `> ${escapeMarkdown(truncate(conv.content, 500))}\n\n`;

			if (conv.response) {
				md += `**Claude**:\n`;
				md += `> ${escapeMarkdown(truncate(conv.response.content, 500))}\n\n`;
			} else {
				md += `**Claude**: (no response captured)\n\n`;
			}

			md += '---\n\n';
		}
	}

	return md;
}

/**
 * Main execution
 */
async function main() {
	try {
		console.log('Scanning session files...');

		// Get all JSONL files
		const files = fs.readdirSync(SESSION_DIR)
			.filter(f => f.endsWith('.jsonl'))
			.map(f => path.join(SESSION_DIR, f));

		console.log(`Found ${files.length} session files`);

		// Filter for relevant sessions
		console.log('Filtering for dashboard/handoff sessions...');
		const relevantFiles = [];

		for (const file of files) {
			const isRelevant = await isRelevantSession(file);
			if (isRelevant) {
				relevantFiles.push(file);
			}
		}

		console.log(`Found ${relevantFiles.length} relevant sessions`);

		// Extract prompts from relevant sessions
		console.log('Extracting conversations...');
		const sessions = [];

		for (const file of relevantFiles) {
			const session = await extractPromptsFromSession(file, startDate, endDate);
			if (session.conversations.length > 0) {
				sessions.push(session);
			}
		}

		console.log(`Extracted conversations from ${sessions.length} sessions`);

		// Apply design-only filtering if requested
		let outputFile = OUTPUT_FILE;
		let stats = null;

		if (designOnly) {
			stats = {
				excludedBySummary: 0,
				keptBySummary: 0,
				keptByPrompt: 0,
				finalCount: 0
			};

			const filteredSessions = [];

			for (const session of sessions) {
				const result = shouldKeepSessionDesignOnly(session);
				if (result.keep) {
					filteredSessions.push(session);
					if (result.reason === 'summary_keep') {
						stats.keptBySummary++;
					} else if (result.reason === 'prompt_keep') {
						stats.keptByPrompt++;
					}
				} else if (result.reason === 'summary_exclude') {
					stats.excludedBySummary++;
				}
			}

			stats.finalCount = filteredSessions.length;

			console.log(`Design-only filtering applied:`);
			console.log(`  - Excluded by summary: ${stats.excludedBySummary}`);
			console.log(`  - Kept by summary: ${stats.keptBySummary}`);
			console.log(`  - Kept by prompt: ${stats.keptByPrompt}`);
			console.log(`  - Final count: ${stats.finalCount}`);

			sessions.length = 0;
			sessions.push(...filteredSessions);
			outputFile = OUTPUT_FILE_DESIGN_ONLY;
		}

		// Generate markdown
		const markdown = generateMarkdown(sessions, startDate, endDate, stats);

		// Ensure .tmp directory exists
		const tmpDir = path.dirname(outputFile);
		if (!fs.existsSync(tmpDir)) {
			fs.mkdirSync(tmpDir, { recursive: true });
		}

		// Write output
		fs.writeFileSync(outputFile, markdown, 'utf8');

		console.log(`Output written to ${outputFile}`);
		console.log(`Total sessions: ${sessions.length}`);
		console.log(`Total conversations: ${sessions.reduce((sum, s) => sum + s.conversations.length, 0)}`);

	} catch (err) {
		console.error('Error:', err);
		process.exit(1);
	}
}

main();
