#!/usr/bin/env python3
"""Dashboard diagnostic tool - execution diagnosis via API + logs."""

import argparse
import json
import re
import sys
from datetime import datetime
from pathlib import Path
from urllib.request import urlopen, Request
from urllib.error import URLError

API_BASE = 'http://localhost:3001/api'

# App log patterns (same as pm2_diag.py --exec-trace)
APP_LOG_RE = re.compile(r'\[(\d{4}-\d{2}-\d{2}T[\d:.+]+)\] \[(\w+)\] \[(\w+)\] (.+)')
BROADCAST_RE = re.compile(r'Broadcast \[(\w+)\] to (\d+) clients for execution (.+)')
DISCONNECT_RE = re.compile(r'Client (\d+) disconnected \(code: (\d+)\)')
SUBSCRIBE_RE = re.compile(r'Client (\d+) subscribed to (.+)')


def api_get(path):
    """GET from dashboard API. Returns parsed JSON or None on error."""
    try:
        req = Request(f'{API_BASE}{path}')
        with urlopen(req, timeout=5) as resp:
            return json.loads(resp.read().decode())
    except URLError as e:
        print(f"API error ({path}): {e}", file=sys.stderr)
        return None
    except json.JSONDecodeError:
        print(f"API returned non-JSON ({path})", file=sys.stderr)
        return None


def get_default_app_log_path():
    """Get default dashboard-backend app log path."""
    return Path.home() / '.pm2' / 'logs' / 'dashboard-backend-out.log'


def parse_app_log_for_exec(log_path, exec_id):
    """Parse app log for events related to a specific execution."""
    events = []
    try:
        with open(log_path, 'r', encoding='utf-8', errors='replace') as f:
            for line in f:
                line = line.rstrip('\n')
                if exec_id not in line:
                    continue
                m = APP_LOG_RE.match(line)
                if not m:
                    continue
                events.append({
                    'timestamp': m.group(1),
                    'level': m.group(2),
                    'category': m.group(3),
                    'message': m.group(4),
                })
    except FileNotFoundError:
        pass  # Log unavailable is not fatal for --exec mode
    return events


def exec_diagnosis(exec_id, app_log_path):
    """Diagnose a specific execution: API + log + JSONL correlation."""
    # 1. API: diag endpoint
    diag = api_get(f'/execution/{exec_id}/diag')
    if not diag:
        # Try history
        history = api_get('/execution/history')
        if history:
            match = [h for h in history if h.get('executionId') == exec_id]
            if match:
                print(f"\nExecution {exec_id} (from history)")
                h = match[0]
                for k in ('featureId', 'command', 'status', 'exitCode', 'startedAt', 'completedAt', 'contextPercent'):
                    print(f"  {k}: {h.get(k, '-')}")
            else:
                print(f"Execution {exec_id} not found in API or history")
                return
        else:
            print(f"Execution {exec_id} not found (API unreachable?)")
            return
    else:
        # Live execution data
        exec_data = diag.get('execution', {})
        subs = diag.get('subscribers', {})
        chain = diag.get('chain', {})

        print(f"\nExecution {exec_id}")
        print(f"  Status:     {exec_data.get('status', '?')}")
        print(f"  Command:    {exec_data.get('command', '?')}")
        print(f"  Feature:    F{exec_data.get('featureId', '?')}")
        print(f"  Exit Code:  {exec_data.get('exitCode', '-')}")
        print(f"  Started:    {exec_data.get('startedAt', '-')}")
        print(f"  Completed:  {exec_data.get('completedAt', '-')}")
        print(f"  Context:    {exec_data.get('contextPercent', '-')}%")
        print(f"  Queue Pos:  {diag.get('queuePosition', -1)}")

        print(f"\n  Subscribers: {subs.get('count', 0)}")
        for c in subs.get('clients', []):
            state_label = {0: 'CONNECTING', 1: 'OPEN', 2: 'CLOSING', 3: 'CLOSED'}.get(c.get('readyState'), '?')
            print(f"    Client {c.get('clientId', '?')}: {state_label}")

        if chain.get('parentId') or chain.get('retryCount', 0) > 0:
            print(f"\n  Chain:")
            print(f"    Parent:         {chain.get('parentId', '-')}")
            print(f"    Retries:        {chain.get('retryCount', 0)}")
            print(f"    Context Retries:{chain.get('contextRetryCount', 0)}")
            for h in chain.get('history', []):
                print(f"    [{h.get('command', '?')}] {h.get('result', '?')}")

    # 2. App log events
    log_events = parse_app_log_for_exec(app_log_path, exec_id)
    if log_events:
        # Find zero-client broadcasts
        zero_broadcasts = []
        for e in log_events:
            bc_m = BROADCAST_RE.search(e['message'])
            if bc_m and int(bc_m.group(2)) == 0:
                zero_broadcasts.append(e)

        if zero_broadcasts:
            print(f"\n  Warning: {len(zero_broadcasts)} broadcast(s) reached 0 clients:")
            for zb in zero_broadcasts:
                print(f"    {zb['timestamp']} {zb['message'][:80]}")

        print(f"\n  Log events: {len(log_events)} related entries in app log")
    else:
        print(f"\n  Log events: none found (app log unavailable or no matching entries)")


