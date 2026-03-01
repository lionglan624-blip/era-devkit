"""
LSP Daemon — HTTP server wrapping Serena's Python API for zero-token semantic ops.

Usage:
    python -m uv run --with "git+https://github.com/oraios/serena" python src/tools/python/lsp-daemon.py

Listens on http://127.0.0.1:19999. Routes JSON POST requests to Serena tool.apply().
"""

import json
import logging
import os
import signal
import sys
import traceback
from http.server import BaseHTTPRequestHandler, HTTPServer

from serena.agent import SerenaAgent
from serena.config.serena_config import SerenaConfig
from serena.tools.symbol_tools import (
    FindReferencingSymbolsTool,
    FindSymbolTool,
    GetSymbolsOverviewTool,
    InsertAfterSymbolTool,
    InsertBeforeSymbolTool,
    RenameSymbolTool,
    ReplaceSymbolBodyTool,
    RestartLanguageServerTool,
)

HOST = "127.0.0.1"
PORT = 19999
PROJECT_PATH = os.environ.get(
    "LSP_PROJECT_PATH",
    os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))),
)

log = logging.getLogger("lsp-daemon")

# Global agent instance (persists across requests — LSP stays warm)
agent: SerenaAgent | None = None

# Command → tool class mapping
TOOL_MAP: dict[str, type] = {
    "symbols": GetSymbolsOverviewTool,
    "find": FindSymbolTool,
    "refs": FindReferencingSymbolsTool,
    "replace": ReplaceSymbolBodyTool,
    "insert-before": InsertBeforeSymbolTool,
    "insert-after": InsertAfterSymbolTool,
    "rename": RenameSymbolTool,
    "restart": RestartLanguageServerTool,
}

# Command → parameter name remapping (CLI arg name → Serena apply() param name)
PARAM_REMAP: dict[str, dict[str, str]] = {
    "find": {"name": "name_path_pattern", "path": "relative_path", "body": "include_body"},
    "symbols": {"path": "relative_path"},
    "refs": {"name": "name_path", "path": "relative_path"},
    "replace": {"name": "name_path", "path": "relative_path"},
    "insert-before": {"name": "name_path", "path": "relative_path"},
    "insert-after": {"name": "name_path", "path": "relative_path"},
    "rename": {"name": "name_path", "path": "relative_path", "new_name": "new_name"},
}

# Keys to strip from args before passing to tool.apply()
STRIP_KEYS = {"command"}


def _remap_args(command: str, args: dict) -> dict:
    """Remap CLI-friendly arg names to Serena tool.apply() parameter names."""
    remapped = {}
    remap = PARAM_REMAP.get(command, {})
    for k, v in args.items():
        if k in STRIP_KEYS:
            continue
        new_key = remap.get(k, k)
        remapped[new_key] = v
    return remapped


def _try_parse_json(s: str):
    """Try to parse a JSON string; return parsed object or original string."""
    try:
        return json.loads(s)
    except (json.JSONDecodeError, TypeError):
        return s


class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        try:
            length = int(self.headers.get("Content-Length", 0))
            body = json.loads(self.rfile.read(length)) if length > 0 else {}
        except json.JSONDecodeError as e:
            self._respond(400, {"error": f"Invalid JSON: {e}"})
            return

        command = body.get("command", "")

        if command == "status":
            self._respond(200, {
                "status": "ok",
                "project": PROJECT_PATH,
                "tools": sorted(TOOL_MAP.keys()),
            })
            return

        tool_class = TOOL_MAP.get(command)
        if tool_class is None:
            self._respond(400, {
                "error": f"Unknown command: {command}",
                "available": sorted(TOOL_MAP.keys()) + ["status"],
            })
            return

        assert agent is not None
        tool = agent.get_tool(tool_class)
        kwargs = _remap_args(command, body)

        try:
            result_str = tool.apply(**kwargs)
            # Serena tools return JSON strings — parse to avoid double-encoding
            result = _try_parse_json(result_str)
            self._respond(200, {"result": result})
        except Exception as e:
            log.error("Tool error: %s", traceback.format_exc())
            self._respond(500, {"error": str(e), "type": type(e).__name__})

    def do_GET(self):
        if self.path == "/status":
            self._respond(200, {
                "status": "ok",
                "project": PROJECT_PATH,
                "tools": sorted(TOOL_MAP.keys()),
            })
        else:
            self._respond(404, {"error": "Use POST or GET /status"})

    def _respond(self, code: int, data: dict):
        payload = json.dumps(data, ensure_ascii=False).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    def log_message(self, format, *args):
        # Suppress per-request logging to reduce noise
        pass


def main():
    global agent

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(name)s] %(levelname)s: %(message)s",
    )

    log.info("Loading Serena config...")
    config = SerenaConfig.from_config_file()
    config.gui_log_window = False
    config.web_dashboard = False

    log.info("Initializing SerenaAgent for project: %s", PROJECT_PATH)
    log.info("This may take 5-10 seconds for Roslyn LSP indexing...")
    agent = SerenaAgent(project=PROJECT_PATH, serena_config=config)
    log.info("SerenaAgent ready.")

    server = HTTPServer((HOST, PORT), Handler)

    def shutdown_handler(signum, frame):
        log.info("Shutting down...")
        server.shutdown()
        if agent is not None:
            agent.shutdown()
        sys.exit(0)

    signal.signal(signal.SIGINT, shutdown_handler)
    signal.signal(signal.SIGTERM, shutdown_handler)

    log.info("LSP daemon listening on http://%s:%d", HOST, PORT)
    print(f"LSP daemon ready on http://{HOST}:{PORT}", flush=True)

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        shutdown_handler(None, None)


if __name__ == "__main__":
    main()
