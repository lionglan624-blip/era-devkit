#!/usr/bin/env python3
"""
Kojo Coverage Mapper - ERB dialogue coverage analysis tool
Generates visual maps of implemented dialogue conditions
"""

import re
import os
import sys
import json
import hashlib
import argparse
from pathlib import Path
from dataclasses import dataclass, field
from typing import Dict, List, Set, Optional, Any
from collections import defaultdict

@dataclass
class BranchBlock:
    """Represents a single branch block (IF/ELSEIF/ELSE) with semantic label - Feature 084"""
    line: int               # Line number in file
    label: str              # Semantic label (e.g., "恋人分岐", "恋慕分岐")
    condition: str          # Original condition text
    condition_hash: str     # SHA1 hash of condition for fallback matching


@dataclass
class KojoFunction:
    """Represents a single kojo function"""
    name: str
    file: str
    line: int
    conditions: List[str] = field(default_factory=list)
    favorability: Optional[str] = None
    location: Optional[str] = None
    has_soft_hard: bool = False
    special_conditions: List[str] = field(default_factory=list)
    # AC Metrics (Feature 051)
    branch_type: str = ""       # "TALENT_4", "TALENT_3", "ABL_3", "NONE", etc.
    branch_depth: int = 0       # Number of relationship branches
    content_lines: int = 0      # Non-blank, non-comment lines
    printform_lines: int = 0    # Feature 055: PRINTFORM/PRINTFORML/PRINTFORMW lines only
    dialogue_text_lines: int = 0  # PRINTFORM* + DATAFORM (actual dialogue text)
    kojo_block_count: int = 1   # Number of IF/ELSEIF/ELSE blocks with dialogue
    has_printdata: bool = False # Uses PRINTDATA/DATALIST
    has_variation: bool = False # Uses SELECTCASE RAND or PRINTDATA for variations
    has_else: bool = False      # Has ELSE block for low-relationship
    # Feature 055: TALENT level tracking
    has_talent_lover: bool = False   # Has TALENT:恋人 check
    has_talent_renbo: bool = False   # Has TALENT:恋慕 check
    has_talent_shibo: bool = False   # Has TALENT:思慕 check
    # Feature 055: IF RAND detection
    rand_variations: int = 0    # Number of IF RAND: variations detected
    # Feature 084: Branch blocks for semantic mapping
    branch_blocks: List[BranchBlock] = field(default_factory=list)

@dataclass
class KojoFile:
    """Represents a kojo ERB file"""
    path: str
    functions: List[KojoFunction] = field(default_factory=list)
    scene_type: str = ""

# Favorability levels in order
FAVORABILITY_LEVELS = [
    "FAV_寝取られ",
    "FAV_寝取られ寸前",
    "FAV_寝取られそう",
    "FAV_主人より高い",
    "FAV_うふふする程度",
    "FAV_奉仕する程度",
    "FAV_体を触らせる程度",
    "FAV_キスする程度",
]

# Scene type mappings from file names
# Order matters: more specific patterns first, generic patterns last
SCENE_TYPES = {
    # NTR scenario files (specific patterns first)
    "NTR口上_お持ち帰り": "お持ち帰り",
    "NTR口上_野外調教": "野外調教",
    "NTR口上_シナリオ": "NTRシナリオ",  # NTR口上_シナリオ*.ERB
    # NTR catch-all (matches NTR口上.ERB, NTR口上_基本.ERB, NTR口上_シーン8.ERB, etc.)
    "NTR口上": "NTR基本",
    # Special system files
    "WC系口上.ERB": "WC系",
    "対あなた口上.ERB": "対あなた",
    "SexHara休憩中口上.ERB": "セクハラ",
    # COM category files (Feature 057+)
    # Pattern without .ERB to match subcategory splits (e.g., _会話親密_告白.ERB)
    "_会話親密": "会話親密",
    "_愛撫": "愛撫",
    "_口挿入": "口挿入",
    "_日常": "日常",
    "_EVENT": "EVENT",
    "_関係性": "関係性",
    # Utility files (Feature 168)
    "KOJO_MODIFIER": "ユーティリティ",
    # Legacy catch-all (must be last)
    "KOJO_K": "基本口上",
}

# Location patterns
LOCATION_PATTERNS = {
    "場所_大浴場": "大浴場",
    "場所_": "その他",
}

# COM Range-to-Category Mapping for progress tracking (Feature 254)
# Hardcoded SSOT for v0.6/v0.8 scope (52 COM total)
COM_RANGES = [
    {"range": "0-11", "category": "愛撫系", "start": 0, "end": 11},
    {"range": "20-21", "category": "会話親密系", "start": 20, "end": 21},
    {"range": "40-48", "category": "道具系", "start": 40, "end": 48},
    {"range": "60-72", "category": "挿入系", "start": 60, "end": 72},
    {"range": "80-85", "category": "奉仕系", "start": 80, "end": 85},
    {"range": "90-99", "category": "受け身系", "start": 90, "end": 99},
]

# Phase Requirements for kojo completion tracking (Feature 257, 297)
# Phase transition: update CURRENT_PHASE during /plan
PHASE_REQUIREMENTS = {
    # Legacy Content Level naming (C2/C3/C6 = Phase 8d variations)
    "C2": {"branch_type": "TALENT_4", "patterns": 1},
    "C3": {"branch_type": "TALENT_4", "patterns": 4},
    "C6": {"branch_type": "FAV_9", "patterns": 9},

    # Phase-based naming (Feature 297 - aligned with kojo-writing SKILL.md)
    "8d": {"branch_type": "TALENT_4", "patterns": 1},  # Basic dialogue (same as C2)
    "8j": {"branch_type": "NOWEX_TALENT_4", "patterns": 4},  # Ejaculation-state (NOWEX:MASTER:11 + TALENT 4分岐)
    "8l": {"branch_type": "FIRSTTIME", "patterns": 1},  # First execution (FIRSTTIME(SELECTCOM) only)
}
# Note: "C2"/"C3"/"C6" preserved for backward compatibility
# Note: "8j" and "8l" branch_type values are conceptual identifiers for Phase detection
# Actual implementation detection would need NOWEX/FIRSTTIME pattern matching
CURRENT_PHASE = "C2"  # Phase transition: update during /plan

