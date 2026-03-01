# Email Notification

> **Purpose**: Task completion email notification when user requests it.

## Usage

```bash
node src/tools/node/feature-dashboard/backend/send-email.mjs --subject "[Claude Code] 件名" --body "本文"
```

**Config**: `src/tools/node/feature-dashboard/backend/email.config.json` (Gmail SMTP, gitignored)

## Setup / Recovery

テンプレートからコピーして値を設定:

```bash
cp src/tools/node/feature-dashboard/backend/email.config.template.json src/tools/node/feature-dashboard/backend/email.config.json
```

1. Gmail アプリパスワード生成: https://myaccount.google.com/apppasswords
2. `user`: 送信元 Gmail アドレス
3. `pass`: 生成したアプリパスワード (16文字、スペース区切り)
4. `statusMail.allowedSenders`: メール指示を受け付ける送信元アドレス

## Rules

- ユーザーが明示的に依頼した場合のみ送信する（自動送信しない）
- 件名は `[Claude Code] 作業内容の要約` 形式
- 本文には完了した作業の概要、結果（成功/失敗）、注意点を含める
- 送信失敗してもタスク自体は続行する（fire-and-forget）
