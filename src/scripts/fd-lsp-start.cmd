@echo off
REM Start the LSP daemon (Serena HTTP wrapper)
REM Uses uv to ensure the serena package is available
echo Starting LSP daemon...
python -m uv run --with "git+https://github.com/oraios/serena" python tools/lsp-daemon.py
