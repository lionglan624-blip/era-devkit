# WSL dotnet test Setup Guide

Smart App Control が `dotnet test` の一時バイナリをブロックする問題の回避策。
WSL2 (Linux) 上で dotnet test を実行することで、Smart App Control の管轄外でテストを行う。

## Status: DONE (2026-02-13)

## Prerequisites

- Windows 11
- 管理者権限

### Setup Note

`/mnt/c/` 上の NTFS ファイルシステムで Unix パーミッション操作を有効にするため、`/etc/wsl.conf` に以下を設定:

```
[automount]
options = "metadata"
```

設定後 `wsl --shutdown` で再起動が必要。

## Setup Steps

### 1. WSL Install (~5 min)

```powershell
wsl --install
```

再起動後、Ubuntu のユーザー名/パスワードを設定。

### 2. .NET SDK Install (~5 min)

```bash
wsl -- bash -c "cd ~ && wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x dotnet-install.sh && ./dotnet-install.sh --channel 10.0 && echo 'export PATH=\$HOME/.dotnet:\$PATH' >> ~/.bashrc"
```

### 3. Symlink (Process.Start 用)

C# テスト内の `Process.Start("dotnet")` が PATH を解決できるようにする:

```bash
wsl -- sudo ln -s /home/siihe/.dotnet/dotnet /usr/local/bin/dotnet
```

### 4. Verify

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c '/home/siihe/.dotnet/dotnet --version'
```

## Usage

Windows ファイルシステムは `/mnt/c/` にマウント済み。ファイル転送不要。

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

> **Note**: ハング防止フラグについては `Skill(testing)` "Hang Detection" セクション参照。

## pre-commit Hook Integration

`.githooks/pre-commit` のテスト実行部分を WSL 経由に変更する:

```bash
# .githooks/pre-commit uses wsl_dotnet() wrapper function
wsl_dotnet test Era.Core.Tests/Era.Core.Tests.csproj --nologo -v m
```

## Resource Impact

| Item | Value |
|------|------:|
| Disk | ~2GB (Ubuntu + .NET SDK) |
| Memory | 0 (idle時は完全停止) |
| Startup | 2-3 sec (初回起動) |
| Test speed | Windows比 同等 (/mnt/c経由のI/O overhead は軽微) |

## Notes

- WSL は使わないときは自動シャットダウンする。常駐なし
- 明示的停止: `wsl --shutdown`
- `/mnt/c/` 経由のファイルアクセスはネイティブ Linux の約 1/5 速度だが、Era.Core テスト (3sec) では問題なし
