import { createLogger } from '../utils/logger.js';
import { nowJST } from '../utils/timeUtils.js';

export class UpdateWatcherService {
    constructor({ emailService, logStreamer, claudeService } = {}) {
        this.logger = createLogger('update-watcher');
        this.emailService = emailService;
        this.logStreamer = logStreamer;
        this.claudeService = claudeService;

        this._lastVersion = null;
        this._analyzing = false;
        this._lastExecutionId = null;
    }

    async handleRelease(version, releaseUrl, rawSource) {
        if (this._lastVersion === version) {
            this.logger.info(`Duplicate version ${version}, skipping`);
            return;
        }

        if (this._analyzing) {
            this.logger.warn(`Already analyzing, skipping ${version}`);
            return;
        }

        this._lastVersion = version;
        this.logger.info(`Processing release ${version}`);

        const changelog = this._extractChangelog(rawSource);
        if (!changelog) {
            this.logger.warn(`No changelog found for ${version}, sending URL-only notification`);
            await this._notifyNoChangelog(version);
            return;
        }

        const prompt = this._buildAnalysisPrompt(version, changelog);

        this._analyzing = true;

        if (!this.claudeService) {
            this.logger.error('claudeService not available, cannot analyze');
            this._analyzing = false;
            await this._notifyNoChangelog(version);
            return;
        }

        const executionId = this.claudeService.executeUpdateAnalysis(prompt, (execution, exitCode) => {
            this._analyzing = false;
            const analysis = exitCode === 0 ? execution.lastAssistantText : null;
            if (exitCode !== 0) {
                this.logger.error(`Analysis execution failed for ${version} (exit ${exitCode})`);
            }
            this._sendEmail(version, changelog, analysis);
        });

        this._lastExecutionId = executionId;
        this.logger.info(`Analysis execution started: ${executionId}`);
    }

    _extractChangelog(rawSource) {
        if (!rawSource) return null;

        const raw = typeof rawSource === 'string' ? rawSource : rawSource.toString('utf8');

        // Find text/plain part or use entire body
        let body = raw;
        const bodyStart = raw.indexOf('\r\n\r\n');
        if (bodyStart !== -1) {
            body = raw.substring(bodyStart + 4);
        }

        // Decode quoted-printable (soft line breaks =\r\n, then =XX byte sequences)
        body = body.replace(/=\r?\n/g, '');
        // Collect =XX sequences into byte arrays and decode as UTF-8
        body = body.replace(/((?:=[0-9A-Fa-f]{2})+)/g, (match) => {
            const bytes = [];
            for (const m of match.matchAll(/=([0-9A-Fa-f]{2})/g)) {
                bytes.push(parseInt(m[1], 16));
            }
            return Buffer.from(bytes).toString('utf8');
        });

        // Extract "What's changed" section
        // Try multiple heading patterns GitHub uses
        const patterns = [
            /##\s*What['\u2019]?s?\s*changed/i,
            /\*\*What['\u2019]?s?\s*changed\*\*/i,
            /What['\u2019]?s?\s*changed/i,
        ];

        let startIdx = -1;
        for (const pattern of patterns) {
            const match = body.search(pattern);
            if (match !== -1) {
                startIdx = match;
                break;
            }
        }

        if (startIdx === -1) {
            // No "What's changed" section found, use full body as changelog
            const cleaned = body
                .replace(/--[\s\S]*$/, '') // Strip email signature
                .replace(/Content-Type:.*$/gim, '')
                .replace(/Content-Transfer-Encoding:.*$/gim, '')
                .replace(/--[a-zA-Z0-9_\-.]+(--)?/g, '')
                .trim();
            return cleaned || null;
        }

        let section = body.substring(startIdx);

        // End at signature or next major section
        const endPatterns = [/\r?\n--\s*\r?\n/, /\r?\nYou are receiving this/, /\r?\nReply to this email/];
        for (const pattern of endPatterns) {
            const endMatch = section.search(pattern);
            if (endMatch !== -1) {
                section = section.substring(0, endMatch);
            }
        }

        return section.trim() || null;
    }

    _buildAnalysisPrompt(version, changelog) {
        return `Claude Code ${version} がリリースされました。以下の changelog を分析し、
feature-dashboard（Claude Code の自動化ダッシュボード）への影響を報告してください。

重点チェック項目:
1. stream-json 出力フォーマットの変更 (streamParser.js でパース)
2. --resume / --output-format / -p CLI フラグの変更
3. AskUserQuestion / tool_use 検出の変更
4. 終了コード挙動の変更
5. レート制限 / コンテキストウィンドウの変更
6. 破壊的変更

Changelog:
${changelog}

回答形式:
- IMPACT: HIGH / MEDIUM / LOW / NONE
- 関連する変更のリスト（各項目に影響の説明）
- 推奨アクション（あれば）`;
    }

    async _notifyNoChangelog(version) {
        if (this.emailService) {
            const html = this._buildEmailHtml(version, 'UNKNOWN', null, null);
            try {
                await this.emailService.sendHtml(`Claude Code ${version} update (UNKNOWN impact)`, html);
            } catch (err) {
                this.logger.error(`Email notification failed: ${err.message}`);
            }
        }
        if (this.logStreamer) {
            this.logStreamer.broadcastAll({
                type: 'claude-code-update',
                version,
                impact: 'UNKNOWN',
                summary: 'Changelog extraction failed',
                timestamp: new Date().toISOString(),
            });
        }
    }

    _sendEmail(version, changelog, analysis) {
        if (!this.emailService) return;

        const impact = this._extractImpact(analysis);
        const subject = `Claude Code ${version} update (${impact} impact)`;
        const html = this._buildEmailHtml(version, impact, changelog, analysis);

        this.emailService.sendHtml(subject, html).catch((err) => {
            this.logger.error(`Email notification failed: ${err.message}`);
        });

        this.logger.info(`Release ${version} processed: ${impact} impact`);
    }

    _extractImpact(analysis) {
        if (!analysis) return 'UNKNOWN';
        const match = analysis.match(/IMPACT:\s*(HIGH|MEDIUM|LOW|NONE)/i);
        return match ? match[1].toUpperCase() : 'UNKNOWN';
    }

    _buildEmailHtml(version, impact, changelog, analysis) {
        const impactColors = {
            HIGH: '#dc3545',
            MEDIUM: '#fd7e14',
            LOW: '#28a745',
            NONE: '#6c757d',
            UNKNOWN: '#6c757d',
        };
        const color = impactColors[impact] || impactColors.UNKNOWN;

        const escape = (s) => (s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

        const parts = [
            `<h2>Claude Code ${escape(version)}</h2>`,
            `<p><span style="background:${color};color:white;padding:2px 8px;border-radius:4px;font-weight:bold">${impact}</span></p>`,
        ];

        if (analysis) {
            parts.push(`<h3>Analysis</h3><pre style="white-space:pre-wrap">${escape(analysis)}</pre>`);
        } else {
            parts.push(`<p><em>Analysis unavailable (timeout or extraction failure)</em></p>`);
        }

        if (changelog) {
            parts.push(`<h3>Changelog</h3><pre style="white-space:pre-wrap">${escape(changelog)}</pre>`);
        }

        parts.push(`<p style="color:#999;font-size:12px">${nowJST()}</p>`);

        return parts.join('\n');
    }

    getLastUpdate() {
        return {
            version: this._lastVersion,
            analyzing: this._analyzing,
            executionId: this._lastExecutionId,
        };
    }

    isAnalyzing() {
        return this._analyzing;
    }
}
