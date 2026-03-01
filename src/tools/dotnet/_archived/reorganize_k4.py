#!/usr/bin/env python3
"""
K4 (咲夜) ファイル再編成スクリプト
Feature 057: COMカテゴリベース分割の先行実施
"""
import os
import re
from pathlib import Path

# Source directory
SRC_DIR = Path(r"c:\Era\era紅魔館NTR\Game\ERB\口上\4_咲夜")

def read_file(filename):
    """Read file content as list of lines."""
    with open(SRC_DIR / filename, 'r', encoding='utf-8-sig') as f:
        return f.readlines()

def write_file(filename, lines):
    """Write lines to file."""
    with open(SRC_DIR / filename, 'w', encoding='utf-8-sig') as f:
        f.writelines(lines)

def extract_section(lines, start_line, end_line):
    """Extract lines from start_line to end_line (1-indexed, inclusive)."""
    return lines[start_line-1:end_line]

def make_header(original_file, category):
    """Create file header."""
    return f""";-------------------------------------------------
; KOJO_K4_{category} - 咲夜口上 ({category}系)
;
; 元ファイル: {original_file}
; Feature 057: COMカテゴリベース分割
;-------------------------------------------------

"""

def find_function_ranges(lines):
    """Find line ranges for each function."""
    functions = {}
    current_func = None
    current_start = None

    for i, line in enumerate(lines, 1):
        if line.startswith('@'):
            if current_func:
                functions[current_func] = (current_start, i - 1)
            match = re.match(r'^@(\w+)', line)
            if match:
                current_func = match.group(1)
                current_start = i

    if current_func:
        functions[current_func] = (current_start, len(lines))

    return functions

def categorize_functions(func_name):
    """Categorize function by COM type."""
    # EVENT functions
    if 'EVENT_K4' in func_name:
        return 'EVENT'
    if 'COUNTER' in func_name:
        return 'EVENT'
    if 'PALAMCNG' in func_name:
        return 'EVENT'
    if 'MARKCNG' in func_name:
        return 'EVENT'
    if 'SeeYou' in func_name:
        return 'EVENT'
    if 'CALLNAME_K4' in func_name:
        return 'EVENT'
    if func_name == 'KOJO_K4':
        return 'EVENT'

    # Extract COM number
    com_match = re.search(r'COM_K4_(\d+)', func_name)
    if com_match:
        com = int(com_match.group(1))
        # 会話親密: COM 300-315, 350-352, 463
        if 300 <= com <= 315 or 350 <= com <= 352:
            return '会話親密'
        # 日常: COM 410-415, 463
        if 410 <= com <= 415 or com == 463:
            return '日常'
        # 愛撫: COM 0-9, 20-21, 40-48
        if 0 <= com <= 9 or 20 <= com <= 21 or 40 <= com <= 48:
            return '愛撫'
        # 口挿入: COM 60-71, 80-148, 180-203
        if 60 <= com <= 71 or 80 <= com <= 148 or 180 <= com <= 203:
            return '口挿入'
        # 特殊: COM 00
        if com == 0:
            return '口挿入'

    # SCOM -> 口挿入
    if 'SCOM_K4' in func_name:
        return '口挿入'

    # NTR wrapper functions by COM
    ntr_com_match = re.search(r'NTR_KOJO_MESSAGE_COM_K4_(\d+)', func_name)
    if ntr_com_match:
        com = int(ntr_com_match.group(1))
        if 300 <= com <= 315 or 350 <= com <= 352:
            return '会話親密'
        if 60 <= com <= 71:
            return '口挿入'

    # CHK_CANCEL -> 口挿入
    if 'CHK_CANCEL' in func_name:
        return '口挿入'

    # NTR_MESSAGE_COM -> 会話親密
    if 'NTR_MESSAGE_COM_K4_350' in func_name:
        return '会話親密'

    return None

