#!/usr/bin/env python3
"""PM2 log diagnostic tool - finds crash/restart events in PM2 logs."""

import argparse
import re
import sys
from datetime import datetime
from pathlib import Path

NTSTATUS = {
    'C0000005': 'ACCESS_VIOLATION',
    'C00000FD': 'STACK_OVERFLOW',
    'C0000409': 'STACK_BUFFER_OVERRUN',
    'C0000374': 'HEAP_CORRUPTION',
    'C0000008': 'INVALID_HANDLE',
    'C000001D': 'ILLEGAL_INSTRUCTION',
    '40010004': 'DBG_TERMINATE_PROCESS',
}

PM2_LINE_RE = re.compile(r'^([\d\-T:.]+): PM2 (log|error): (.+)$')
APP_EVENT_RE = re.compile(r'App \[([^:]+):(\d+)\] (.+)')
EXIT_CODE_RE = re.compile(r'exited with code \[(\d+)\]')
SIGNAL_RE = re.compile(r'via signal \[(\w+)\]')

# App log patterns (dashboard-backend-out.log)
APP_LOG_RE = re.compile(r'\[(\d{4}-\d{2}-\d{2}T[\d:.+]+)\] \[(\w+)\] \[(\w+)\] (.+)')
SUBSCRIBE_RE = re.compile(r'Client (\d+) subscribed to (.+)')
UNSUBSCRIBE_RE = re.compile(r'Client (\d+) unsubscribed from (.+)')
DISCONNECT_RE = re.compile(r'Client (\d+) disconnected \(code: (\d+)\)')
CONNECT_RE = re.compile(r'Client (\d+) connected from (.+)')
BROADCAST_RE = re.compile(r'Broadcast \[(\w+)\] to (\d+) clients for execution (.+)')
SPAWN_RE = re.compile(r'spawn args: .+ -p /(\w+) (\d+)')
EXIT_RE = re.compile(r'Execution (.+) completed with code (\d+)')


def get_default_app_log_path():
    """Get default dashboard-backend app log path."""
    home = Path.home()
    return home / '.pm2' / 'logs' / 'dashboard-backend-out.log'


def parse_app_log(log_path, exec_id):
    """Parse app log for events related to a specific execution ID."""
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

                timestamp_str = m.group(1)
                level = m.group(2)
                category = m.group(3)
                message = m.group(4)

                event = {
                    'timestamp_str': timestamp_str,
                    'level': level,
                    'category': category,
                    'message': message,
                    'type': 'unknown',
                    'subscribers': None,
                }

                # Classify event
                sub_m = SUBSCRIBE_RE.search(message)
                if sub_m:
                    event['type'] = 'subscribe'
                    event['client_id'] = int(sub_m.group(1))

                unsub_m = UNSUBSCRIBE_RE.search(message)
                if unsub_m:
                    event['type'] = 'unsubscribe'
                    event['client_id'] = int(unsub_m.group(1))

                disc_m = DISCONNECT_RE.search(message)
                if disc_m:
                    event['type'] = 'disconnect'
                    event['client_id'] = int(disc_m.group(1))

                bc_m = BROADCAST_RE.search(message)
                if bc_m:
                    event['type'] = 'broadcast'
                    event['broadcast_type'] = bc_m.group(1)
                    event['broadcast_clients'] = int(bc_m.group(2))

                exit_m = EXIT_RE.search(message)
                if exit_m:
                    event['type'] = 'exit'
                    event['exit_code'] = int(exit_m.group(2))

                events.append(event)
    except FileNotFoundError:
        print(f"Error: App log not found at {log_path}", file=sys.stderr)
        print("Note: --exec-trace reads dashboard-backend app log (requires debug-level logging)", file=sys.stderr)
        sys.exit(1)
    except PermissionError:
        print(f"Error: Permission denied reading {log_path}", file=sys.stderr)
        sys.exit(1)
    return events


def track_subscribers(events):
    """Track subscriber count through event timeline."""
    subscribers = set()  # client_ids currently subscribed
    for event in events:
        if event['type'] == 'subscribe':
            subscribers.add(event.get('client_id'))
        elif event['type'] in ('unsubscribe', 'disconnect'):
            subscribers.discard(event.get('client_id'))
        event['subscribers'] = len(subscribers)
    return events


