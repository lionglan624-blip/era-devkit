#!/usr/bin/env node

/**
 * organize-by-topic.cjs
 *
 * Organizes extracted design prompts by topic.
 *
 * Input: .tmp/user-prompts-design-only.md
 * Output: .tmp/design-docs/{topic}.md
 *
 * Topics:
 * - dashboard-ui: Dashboard UI/UX design decisions
 * - workflow-system: FL, /fc, status lifecycle, AC:Task alignment
 * - infrastructure: PM2, proxy, CCS, Windows Terminal
 * - community-tooling: YAML validator, VS Code integration
 * - architecture: HANDOFF, subagent strategy, Progressive Disclosure
 * - uncategorized: Sessions that don't fit other categories
 */

const fs = require('fs');
const path = require('path');

const INPUT_FILE = '.tmp/user-prompts-design-only.md';
const OUTPUT_DIR = '.tmp/design-docs';

const TOPIC_PATTERNS = {
  'dashboard-ui': [
    /dashboard.*(?:UI|UX|タイル|tile|button|ボタン|layout|レイアウト|mobile|モバイル|TreeView|panel)/i,
    /(?:表示|display|stream|WebSocket|real-?time)/i,
    /(?:color|色|terminal|ターミナル|カラーコード)/i,
  ],
  'workflow-system': [
    /(?:FL|\/fl|\/fc|\/run|\/next).*(?:workflow|phase|ワークフロー)/i,
    /(?:DRAFT|PROPOSED|REVIEWED|WIP|DONE).*status/i,
    /AC:?Task|alignment|Progressive Disclosure/i,
    /dependency.*gate|blocker|依存関係/i,
    /(?:feature.*creation|feature.*quality)/i,
  ],
  'infrastructure': [
    /PM2|daemon|proxy|プロキシ/i,
    /CCS|profile.*switch|アカウント切り替え/i,
    /Windows Terminal|設定|config|環境変数/i,
    /Git Bash|シェル|shell/i,
  ],
  'community-tooling': [
    /validator|linter|YAML.*(?:schema|check)/i,
    /VS ?Code|IDE|extension|拡張機能/i,
    /contributor|コミュニティ|共同開発/i,
    /(?:community|com)-validator/i,
  ],
  'architecture': [
    /HANDOFF|引継ぎ|handoff/i,
    /subagent|orchestrat|dispatch|エージェント戦略/i,
    /architecture|アーキテクチャ/i,
    /SSOT|single source/i,
    /requires|frontmatter|Progressive Disclosure/i,
  ],
};

const TOPIC_DESCRIPTIONS = {
  'dashboard-ui': 'Dashboard UI/UX design decisions - タイル表示、カラー処理、TreeView、mobile対応',
  'workflow-system': 'FL, /fc, /run workflow - status lifecycle, AC:Task alignment, dependency gate',
  'infrastructure': 'Infrastructure - PM2, proxy, CCS, Windows Terminal, Git Bash',
  'community-tooling': 'Community tooling - YAML validator, VS Code integration',
  'architecture': 'Architecture - HANDOFF, subagent strategy, Progressive Disclosure, SSOT',
  'uncategorized': 'Uncategorized sessions - sessions that don\'t fit other categories',
};

/**
 * Parse sessions from markdown file
 * @param {string} content - File content
 * @returns {Array<{id: string, summary: string, period: string, content: string}>}
 */
function parseSessions(content) {
  const sessions = [];
  const lines = content.split('\n');

  let currentSession = null;
  let sessionContent = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];

    // Session header: ## Session: {id}
    if (line.startsWith('## Session: ')) {
      // Save previous session
      if (currentSession) {
        currentSession.content = sessionContent.join('\n');
        sessions.push(currentSession);
      }

      // Start new session
      const id = line.replace('## Session: ', '').trim();
      currentSession = { id, summary: '', period: '', content: '' };
      sessionContent = [line];
    } else if (currentSession) {
      sessionContent.push(line);

      // Extract summary
      if (line.startsWith('Summary: ')) {
        currentSession.summary = line.replace('Summary: ', '').trim();
      }

      // Extract period
      if (line.startsWith('Period: ')) {
        currentSession.period = line.replace('Period: ', '').trim();
      }
    }
  }

  // Save last session
  if (currentSession) {
    currentSession.content = sessionContent.join('\n');
    sessions.push(currentSession);
  }

  return sessions;
}

/**
 * Categorize session by keywords
 * @param {object} session - Session object
 * @returns {string} - Topic name
 */