def main():
    print("Reading source files...")
    kojo_k4 = read_file("KOJO_K4.ERB")
    ntr_ext = read_file("KOJO_K4_NTR拡張.ERB")
    tai_anata = read_file("対あなた口上.ERB")

    print(f"KOJO_K4.ERB: {len(kojo_k4)} lines")
    print(f"KOJO_K4_NTR拡張.ERB: {len(ntr_ext)} lines")
    print(f"対あなた口上.ERB: {len(tai_anata)} lines")

    # Find function ranges in each file
    print("\nFinding function ranges...")
    k4_funcs = find_function_ranges(kojo_k4)
    ntr_funcs = find_function_ranges(ntr_ext)
    anata_funcs = find_function_ranges(tai_anata)

    print(f"KOJO_K4.ERB: {len(k4_funcs)} functions")
    print(f"KOJO_K4_NTR拡張.ERB: {len(ntr_funcs)} functions")
    print(f"対あなた口上.ERB: {len(anata_funcs)} functions")

    # Categorize functions
    categories = {'EVENT': [], '会話親密': [], '愛撫': [], '口挿入': [], '日常': []}

    for func, (start, end) in k4_funcs.items():
        cat = categorize_functions(func)
        if cat:
            categories[cat].append(('KOJO_K4.ERB', func, start, end, kojo_k4))

    for func, (start, end) in ntr_funcs.items():
        cat = categorize_functions(func)
        if cat:
            categories[cat].append(('KOJO_K4_NTR拡張.ERB', func, start, end, ntr_ext))

    for func, (start, end) in anata_funcs.items():
        cat = categorize_functions(func)
        if cat:
            categories[cat].append(('対あなた口上.ERB', func, start, end, tai_anata))

    print("\nFunction counts by category:")
    for cat, funcs in categories.items():
        print(f"  {cat}: {len(funcs)} functions")

    # Generate output files
    print("\nGenerating output files...")

    # 会話親密.ERB
    output = [make_header("KOJO_K4.ERB + KOJO_K4_NTR拡張.ERB + 対あなた口上.ERB", "会話親密")]
    for src, func, start, end, lines in sorted(categories['会話親密'], key=lambda x: (x[0] != 'KOJO_K4.ERB', x[2])):
        output.append(f";--- from {src} ---\n")
        output.extend(lines[start-1:end])
        output.append("\n")
    write_file("KOJO_K4_会話親密.ERB", output)
    print(f"  KOJO_K4_会話親密.ERB: {len(output)} lines")

    # 愛撫.ERB
    output = [make_header("KOJO_K4.ERB", "愛撫")]
    for src, func, start, end, lines in sorted(categories['愛撫'], key=lambda x: x[2]):
        output.extend(lines[start-1:end])
        output.append("\n")
    write_file("KOJO_K4_愛撫.ERB", output)
    print(f"  KOJO_K4_愛撫.ERB: {len(output)} lines")

    # 口挿入.ERB
    output = [make_header("KOJO_K4.ERB + KOJO_K4_NTR拡張.ERB", "口挿入")]
    for src, func, start, end, lines in sorted(categories['口挿入'], key=lambda x: (x[0] != 'KOJO_K4.ERB', x[2])):
        if src != 'KOJO_K4.ERB':
            output.append(f";--- from {src} ---\n")
        output.extend(lines[start-1:end])
        output.append("\n")
    write_file("KOJO_K4_口挿入.ERB", output)
    print(f"  KOJO_K4_口挿入.ERB: {len(output)} lines")

    # 日常.ERB
    output = [make_header("KOJO_K4.ERB + 対あなた口上.ERB", "日常")]
    for src, func, start, end, lines in sorted(categories['日常'], key=lambda x: (x[0] != 'KOJO_K4.ERB', x[2])):
        if src != 'KOJO_K4.ERB':
            output.append(f";--- from {src} ---\n")
        output.extend(lines[start-1:end])
        output.append("\n")
    write_file("KOJO_K4_日常.ERB", output)
    print(f"  KOJO_K4_日常.ERB: {len(output)} lines")

    # EVENT.ERB
    output = [make_header("KOJO_K4.ERB + 対あなた口上.ERB", "EVENT")]
    for src, func, start, end, lines in sorted(categories['EVENT'], key=lambda x: (x[0] != 'KOJO_K4.ERB', x[2])):
        if src != 'KOJO_K4.ERB':
            output.append(f";--- from {src} ---\n")
        output.extend(lines[start-1:end])
        output.append("\n")
    write_file("KOJO_K4_EVENT.ERB", output)
    print(f"  KOJO_K4_EVENT.ERB: {len(output)} lines")

    print("\nDone! Files created in:", SRC_DIR)
    print("\nNext steps:")
    print("  1. Review generated files")
    print("  2. Delete original files")
    print("  3. Rename NTR口上 files")
    print("  4. Run ErbLinter")
    print("  5. Test with headless")

if __name__ == '__main__':
    main()
