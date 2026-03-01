import fs from 'fs/promises';
import path from 'path';

/**
 * Discovers and sorts JSONL session files by modification time
 * @param {string} sessionDir - Directory containing .jsonl files
 * @returns {Promise<{path: string, mtime: number}[]>} - Sorted session file entries
 */
export async function discoverSessions(sessionDir) {
  try {
    const entries = await fs.readdir(sessionDir, { withFileTypes: true });
    const sessionFiles = [];

    // Root level .jsonl files
    for (const entry of entries) {
      if (!entry.isFile() || !entry.name.endsWith('.jsonl')) {
        continue;
      }

      const filePath = path.join(sessionDir, entry.name);

      try {
        const stats = await fs.stat(filePath);
        sessionFiles.push({
          path: filePath,
          mtime: stats.mtime.getTime()
        });
      } catch (err) {
        // Log and skip unreadable files
        console.error(`Warning: Unable to stat file ${filePath}: ${err.message}`);
        continue;
      }
    }

    // Subagent .jsonl files in */subagents/ subdirectories
    for (const entry of entries) {
      if (!entry.isDirectory()) {
        continue;
      }

      const subagentsPath = path.join(sessionDir, entry.name, 'subagents');

      try {
        // Check if subagents directory exists using fs.stat
        await fs.stat(subagentsPath);
      } catch (err) {
        // Directory doesn't exist or is inaccessible, skip
        continue;
      }

      try {
        const subagentEntries = await fs.readdir(subagentsPath, { withFileTypes: true });

        for (const subEntry of subagentEntries) {
          if (!subEntry.isFile() || !subEntry.name.endsWith('.jsonl')) {
            continue;
          }

          const filePath = path.join(subagentsPath, subEntry.name);

          try {
            const stats = await fs.stat(filePath);
            sessionFiles.push({
              path: filePath,
              mtime: stats.mtime.getTime()
            });
          } catch (err) {
            console.error(`Warning: Unable to stat file ${filePath}: ${err.message}`);
            continue;
          }
        }
      } catch (err) {
        console.error(`Warning: Unable to read subagents directory ${subagentsPath}: ${err.message}`);
        continue;
      }
    }

    // Sort by mtime ascending (oldest first) for chronological ordering
    // If mtime is identical, sort by filename lexicographically (stable sort)
    sessionFiles.sort((a, b) => {
      if (a.mtime !== b.mtime) {
        return a.mtime - b.mtime;
      }
      return a.path.localeCompare(b.path);
    });

    return sessionFiles;
  } catch (err) {
    console.error(`Error reading session directory ${sessionDir}: ${err.message}`);
    return [];
  }
}