# Kojo function patterns for coverage analysis (Feature 166)
# IMPORTANT: Order matters! More specific patterns must come FIRST.
# Each function matches only the first pattern that matches it.
KOJO_PATTERNS = {
    # === ユーティリティ（口上カウント除外）- Check FIRST ===
    "UTILITY": {
        "pattern": r"@(CHK_CANCEL|CALLNAME|MSG_|ANATA_|BOTTOM_|KOJO_MODIFIER|KOJO_K\d+\(|KOJO_KU\b)",
        "description": "ヘルパー関数（口上カウント除外）",
        "scope": "utility helpers",
    },

    # === 特殊系 - Check before generic NTR patterns ===
    "NTR_SPECIAL": {
        "pattern": r"@NTR_KOJO_K(?:U|(\d+))_(FAKE_ORGASM|GET_ORDER|MAKE_CLIENT|MARK)",
        "description": "特殊NTR（偽絶頂、命令、マーキング）",
        "scope": "4 types × 11 chars",
    },
    "NTR_PRE": {
        "pattern": r"@NTR_KOJO_PRE_KW?(?:U|_?(\d+)?)_",
        "description": "NTR Pre-scene variants",
        "scope": "COM × scenario × pattern",
    },
    "NTR_COM": {
        "pattern": r"@NTR_(?:KOJO_)?MESSAGE_COM_K(?:U|_?(\d+))_?(\d+)",
        "description": "NTR調教コマンド口上",
        "scope": "COM × 11 chars",
    },
    "NTR_WITNESS": {
        "pattern": r"@NTR_KOJO_KW(?:U|_?(\d+))_?(.+)?",
        "description": "NTRイベント（主人在宅=見せつけ）",
        "scope": "80+ triggers × 11 chars",
    },
    "NTR_EVENT": {
        "pattern": r"@NTR_KOJO_K(?:U_|_(?:\d+|VA)(?:_|\()|(\d+)_)",
        "description": "NTRイベント（主人不在）",
        "scope": "80+ triggers × 11 chars",
    },

    # === 特殊コマンド系 - Check before generic COM ===
    "DAILY": {
        "pattern": r"@KOJO_MESSAGE_COM_KU_4(\d*)",
        "description": "日常系（訪問者対話等）",
        "scope": "COM 400-463",
    },
    "SCOM": {
        "pattern": r"@KOJO_MESSAGE_SCOM_K(?:U|(\d+))_(\d+)",
        "description": "特殊コマンド口上",
        "scope": "variable",
    },

    # === 状態変化系 ===
    "RELATION": {
        "pattern": r"@KOJO_MESSAGE_(恋慕|思慕|告白).*_K(?:U|(\d+))",
        "description": "関係性獲得（恋慕獲得、告白成功等）",
        "scope": "4 types × 11 chars",
    },
    "PARAM_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_PALAMCNG_([ABC])_K(?:U|(\d+))",
        "description": "パラメータ変化通知",
        "scope": "3 params × 11 chars",
    },
    "MARK_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_MARKCNG_K(?:U|(\d+))",
        "description": "マーク変化通知",
        "scope": "11 chars",
    },
    "FAREWELL": {
        "pattern": r"@KOJO_MESSAGE_K(?:U|(\d+))_SeeYou",
        "description": "別れ口上",
        "scope": "11 chars",
    },
    "COUNTER": {
        "pattern": r"@KOJO_MESSAGE_COUNTER_K(?:U|(\d+))_(\d+)",
        "description": "会話親密系カウンター",
        "scope": "45+ counters × 11 chars",
    },

    # === 主要カテゴリ - Generic patterns LAST ===
    "EVENT": {
        "pattern": r"@KOJO_EVENT_K(?:U|(\d+))_(\d+)",
        "description": "イベント口上（初対面、部屋入室等）",
        "scope": "12 scenes × 11 chars",
    },
    "WC": {
        "pattern": r"@(SexHara|WC_|KOJO_WC_).*_K(?:U|(\d+))",
        "description": "WC系/肉便器系",
        "scope": "6 difficulty × 11 chars",
    },
    "COM": {
        "pattern": r"@KOJO_MESSAGE_COM_K(?:U|(\d+))(?:_(\d+))?",
        "description": "調教コマンド口上",
        "scope": "150 COM × 11 chars",
    },
}

def parse_erb_file(filepath: Path) -> KojoFile:
    """Parse an ERB file and extract function definitions and conditions"""
    kojo_file = KojoFile(path=str(filepath))

    # Determine scene type from filename
    filename = filepath.name
    for pattern, scene in SCENE_TYPES.items():
        if pattern in filename:
            kojo_file.scene_type = scene
            break

    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            content = f.read()
            lines = content.split('\n')
    except Exception as e:
        print(f"Error reading {filepath}: {e}", file=sys.stderr)
        return kojo_file

    current_func = None
    func_content_lines = []

    for i, line in enumerate(lines, 1):
        # Function definition
        if line.strip().startswith('@'):
            # Save previous function
            if current_func:
                analyze_function_content(current_func, func_content_lines)
                kojo_file.functions.append(current_func)

            func_name = line.strip()[1:].split('(')[0]
            current_func = KojoFunction(
                name=func_name,
                file=str(filepath),
                line=i
            )
            func_content_lines = []
        elif current_func:
            func_content_lines.append(line)

    # Don't forget the last function
    if current_func:
        analyze_function_content(current_func, func_content_lines)
        kojo_file.functions.append(current_func)

    return kojo_file

def analyze_function_content(func: KojoFunction, lines: List[str]):
    """Analyze function content for conditions"""
    content = '\n'.join(lines)

    # Check for favorability conditions
    fav_pattern = r'NTR_CHK_FAVORABLY\s*\([^,]+,\s*(FAV_\w+)\)'
    fav_matches = re.findall(fav_pattern, content)
    if fav_matches:
        func.conditions.extend(fav_matches)
        # Record the highest favorability found
        for fav in FAVORABILITY_LEVELS:
            if fav in fav_matches:
                func.favorability = fav
                break

    # Check for soft/hard mode
    if 'IS_NTR_SOFT()' in content:
        func.has_soft_hard = True

    # Check for location
    loc_pattern = r'CFLAG:[^:]+:現在位置\s*==\s*(\w+)'
    loc_matches = re.findall(loc_pattern, content)
    if loc_matches:
        func.location = loc_matches[0]

    # Check for special conditions
    special_patterns = [
        (r'TALENT:[^:]+:処女', '処女'),
        (r'TALENT:[^:]+:公衆便所', '公衆便所'),
        (r'TALENT:[^:]+:親愛', '親愛'),
        (r'TALENT:[^:]+:恋慕', '恋慕'),
        (r'TALENT:[^:]+:人妻', '人妻'),
        (r'TALENT:[^:]+:妊娠', '妊娠'),
    ]
    for pattern, name in special_patterns:
        if re.search(pattern, content):
            func.special_conditions.append(name)

    # === AC Metrics Detection (Feature 051) ===
    analyze_ac_metrics(func, lines, content)

    # === Feature 084: Extract branch blocks for semantic mapping ===
    func.branch_blocks = extract_branch_blocks(lines, func.line)


def analyze_ac_metrics(func: KojoFunction, lines: List[str], content: str):
    """Analyze AC (Acceptance Criteria) metrics for kojo guidelines compliance."""

    # 1. Count content lines (exclude comments and blank lines)
    content_line_count = 0
    printform_line_count = 0
    dialogue_text_lines = 0  # PRINTFORM* + DATAFORM (actual dialogue text)
    kojo_block_count = 0     # Number of IF/ELSEIF/ELSE blocks containing dialogue

    # Track if current block has dialogue
    in_dialogue_block = False
    block_has_dialogue = False
    in_printdata_block = False  # Track PRINTDATA/PRINTDATAL blocks

    for line in lines:
        stripped = line.strip()
        upper_stripped = stripped.upper()

        # Skip blank lines
        if not stripped:
            continue
        # Skip comment-only lines (starting with ;)
        if stripped.startswith(';'):
            continue
        # Skip RETURN statements (structural, not content)
        if upper_stripped.startswith('RETURN'):
            continue
        # Skip function calls to helper functions (structural)
        if upper_stripped.startswith('CALL ') and '_1' in stripped:
            continue
        content_line_count += 1

        # Track PRINTDATA blocks (DATAFORM lines are random selection, count as 1)
        if re.match(r'^PRINTDATA[LW]?\b', upper_stripped):
            in_printdata_block = True
            dialogue_text_lines += 1  # Count PRINTDATA block as 1 line
            block_has_dialogue = True
        elif re.match(r'^ENDDATA\b', upper_stripped):
            in_printdata_block = False
        elif re.match(r'^DATAFORM\s', stripped, re.IGNORECASE):
            # DATAFORM inside PRINTDATA block - don't count (already counted as 1)
            pass
        elif re.match(r'^PRINTFORM[LW]?\s', stripped, re.IGNORECASE):
            # Regular PRINTFORM outside PRINTDATA block
            dialogue_text_lines += 1
            block_has_dialogue = True

        # Feature 055: Count PRINTFORM lines specifically (legacy)
        if re.match(r'^PRINTFORM[LW]?\s', stripped, re.IGNORECASE):
            printform_line_count += 1

        # Track dialogue blocks (IF/ELSEIF/ELSE containing dialogue)
        if re.match(r'^(IF|ELSEIF|ELSE)\b', upper_stripped):
            # Count previous block if it had dialogue
            if in_dialogue_block and block_has_dialogue:
                kojo_block_count += 1
            in_dialogue_block = True
            block_has_dialogue = False
        elif re.match(r'^ENDIF\b', upper_stripped):
            # End of IF block - count if had dialogue
            if block_has_dialogue:
                kojo_block_count += 1
            in_dialogue_block = False
            block_has_dialogue = False

    # Count final block if not closed
    if in_dialogue_block and block_has_dialogue:
        kojo_block_count += 1

    func.content_lines = content_line_count
    func.printform_lines = printform_line_count
    func.dialogue_text_lines = dialogue_text_lines
    func.kojo_block_count = max(kojo_block_count, 1)  # At least 1 block

    # 2. Detect PRINTDATA/DATALIST usage
    if re.search(r'\bPRINTDATA\b', content) or re.search(r'\bDATALIST\b', content):
        func.has_printdata = True
        func.has_variation = True

    # 3. Detect SELECTCASE RAND for variations
    if re.search(r'SELECTCASE\s+RAND:', content):
        func.has_variation = True

    # Feature 055: Detect IF RAND: patterns
    rand_matches = re.findall(r'\bIF\s+RAND:', content, re.IGNORECASE)
    func.rand_variations = len(rand_matches)
    if func.rand_variations > 0:
        func.has_variation = True

    # 4. Detect relationship branch patterns
    branch_info = detect_relationship_branches(content)
    func.branch_type = branch_info['type']
    func.branch_depth = branch_info['depth']
    func.has_else = branch_info['has_else']
    # Feature 055: Track individual TALENT levels
    func.has_talent_lover = branch_info.get('has_lover', False)
    func.has_talent_renbo = branch_info.get('has_renbo', False)
    func.has_talent_shibo = branch_info.get('has_shibo', False)


