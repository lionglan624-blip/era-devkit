"""Minimal MCP stdio server exposing read-only C# LSP operations."""

import json
import sys
import urllib.error
import urllib.request

DAEMON = "http://127.0.0.1:19999"
_opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))

TOOLS = [
    {
        "name": "symbols",
        "description": "List symbols in a C# file",
        "inputSchema": {
            "type": "object",
            "properties": {
                "path": {"type": "string", "description": "Relative file path"},
                "depth": {"type": "integer"},
            },
            "required": ["path"],
        },
    },
    {
        "name": "find",
        "description": "Find C# symbol by name",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {"type": "string", "description": "e.g. Class/Method"},
                "path": {"type": "string"},
                "depth": {"type": "integer"},
                "body": {"type": "boolean"},
            },
            "required": ["name"],
        },
    },
    {
        "name": "refs",
        "description": "Find references to a C# symbol",
        "inputSchema": {
            "type": "object",
            "properties": {
                "name": {"type": "string"},
                "path": {"type": "string"},
            },
            "required": ["name", "path"],
        },
    },
]


def _post(data: dict) -> dict:
    payload = json.dumps(data).encode()
    req = urllib.request.Request(
        DAEMON, data=payload, headers={"Content-Type": "application/json"}, method="POST"
    )
    try:
        with _opener.open(req, timeout=120) as resp:
            return json.loads(resp.read())
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8", errors="replace")
        try:
            return json.loads(body)
        except json.JSONDecodeError:
            return {"error": f"HTTP {e.code}: {body}"}
    except urllib.error.URLError as e:
        return {"error": f"Cannot connect to lsp-daemon: {e.reason}"}


def _write(obj: dict):
    line = json.dumps(obj, ensure_ascii=False)
    msg = f"Content-Length: {len(line.encode())}\r\n\r\n{line}"
    sys.stdout.buffer.write(msg.encode())
    sys.stdout.buffer.flush()


def _read() -> dict | None:
    # Read Content-Length header (binary mode to avoid Windows CRLF issues)
    inp = sys.stdin.buffer
    while True:
        header = inp.readline()
        if not header:
            return None  # EOF
        header_str = header.decode("utf-8").strip()
        if header_str == "":
            continue
        if header_str.startswith("Content-Length:"):
            length = int(header_str.split(":")[1].strip())
            # Read blank line separator
            inp.readline()
            body = inp.read(length)
            return json.loads(body)


def handle(msg: dict) -> dict:
    method = msg.get("method", "")
    mid = msg.get("id")

    if method == "initialize":
        return {
            "jsonrpc": "2.0",
            "id": mid,
            "result": {
                "protocolVersion": "2024-11-05",
                "capabilities": {"tools": {}},
                "serverInfo": {"name": "csharp", "version": "1.0.0"},
            },
        }

    if method == "notifications/initialized":
        return None  # No response for notifications

    if method == "tools/list":
        return {"jsonrpc": "2.0", "id": mid, "result": {"tools": TOOLS}}

    if method == "tools/call":
        params = msg.get("params", {})
        tool_name = params.get("name", "")
        args = params.get("arguments", {})

        if tool_name not in ("symbols", "find", "refs"):
            return {
                "jsonrpc": "2.0",
                "id": mid,
                "result": {
                    "content": [{"type": "text", "text": f"Unknown tool: {tool_name}"}],
                    "isError": True,
                },
            }

        data = {"command": tool_name, **args}
        resp = _post(data)

        if "error" in resp:
            text = json.dumps(resp, indent=2, ensure_ascii=False)
            return {
                "jsonrpc": "2.0",
                "id": mid,
                "result": {"content": [{"type": "text", "text": text}], "isError": True},
            }

        result = resp.get("result", resp)
        text = json.dumps(result, indent=2, ensure_ascii=False)
        return {
            "jsonrpc": "2.0",
            "id": mid,
            "result": {"content": [{"type": "text", "text": text}]},
        }

    # Unknown method
    if mid is not None:
        return {
            "jsonrpc": "2.0",
            "id": mid,
            "error": {"code": -32601, "message": f"Unknown method: {method}"},
        }
    return None


def main():
    while True:
        msg = _read()
        if msg is None:
            break
        resp = handle(msg)
        if resp is not None:
            _write(resp)


if __name__ == "__main__":
    main()