def format_exec_trace(events, exec_id):
    """Format execution trace as timeline."""
    if not events:
        print(f"No events found for execution {exec_id}")
        return

    print(f"\nExecution Trace: {exec_id}")
    print(f"  {'#':>3}  {'Timestamp':<26}  {'Subs':>4}  {'Type':<12}  {'Detail'}")
    print(f"  {'---':>3}  {'-' * 26:<26}  {'----':>4}  {'-' * 12:<12}  {'------'}")

    for i, e in enumerate(events, 1):
        ts = e['timestamp_str'][:26]
        subs = str(e['subscribers']) if e['subscribers'] is not None else '?'
        etype = e['type']
        detail = e['message']

        # Add warning markers
        warning = ''
        if e['type'] == 'broadcast' and e.get('broadcast_clients', 1) == 0:
            warning = ' ⚠️  0 clients received!'
        if e['type'] == 'exit':
            code = e.get('exit_code', '?')
            warning = f' (exit code: {code})'

        # Truncate detail
        if len(detail) > 60:
            detail = detail[:57] + '...'

        print(f"  {i:>3}  {ts:<26}  {subs:>4}  {etype:<12}  {detail}{warning}")

    print(f"\n  Total events: {len(events)}")

    # Summary warnings
    zero_broadcasts = [e for e in events if e['type'] == 'broadcast' and e.get('broadcast_clients', 0) == 0]
    if zero_broadcasts:
        print(f"  ⚠️  {len(zero_broadcasts)} broadcast(s) reached 0 clients")


def decode_exit_code(code):
    if code == 0:
        return 'OK'
    if code == 1:
        return 'SIGINT/normal-exit'
    hex_str = format(code & 0xFFFFFFFF, 'X')
    return NTSTATUS.get(hex_str, f'0x{hex_str}')


def is_crash(code):
    """Return True if exit code indicates abnormal termination."""
    return code is not None and code not in (0, 1)


def parse_pm2_log(log_path):
    """Parse PM2 log file into structured events."""
    events = []
    try:
        with open(log_path, 'r', encoding='utf-8', errors='replace') as f:
            for line in f:
                line = line.rstrip('\n')
                m = PM2_LINE_RE.match(line)
                if not m:
                    continue
                timestamp_str, level, message = m.group(1), m.group(2), m.group(3)

                try:
                    timestamp = datetime.fromisoformat(timestamp_str)
                except ValueError:
                    timestamp = None

                event = {
                    'timestamp': timestamp,
                    'timestamp_str': timestamp_str,
                    'level': level,
                    'message': message,
                    'app': None,
                    'instance': None,
                    'action': None,
                    'exit_code': None,
                    'signal': None,
                }

                app_m = APP_EVENT_RE.match(message)
                if app_m:
                    event['app'] = app_m.group(1)
                    event['instance'] = int(app_m.group(2))
                    event['action'] = app_m.group(3)

                    exit_m = EXIT_CODE_RE.search(event['action'])
                    if exit_m:
                        event['exit_code'] = int(exit_m.group(1))

                    sig_m = SIGNAL_RE.search(event['action'])
                    if sig_m:
                        event['signal'] = sig_m.group(1)

                events.append(event)
    except FileNotFoundError:
        print(f"Error: PM2 log not found at {log_path}", file=sys.stderr)
        sys.exit(1)
    except PermissionError:
        print(f"Error: Permission denied reading {log_path}", file=sys.stderr)
        sys.exit(1)
    return events


def filter_events(events, args):
    """Apply CLI filters to events."""
    filtered = events

    if args.after:
        try:
            after_dt = datetime.fromisoformat(args.after)
            if after_dt.hour == 0 and after_dt.minute == 0:
                # Date-only: treat as start of day
                pass
            filtered = [e for e in filtered if e['timestamp'] and e['timestamp'] >= after_dt]
        except ValueError:
            print(f"Error: Invalid date format: {args.after}", file=sys.stderr)
            sys.exit(1)

    if args.app:
        filtered = [e for e in filtered if e['app'] and args.app in e['app']]

    if args.type == 'crash':
        filtered = [e for e in filtered if e['exit_code'] is not None and is_crash(e['exit_code'])]
    elif args.type == 'restart':
        filtered = [e for e in filtered if e['action'] and (
            'starting in' in e['action'] or 'online' in e['action'] or
            'will restart' in e['action']
        )]
    elif args.type == 'error':
        filtered = [e for e in filtered if e['level'] == 'error']

    if args.pattern:
        pat = args.pattern.lower()
        filtered = [e for e in filtered if pat in e['message'].lower() or
                    (e['exit_code'] is not None and pat in decode_exit_code(e['exit_code']).lower())]

    return filtered