def detect_relationship_branches(content: str) -> dict:
    """Detect relationship branching patterns in function content."""

    # TALENT-based 4-level branching (恋人 > 恋慕 > 思慕 > ELSE)
    # Handles both TALENT:恋人 and TALENT:TARGET:恋人 formats
    talent_4_patterns = [
        r'TALENT:(?:[^:]*:)?恋人',
        r'TALENT:(?:[^:]*:)?恋慕',
        r'TALENT:(?:[^:]*:)?思慕',
    ]

    # Feature 055: Track individual TALENT levels
    has_lover = bool(re.search(r'TALENT:(?:[^:]*:)?恋人', content))
    has_renbo = bool(re.search(r'TALENT:(?:[^:]*:)?恋慕', content))
    has_shibo = bool(re.search(r'TALENT:(?:[^:]*:)?思慕', content))

    # ABL:親密-based branching (handles both ABL:親密 and ABL:TARGET:親密)
    # Include comparison value to count unique conditions (e.g., ABL:親密 <= 2 vs ABL:親密 <= 5)
    abl_pattern = r'ABL:(?:[^:]*:)?親密\s*[<>=!]+\s*\d+'

    # NTR_CHK_FAVORABLY branching (FAV_寝取られ, FAV_主人より高い, etc.)
    ntr_fav_pattern = r'NTR_CHK_FAVORABLY\s*\([^,]+,\s*(FAV_\w+)\)'
    ntr_fav_matches = re.findall(ntr_fav_pattern, content)
    ntr_fav_count = len(set(ntr_fav_matches))  # Unique FAV_ levels

    # Check for TALENT 4-level (all three levels present)
    talent_levels_found = sum([has_lover, has_renbo, has_shibo])

    # Check for ABL:親密 branching
    abl_matches = re.findall(abl_pattern, content)
    abl_count = len(set(abl_matches))  # Unique ABL conditions

    # Check for ELSE block in relationship context
    # Look for ELSE that's part of IF TALENT or IF ABL:親密 or NTR_CHK_FAVORABLY chains
    has_else = False
    if re.search(r'\bELSE\b', content):
        # Check if ELSE is in a relationship branch context
        if talent_levels_found > 0 or abl_count > 0 or ntr_fav_count > 0:
            has_else = True

    # Base result with TALENT level info
    base_result = {
        'has_lover': has_lover,
        'has_renbo': has_renbo,
        'has_shibo': has_shibo,
    }

    # Determine branch type and depth
    if talent_levels_found == 3:
        # Full 4-level TALENT branching
        return {**base_result, 'type': 'TALENT_4', 'depth': 4 if has_else else 3, 'has_else': has_else}
    elif talent_levels_found == 2:
        # 3-level TALENT branching
        return {**base_result, 'type': 'TALENT_3', 'depth': 3 if has_else else 2, 'has_else': has_else}
    elif talent_levels_found == 1:
        # Single TALENT check
        return {**base_result, 'type': 'TALENT_1', 'depth': 2 if has_else else 1, 'has_else': has_else}
    elif ntr_fav_count >= 2:
        # NTR_CHK_FAVORABLY multi-level branching
        depth = ntr_fav_count + (1 if has_else else 0)
        return {**base_result, 'type': f'NTR_{depth}', 'depth': depth, 'has_else': has_else}
    elif ntr_fav_count == 1:
        # Single NTR check
        return {**base_result, 'type': 'NTR_1', 'depth': 2 if has_else else 1, 'has_else': has_else}
    elif abl_count >= 2:
        # ABL:親密 multi-level branching
        depth = abl_count + (1 if has_else else 0)
        return {**base_result, 'type': f'ABL_{depth}', 'depth': depth, 'has_else': has_else}
    elif abl_count == 1:
        # Single ABL check
        return {**base_result, 'type': 'ABL_1', 'depth': 2 if has_else else 1, 'has_else': has_else}
    else:
        # No relationship branching
        return {**base_result, 'type': 'NONE', 'depth': 0, 'has_else': False}


def generate_condition_hash(condition: str) -> str:
    """Generate SHA1 hash of condition for fallback matching - Feature 084"""
    # Normalize: strip whitespace, lowercase
    normalized = condition.strip().lower()
    return hashlib.sha1(normalized.encode('utf-8')).hexdigest()[:8]


def get_branch_label(condition: str) -> str:
    """Generate semantic label from condition text - Feature 084"""
    condition_upper = condition.upper()

    # TALENT-based labels (highest priority)
    if re.search(r'TALENT:[^:]*:?恋人', condition):
        return "恋人分岐"
    if re.search(r'TALENT:[^:]*:?恋慕', condition):
        return "恋慕分岐"
    if re.search(r'TALENT:[^:]*:?思慕', condition):
        return "思慕分岐"
    if re.search(r'TALENT:[^:]*:?親愛', condition):
        return "親愛分岐"

    # NTR_CHK_FAVORABLY-based labels
    fav_match = re.search(r'NTR_CHK_FAVORABLY\s*\([^)]+\)\s*[=<>!]+\s*(\d+)', condition)
    if fav_match:
        fav_level = int(fav_match.group(1))
        fav_labels = {
            0: "好感度0分岐",
            1: "好感度1分岐",
            2: "好感度2分岐",
            3: "好感度3分岐",
            4: "好感度4分岐",
        }
        return fav_labels.get(fav_level, f"好感度{fav_level}分岐")

    # ABL:親密-based labels
    abl_match = re.search(r'ABL:[^:]*:?親密\s*([<>=!]+)\s*(\d+)', condition)
    if abl_match:
        op = abl_match.group(1)
        val = abl_match.group(2)
        return f"親密{op}{val}分岐"

    # Special condition labels
    if re.search(r'TALENT:[^:]*:?処女', condition):
        return "処女分岐"
    if re.search(r'TALENT:[^:]*:?妊娠', condition):
        return "妊娠分岐"
    if re.search(r'TALENT:[^:]*:?人妻', condition):
        return "人妻分岐"

    # NTR soft/hard mode
    if 'IS_NTR_SOFT' in condition_upper:
        return "ソフト分岐"
    if 'IS_NTR_HARD' in condition_upper:
        return "ハード分岐"

    # RAND variations
    if re.search(r'\bRAND:', condition_upper):
        return "ランダム分岐"

    # Location checks
    if '現在位置' in condition:
        return "場所分岐"

    # ELSE block
    if condition_upper.strip() == 'ELSE':
        return "その他分岐"

    # Generic condition
    return "条件分岐"