function categorizeSession(session) {
  const searchText = `${session.summary}\n${session.content}`;

  for (const [topic, patterns] of Object.entries(TOPIC_PATTERNS)) {
    for (const pattern of patterns) {
      if (pattern.test(searchText)) {
        return topic;
      }
    }
  }

  return 'uncategorized';
}

/**
 * Group sessions by topic
 * @param {Array} sessions - Array of session objects
 * @returns {Object} - Sessions grouped by topic
 */
function groupByTopic(sessions) {
  const groups = {};

  for (const session of sessions) {
    const topic = categorizeSession(session);
    if (!groups[topic]) {
      groups[topic] = [];
    }
    groups[topic].push(session);
  }

  return groups;
}

/**
 * Extract timestamp from period string
 * @param {string} period - Period string like "2026-01-20 12:11 to 2026-01-20 12:14"
 * @returns {Date}
 */
function extractTimestamp(period) {
  const match = period.match(/(\d{4}-\d{2}-\d{2} \d{2}:\d{2})/);
  if (match) {
    return new Date(match[1]);
  }
  return new Date(0); // Fallback
}

/**
 * Sort sessions by timestamp
 * @param {Array} sessions - Array of session objects
 * @returns {Array} - Sorted sessions
 */
function sortByTimestamp(sessions) {
  return sessions.sort((a, b) => {
    const timeA = extractTimestamp(a.period);
    const timeB = extractTimestamp(b.period);
    return timeA - timeB;
  });
}

/**
 * Generate topic file content
 * @param {string} topic - Topic name
 * @param {Array} sessions - Array of session objects
 * @returns {string} - Markdown content
 */
function generateTopicFile(topic, sessions) {
  const sorted = sortByTimestamp(sessions);
  const description = TOPIC_DESCRIPTIONS[topic] || topic;

  let content = `# ${topic}\n\n`;
  content += `${description}\n\n`;
  content += `**Session count**: ${sessions.length}\n\n`;
  content += `---\n\n`;

  for (const session of sorted) {
    content += `${session.content}\n\n`;
    content += `---\n\n`;
  }

  return content;
}

/**
 * Generate index file
 * @param {Object} groups - Sessions grouped by topic
 * @returns {string} - Markdown content
 */
function generateIndex(groups) {
  let content = `# Design Prompts - Topic Index\n\n`;
  content += `Generated: ${new Date().toISOString()}\n\n`;

  const topics = Object.keys(groups).sort((a, b) => {
    // uncategorized last
    if (a === 'uncategorized') return 1;
    if (b === 'uncategorized') return -1;
    return a.localeCompare(b);
  });

  content += `## Summary\n\n`;
  content += `| Topic | Sessions | Description |\n`;
  content += `|-------|:--------:|-------------|\n`;

  for (const topic of topics) {
    const count = groups[topic].length;
    const desc = TOPIC_DESCRIPTIONS[topic] || topic;
    content += `| [${topic}](${topic}.md) | ${count} | ${desc} |\n`;
  }

  content += `\n---\n\n`;
  content += `## Topics\n\n`;

  for (const topic of topics) {
    const sessions = groups[topic];
    const desc = TOPIC_DESCRIPTIONS[topic] || topic;

    content += `### [${topic}](${topic}.md)\n\n`;
    content += `${desc}\n\n`;
    content += `**Sessions** (${sessions.length}):\n\n`;

    const sorted = sortByTimestamp(sessions);
    for (const session of sorted) {
      content += `- ${session.period}: ${session.summary}\n`;
    }

    content += `\n`;
  }

  return content;
}

/**
 * Main execution
 */
function main() {
  console.log('Reading input file...');
  const content = fs.readFileSync(INPUT_FILE, 'utf-8');

  console.log('Parsing sessions...');
  const sessions = parseSessions(content);
  console.log(`Found ${sessions.length} sessions`);

  console.log('Categorizing sessions...');
  const groups = groupByTopic(sessions);

  // Create output directory
  if (!fs.existsSync(OUTPUT_DIR)) {
    fs.mkdirSync(OUTPUT_DIR, { recursive: true });
  }

  console.log('Writing topic files...');
  for (const [topic, topicSessions] of Object.entries(groups)) {
    const filename = path.join(OUTPUT_DIR, `${topic}.md`);
    const content = generateTopicFile(topic, topicSessions);
    fs.writeFileSync(filename, content, 'utf-8');
    console.log(`  ${filename} (${topicSessions.length} sessions)`);
  }

  console.log('Writing index...');
  const indexContent = generateIndex(groups);
  fs.writeFileSync(path.join(OUTPUT_DIR, 'index.md'), indexContent, 'utf-8');

  console.log('\nDone!');
  console.log(`Output: ${OUTPUT_DIR}/`);
}

main();
