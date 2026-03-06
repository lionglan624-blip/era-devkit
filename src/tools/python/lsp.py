"""
LSP CLI client — queries the lsp-daemon via HTTP POST.

Usage:
    python src/tools/python/lsp.py status
    python src/tools/python/lsp.py symbols src/Era.Core/NtrEngine.cs
    python src/tools/python/lsp.py symbols src/Era.Core/NtrEngine.cs --depth 1
    python src/tools/python/lsp.py find NtrEngine --path src/Era.Core/ --depth 1 --body
    python src/tools/python/lsp.py refs NtrEngine --path src/Era.Core/NtrEngine.cs
    python src/tools/python/lsp.py rename OldName NewName --path src/Era.Core/Foo.cs
    python src/tools/python/lsp.py replace ClassName/Method --path src/Era.Core/Foo.cs --body "new code"
    python src/tools/python/lsp.py insert-after ClassName --path src/Era.Core/Foo.cs --body "new code"
    python src/tools/python/lsp.py insert-before ClassName --path src/Era.Core/Foo.cs --body "new code"
    python src/tools/python/lsp.py restart
"""

import argparse
import json
import sys
import urllib.error
import urllib.request

DAEMON_URL = "http://127.0.0.1:19999"

# Bypass system proxy for localhost
_opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))


def post(data: dict) -> dict:
    payload = json.dumps(data).encode("utf-8")
    req = urllib.request.Request(
        DAEMON_URL,
        data=payload,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with _opener.open(req, timeout=120) as resp:
            return json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8", errors="replace")
        try:
            return json.loads(body)
        except json.JSONDecodeError:
            return {"error": f"HTTP {e.code}: {body}"}
    except urllib.error.URLError as e:
        return {"error": f"Cannot connect to daemon at {DAEMON_URL}: {e.reason}"}


def main():
    parser = argparse.ArgumentParser(
        prog="lsp",
        description="Query Serena LSP daemon for C# semantic operations",
    )
    sub = parser.add_subparsers(dest="command", required=True)

    # status
    sub.add_parser("status", help="Check daemon health")

    # restart
    sub.add_parser("restart", help="Restart language server")

    # symbols <path> [--depth N]
    p = sub.add_parser("symbols", help="List symbols in a file")
    p.add_argument("path", help="Path relative to Serena project root (src/)")
    p.add_argument("--depth", type=int, default=0, help="Child depth (default: 0)")

    # find <name> [--path PATH] [--depth N] [--body]
    p = sub.add_parser("find", help="Find symbol by name path pattern")
    p.add_argument("name", help="Name path pattern (e.g. ClassName/Method)")
    p.add_argument("--path", default="", help="Restrict to file/directory (relative to Serena project root src/)")
    p.add_argument("--depth", type=int, default=0, help="Child depth")
    p.add_argument("--body", action="store_true", help="Include source code")

    # refs <name> --path PATH
    p = sub.add_parser("refs", help="Find references to a symbol")
    p.add_argument("name", help="Name path of symbol")
    p.add_argument("--path", required=True, help="File containing the symbol")

    # replace <name> --path PATH --body BODY
    p = sub.add_parser("replace", help="Replace symbol body")
    p.add_argument("name", help="Name path of symbol")
    p.add_argument("--path", required=True, help="File containing the symbol")
    p.add_argument("--body", required=True, help="New symbol body")

    # insert-before <name> --path PATH --body BODY
    p = sub.add_parser("insert-before", help="Insert code before symbol")
    p.add_argument("name", help="Name path of symbol")
    p.add_argument("--path", required=True, help="File containing the symbol")
    p.add_argument("--body", required=True, help="Code to insert")

    # insert-after <name> --path PATH --body BODY
    p = sub.add_parser("insert-after", help="Insert code after symbol")
    p.add_argument("name", help="Name path of symbol")
    p.add_argument("--path", required=True, help="File containing the symbol")
    p.add_argument("--body", required=True, help="Code to insert")

    # rename <name> <new_name> --path PATH
    p = sub.add_parser("rename", help="Rename symbol across codebase")
    p.add_argument("name", help="Name path of symbol")
    p.add_argument("new_name", help="New name for the symbol")
    p.add_argument("--path", required=True, help="File containing the symbol")

    args = parser.parse_args()
    data = {k: v for k, v in vars(args).items() if v is not None}

    resp = post(data)

    if "error" in resp:
        print(json.dumps(resp, indent=2, ensure_ascii=False), file=sys.stderr)
        sys.exit(1)
    else:
        result = resp.get("result", resp)
        print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