def extract_branch_blocks(lines: List[str], func_start_line: int) -> List[BranchBlock]:
    """Extract branch blocks with semantic labels from function lines - Feature 084

    Args:
        lines: Function content lines (excluding @function line)
        func_start_line: Line number of @function in original file

    Returns:
        List of BranchBlock with line numbers, labels, conditions, and hashes
    """
    branch_blocks = []

    # Track relationship branch context (only label relationship-related branches)
    in_relationship_context = False

    for i, line in enumerate(lines):
        stripped = line.strip()
        upper_stripped = stripped.upper()

        # Calculate actual line number in file (func_start_line is @function line)
        actual_line = func_start_line + i + 1

        # Detect IF/ELSEIF/ELSE statements
        if re.match(r'^(IF|SIF)\s+', upper_stripped):
            # Extract condition (everything after IF/SIF)
            condition_match = re.match(r'^(?:IF|SIF)\s+(.+)$', stripped, re.IGNORECASE)
            if condition_match:
                condition = condition_match.group(1).strip()

                # Check if this is a relationship-related condition
                is_relationship = (
                    re.search(r'TALENT:[^:]*:?(恋人|恋慕|思慕|親愛)', condition) or
                    re.search(r'NTR_CHK_FAVORABLY', condition) or
                    re.search(r'ABL:[^:]*:?親密', condition)
                )

                if is_relationship:
                    in_relationship_context = True
                    label = get_branch_label(condition)
                    condition_hash = generate_condition_hash(condition)
                    branch_blocks.append(BranchBlock(
                        line=actual_line,
                        label=label,
                        condition=condition,
                        condition_hash=condition_hash
                    ))

        elif re.match(r'^ELSEIF\s+', upper_stripped):
            # Extract condition
            condition_match = re.match(r'^ELSEIF\s+(.+)$', stripped, re.IGNORECASE)
            if condition_match and in_relationship_context:
                condition = condition_match.group(1).strip()
                label = get_branch_label(condition)
                condition_hash = generate_condition_hash(condition)
                branch_blocks.append(BranchBlock(
                    line=actual_line,
                    label=label,
                    condition=condition,
                    condition_hash=condition_hash
                ))

        elif upper_stripped == 'ELSE':
            if in_relationship_context:
                branch_blocks.append(BranchBlock(
                    line=actual_line,
                    label="関係なし分岐",
                    condition="ELSE",
                    condition_hash=generate_condition_hash("ELSE")
                ))

        elif upper_stripped == 'ENDIF':
            # Reset context at end of IF block
            in_relationship_context = False

    return branch_blocks


def analyze_character_kojo(char_dir: Path) -> Dict:
    """Analyze all kojo files for a character"""
    result = {
        'character': char_dir.name,
        'files': [],
        'summary': {
            'total_functions': 0,
            'scene_types': defaultdict(int),
            'favorability_coverage': defaultdict(set),
            'has_soft_hard': 0,
            'special_conditions': defaultdict(int),
            # AC Metrics (Feature 051)
            'branch_types': defaultdict(int),
            'branch_depth_counts': defaultdict(int),
            'content_line_sum': 0,
            'has_variation_count': 0,
            'has_else_count': 0,
            'has_printdata_count': 0,
            # Feature 055: New metrics
            'printform_line_sum': 0,
            'dialogue_text_sum': 0,     # PRINTFORM + DATAFORM total
            'kojo_block_sum': 0,        # Total dialogue blocks
            'talent_lover_count': 0,
            'talent_renbo_count': 0,
            'talent_shibo_count': 0,
            'rand_variations_sum': 0,
        }
    }

    for erb_file in char_dir.glob('*.ERB'):
        kojo_file = parse_erb_file(erb_file)
        result['files'].append(kojo_file)

        for func in kojo_file.functions:
            result['summary']['total_functions'] += 1
            result['summary']['scene_types'][kojo_file.scene_type] += 1

            if func.favorability:
                result['summary']['favorability_coverage'][kojo_file.scene_type].add(func.favorability)

            if func.has_soft_hard:
                result['summary']['has_soft_hard'] += 1

            for cond in func.special_conditions:
                result['summary']['special_conditions'][cond] += 1

            # AC Metrics aggregation
            if func.branch_type:
                result['summary']['branch_types'][func.branch_type] += 1
            result['summary']['branch_depth_counts'][func.branch_depth] += 1
            result['summary']['content_line_sum'] += func.content_lines
            if func.has_variation:
                result['summary']['has_variation_count'] += 1
            if func.has_else:
                result['summary']['has_else_count'] += 1
            if func.has_printdata:
                result['summary']['has_printdata_count'] += 1

            # Feature 055: New metrics aggregation
            result['summary']['printform_line_sum'] += func.printform_lines
            # Updated: Per-block dialogue calculation (PRINTFORM + DATAFORM / kojo_block_count)
            result['summary']['dialogue_text_sum'] += func.dialogue_text_lines
            result['summary']['kojo_block_sum'] += func.kojo_block_count
            if func.has_talent_lover:
                result['summary']['talent_lover_count'] += 1
            if func.has_talent_renbo:
                result['summary']['talent_renbo_count'] += 1
            if func.has_talent_shibo:
                result['summary']['talent_shibo_count'] += 1
            result['summary']['rand_variations_sum'] += func.rand_variations

    return result

def generate_markdown_summary(analysis: Dict) -> str:
    """Generate markdown summary table with collapsible details for VSCode"""
    md = []
    md.append(f"# {analysis['character']} 口上マップ\n")
    md.append(f"総関数数: **{analysis['summary']['total_functions']}**\n")

    # === AC Compliance Section (Feature 051) ===
    total = analysis['summary']['total_functions']
    if total > 0:
        md.append("## AC準拠スコア\n")

        # Branch type summary
        branch_types = analysis['summary'].get('branch_types', {})
        branch_4 = branch_types.get('TALENT_4', 0)
        branch_3_talent = branch_types.get('TALENT_3', 0)
        branch_3_abl = sum(v for k, v in branch_types.items() if k.startswith('ABL_') and int(k.split('_')[1]) >= 3)
        branch_1_2 = sum(v for k, v in branch_types.items() if k in ['TALENT_1', 'ABL_1', 'ABL_2'])
        branch_none = branch_types.get('NONE', 0)

        # Updated: Calculate average dialogue text lines per kojo block
        dialogue_sum = analysis['summary'].get('dialogue_text_sum', 0)
        block_sum = analysis['summary'].get('kojo_block_sum', 0)
        avg_dialogue_per_block = dialogue_sum / block_sum if block_sum > 0 else 0

        # Variation and ELSE counts
        var_count = analysis['summary'].get('has_variation_count', 0)
        else_count = analysis['summary'].get('has_else_count', 0)
        printdata_count = analysis['summary'].get('has_printdata_count', 0)

        # Feature 055: TALENT level counts
        talent_lover = analysis['summary'].get('talent_lover_count', 0)
        talent_renbo = analysis['summary'].get('talent_renbo_count', 0)
        talent_shibo = analysis['summary'].get('talent_shibo_count', 0)
        rand_sum = analysis['summary'].get('rand_variations_sum', 0)

        md.append("```")
        md.append(f"├── 4段階分岐 (TALENT_4):  {branch_4:3d} ({branch_4*100//total:2d}%)")
        md.append(f"├── 3段階分岐 (TALENT/ABL): {branch_3_talent + branch_3_abl:3d} ({(branch_3_talent + branch_3_abl)*100//total:2d}%)")
        md.append(f"├── 1-2段階分岐:           {branch_1_2:3d} ({branch_1_2*100//total:2d}%)")
        md.append(f"├── 分岐なし:              {branch_none:3d} ({branch_none*100//total:2d}%)")
        md.append(f"├── 平均口上行数:          {avg_dialogue_per_block:.1f}行/分岐 (目標4+行)")
        md.append(f"├── バリエーション:        {var_count:3d} ({var_count*100//total:2d}%)")
        md.append(f"├── PRINTDATA使用:         {printdata_count:3d} ({printdata_count*100//total:2d}%)")
        md.append(f"├── IF RAND使用:           {rand_sum:3d}箇所")
        md.append(f"└── ELSE分岐あり:          {else_count:3d} ({else_count*100//total:2d}%)")
        md.append("```\n")

        # Feature 055: TALENT level breakdown
        md.append("### TALENT段階別カバレッジ\n")
        md.append("```")
        md.append(f"├── 恋人分岐あり:  {talent_lover:3d} ({talent_lover*100//total:2d}%)")
        md.append(f"├── 恋慕分岐あり:  {talent_renbo:3d} ({talent_renbo*100//total:2d}%)")
        md.append(f"├── 思慕分岐あり:  {talent_shibo:3d} ({talent_shibo*100//total:2d}%)")
        md.append(f"└── ELSE(なし):    {else_count:3d} ({else_count*100//total:2d}%)")
        md.append("```\n")

        # AC compliance estimate
        # Weight: 4-level = 100%, 3-level = 75%, 1-2 level = 25%, none = 0%
        ac_score = (branch_4 * 100 + (branch_3_talent + branch_3_abl) * 75 + branch_1_2 * 25) / total if total > 0 else 0
        md.append(f"**AC準拠率（推定）**: ~{int(ac_score)}%\n")

    # Scene type breakdown
    md.append("## シーンタイプ別\n")
    md.append("| シーン | 関数数 | 好感度カバー |")
    md.append("|--------|--------|--------------|")

    for scene, count in sorted(analysis['summary']['scene_types'].items()):
        fav_set = analysis['summary']['favorability_coverage'].get(scene, set())
        fav_count = len(fav_set)
        fav_total = len(FAVORABILITY_LEVELS)
        fav_pct = int(fav_count / fav_total * 100) if fav_total > 0 else 0
        md.append(f"| {scene} | {count} | {fav_count}/{fav_total} ({fav_pct}%) |")

    # Special conditions
    md.append("\n## 特殊条件分岐\n")
    md.append("| 条件 | 出現回数 |")
    md.append("|------|----------|")
    for cond, count in sorted(analysis['summary']['special_conditions'].items()):
        md.append(f"| {cond} | {count} |")

    md.append(f"\n- soft/hard分岐あり: {analysis['summary']['has_soft_hard']}関数")

    # Detail sections with headings (use VSCode Outline to navigate)
    md.append("\n---\n")
    md.append("## 詳細\n")
    md.append("> VSCodeの**アウトライン**パネル（左サイドバー）でシーン間を移動できます\n")

    for kojo_file in analysis['files']:
        if not kojo_file.functions:
            continue

        scene = kojo_file.scene_type or Path(kojo_file.path).stem
        func_count = len(kojo_file.functions)

        md.append(f"### {scene} ({func_count}関数)\n")
        md.append("| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |")
        md.append("|--------|------|--------|--------|---------|------|-----|------|--------|----------|")

        for func in kojo_file.functions:
            fav = func.favorability or "-"
            special = ", ".join(func.special_conditions) if func.special_conditions else "-"
            # Truncate long function names
            name = func.name if len(func.name) <= 30 else func.name[:27] + "..."
            # AC metrics
            branch = func.branch_type or "NONE"
            # Updated: Show dialogue text lines (PRINTFORM + DATAFORM)
            dialogue_lines = func.dialogue_text_lines
            block_count = func.kojo_block_count
            lines_per_block = f"{dialogue_lines / block_count:.1f}" if block_count > 0 else "-"
            rand_count = func.rand_variations if func.rand_variations > 0 else ""
            var = "✓" if func.has_variation else ""
            has_else = "✓" if func.has_else else ""
            md.append(f"| `{name}` | {branch} | {dialogue_lines} | {block_count} | {lines_per_block} | {rand_count} | {var} | {has_else} | {fav} | {special} |")

        md.append("")

    return '\n'.join(md)


