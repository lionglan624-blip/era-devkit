# Email Notification

> **Purpose**: Task completion email notification.

## Two Systems

### 1. Dashboard Auto-Email (emailService.js)

Automatic notifications from the dashboard backend. Subject prefix: `[ERA]`.

**Subject format**: `[ERA] CMD {ID} {result}` (feature) / `[ERA] {command} {result} HH:MM` (non-feature)

**Notification rules**:
- Completion (ok/fail): always sent
- input-wait / askuserquestion: **delayed 5 minutes** — cancelled if user answers in browser
- Terminal handoff: sent immediately (requires user action)
- account-limit: only sent when no auto-retry scheduled
- rate-limit-recovered: suppressed (auto-recovery is normal operation)
- rate-limit-exhausted: always sent
- resume: prefix stripped (`RESUME:RUN` → `RUN`)

### 2. CLI Manual Email (send-email.mjs)

Manual notification via Claude Code agent. Subject prefix: `[Claude Code]`.

```bash
node C:/Era/dashboard/backend/send-email.mjs --subject "[Claude Code] 件名" --body "本文"
```

**Rules**:
- ユーザーが明示的に依頼した場合のみ送信する（自動送信しない）
- 件名は `[Claude Code] 作業内容の要約` 形式
- 本文には完了した作業の概要、結果（成功/失敗）、注意点を含める
- 送信失敗してもタスク自体は続行する（fire-and-forget）

## Setup / Recovery

**Config**: `C:\Era\dashboard\backend\email.config.json` (Gmail SMTP, gitignored)

テンプレートからコピーして値を設定:

```bash
cp C:/Era/dashboard/backend/email.config.template.json C:/Era/dashboard/backend/email.config.json
```

1. Gmail アプリパスワード生成: https://myaccount.google.com/apppasswords
2. `user`: 送信元 Gmail アドレス
3. `pass`: 生成したアプリパスワード (16文字、スペース区切り)
4. `statusMail.allowedSenders`: メール指示を受け付ける送信元アドレス