def check_stale(app_log_path):
    """Find executions that completed but broadcast reached 0 clients."""
    # Get all executions
    executions = api_get('/execution')
    if not executions:
        print("No executions from API (server running?)")
        return

    # Get history too
    history = api_get('/execution/history') or []

    stale = []
    for exec_data in executions:
        if exec_data.get('status') in ('failed', 'completed', 'cancelled'):
            exec_id = exec_data.get('id')
            # Check app log for zero-client broadcasts
            log_events = parse_app_log_for_exec(app_log_path, exec_id)
            for e in log_events:
                bc_m = BROADCAST_RE.search(e['message'])
                if bc_m and int(bc_m.group(2)) == 0 and bc_m.group(1) == 'status':
                    stale.append({
                        'id': exec_id,
                        'feature': exec_data.get('featureId', '?'),
                        'command': exec_data.get('command', '?'),
                        'status': exec_data.get('status'),
                        'timestamp': e['timestamp'],
                    })
                    break

    if not stale:
        print("No stale executions found (all status broadcasts reached >=1 client)")
        return

    print(f"\nWarning: {len(stale)} stale execution(s) (status broadcast reached 0 clients):")
    print(f"  {'ID':<40}  {'Feature':<8}  {'Cmd':<6}  {'Status':<10}  {'When'}")
    print(f"  {'-' * 40}  {'-' * 8}  {'-' * 6}  {'-' * 10}  {'-' * 20}")
    for s in stale:
        print(f"  {s['id']:<40}  F{s['feature']:<7}  {s['command']:<6}  {s['status']:<10}  {s['timestamp'][:19]}")


def list_executions(after):
    """List executions from API history."""
    history = api_get('/execution/history')
    if not history:
        print("No history available")
        return

    if after:
        try:
            after_dt = datetime.fromisoformat(after)
            history = [h for h in history
                       if h.get('startedAt') and datetime.fromisoformat(h['startedAt'].replace('Z', '+00:00')) >= after_dt]
        except ValueError:
            print(f"Invalid date format: {after}", file=sys.stderr)
            sys.exit(1)

    # Current in-memory executions
    current = api_get('/execution') or []

    all_execs = []
    seen_ids = set()

    for c in current:
        all_execs.append(c)
        seen_ids.add(c.get('id'))

    for h in history:
        eid = h.get('executionId')
        if eid not in seen_ids:
            all_execs.append({
                'id': eid,
                'featureId': h.get('featureId'),
                'command': h.get('command'),
                'status': h.get('status'),
                'startedAt': h.get('startedAt'),
                'completedAt': h.get('completedAt'),
                'exitCode': h.get('exitCode'),
            })

    if not all_execs:
        print("No executions found")
        return

    print(f"\nExecutions ({len(all_execs)} total)")
    print(f"  {'ID':<12}  {'Feature':<8}  {'Cmd':<6}  {'Status':<10}  {'Exit':<5}  {'Started'}")
    print(f"  {'-' * 12}  {'-' * 8}  {'-' * 6}  {'-' * 10}  {'-' * 5}  {'-' * 20}")
    for e in all_execs:
        eid = (e.get('id') or '')[:12]
        fid = f"F{e.get('featureId', '?')}"
        cmd = e.get('command', '?')
        status = e.get('status', '?')
        exit_code = str(e.get('exitCode', '-')) if e.get('exitCode') is not None else '-'
        started = (e.get('startedAt') or '-')[:19]
        print(f"  {eid:<12}  {fid:<8}  {cmd:<6}  {status:<10}  {exit_code:<5}  {started}")


def chain_trace(exec_id):
    """Show chain parent-child relationships."""
    diag = api_get(f'/execution/{exec_id}/diag')
    if not diag:
        print(f"Execution {exec_id} not found")
        return

    chain = diag.get('chain', {})
    exec_data = diag.get('execution', {})

    print(f"\nChain Trace: {exec_id}")
    print(f"  Command:         {exec_data.get('command', '?')}")
    print(f"  Status:          {exec_data.get('status', '?')}")
    print(f"  Parent:          {chain.get('parentId') or '(root)'}")
    print(f"  Retry Count:     {chain.get('retryCount', 0)}")
    print(f"  Context Retries: {chain.get('contextRetryCount', 0)}")

    history = chain.get('history', [])
    if history:
        print(f"\n  Chain History:")
        for i, h in enumerate(history, 1):
            print(f"    {i}. [{h.get('command', '?')}] -> {h.get('result', '?')}")
    else:
        print(f"\n  No chain history (single execution or root)")


def main():
    parser = argparse.ArgumentParser(
        description='Dashboard diagnostic tool - execution diagnosis via API + logs',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""examples:
  %(prog)s --exec fec15530-...          Diagnose specific execution
  %(prog)s --check-stale                Find stale executions (0-client broadcasts)
  %(prog)s --list --after 2026-03-04    List recent executions
  %(prog)s --chain fec15530-...         Show chain parent-child trace

requires:
  Dashboard backend running on localhost:3001
  App log at ~/.pm2/logs/dashboard-backend-out.log (for log analysis)""",
    )
    parser.add_argument('--exec', metavar='ID',
                        help='Diagnose execution by ID (API + logs)')
    parser.add_argument('--check-stale', action='store_true',
                        help='Find executions where status broadcast reached 0 clients')
    parser.add_argument('--list', action='store_true',
                        help='List all executions (API + history)')
    parser.add_argument('--after', metavar='DATE',
                        help='Filter --list by start date (ISO format)')
    parser.add_argument('--chain', metavar='ID',
                        help='Show chain trace for execution')
    parser.add_argument('--app-log', metavar='PATH',
                        help='App log path override (default: ~/.pm2/logs/dashboard-backend-out.log)')

    args = parser.parse_args()

    app_log = Path(args.app_log) if args.app_log else get_default_app_log_path()

    if args.exec:
        exec_diagnosis(args.exec, app_log)
    elif args.check_stale:
        check_stale(app_log)
    elif args.list:
        list_executions(args.after)
    elif args.chain:
        chain_trace(args.chain)
    else:
        parser.print_help()


if __name__ == '__main__':
    main()