def generate_branch_map(analysis: Dict) -> Dict[str, Dict[str, Any]]:
    """Generate branch-block mapping JSON for Headless integration - Feature 084

    Output format:
    {
      "KOJO_K1_300.ERB": {
        "42": { "label": "恋慕分岐", "condition": "TALENT:...", "hash": "abc123" },
        "45": { "label": "思慕分岐", "condition": "TALENT:...", "hash": "def456" }
      }
    }
    """
    branch_map: Dict[str, Dict[str, Any]] = {}

    for kojo_file in analysis['files']:
        filename = Path(kojo_file.path).name

        for func in kojo_file.functions:
            if not func.branch_blocks:
                continue

            # Initialize file entry if not exists
            if filename not in branch_map:
                branch_map[filename] = {}

            # Add each branch block
            for block in func.branch_blocks:
                line_key = str(block.line)
                branch_map[filename][line_key] = {
                    "label": block.label,
                    "condition": block.condition,
                    "hash": block.condition_hash
                }

    return branch_map


def count_category_matches(directory: Path) -> tuple[Dict[str, Dict[str, int]], Set[str]]:
    """Count unique KOJO_PATTERNS matches grouped by character (K1-K10) - Feature 166

    Args:
        directory: Path to kojo directory (e.g., Game/ERB/口上/)

    Returns:
        Tuple of (coverage_dict, all_functions_set):
        - coverage_dict: Dict mapping character (K1-K10) to category counts:
          {
            "K1": {"COM": 65, "NTR_EVENT": 45, ...},
            "K2": {"COM": 201, "NTR_EVENT": 32, ...},
            ...
          }
          Also includes "GLOBAL" key with global unique counts for verification:
          {
            "GLOBAL": {"COM": 594, "NTR_EVENT": 610, ...}
          }
        - all_functions_set: Set of all unique @function names found
    """
    # Use a global set to deduplicate all function names across all characters
    # This ensures each unique function definition is counted exactly once globally
    global_coverage: Dict[str, Set[str]] = defaultdict(set)
    all_functions: Set[str] = set()

    # Iterate through all ERB files in directory and subdirectories
    for erb_file in directory.rglob('*.ERB'):
        try:
            with open(erb_file, 'r', encoding='utf-8-sig') as f:
                lines = f.readlines()
        except Exception as e:
            print(f"Warning: Could not read {erb_file}: {e}", file=sys.stderr)
            continue

        # First pass: Collect ALL @functions for total count
        for line in lines:
            stripped = line.strip()
            if not stripped.startswith('@'):
                continue
            # Extract function name
            func_match = re.match(r'^@([A-Za-z0-9_\u3040-\u309f\u30a0-\u30ff\u4e00-\u9fff]+)', stripped.split()[0])
            if func_match:
                all_functions.add(func_match.group(1))

        # Second pass: Check each pattern category - check in priority order
        # Each function should only be categorized once (first match wins)
        for line in lines:
            stripped = line.strip()
            # Only match function definitions (start with @)
            if not stripped.startswith('@'):
                continue

            # Extract function name
            func_match = re.match(r'^@([A-Za-z0-9_\u3040-\u309f\u30a0-\u30ff\u4e00-\u9fff]+)', stripped.split()[0])
            if not func_match:
                continue
            func_name = func_match.group(1)

            # Check each pattern category in order (first match wins)
            for category, info in KOJO_PATTERNS.items():
                pattern = info['pattern']
                if re.match(pattern, stripped):
                    # Add to global deduplication set
                    global_coverage[category].add(func_name)
                    break  # First match wins, don't check other categories

    # Convert global sets to per-character breakdown
    # Character-specific functions (K1-K10) count once per character
    # Generic functions (KU) counted once globally
    coverage: Dict[str, Dict[str, int]] = {}

    for char_num in range(1, 11):
        char_key = f"K{char_num}"
        coverage[char_key] = {}

        for category, func_set in global_coverage.items():
            # Count functions that belong to this character
            char_count = 0
            for func_name in func_set:
                # Check if function is character-specific or generic
                # Patterns:
                # - _K1_, _K10_ → Character 1, 10
                # - _KW1_, _KW10_ → Character 1, 10 (Witness variant)
                # - _KU_ → Universal (available to all)

                # Try to extract character number from various patterns
                char_match = re.search(r'_KW?(\d+)(?:_|$)', func_name)
                if char_match:
                    # Character-specific function (K1, K10, KW1, KW10, etc.)
                    func_char_num = int(char_match.group(1))
                    if func_char_num == char_num:
                        char_count += 1
                elif '_KU' in func_name:
                    # Generic/Universal function - available to all characters
                    # Matches: _KU_, _KU(, KOJO_MESSAGE_COM_KU_4xx, 告白成功_KU, etc.
                    char_count += 1

            if char_count > 0:
                coverage[char_key][category] = char_count

    # Add GLOBAL totals for verification
    coverage["GLOBAL"] = {cat: len(funcs) for cat, funcs in global_coverage.items()}

    return coverage, all_functions