def format_table(events, limit):
    """Format events as a compact table."""
    if not events:
        print("No matching events found.")
        return

    # Take last N events, show newest first
    shown = events[-limit:] if len(events) > limit else events
    shown = list(reversed(shown))

    # Group by app for header
    apps = set(e['app'] for e in shown if e['app'])
    app_label = ', '.join(sorted(apps)) if apps else 'all'
    print(f"\nPM2 Event History ({app_label})")

    # Determine columns based on event types
    has_exit = any(e['exit_code'] is not None for e in shown)

    if has_exit:
        print(f"  {'#':>3}  {'Timestamp':<22}  {'Exit Code':<12}  {'Signal':<8}  {'Decoded'}")
        print(f"  {'---':>3}  {'----------------------':<22}  {'------------':<12}  {'--------':<8}  {'-------'}")
        for i, e in enumerate(shown, 1):
            ts = e['timestamp_str'][:22] if e['timestamp_str'] else '?'
            code = str(e['exit_code']) if e['exit_code'] is not None else '-'
            signal = e['signal'] or '-'
            decoded = decode_exit_code(e['exit_code']) if e['exit_code'] is not None else '-'
            crash_mark = ' ★' if e['exit_code'] is not None and is_crash(e['exit_code']) else ''
            print(f"  {i:>3}  {ts:<22}  {code:<12}  {signal:<8}  {decoded}{crash_mark}")
        print(f"\n★ = abnormal exit (NTSTATUS)")
    else:
        print(f"  {'#':>3}  {'Timestamp':<22}  {'App':<25}  {'Event'}")
        print(f"  {'---':>3}  {'----------------------':<22}  {'-------------------------':<25}  {'-----'}")
        for i, e in enumerate(shown, 1):
            ts = e['timestamp_str'][:22] if e['timestamp_str'] else '?'
            app = f"{e['app']}:{e['instance']}" if e['app'] else '-'
            action = e['action'] or e['message']
            # Truncate long actions
            if len(action) > 60:
                action = action[:57] + '...'
            print(f"  {i:>3}  {ts:<22}  {app:<25}  {action}")

    print(f"\n  Showing {len(shown)} of {len(events)} matching events")


def get_default_log_path():
    """Get default PM2 log path."""
    home = Path.home()
    return home / '.pm2' / 'pm2.log'


def main():
    parser = argparse.ArgumentParser(
        description='PM2 log diagnostic tool - finds crash/restart events',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""examples:
  %(prog)s                              Recent restart/crash events (last 20)
  %(prog)s --after 2026-03-04           Events after date
  %(prog)s --type crash                 Abnormal exits only (code != 0,1)
  %(prog)s --type restart               Restart events only
  %(prog)s --type error                 PM2 error logs only
  %(prog)s --app dashboard-backend      Filter by app name
  %(prog)s "ACCESS_VIOLATION"           Search in messages and decoded names
  %(prog)s --limit 50                   Show more events
  %(prog)s --exec-trace fec15530        Trace execution lifecycle (app log)""",
    )
    parser.add_argument('pattern', nargs='?', default=None,
                        help='Search pattern (matches message text and decoded exit names)')
    parser.add_argument('--after', metavar='DATE',
                        help='Show events after DATE (ISO format, e.g. 2026-03-04)')
    parser.add_argument('--app', metavar='NAME',
                        help='Filter by app name (substring match)')
    parser.add_argument('--type', choices=['crash', 'restart', 'error'],
                        help='Filter by event type')
    parser.add_argument('--limit', type=int, default=20,
                        help='Max events to show (default: 20)')
    parser.add_argument('--log-path', metavar='PATH',
                        help='PM2 log file path (default: ~/.pm2/pm2.log)')
    parser.add_argument('--exec-trace', metavar='ID',
                        help='Trace execution lifecycle by ID (reads app log, requires debug-level logging)')
    parser.add_argument('--app-log', metavar='PATH',
                        help='App log path override (default: ~/.pm2/logs/dashboard-backend-out.log)')

    args = parser.parse_args()

    # --exec-trace mode: read app log instead of PM2 system log
    if args.exec_trace:
        app_log = Path(args.app_log) if args.app_log else get_default_app_log_path()
        events = parse_app_log(app_log, args.exec_trace)
        events = track_subscribers(events)
        format_exec_trace(events, args.exec_trace)
        sys.exit(0)

    log_path = Path(args.log_path) if args.log_path else get_default_log_path()
    events = parse_pm2_log(log_path)

    # Default: show exit events (most useful for diagnostics)
    if not args.type and not args.pattern:
        events = [e for e in events if e['exit_code'] is not None or e['level'] == 'error']

    filtered = filter_events(events, args)
    format_table(filtered, args.limit)


if __name__ == '__main__':
    main()
