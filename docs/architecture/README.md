# Designs

承認前の設計提案・技術検討ドキュメント。

## Design Granularity Standard

**1 Design = 1 Version**

Each design document should target a single version (e.g., v2.0, v3.5). Designs spanning multiple versions violate the "1 Design = 1 Version" principle and should be split or moved to reference/.

### Directory Roles

- **designs/**: 実装設計 (Implementation design) per version. Version-specific technical proposals and planning documents. Each design targets one version.
- **reference/**: アーキテクチャ概要 (Architecture overview) spanning versions. High-level system architecture and cross-version documentation that does not target specific versions.

## Status

| Status | 意味 |
|--------|------|
| `DRAFT` | 議論中 |
| `UNDER_REVIEW` | レビュー待ち |
| `APPROVED` | 承認済み → feature化へ |
| `ARCHIVED` | 実装完了またはキャンセル |

## Designs

### v1.7-v1.8: System Extensions (S0.5-S0.6)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [new-commands.md](new-commands.md) | v1.7 | DRAFT | 新コマンド (+37) + TCVAR:116 |
| [m-orgasm-system.md](m-orgasm-system.md) | v1.8 | DRAFT | M絶頂/五重絶頂システム (快Ｍ + 無自覚絶頂) |

### v2.x-v5.x: NTR Core (S1-S3)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [phase-system.md](phase-system.md) | v3.x | DRAFT | Phase管理詳細（Phase0-8+/Route分岐/Body Zone） |
| [visitor-type-system.md](visitor-type-system.md) | v4.x | DRAFT | 訪問者タイプ選択（タイプ→ルート対応/プレイヤー介入度） |

### v6.x: Netorase (S4)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [netorase-system.md](netorase-system.md) | v6.x | DRAFT | 寝取らせ基盤 + 売春 |
| [brothel-mob-system.md](brothel-mob-system.md) | v6.x | DRAFT | 風俗店・モブ客システム |

### v7.x: Reconciliation (S5)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [reconciliation-system.md](reconciliation-system.md) | v7.x | DRAFT | 復縁（二軸好感度/再発リスク/訪問者反応/NTR刻印） |
| [corruption-outfit-system.md](corruption-outfit-system.md) | v7.x | DRAFT | 堕落度連動衣装変化 |

### v8.x: MC Growth (S6)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [ntr-flavor-stats.md](ntr-flavor-stats.md) | v8.x | DRAFT | フレーバーステータス（ランク/比較/主人公成長） |
| [chastity-belt-system.md](chastity-belt-system.md) | v8.x | DRAFT | 貞操帯（バリエーション/鍵管理/NTR連携） |

### v9.x: Pregnancy (S7)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [pregnancy-system.md](pregnancy-system.md) | v9.x | DRAFT | 妊娠/出産/托卵/攻略対象化 |

### v10.x: Incident War (S8)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [incident-war-system.md](incident-war-system.md) | v10.x | DRAFT | 異変・勢力間戦争（弾幕ごっこ）システム |

### v1.1-v1.6: Kojo Variations (C3-C8)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [character-ai-system.md](character-ai-system.md) | v1.5 | DRAFT | キャラクターAI・行動パターン |
| [character-interaction-system.md](character-interaction-system.md) | v1.5 | DRAFT | キャラクター間インタラクション |

### v0.7: Audit Fix (F253)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [v0.7-audit-fix.md](v0.7-audit-fix.md) | v0.7 | DRAFT | F253監査結果修正（配置エラー/空スケルトン/解釈問題） |

### v0.8: Content Phase 8d (80-90 Kojo)

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [v0.8-kojo-80-90.md](v0.8-kojo-80-90.md) | v0.8 | DRAFT | 80-90番台口上（16 COM × 10キャラ、C2品質） |

### Infrastructure / Architecture

| Name | Version | Status | Description |
|------|:-------:|:------:|-------------|
| [full-csharp-architecture.md](full-csharp-architecture.md) | v1.9-v1.11 | FINAL | Full C#/Unity Migration Architecture (F343) - Exception: spans 3 versions as single migration unit |
| [anthropic-recommended-transition.md](anthropic-recommended-transition.md) | - | DRAFT | Hooks/Sessions/ドキュメント設計転換 |
| [test-strategy.md](test-strategy.md) | - | FINAL | Era.Core test strategy (IRandomProvider, test layers, F499) |
| [hook-testing.md](hook-testing.md) | - | DRAFT | Hook動作検証設計提案 (F202/F212/F214) |
| [extensible-orchestrator.md](extensible-orchestrator.md) | - | DRAFT | Extensible Orchestrator Design (F612) |
| [callback-error-handling.md](callback-error-handling.md) | - | FINAL | Callback Factory error handling pattern analysis (F415) |
| [dashboard-recovery-plan.md](dashboard-recovery-plan.md) | - | FINAL | Repository recovery procedure (F733-F736) |
| [performance-analytics.md](performance-analytics.md) | - | DRAFT | Performance analytics architecture (F607) |
| [lifestyle-system.md](lifestyle-system.md) | v2.x-v8.x | DRAFT | 生活システム（食事・運動・弾幕）設計書 |

### Analysis & Migration Reports

| Name | Description |
|------|-------------|
| [codebase-analysis-report.md](codebase-analysis-report.md) | ERB/C# codebase analysis for migration design (F344) |
| [test-migration-audit.md](test-migration-audit.md) | Test infrastructure categorization for C# migration (F362) |
| [architecture-analysis-562.md](architecture-analysis-562.md) | C# Engine + YAML content boundary definition (F562) |
| [architecture-review-15.md](architecture-review-15.md) | Phase 1-4 compliance review (F493) |
| [testability-assessment-15.md](testability-assessment-15.md) | Era.Core testability assessment (F498) |
| [folder-structure-15.md](folder-structure-15.md) | Folder structure validation Phase 15 (F496) |
| [naming-conventions-15.md](naming-conventions-15.md) | Naming convention audit Phase 15 (442 files) |
| [sessions-analysis.md](sessions-analysis.md) | Sessions-based context sharing analysis |
| [claude-code-current.md](claude-code-current.md) | Claude Code architecture current state inventory (F149) |
| [claude-code-gap-analysis.md](claude-code-gap-analysis.md) | Anthropic best practices gap analysis |
| [claude-code-migration.md](claude-code-migration.md) | Claude Code migration plan |
| [claude-code-target.md](claude-code-target.md) | Claude Code target architecture design (F149) |

### 参考資料

| Name | Description |
|------|-------------|
| [ntr-phase.md](ntr-phase.md) | Phase System設計議論ログ（ChatGPT） |
| [eratw-missing-commands.md](eratw-missing-commands.md) | eraTW未実装コマンド調査 |

## Workflow

```
/plan v{X.Y} (Version計画)
    │ goal-setter → dependency-analyzer → spec-writer
    ▼
designs/{name}.md (DRAFT)
    │ 技術検討・議論
    ▼ 承認後 (Status: APPROVED)
/next (Feature分割)
    │ feature-{ID}.md作成
    ▼
/imple {ID} (実装)
    │ 分割して実装
    ▼ 完了後
reference/ (リファレンス)
```