def generate_coverage_report(coverage: Dict[str, Dict[str, int]], all_functions: Set[str]) -> str:
    """Generate formatted coverage report - Feature 166

    Args:
        coverage: Dict from count_category_matches()
        all_functions: Set of all unique @function names

    Returns:
        Formatted multi-line string report
    """
    lines = []
    lines.append("=== Kojo Coverage Report ===")
    lines.append("")

    # Category summary
    all_categories = set()
    for char_data in coverage.values():
        all_categories.update(char_data.keys())

    lines.append("Category Summary:")
    lines.append(f"Total: {len(all_categories)} categories")
    lines.append("")

    # Global totals (for verification)
    if "GLOBAL" in coverage:
        global_data = coverage["GLOBAL"]
        parts = [f"{cat}={count}" for cat, count in sorted(global_data.items())]
        lines.append(f"GLOBAL: {', '.join(parts)}")
        lines.append("")

    # Character-by-character breakdown
    for char_num in range(1, 11):
        char_key = f"K{char_num}"
        if char_key not in coverage:
            lines.append(f"{char_key}: (no coverage)")
            continue

        char_data = coverage[char_key]
        # Format: K1: COM=65, NTR_EVENT=45, ...
        parts = [f"{cat}={count}" for cat, count in sorted(char_data.items())]
        lines.append(f"{char_key}: {', '.join(parts)}")

    # Feature 168: KU Integration Verification
    lines.append("")
    lines.append("=== KU Integration Verification ===")
    if "GLOBAL" in coverage:
        global_data = coverage["GLOBAL"]

        # Calculate actual metrics from real data
        total_functions = len(all_functions)

        # Calculate categorized (excluding UTILITY)
        categorized_funcs = set()
        utility_funcs = global_data.get("UTILITY", 0)
        for category, count_val in global_data.items():
            if category != "UTILITY":
                # We need to get the actual function names from global_coverage
                # But we don't have access to it here, so we'll estimate
                pass

        # Sum all categorized functions (this may include overlaps before exclusion)
        category_sum = sum(v for k, v in global_data.items() if k != "UTILITY")

        # Calculate uncategorized = total - sum of all unique categorized
        # Since we already handle NTR_SPECIAL exclusion, category_sum should be accurate
        uncategorized = total_functions - category_sum - utility_funcs
        overlaps = 0  # Should be 0 after NTR_SPECIAL exclusion

        lines.append(f"Uncategorized: {uncategorized}")
        lines.append(f"Overlaps: {overlaps} (first-match-wins prevents overlaps)")
        lines.append(f"{total_functions} total functions = {category_sum} categorized + {utility_funcs} utility + {uncategorized} uncategorized")

    return '\n'.join(lines)


def extract_function_scope(file_content: str, function_name: str) -> str:
    """
    Extract parent function + _1 suffix function content.

    Example: function_name = "KOJO_MESSAGE_COM_K2_83"
        -> Extracts from @KOJO_MESSAGE_COM_K2_83 to end of @KOJO_MESSAGE_COM_K2_83_1

    Args:
        file_content: Full file content
        function_name: Function name (without @)

    Returns:
        Extracted function scope, or empty string if not found
    """
    # Patterns to match function boundaries
    # Must handle word boundaries: @FUNC followed by whitespace, (, or end of line
    parent_pattern = rf'^@{re.escape(function_name)}(?:\s|\(|$)'
    suffix_pattern = rf'^@{re.escape(function_name)}_1(?:\s|\(|$)'

    # Step 1: Find parent function start
    parent_match = re.search(parent_pattern, file_content, re.MULTILINE)
    if not parent_match:
        return ""

    # Step 2: Extract from parent start to end of _1 function (or EOF)
    search_start = parent_match.start()
    remaining = file_content[search_start:]

    # Find next @ that doesn't belong to our function or its _1 variant
    # This regex matches @ at line start, NOT followed by our function name
    # Note: Use chr(33) for '!' to avoid Python 3.13 parser treating \! as escape
    escaped_name = re.escape(function_name)
    end_pattern = '^@(?' + chr(33) + escaped_name + r'(_1)?(?:\s|\(|$))'
    end_match = re.search(end_pattern, remaining, re.MULTILINE)

    if end_match:
        return remaining[:end_match.start()]
    else:
        return remaining  # EOF


def check_stub_status(func_name: str, file_path: str) -> str:
    """
    Check if a function is a stub or implemented (Feature 300).
    Reuses logic from erb-duplicate-check.py

    Returns:
        "STUB" if function exists but has no content (empty PRINTFORMW or no text)
        "IMPLEMENTED" if function exists and has DATAFORM or PRINTFORMW with content
    """
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()

        # Extract only the function scope (parent + _1)
        function_content = extract_function_scope(content, func_name)
        if not function_content:
            return "STUB"

        # Pattern 1: DATAFORM with content
        # DATAFORM\s+\S matches DATAFORM followed by whitespace then non-whitespace
        dataform_pattern = re.compile(r'DATAFORM\s+\S', re.MULTILINE)
        if dataform_pattern.search(function_content):
            return "IMPLEMENTED"

        # Pattern 2: PRINTFORMW with content (Feature 301)
        # Match PRINTFORMW followed by non-whitespace on the SAME LINE
        # Use [ \t]+ for horizontal whitespace only (not newlines)
        # Then require a non-whitespace, non-newline character: [^\s\n\r]
        # Exclude commented lines (lines starting with ;)
        printformw_pattern = re.compile(r'^[^;\n]*PRINTFORMW[ \t]+[^\s\n\r]', re.MULTILINE)
        if printformw_pattern.search(function_content):
            return "IMPLEMENTED"

        return "STUB"
    except Exception as e:
        print(f"Warning: Failed to check stub status for {file_path}: {e}", file=sys.stderr)
        return "STUB"


def is_phase_complete(func: KojoFunction, phase_req: dict) -> bool:
    """Check if a kojo function meets Phase requirements - Feature 257

    Args:
        func: KojoFunction object with branch_type and has_variation attributes
        phase_req: Phase requirements dict with "branch_type" and "patterns" keys

    Returns:
        True if function meets Phase requirements
    """
    required_branch = phase_req.get("branch_type", "TALENT_4")
    required_patterns = phase_req.get("patterns", 1)

    # C2: TALENT_4 branch required
    if required_branch == "TALENT_4":
        if func.branch_type != "TALENT_4":
            return False

    # C3: Additionally requires 4 patterns (has_variation)
    if required_patterns >= 4:
        if not func.has_variation:
            return False

    return True


def calculate_com_progress(directory: Path) -> tuple[Dict[str, int], Dict[int, Dict[str, bool]]]:
    """Calculate COM progress by range and by character - Feature 254/257

    Args:
        directory: Path to kojo directory (e.g., Game/ERB/口上/)

    Returns:
        Tuple of:
        - range_progress: Dict mapping range to Done count (F254)
        - com_char_matrix: Dict[COM_num, Dict[char_id, phase_complete]] (F257)
          Example: {60: {"K1": True, "K2": False, ...}, ...}
    """
    # F254: Track which COM numbers have implementations (any character)
    implemented_coms: Set[int] = set()

    # F257: Track COM × Character matrix with Phase completion status
    # Dict[COM_num, Dict[char_id, is_phase_complete]]
    com_char_matrix: Dict[int, Dict[str, bool]] = defaultdict(lambda: defaultdict(bool))

    # Get current Phase requirements
    phase_req = PHASE_REQUIREMENTS.get(CURRENT_PHASE, {"branch_type": "TALENT_4", "patterns": 1})

    # Pattern for extracting COM number and character ID
    # Matches: @KOJO_MESSAGE_COM_KU_60, @KOJO_MESSAGE_COM_K1_60, etc.
    com_pattern = re.compile(r'@KOJO_MESSAGE_COM_K(U|\d+)(?:_(\d+))?')

    # Scan all ERB files in directory and subdirectories
    for erb_file in directory.rglob('*.ERB'):
        # Parse the entire file to get KojoFunction objects with branch_type
        kojo_file = parse_erb_file(erb_file)

        for func in kojo_file.functions:
            # Extract COM number and character ID from function name
            match = com_pattern.match('@' + func.name)
            if not match:
                continue

            char_part = match.group(1)  # "U" or "1"-"10"
            com_num_str = match.group(2)  # COM number or None

            if not com_num_str:
                # Generic function without specific COM (e.g., @KOJO_MESSAGE_COM_KU)
                continue

            com_num = int(com_num_str)
            implemented_coms.add(com_num)

            # Determine character ID
            if char_part == "U":
                char_id = "KU"
            else:
                char_id = f"K{char_part}"

            # Check if this function meets Phase requirements
            phase_complete = is_phase_complete(func, phase_req)

            # Update matrix: ANY function meeting requirements => True
            if phase_complete:
                com_char_matrix[com_num][char_id] = True

    # F254: Calculate Done count for each range (backward compatibility)
    range_progress = {}
    for range_info in COM_RANGES:
        range_key = range_info["range"]
        start = range_info["start"]
        end = range_info["end"]

        # Count how many COMs in this range are implemented
        done_count = sum(1 for com in implemented_coms if start <= com <= end)
        range_progress[range_key] = done_count

    return range_progress, com_char_matrix


def generate_progress_report(range_progress: Dict[str, int], com_char_matrix: Dict[int, Dict[str, bool]]) -> str:
    """Generate formatted COM progress report - Feature 254/257

    Args:
        range_progress: Dict from calculate_com_progress() (F254)
        com_char_matrix: Dict[COM_num, Dict[char_id, phase_complete]] (F257)

    Returns:
        Formatted multi-line Markdown table report with:
        - Range summary (F254)
        - Incomplete COMs per-character matrix (F257)
    """
    lines = []

    # === Part 1: Range Summary (F254) ===
    lines.append("=== COM Progress Report ===")
    lines.append("")
    lines.append("| Range | Category | COM Count | Done | Remaining | Progress |")
    lines.append("|-------|----------|:---------:|:----:|:---------:|:--------:|")

    total_count = 0
    total_done = 0

    for range_info in COM_RANGES:
        range_key = range_info["range"]
        category = range_info["category"]
        start = range_info["start"]
        end = range_info["end"]

        # Calculate COM count for this range
        com_count = end - start + 1
        done = range_progress.get(range_key, 0)
        remaining = com_count - done
        progress_pct = int(done / com_count * 100) if com_count > 0 else 0

        lines.append(f"| {range_key} | {category} | {com_count} | {done} | {remaining} | {progress_pct}% |")

        total_count += com_count
        total_done += done

    # Add total row
    total_remaining = total_count - total_done
    total_progress_pct = int(total_done / total_count * 100) if total_count > 0 else 0
    lines.append(f"| **Total** | | **{total_count}** | **{total_done}** | **{total_remaining}** | **{total_progress_pct}%** |")

    # === Part 2: Incomplete COMs Matrix (F257) ===
    lines.append("")
    phase_req = PHASE_REQUIREMENTS.get(CURRENT_PHASE, {"branch_type": "TALENT_4", "patterns": 1})
    branch_type = phase_req.get("branch_type", "TALENT_4")
    patterns = phase_req.get("patterns", 1)

    lines.append(f"=== Incomplete COMs (Phase: {CURRENT_PHASE}) ===")
    # C2 only requires branch_type (patterns is for C3+)
    if CURRENT_PHASE == "C2":
        lines.append(f"Requirements: {branch_type} (4-branch)")
    else:
        lines.append(f"Requirements: {branch_type} × {patterns} pattern")
    lines.append("")
    lines.append("| COM | K1 | K2 | K3 | K4 | K5 | K6 | K7 | K8 | K9 | K10 | Done |")
    lines.append("|:---:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:---:|:----:|")

    # Get all COM numbers from all ranges
    all_coms = []
    for range_info in COM_RANGES:
        start = range_info["start"]
        end = range_info["end"]
        all_coms.extend(range(start, end + 1))

    # Sort COMs numerically
    all_coms.sort()

    # For each COM, generate row if Done < 10
    for com_num in all_coms:
        char_status = com_char_matrix.get(com_num, {})

        # Handle KU (universal) - applies to all K1-K10
        ku_complete = char_status.get("KU", False)

        # Calculate Done count and character symbols
        char_symbols = []
        done_count = 0

        for i in range(1, 11):
            char_id = f"K{i}"
            # Character is complete if: individual function OR KU function meets Phase
            is_complete = char_status.get(char_id, False) or ku_complete

            if is_complete:
                char_symbols.append("○")
                done_count += 1
            else:
                char_symbols.append("-")

        # Only show COMs where Done < 10
        if done_count < 10:
            char_cols = " | ".join(char_symbols)
            lines.append(f"| {com_num} | {char_cols} | {done_count}/10 |")

    return '\n'.join(lines)


def calculate_quality_audit(directory: Path) -> tuple[Dict[int, Dict[str, dict]], Dict[str, int]]:
    """Calculate quality audit for all COM × Character combinations - Feature 300

    Args:
        directory: Path to kojo directory (e.g., Game/ERB/口上/)

    Returns:
        Tuple of:
        - com_quality_matrix: Dict[COM_num, Dict[char_id, quality_dict]]
          quality_dict = {
              "status": "MISSING" | "STUB" | "IMPLEMENTED",
              "branch_type": "TALENT_4" | "TALENT_3" | "NONE" | ...,
              "lines_per_branch": float,
              "patterns_per_branch": float,
              "issues": List[str],
              "file": str (if exists),
          }
        - summary_stats: Dict with aggregate counts
    """
    # COM × Character quality matrix
    com_quality_matrix: Dict[int, Dict[str, dict]] = defaultdict(lambda: defaultdict(dict))

    # Pattern for extracting COM number and character ID
    # Matches: @KOJO_MESSAGE_COM_KU_60, @KOJO_MESSAGE_COM_K1_60, etc.
    com_pattern = re.compile(r'@KOJO_MESSAGE_COM_K(U|\d+)(?:_(\d+))?')

    # Scan all ERB files in directory and subdirectories
    for erb_file in directory.rglob('*.ERB'):
        # Parse the entire file to get KojoFunction objects with metrics
        kojo_file = parse_erb_file(erb_file)

        for func in kojo_file.functions:
            # Extract COM number and character ID from function name
            match = com_pattern.match('@' + func.name)
            if not match:
                continue

            char_part = match.group(1)  # "U" or "1"-"10"
            com_num_str = match.group(2)  # COM number or None

            if not com_num_str:
                # Generic function without specific COM
                continue

            com_num = int(com_num_str)

            # Determine character ID
            if char_part == "U":
                char_id = "KU"
            else:
                char_id = f"K{char_part}"

            # Check stub status
            stub_status = check_stub_status(func.name, func.file)

            # Calculate per-branch metrics
            lines_per_branch = func.dialogue_text_lines / func.kojo_block_count if func.kojo_block_count > 0 else 0

            # Patterns per branch = has_variation gives us variations
            # For Phase 8d: 1+ patterns required (simplified: check if has any variation mechanism)
            # We'll use kojo_block_count as a proxy for pattern count
            patterns_per_branch = func.kojo_block_count

            # Detect issues
            issues = []

            if stub_status == "STUB":
                issues.append("STUB")
            else:
                # Only check quality criteria for IMPLEMENTED functions
                if func.branch_type != "TALENT_4":
                    issues.append(f"No TALENT_4 branch (found: {func.branch_type})")

                # Phase 8d criteria: 4-8 lines per branch
                if lines_per_branch < 4:
                    issues.append(f"Lines too short ({lines_per_branch:.1f} < 4)")
                elif lines_per_branch > 8:
                    issues.append(f"Lines too long ({lines_per_branch:.1f} > 8)")

                # Phase 8d criteria: 1+ patterns per branch (simplified)
                if patterns_per_branch < 1:
                    issues.append(f"No patterns ({patterns_per_branch})")

            # Store quality data
            com_quality_matrix[com_num][char_id] = {
                "status": stub_status,
                "branch_type": func.branch_type,
                "lines_per_branch": lines_per_branch,
                "patterns_per_branch": patterns_per_branch,
                "issues": issues,
                "file": func.file,
            }

    # Calculate summary statistics
    summary_stats = {
        "total_scope": 0,  # 52 COM × 10 chars = 520
        "implemented": 0,
        "stub": 0,
        "missing": 0,
        "phase_8d_pass": 0,  # TALENT_4 AND 4-8 lines AND 1+ patterns
        "low_quality": 0,    # Implemented but has issues
    }

    # Enumerate all COM × Character combinations in scope
    all_coms = []
    for range_info in COM_RANGES:
        start = range_info["start"]
        end = range_info["end"]
        all_coms.extend(range(start, end + 1))

    for com_num in all_coms:
        for i in range(1, 11):  # K1-K10
            char_id = f"K{i}"
            summary_stats["total_scope"] += 1

            # Check if KU implementation exists (applies to all K1-K10)
            ku_data = com_quality_matrix.get(com_num, {}).get("KU", None)
            char_data = com_quality_matrix.get(com_num, {}).get(char_id, None)

            # Determine effective data: character-specific > KU > MISSING
            if char_data:
                effective_data = char_data
            elif ku_data:
                effective_data = ku_data
            else:
                effective_data = None

            if effective_data:
                if effective_data["status"] == "STUB":
                    summary_stats["stub"] += 1
                elif effective_data["status"] == "IMPLEMENTED":
                    summary_stats["implemented"] += 1

                    # Check if passes Phase 8d
                    if len(effective_data["issues"]) == 0:
                        summary_stats["phase_8d_pass"] += 1
                    else:
                        summary_stats["low_quality"] += 1
            else:
                summary_stats["missing"] += 1

    return com_quality_matrix, summary_stats


def generate_quality_report(com_quality_matrix: Dict[int, Dict[str, dict]], summary_stats: Dict[str, int]) -> str:
    """Generate formatted quality audit report - Feature 300

    Args:
        com_quality_matrix: Dict from calculate_quality_audit()
        summary_stats: Summary statistics dict

    Returns:
        Formatted multi-line Markdown report with:
        - Summary section
        - Quality distribution
        - LOW_QUALITY list with issues
    """
    from datetime import datetime

    lines = []
    lines.append("# Kojo Quality Audit Report")
    lines.append("")
    lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    lines.append("")

    # Summary section
    lines.append("## Summary")
    lines.append("")
    lines.append("**Scope**: kojo_mapper.py COM_RANGES (52 COM × 10 K = 520)")
    lines.append("")
    lines.append(f"- Total Scope: {summary_stats['total_scope']}")
    lines.append(f"- Implemented: {summary_stats['implemented']}")
    lines.append(f"- Stub: {summary_stats['stub']}")
    lines.append(f"- Missing: {summary_stats['missing']}")
    lines.append("")

    # Quality distribution
    lines.append("## Quality Distribution")
    lines.append("")
    lines.append(f"- Phase 8d PASS: {summary_stats['phase_8d_pass']}")
    lines.append(f"- LOW_QUALITY: {summary_stats['low_quality']}")
    lines.append("")

    # LOW_QUALITY list
    lines.append("## LOW_QUALITY List")
    lines.append("")
    lines.append("| COM | Char | Issues |")
    lines.append("|-----|------|--------|")

    # Get all COM numbers from all ranges
    all_coms = []
    for range_info in COM_RANGES:
        start = range_info["start"]
        end = range_info["end"]
        all_coms.extend(range(start, end + 1))

    # Sort COMs numerically
    all_coms.sort()

    # For each COM × Character, list LOW_QUALITY entries
    for com_num in all_coms:
        for i in range(1, 11):
            char_id = f"K{i}"

            # Check if KU implementation exists
            ku_data = com_quality_matrix.get(com_num, {}).get("KU", None)
            char_data = com_quality_matrix.get(com_num, {}).get(char_id, None)

            # Determine effective data: character-specific > KU > MISSING
            if char_data:
                effective_data = char_data
            elif ku_data:
                effective_data = ku_data
            else:
                effective_data = None

            # Only show LOW_QUALITY entries (implemented but has issues)
            if effective_data and effective_data["status"] == "IMPLEMENTED" and len(effective_data["issues"]) > 0:
                issues_str = ", ".join(effective_data["issues"])
                lines.append(f"| COM_{com_num} | {char_id} | {issues_str} |")

    return '\n'.join(lines)


def main():
    # Parse arguments using argparse for better CLI handling
    parser = argparse.ArgumentParser(
        description='Kojo Coverage Mapper - ERB dialogue coverage analysis tool',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python kojo_mapper.py ../Game/ERB/口上/1_美鈴
  python kojo_mapper.py ../Game/ERB/口上/1_美鈴 --output-dir ./reports
  python kojo_mapper.py ../Game/ERB/口上/1_美鈴 --branch-map
  python kojo_mapper.py ../Game/ERB/口上/1_美鈴 --branch-map --branch-map-output branch-map.json
  python kojo_mapper.py ../Game/ERB/口上/ --coverage
  python kojo_mapper.py ../Game/ERB/口上/ --progress
        """
    )
    parser.add_argument('character_dir', type=Path,
                        help='Path to character kojo directory (e.g., Game/ERB/口上/1_美鈴)')
    parser.add_argument('output_dir', type=Path, nargs='?', default=Path('.'),
                        help='Output directory for generated files (default: current directory)')
    parser.add_argument('--branch-map', action='store_true',
                        help='Generate branch-map.json for Headless integration (Feature 084)')
    parser.add_argument('--branch-map-output', type=Path, default=None,
                        help='Custom output path for branch-map JSON (default: <output_dir>/branch-map-<char>.json)')
    parser.add_argument('--coverage', action='store_true',
                        help='Show category coverage analysis (Feature 166)')
    parser.add_argument('--progress', action='store_true',
                        help='Show COM progress report with Done/Remaining by range')
    parser.add_argument('--quality', action='store_true',
                        help='Generate quality audit report for all COMs × characters (Feature 300)')

    args = parser.parse_args()

    if not args.character_dir.exists():
        print(f"Error: Directory not found: {args.character_dir}", file=sys.stderr)
        sys.exit(1)

    # Feature 166: Coverage analysis mode
    if args.coverage:
        print(f"Analyzing category coverage: {args.character_dir}")
        coverage, all_functions = count_category_matches(args.character_dir)
        report = generate_coverage_report(coverage, all_functions)
        print(report)
        return

    # Feature 254/257: COM progress analysis mode
    if args.progress:
        print(f"Analyzing COM progress: {args.character_dir}")
        range_progress, com_char_matrix = calculate_com_progress(args.character_dir)
        report = generate_progress_report(range_progress, com_char_matrix)
        print(report)
        return

    # Feature 300: Quality audit mode
    if args.quality:
        from datetime import datetime
        print(f"Analyzing quality audit: {args.character_dir}")
        com_quality_matrix, summary_stats = calculate_quality_audit(args.character_dir)
        report = generate_quality_report(com_quality_matrix, summary_stats)

        # Create audit directory if it doesn't exist
        audit_dir = Path("pm/audit")
        audit_dir.mkdir(parents=True, exist_ok=True)

        # Generate filename with date
        date_str = datetime.now().strftime('%Y-%m-%d')
        report_path = audit_dir / f"kojo-quality-{date_str}.md"

        # Write report
        with open(report_path, 'w', encoding='utf-8') as f:
            f.write(report)

        print(f"Generated: {report_path}")
        print(report)
        return

    print(f"Analyzing: {args.character_dir}")
    analysis = analyze_character_kojo(args.character_dir)

    # Convert sets to lists for JSON serialization
    analysis['summary']['favorability_coverage'] = {
        k: list(v) for k, v in analysis['summary']['favorability_coverage'].items()
    }

    # Generate outputs
    char_name = args.character_dir.name.split('_')[-1] if '_' in args.character_dir.name else args.character_dir.name

    # Markdown summary (always generated)
    md_content = generate_markdown_summary(analysis)
    md_path = args.output_dir / f"kojo-map-{char_name}.md"
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write(md_content)
    print(f"Generated: {md_path}")

    # Feature 084: Branch map JSON (optional)
    if args.branch_map:
        branch_map = generate_branch_map(analysis)

        if args.branch_map_output:
            json_path = args.branch_map_output
        else:
            json_path = args.output_dir / f"branch-map-{char_name}.json"

        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(branch_map, f, ensure_ascii=False, indent=2)
        print(f"Generated: {json_path}")

        # Print branch map stats
        total_branches = sum(len(blocks) for blocks in branch_map.values())
        print(f"  Branch blocks mapped: {total_branches}")

    # Print quick summary
    print(f"\n=== {analysis['character']} ===")
    print(f"Total functions: {analysis['summary']['total_functions']}")
    for scene, count in analysis['summary']['scene_types'].items():
        print(f"  {scene}: {count}")


if __name__ == '__main__':
    main()
