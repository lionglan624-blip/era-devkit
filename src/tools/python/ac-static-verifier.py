#!/usr/bin/env python3
"""
AC Static Verifier - Verifies code/build/file type ACs statically

Validates code, build, and file type Acceptance Criteria by performing
static checks (grep, dotnet build, file existence) and generates JSON logs.

Slash commands (starting with /) cannot be executed via subprocess per Testing SKILL.
Such ACs are marked as requiring manual verification and return MANUAL status.

Usage:
    python tools/ac-static-verifier.py --feature {ID} --ac-type {code|build|file}

Output:
    _out/logs/prod/ac/{type}/feature-{ID}/{type}-result.json

Exit codes:
    0 = All ACs passed
    1 = One or more ACs failed
"""

import argparse
import json
import os
import re
import subprocess
import sys
from enum import Enum, auto
from pathlib import Path
from typing import List, Dict, Any, Optional

WSL_DOTNET_PATH = "/home/siihe/.dotnet/dotnet"

# Binary file extensions to skip during directory enumeration
BINARY_EXTENSIONS = {
    '.dll', '.exe', '.so', '.dylib', '.a', '.lib',
    '.pdb', '.exp', '.ilk',
    '.cache', '.obj', '.bin', '.dat',
    '.pack', '.idx',
}

# Cross-repo prefix mapping: prefix -> (env_var_name, default_path)
# Resolves paths starting with these prefixes against their respective repo locations.
# Environment variables override defaults; defaults match CLAUDE.md conventions.
_CROSS_REPO_PREFIX_MAP: dict[str, tuple[str, str]] = {
    "engine/": ("ENGINE_PATH", "C:/Era/engine"),
    "core/":   ("CORE_PATH",   "C:/Era/core"),
    "game/":   ("GAME_PATH",   "C:/Era/game"),
    "dashboard/": ("DASHBOARD_PATH", "C:/Era/dashboard"),
}


class PatternType(Enum):
    """Pattern type classification for AC verification."""
    LITERAL = auto()
    REGEX = auto()
    GLOB = auto()
    COMPLEX_METHOD = auto()
    COUNT = auto()  # NEW: For count_equals, gt, gte, lt, lte
    UNKNOWN = auto()


class ACDefinition:
    """Represents a single AC from the feature markdown."""

    def __init__(self, ac_number: int, description: str, ac_type: str,
                 method: str, matcher: str, expected: str):
        self.ac_number = ac_number
        self.description = description
        self.ac_type = ac_type
        self.method = method
        self.matcher = matcher
        self.expected = expected
        self.pattern_type = None  # Set by classify_pattern()


class ACVerifier:
    """Main verifier class for AC static verification."""

    def __init__(self, feature_id: str, ac_type: str, repo_root: Path, verbose: bool = False):
        self.feature_id = feature_id
        self.ac_type = ac_type
        self.repo_root = repo_root
        self.verbose = verbose
        self.feature_file = repo_root / "pm" / "features" / f"feature-{feature_id}.md"

    def classify_pattern(self, ac: ACDefinition) -> PatternType:
        """Classify pattern type based on AC definition."""
        # Complex Method: has named parameters like Grep(path="...", pattern="...", type=cs)
        if re.search(r'Grep\s*\(.*=.*\)', ac.method, re.IGNORECASE):
            return PatternType.COMPLEX_METHOD

        # Regex: matcher is "matches"
        if ac.matcher.lower() == "matches":
            return PatternType.REGEX

        if ac.matcher.lower() == "not_matches":
            return PatternType.REGEX

        # Glob: file type with exists/not_exists matcher (glob path pattern)
        if ac.matcher.lower() in ("exists", "not_exists"):
            return PatternType.GLOB

        # Literal: default (contains/not_contains with plain strings)
        if ac.matcher.lower() in ("contains", "not_contains"):
            return PatternType.LITERAL

        # Count/Numeric matchers: count_equals, gt, gte, lt, lte
        if ac.matcher.lower() in ("count_equals", "gt", "gte", "lt", "lte"):
            return PatternType.COUNT

        return PatternType.UNKNOWN

    def _convert_to_wsl_path(self, windows_path: str) -> str:
        """Convert Windows path (C:/Era/devkit or C:\\Era\\devkit) to WSL mount path (/mnt/c/Era/devkit)."""
        path = windows_path.replace('\\', '/')
        if len(path) >= 2 and path[1] == ':':
            drive = path[0].lower()
            return f'/mnt/{drive}{path[2:]}'
        return path

    def _resolve_cross_repo_root(self, text: str) -> tuple[Optional[Path], str]:
        """Find first matching cross-repo prefix in text using _CROSS_REPO_PREFIX_MAP.

        Returns (repo_root, stripped_text) where:
        - repo_root: resolved repo root Path if a prefix matched, else None
        - stripped_text: text with the matching prefix removed, or original text if no match
        """
        for prefix, (env_var, default) in _CROSS_REPO_PREFIX_MAP.items():
            if text.startswith(prefix):
                return Path(os.environ.get(env_var, default)), text[len(prefix):]
        return None, text

    def _resolve_build_cwd(self, build_command: str) -> tuple[Optional[Path], str]:
        """Resolve cross-repo CWD for a build command.

        Returns (cross_repo_root, build_args) where:
        - cross_repo_root: resolved repo root Path if a cross-repo prefix was found, else None
        - build_args: build_command with the matching prefix token stripped, or
          the original command if no prefix matched or an explicit 'cd ' was found

        Commands containing 'cd ' are returned unchanged (None, build_command).
        """
        if "cd " in build_command:
            return None, build_command

        if build_command.startswith("dotnet "):
            args_portion = build_command[len("dotnet "):]
        else:
            args_portion = build_command

        tokens = args_portion.split()
        for token in tokens:
            cross_repo_root, stripped_token = self._resolve_cross_repo_root(token)
            if cross_repo_root is not None:
                build_args = build_command.replace(token, stripped_token, 1)
                return cross_repo_root, build_args

        return None, build_command

    def _safe_relative_path(self, path: Path) -> str:
        """Convert path to display string, using relative path if within repo_root."""
        try:
            return str(path.relative_to(self.repo_root))
        except ValueError:
            return str(path)

    def _expand_glob_path(self, file_path: str) -> tuple[bool, Optional[str], List[Path]]:
        """Expand glob pattern in file path and return matched files.

        Args:
            file_path: File path potentially containing glob patterns (*, ?, [)

        Returns:
            Tuple of (success, error_message, matched_files)
            - success: True if glob expansion succeeded or no glob pattern present
            - error_message: Error string if expansion failed, None otherwise
            - matched_files: List of Path objects matching the pattern
        """
        # Check if path contains comma (comma-separated patterns)
        if ',' in file_path:
            # First check if the literal path with comma exists
            file_path_obj = Path(file_path)
            if file_path_obj.is_absolute():
                target_file = file_path_obj  # Skip repo_root prepend for absolute paths
            else:
                target_file = self.repo_root / file_path
            if target_file.exists():
                return True, None, [target_file]

            # If not, split on comma
            patterns = [p.strip() for p in file_path.split(',')]
            all_matches = []
            for pattern in patterns:
                success, error_msg, matches = self._expand_glob_path(pattern)  # Recursive call
                # Don't fail if a pattern matches nothing - collect all matches from all patterns
                if success:
                    all_matches.extend(matches)
            if not all_matches:
                return False, f"No files match any pattern in: {file_path}", []
            return True, None, all_matches

        # Original logic for single pattern
        cross_repo_root, stripped_path = self._resolve_cross_repo_root(file_path)
        if cross_repo_root is not None:
            target_file = cross_repo_root / stripped_path
        else:
            file_path_obj = Path(file_path)
            if file_path_obj.is_absolute():
                target_file = file_path_obj  # Skip repo_root prepend for absolute paths
            else:
                target_file = self.repo_root / file_path

        # Check if path contains glob patterns
        has_glob_pattern = any(c in file_path for c in ['*', '?', '['])

        if has_glob_pattern:
            # Use glob for pattern matching
            import glob as glob_module
            matches = list(glob_module.glob(str(target_file), recursive=True))
            if not matches:
                return False, f"No files match glob pattern: {file_path}", []
            # Return all matched files
            return True, None, [Path(m) for m in matches]
        else:
            # Direct path check
            if not target_file.exists():
                return False, f"File not found: {file_path}", []

            # NEW: Directory detection and recursive enumeration
            if target_file.is_dir():
                # Recursively enumerate all files in directory, excluding binary files
                all_files = []
                skipped_count = 0
                for f in target_file.rglob('*'):
                    if f.is_file():
                        # Skip binary files by extension
                        if f.suffix.lower() in BINARY_EXTENSIONS:
                            skipped_count += 1
                            continue
                        all_files.append(f)

                # Log if binary files were skipped
                if skipped_count > 0 and self.verbose:
                    print(f"INFO: Skipped {skipped_count} binary file(s) in {file_path}", file=sys.stderr)

                return True, None, all_files

            # Existing: literal file path
            return True, None, [target_file]

    def _search_pattern_native(self, files: List[Path], pattern: str) -> tuple[bool, List[str]]:
        """Search for pattern in files using Python native string search.

        Uses `pattern in content` for literal string matching.
        PASS if pattern found in ANY file.

        Args:
            files: List of Path objects to search
            pattern: Literal string pattern to search for

        Returns:
            Tuple of (pattern_found, matched_files)
            - pattern_found: True if pattern found in at least one file
            - matched_files: List of relative file paths where pattern was found
        """
        pattern_found = False
        matched_files = []
        for file_path in files:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    if pattern in content:
                        pattern_found = True
                        matched_files.append(self._safe_relative_path(file_path))
            except UnicodeDecodeError:
                # Binary file not caught by extension filter (unusual extension)
                if self.verbose:
                    print(f"WARNING: Skipping binary file: {file_path}", file=sys.stderr)
                continue
        return pattern_found, matched_files

    # Shared unescape rules — single source for all unescape variants.
    # New rules MUST be added here only. Both unescape() and unescape_for_regex_pattern() draw from this list.
    _UNESCAPE_RULES = [
        (r'\"', '"'),
        (r'\\[', r'\['),
        (r'\\]', r'\]'),
        (r'\\(', r'\('),
        (r'\\)', r'\)'),
        (r'\\.', r'\.'),
        (r'\\?', r'\?'),
        (r'\\w', r'\w'),
    ]
    _PIPE_RULE = (r'\|', '|')  # markdown pipe escapes — Expected column only

    @staticmethod
    def unescape(s: str) -> str:
        r"""Unescape backslash escape sequences in a string.

        Handles backslash escape sequences in Expected values:
        - \" -> " (double quotes)
        - \\[ -> \[ (markdown bracket escapes for regex patterns)
        - \\] -> \] (markdown bracket escapes for regex patterns)
        - \| -> | (markdown pipe escapes)
        - \\( -> \( (CommonMark punctuation escape: open-paren)
        - \\) -> \) (CommonMark punctuation escape: close-paren)
        - \\. -> \. (CommonMark punctuation escape: dot)
        - \\? -> \? (CommonMark punctuation escape: question mark)
        - \\w -> \w (word-class per C3 spec; not CommonMark but per F804 evidence)

        Args:
            s: String with potential backslash escape sequences

        Returns:
            String with backslash escapes processed
        """
        for from_str, to_str in ACVerifier._UNESCAPE_RULES:
            s = s.replace(from_str, to_str)
        s = s.replace(*ACVerifier._PIPE_RULE)  # markdown pipe escapes
        return s

    @staticmethod
    def unescape_for_regex_pattern(s: str) -> str:
        r"""Unescape backslash escape sequences for Method-column regex patterns.

        Applies all _UNESCAPE_RULES EXCEPT _PIPE_RULE, because in regex context
        \| means literal pipe and must be preserved as-is.

        Args:
            s: Pattern string from Method column with potential markdown escape sequences

        Returns:
            String with markdown escapes processed, regex pipe escape preserved
        """
        for from_str, to_str in ACVerifier._UNESCAPE_RULES:
            s = s.replace(from_str, to_str)
        return s

    @staticmethod
    def unescape_for_literal_search(s: str) -> str:
        r"""Unescape pattern for literal string search in contains matcher.

        After markdown unescape (\\[ -> \[), further unescape for literal matching:
        - \[ -> [ (escaped bracket becomes literal bracket)
        - \] -> ] (escaped bracket becomes literal bracket)
        - \| -> | (escaped pipe becomes literal pipe)

        This allows patterns like \[DRAFT\] to match literal [DRAFT] in files.

        Args:
            s: String with backslash-escaped brackets and pipes

        Returns:
            String with bracket and pipe escapes removed for literal matching
        """
        return s.replace(r'\[', '[').replace(r'\]', ']').replace(r'\|', '|')

    @staticmethod
    def _contains_regex_metacharacters(pattern: str) -> bool:
        r"""Check if pattern contains clear regex patterns unsuitable for contains.

        Detects unambiguous regex patterns that should use 'matches' instead:
        - Quantifiers: .*, .+, .?, +, ?
        - Character classes: [a-z], [0-9], \d, \w, \s
        - Anchors: ^start, end$

        Does NOT flag single occurrences of {}, (), |, which are common in JSON/code/text.

        Args:
            pattern: The pattern string to check

        Returns:
            True if pattern contains unambiguous regex patterns
        """
        # Unambiguous regex patterns
        regex_patterns = [
            r'\.\*',    # .*
            r'\.\+',    # .+
            r'\.\?',    # .?
            r'[^\\]\+', # + not preceded by backslash
            r'[^\\]\?', # ? not preceded by backslash
            r'(?<!\\)\[(?:[^\]]*-[^\]]*|\\[dDwWsS]|\^[^\]]+|[^\]]{2,4})\]', # character classes with ranges, escapes, negation, or short sequences (2-4 chars)
            r'\\[dDwWsS]', # \d, \D, \w, \W, \s, \S
            r'^\^',     # starts with ^
            r'\$$',     # ends with $
        ]
        return any(re.search(p, pattern) for p in regex_patterns)

    def _is_key_value_start(self, params_str: str, pos: int) -> bool:
        """Check if position starts a key=value pair (not a positional argument)."""
        peek = pos
        while peek < len(params_str) and (params_str[peek].isalnum() or params_str[peek] == '_'):
            peek += 1
        # skip whitespace after candidate key
        while peek < len(params_str) and params_str[peek].isspace():
            peek += 1
        return peek < len(params_str) and params_str[peek] == '='

    def _parse_complex_method(self, method: str) -> Optional[Dict[str, str]]:
        """Parse Grep(key1=value1, key2="value2", ...) generic format.

        Args:
            method: Method column value potentially in complex format

        Returns:
            Dict with parsed key-value pairs from Method column
            None if not a complex format or parsing fails

        Example:
            Grep(path="tools/*.py", pattern="def classify", type=cs)
            → {'path': 'tools/*.py', 'pattern': 'def classify', 'type': 'cs'}
        """
        # Check if method matches complex format (has '=' inside parentheses)
        if not re.search(r'Grep\s*\(.*=.*\)', method, re.IGNORECASE):
            return None

        # Extract content inside parentheses
        match = re.search(r'Grep\s*\((.*)\)', method, re.IGNORECASE)
        if not match:
            return None

        params_str = match.group(1)
        result = {}
        positional_index = 0

        # Manual parsing to handle unquoted values with spaces/special chars
        i = 0
        while i < len(params_str):
            # Skip whitespace
            while i < len(params_str) and params_str[i].isspace():
                i += 1
            if i >= len(params_str):
                break

            # Check if current position starts a positional argument (not key=value)
            if not self._is_key_value_start(params_str, i):
                # Positional argument: read until comma
                pos_start = i
                while i < len(params_str) and params_str[i] != ',':
                    i += 1
                result[f'_positional_{positional_index}'] = params_str[pos_start:i].strip()
                positional_index += 1
                # Skip comma
                if i < len(params_str) and params_str[i] == ',':
                    i += 1
                continue

            # Read key (alphanumeric + underscore)
            key_start = i
            while i < len(params_str) and (params_str[i].isalnum() or params_str[i] == '_'):
                i += 1
            if i == key_start:
                break
            key = params_str[key_start:i]

            # Skip whitespace and '='
            while i < len(params_str) and params_str[i].isspace():
                i += 1
            if i >= len(params_str) or params_str[i] != '=':
                break
            i += 1  # skip '='
            while i < len(params_str) and params_str[i].isspace():
                i += 1

            # Read value (quoted or unquoted)
            if i >= len(params_str):
                break

            if params_str[i] in ('"', "'"):
                # Quoted value
                quote_char = params_str[i]
                i += 1
                value_start = i
                while i < len(params_str) and params_str[i] != quote_char:
                    i += 1
                value = params_str[value_start:i]
                if i < len(params_str):
                    i += 1  # skip closing quote
            else:
                # Unquoted value: read until comma or next key=value pattern
                value_start = i
                # Look ahead for next comma or key= pattern
                while i < len(params_str):
                    # Check if we've hit a comma at the current level
                    if params_str[i] == ',':
                        break
                    # Check if we've hit the start of next key= (word followed by =)
                    # Look ahead for pattern: whitespace + word + whitespace + =
                    if i > value_start:  # Don't check at start of value
                        lookahead = i
                        # Skip whitespace
                        while lookahead < len(params_str) and params_str[lookahead].isspace():
                            lookahead += 1
                        # Check if next non-space is start of word
                        if lookahead < len(params_str) and (params_str[lookahead].isalpha() or params_str[lookahead] == '_'):
                            # Read word
                            word_start = lookahead
                            while lookahead < len(params_str) and (params_str[lookahead].isalnum() or params_str[lookahead] == '_'):
                                lookahead += 1
                            # Skip whitespace after word
                            while lookahead < len(params_str) and params_str[lookahead].isspace():
                                lookahead += 1
                            # Check if followed by '='
                            if lookahead < len(params_str) and params_str[lookahead] == '=':
                                # Found next key=value, stop here
                                break
                    i += 1

                value = params_str[value_start:i].rstrip()  # Strip trailing whitespace

            result[key] = value

            # Skip comma if present
            while i < len(params_str) and params_str[i].isspace():
                i += 1
            if i < len(params_str) and params_str[i] == ',':
                i += 1

        return result if result else None

    def _extract_grep_params(self, ac: ACDefinition) -> tuple[Optional[str], Optional[str], Optional[Dict[str, Any]]]:
        """Extract file path and pattern from AC Method field for Grep operations.

        Handles both complex method format (Grep(path="...", pattern="..."))
        and simple format (Grep(path) or Grep path).

        Args:
            ac: AC definition with Method field containing Grep specification

        Returns:
            Tuple of (file_path, pattern, error_result):
            - Success: (file_path, pattern, None) - extracted parameters
            - Failure: (None, None, error_result_dict) - error result dictionary

        Logic:
            1. Try complex method parse: Grep(path="...", pattern="...")
            2. If complex, use parsed path and pattern (pattern from method takes precedence)
            3. If simple, extract path from Grep(path) or Grep path format
            4. Pattern defaults to ac.expected if not in method
        """
        # Try complex method parse first
        parsed = self._parse_complex_method(ac.method)
        if parsed:
            # Map positional first argument to 'path' if named 'path' not present
            if '_positional_0' in parsed and 'path' not in parsed:
                parsed['path'] = parsed['_positional_0']
            # Complex method format: use parsed parameters
            file_path = parsed.get('path')
            if not file_path:
                return None, None, {
                    "ac_number": ac.ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": f"Complex method format missing 'path' parameter: {ac.method}",
                        "pattern": ac.expected,
                        "matched_files": []
                    }
                }
            # Pattern from method takes precedence over Expected column
            pattern = parsed.get('pattern', ac.expected)
            # Strip backticks from pattern if present (complex method values may be backtick-wrapped)
            if pattern and pattern.startswith('`') and pattern.endswith('`') and len(pattern) >= 2:
                pattern = pattern[1:-1]
            # Unescape markdown escape sequences in complex method pattern
            # (mirrors Expected column unescape at line 792; unescape markdown escapes from parsed complex method pattern)
            if pattern:
                pattern = self.unescape_for_regex_pattern(pattern)
            # Consume glob parameter: filter path by glob suffix if present
            glob_param = parsed.get('glob')
            if glob_param and file_path and not any(c in file_path for c in ['*', '?', '[']):
                file_path = file_path.rstrip('/') + '/' + glob_param
        else:
            # Extract file path from Method field: "Grep(path)" or "Grep path"
            match = re.search(r'Grep\s*\(\s*([^)]+)\s*\)', ac.method, re.IGNORECASE)
            if not match:
                # Fallback: try space-separated format "Grep path"
                match = re.search(r'Grep\s+(.+)', ac.method, re.IGNORECASE)
            if not match:
                return None, None, {
                    "ac_number": ac.ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": f"Invalid Method format (expected 'Grep(path)' or 'Grep path'): {ac.method}",
                        "pattern": ac.expected,
                        "matched_files": []
                    }
                }

            file_path = match.group(1).strip()
            pattern = ac.expected

        return file_path, pattern, None

    def _verify_content(self, file_path: str, pattern: str, matcher: str, pattern_type: 'PatternType', ac_number: int, expected_count: Optional[int] = None) -> Dict[str, Any]:
        """Unified content verification for code and file types.

        Consolidates the duplicated logic from verify_code_ac and _verify_file_content.

        Args:
            file_path: File path or glob pattern to search
            pattern: String pattern to match (literal or regex depending on matcher)
            matcher: Matcher type (contains, not_contains, matches)
            pattern_type: PatternType classification for this AC
            ac_number: AC number for result reporting
            expected_count: Optional count for Format C (bare numeric Expected, pattern from Method)

        Returns:
            Result dict with ac_number, result (PASS/FAIL), details

        Matcher semantics (preserved from existing implementation):
            - contains: literal string presence (with regex metacharacter validation)
            - not_contains: literal string absence
            - matches: regex pattern matching
        """
        matcher = matcher.lower()

        # Log warning for UNKNOWN pattern types (falls back to matcher-based verification)
        if pattern_type == PatternType.UNKNOWN:
            print(f"WARNING: AC#{ac_number} has UNKNOWN pattern type (matcher: {matcher}), falling back to matcher-based verification", file=sys.stderr)

        # Expand glob patterns if present
        success, error_msg, target_files = self._expand_glob_path(file_path)
        if not success:
            return {
                "ac_number": ac_number,
                "result": "FAIL",
                "details": {
                    "error": error_msg,
                    "pattern": pattern,
                    "file_path": file_path,
                    "matcher": matcher,
                    "matched_files": []
                }
            }

        # Apply matcher logic
        if matcher == "contains":
            # Validate pattern doesn't contain unambiguous regex patterns
            if self._contains_regex_metacharacters(pattern):
                return {
                    "ac_number": ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": "Pattern contains regex patterns. Use 'matches' matcher for regex patterns.",
                        "pattern": pattern,
                        "file_path": file_path,
                        "matcher": matcher,
                        "guidance": "Change matcher from 'contains' to 'matches' for regex support",
                        "matched_files": []
                    }
                }
            # Use Python native search for contains matcher
            # Unescape bracket escapes for literal matching (\[ -> [)
            literal_pattern = self.unescape_for_literal_search(pattern)
            pattern_found, matched_files = self._search_pattern_native(target_files, literal_pattern)
            passed = pattern_found
        elif matcher == "not_contains":
            # Use Python native search for not_contains matcher
            # Unescape bracket escapes for literal matching (\[ -> [)
            literal_pattern = self.unescape_for_literal_search(pattern)
            pattern_found, _ = self._search_pattern_native(target_files, literal_pattern)
            passed = not pattern_found
            matched_files = [] if passed else [file_path]
        elif matcher == "matches":
            # Use Python regex for pattern matching
            try:
                pattern_found = False
                matched_files = []
                for tf in target_files:
                    try:
                        with open(tf, 'r', encoding='utf-8') as f:
                            content = f.read()
                            if re.search(pattern, content, re.MULTILINE) is not None:
                                pattern_found = True
                                matched_files.append(self._safe_relative_path(tf))
                    except UnicodeDecodeError:
                        # Binary file not caught by extension filter
                        if self.verbose:
                            print(f"WARNING: Skipping binary file: {tf}", file=sys.stderr)
                        continue
                passed = pattern_found
            except re.error as e:
                return {
                    "ac_number": ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": f"Invalid regex pattern: {str(e)}",
                        "pattern": pattern,
                        "file_path": file_path,
                        "matcher": matcher,
                        "matched_files": []
                    }
                }
        elif matcher == "not_matches":
            # Use Python regex for negative pattern matching (inverse of matches)
            try:
                pattern_found = False
                matched_files = []
                for tf in target_files:
                    try:
                        with open(tf, 'r', encoding='utf-8') as f:
                            content = f.read()
                            if re.search(pattern, content, re.MULTILINE) is not None:
                                pattern_found = True
                                matched_files.append(self._safe_relative_path(tf))
                    except UnicodeDecodeError:
                        # Binary file not caught by extension filter
                        if self.verbose:
                            print(f"WARNING: Skipping binary file: {tf}", file=sys.stderr)
                        continue
                passed = not pattern_found  # INVERTED: pattern found means FAIL
            except re.error as e:
                return {
                    "ac_number": ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": f"Invalid regex pattern: {str(e)}",
                        "pattern": pattern,
                        "file_path": file_path,
                        "matcher": matcher,
                        "matched_files": []
                    }
                }
        elif matcher in ("count_equals", "gt", "gte", "lt", "lte"):
            # Content-type numeric comparison: count pattern occurrences in file content
            # Three supported formats:
            #   Format C: expected_count provided by caller (bare numeric Expected, pattern from Method)
            #   Format A: `regex_pattern` = N  (backtick-wrapped, regex counting)
            #   Format B: Pattern (N)          (literal counting)

            if expected_count is not None:
                # Format C: count already parsed by caller, pattern is the search string
                search_pattern = pattern
                use_regex = False
                # Check if pattern looks like regex (contains unescaped metacharacters)
                if any(c in pattern for c in ['.', '*', '+', '?', '(', ')', '[', ']', '{', '}', '|', '^', '$', '\\']):
                    use_regex = True
                exp_count = expected_count
            else:
                format_a = re.match(r'^`(.+)`\s*(?:>=|<=|>|<|=)\s*(\d+)$', pattern)
                format_b = re.match(r'^(.*)\s+\((\d+)\)$', pattern)

                if format_a:
                    search_pattern = format_a.group(1)
                    exp_count = int(format_a.group(2))
                    use_regex = True
                elif format_b:
                    search_pattern = format_b.group(1)
                    exp_count = int(format_b.group(2))
                    use_regex = False
                else:
                    return {
                        "ac_number": ac_number,
                        "result": "FAIL",
                        "details": {
                            "error": f"Expected value must be in '`pattern` = N' or 'Pattern (N)' format for content-type {matcher} matcher, got: {pattern}",
                            "pattern": pattern,
                            "file_path": file_path,
                            "matcher": matcher,
                            "matched_files": []
                        }
                    }

            # Count occurrences across all target files
            actual_count = 0
            matched_files = []
            for tf in target_files:
                try:
                    with open(tf, 'r', encoding='utf-8') as f:
                        content = f.read()
                        if use_regex:
                            try:
                                file_count = len(re.findall(search_pattern, content, re.MULTILINE))
                            except re.error as e:
                                return {
                                    "ac_number": ac_number,
                                    "result": "FAIL",
                                    "details": {
                                        "error": f"Invalid regex pattern: {str(e)}",
                                        "pattern": search_pattern,
                                        "file_path": file_path,
                                        "matcher": matcher,
                                        "matched_files": []
                                    }
                                }
                        else:
                            file_count = content.count(search_pattern)
                        if file_count > 0:
                            actual_count += file_count
                            matched_files.append(self._safe_relative_path(tf))
                except UnicodeDecodeError:
                    if self.verbose:
                        print(f"WARNING: Skipping binary file: {tf}", file=sys.stderr)
                    continue

            # Perform numeric comparison (use exp_count instead of expected_count to avoid shadowing)
            if matcher == "count_equals":
                passed = actual_count == exp_count
            elif matcher == "gt":
                passed = actual_count > exp_count
            elif matcher == "gte":
                passed = actual_count >= exp_count
            elif matcher == "lt":
                passed = actual_count < exp_count
            elif matcher == "lte":
                passed = actual_count <= exp_count
            else:
                passed = False

            return {
                "ac_number": ac_number,
                "result": "PASS" if passed else "FAIL",
                "details": {
                    "pattern": search_pattern,
                    "file_path": file_path,
                    "matcher": matcher,
                    "expected_count": exp_count,
                    "actual_count": actual_count,
                    "matched_files": matched_files
                }
            }
        else:
            return {
                "ac_number": ac_number,
                "result": "FAIL",
                "details": {
                    "error": f"Unknown matcher: {matcher}",
                    "pattern": pattern,
                    "file_path": file_path,
                    "matcher": matcher,
                    "matched_files": []
                }
            }

        return {
            "ac_number": ac_number,
            "result": "PASS" if passed else "FAIL",
            "details": {
                "pattern": pattern,
                "file_path": file_path,
                "matcher": matcher,
                "pattern_found": pattern_found,
                "matched_files": matched_files if pattern_found else []
            }
        }

    def parse_feature_markdown(self) -> List[ACDefinition]:
        """Parse feature markdown to extract AC definitions.

        Returns:
            List of ACDefinition objects matching the requested ac_type
        """
        if not self.feature_file.exists():
            raise FileNotFoundError(f"Feature file not found: {self.feature_file}")

        acs = []
        in_ac_table = False

        with open(self.feature_file, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()

                # Detect AC table header
                if line.startswith("| AC# |"):
                    in_ac_table = True
                    continue

                # Skip separator line
                if in_ac_table and line.startswith("|:---:|"):
                    continue

                # Parse AC row
                if in_ac_table and line.startswith("|"):
                    # End of table check (empty row or section break)
                    if not line.strip("|").strip():
                        break

                    # Split on pipes, but respect quoted regions (don't split pipes inside quotes or backticks)
                    parts = []
                    current_part = ""
                    in_quotes = False
                    in_backticks = False
                    i = 0
                    while i < len(line):
                        char = line[i]
                        if char == '"' and not in_backticks:
                            # Count consecutive backslashes before the quote
                            num_backslashes = 0
                            j = i - 1
                            while j >= 0 and line[j] == '\\':
                                num_backslashes += 1
                                j -= 1
                            # Even number of backslashes means quote is NOT escaped
                            if num_backslashes % 2 == 0:
                                in_quotes = not in_quotes
                            current_part += char
                        elif char == '`' and not in_quotes:
                            # Backtick toggle (backticks are not escaped in markdown)
                            in_backticks = not in_backticks
                            current_part += char
                        elif char == '|' and not in_quotes and not in_backticks:
                            # Check if pipe is backslash-escaped
                            num_backslashes = 0
                            j = i - 1
                            while j >= 0 and line[j] == '\\':
                                num_backslashes += 1
                                j -= 1
                            if num_backslashes % 2 == 0:
                                # Not escaped - split here
                                parts.append(current_part.strip())
                                current_part = ""
                            else:
                                # Escaped pipe - include as content (without the escaping backslash)
                                # Remove the trailing backslash from current_part since it was an escape character
                                current_part = current_part[:-1] + char
                        else:
                            current_part += char
                        i += 1
                    # Add final part
                    if current_part or not parts:
                        parts.append(current_part.strip())
                    if len(parts) < 8:  # Need at least 8 parts (empty + 6 columns + empty)
                        continue

                    try:
                        ac_num_str = parts[1]
                        # Skip reserved ACs
                        if not ac_num_str.isdigit():
                            continue

                        ac_number = int(ac_num_str)
                        description = parts[2]
                        ac_type = parts[3]
                        method = parts[4]
                        matcher = parts[5]
                        # Strip outer double quotes and backticks first, then unescape backslash sequences
                        expected_raw = parts[6]
                        # Remove single pair of outer double quotes (maintain original order: quotes first)
                        if expected_raw.startswith('"') and expected_raw.endswith('"') and len(expected_raw) >= 2:
                            expected_raw = expected_raw[1:-1]
                        # Remove outer backticks (legacy support)
                        if expected_raw.startswith('`') and expected_raw.endswith('`'):
                            expected_raw = expected_raw[1:-1]
                        expected = self.unescape(expected_raw)

                        # Filter by requested ac_type
                        if ac_type == self.ac_type:
                            ac_obj = ACDefinition(
                                ac_number=ac_number,
                                description=description,
                                ac_type=ac_type,
                                method=method,
                                matcher=matcher,
                                expected=expected
                            )
                            ac_obj.pattern_type = self.classify_pattern(ac_obj)
                            acs.append(ac_obj)
                    except (ValueError, IndexError):
                        # Skip malformed rows
                        continue

        return acs

    def _resolve_count_expected(self, ac: 'ACDefinition', pattern: str) -> Optional[int]:
        """Resolve Format C expected_count from AC definition and extracted pattern.

        For count matchers (count_equals, gt, gte, lt, lte), when the Expected column
        contains a bare integer AND the pattern was extracted from the Method column
        (i.e., pattern != ac.expected), pass expected_count separately to _verify_content.

        This guard (pattern != ac.expected) distinguishes Format C (pattern from Method
        column) from Format A/B where ac.expected IS the pattern.

        Args:
            ac: AC definition with matcher and expected fields
            pattern: The regex/literal pattern extracted by _extract_grep_params

        Returns:
            int if matcher is a count matcher AND ac.expected is a bare integer AND
            pattern differs from ac.expected; None otherwise
        """
        expected_count = None
        if ac.matcher.lower() in ("count_equals", "gt", "gte", "lt", "lte"):
            if ac.expected.strip().isdigit() and pattern != ac.expected:
                expected_count = int(ac.expected.strip())
        return expected_count

    def verify_code_ac(self, ac: ACDefinition) -> Dict[str, Any]:
        """Verify a code type AC using grep.

        Args:
            ac: AC definition with Type=code, Method=Grep(path), Matcher=contains/not_contains/matches

        Returns:
            Result dictionary with ac_number, result (PASS/FAIL), details

        Supported matchers:
            - contains: literal string containment
            - not_contains: literal string absence
            - matches: regex pattern matching
        """
        # Extract Grep parameters using shared method
        file_path, pattern, error_result = self._extract_grep_params(ac)
        if error_result is not None:
            return error_result

        expected_count = self._resolve_count_expected(ac, pattern)

        # Delegate to unified content verification method
        return self._verify_content(file_path, pattern, ac.matcher, ac.pattern_type, ac.ac_number, expected_count=expected_count)

    def verify_build_ac(self, ac: ACDefinition) -> Dict[str, Any]:
        """Verify a build type AC using dotnet build.

        Args:
            ac: AC definition with Type=build, Matcher=succeeds/fails

        Returns:
            Result dictionary with ac_number, result (PASS/FAIL), details
        """
        matcher = ac.matcher.lower()

        # Extract build command from Method column (preferred) or Expected field
        # Use Method column when Expected is "-" or a pure number (exit code like "0")
        expected_stripped = ac.expected.strip()
        if expected_stripped == "-" or expected_stripped.isdigit():
            build_command = ac.method.strip()
        else:
            build_command = expected_stripped  # Legacy: command in Expected column

        # Resolve cross-repo CWD and strip prefix from build args
        cross_repo_root, resolved_command = self._resolve_build_cwd(build_command)

        # Execute build command
        try:
            if resolved_command.startswith("dotnet ") or resolved_command.strip() == "dotnet":
                # Run dotnet commands via WSL from repository root (or resolved cross-repo root)
                wsl_dotnet = WSL_DOTNET_PATH
                # Strip leading 'dotnet' from resolved_command to avoid duplication with wsl_dotnet
                # e.g., "dotnet build devkit.sln" → "build devkit.sln"
                if resolved_command.startswith("dotnet "):
                    build_args = resolved_command[len("dotnet "):]
                else:
                    build_args = "build"  # default subcommand
                cross_repo_root = cross_repo_root if cross_repo_root is not None else self.repo_root
                wsl_cross_repo_root = self._convert_to_wsl_path(str(cross_repo_root))
                env = {**os.environ, "MSYS_NO_PATHCONV": "1"}  # Harmless when called from Python; needed if invoked from Git Bash
                result = subprocess.run(
                    ["wsl", "--", "bash", "-c", f"cd {wsl_cross_repo_root} && {wsl_dotnet} {build_args}"],
                    capture_output=True,
                    text=True,
                    encoding='utf-8',
                    errors='replace',
                    timeout=300,
                    env=env
                )
            else:
                # Non-dotnet commands: run directly without WSL wrapper
                cross_repo_root = cross_repo_root if cross_repo_root is not None else self.repo_root
                result = subprocess.run(
                    resolved_command.split(),
                    capture_output=True,
                    text=True,
                    encoding='utf-8',
                    errors='replace',
                    timeout=300,
                    cwd=str(cross_repo_root)
                )
            exit_code = result.returncode
            stdout = result.stdout
            stderr = result.stderr
        except Exception as e:
            return {
                "ac_number": ac.ac_number,
                "result": "FAIL",
                "details": {
                    "matcher": matcher,
                    "error": f"Build execution failed: {str(e)}",
                    "command": build_command
                }
            }

        # Apply matcher logic
        if matcher == "succeeds":
            passed = (exit_code == 0)
        elif matcher == "fails":
            passed = (exit_code != 0)
        else:
            return {
                "ac_number": ac.ac_number,
                "result": "FAIL",
                "details": {
                    "matcher": matcher,
                    "error": f"Unknown matcher: {matcher}",
                    "exit_code": exit_code
                }
            }

        return {
            "ac_number": ac.ac_number,
            "result": "PASS" if passed else "FAIL",
            "details": {
                "matcher": matcher,
                "command": build_command,
                "exit_code": exit_code,
                "stdout": stdout[:500] if stdout else "",  # Truncate for brevity
                "stderr": stderr[:500] if stderr else ""   # Truncate for brevity
            }
        }

    def _handle_slash_command_ac(self, method: str, matcher: str, expected: str, ac_number: int) -> Dict[str, Any]:
        """Handle slash command AC by marking as manual verification.

        Per Testing SKILL line 79, slash commands cannot be executed via subprocess.
        Alternative approach: Mark as manual verification type.

        Args:
            method: Slash command (e.g., "/audit")
            matcher: Expected matcher (e.g., "succeeds")
            expected: Expected value (e.g., "-" or specific output)
            ac_number: AC number for result reporting

        Returns:
            Dict with status MANUAL and guidance for manual verification
        """
        return {
            "ac_number": ac_number,
            "result": "MANUAL",
            "details": {
                "slash_command": method,
                "matcher": matcher,
                "expected": expected,
                "manual_verification": "Slash commands require manual verification - execute the command and verify results manually"
            }
        }

    def verify_file_ac(self, ac: ACDefinition) -> Dict[str, Any]:
        """Verify a file type AC using file existence check or content verification.

        Args:
            ac: AC definition with Type=file, Matcher=exists/not_exists/contains/not_contains

        Returns:
            Result dictionary with ac_number, result (PASS/FAIL/MANUAL), details
        """
        # Check if Method is a slash command pattern
        if ac.method.startswith('/'):
            return self._handle_slash_command_ac(ac.method, ac.matcher, ac.expected, ac.ac_number)

        # Check if Method contains "Grep" for content verification
        if "grep" in ac.method.lower():
            # Extract Grep parameters using shared method
            file_path, pattern, error_result = self._extract_grep_params(ac)
            if error_result is not None:
                return error_result

            expected_count = self._resolve_count_expected(ac, pattern)

            # Delegate to unified content verification method
            return self._verify_content(file_path, pattern, ac.matcher, ac.pattern_type, ac.ac_number, expected_count=expected_count)

        # Check if Method contains Glob(pattern) syntax
        if "Glob(" in ac.method:
            match = re.search(r'Glob\((.*?)\)', ac.method)
            if match:
                file_path = match.group(1)
            else:
                file_path = ac.expected  # Fallback
        else:
            file_path = ac.expected  # Traditional behavior
        matcher = ac.matcher.lower()

        # Use _expand_glob_path for consistent handling of glob patterns and comma-separated patterns
        success, error_msg, matched_path_objs = self._expand_glob_path(file_path)

        if success:
            file_exists = True
            matched_files = [self._safe_relative_path(p) for p in matched_path_objs]
        else:
            file_exists = False
            matched_files = []

        # Apply matcher logic
        if matcher == "exists":
            passed = file_exists
        elif matcher == "not_exists":
            passed = not file_exists
        elif matcher in ("count_equals", "gt", "gte", "lt", "lte"):
            # Numeric comparison matchers
            # For file type, count files matching the glob/path pattern
            # Expected column contains the numeric threshold

            # Validate Expected is numeric
            try:
                expected_count = int(ac.expected)
            except ValueError:
                return {
                    "ac_number": ac.ac_number,
                    "result": "FAIL",
                    "details": {
                        "error": f"Expected value must be numeric for {matcher} matcher, got: {ac.expected}",
                        "file_path": file_path,
                        "matcher": matcher,
                        "matched_files": []
                    }
                }

            actual_count = len(matched_files) if file_exists else 0

            # Perform numeric comparison
            if matcher == "count_equals":
                passed = actual_count == expected_count
            elif matcher == "gt":
                passed = actual_count > expected_count
            elif matcher == "gte":
                passed = actual_count >= expected_count
            elif matcher == "lt":
                passed = actual_count < expected_count
            elif matcher == "lte":
                passed = actual_count <= expected_count
            else:
                passed = False

            return {
                "ac_number": ac.ac_number,
                "result": "PASS" if passed else "FAIL",
                "details": {
                    "file_path": file_path,
                    "matcher": matcher,
                    "expected_count": expected_count,
                    "actual_count": actual_count,
                    "matched_files": matched_files
                }
            }
        else:
            return {
                "ac_number": ac.ac_number,
                "result": "FAIL",
                "details": {
                    "error": f"Unknown matcher: {matcher}",
                    "file_path": file_path,
                    "matcher": matcher,
                    "exists": file_exists,
                    "matched_files": matched_files
                }
            }

        return {
            "ac_number": ac.ac_number,
            "result": "PASS" if passed else "FAIL",
            "details": {
                "file_path": file_path,
                "matcher": matcher,
                "exists": file_exists,
                "matched_files": matched_files
            }
        }

    def verify_ac(self, ac: ACDefinition) -> Dict[str, Any]:
        """Dispatch AC verification to appropriate method."""
        if self.ac_type == "code":
            return self.verify_code_ac(ac)
        elif self.ac_type == "build":
            return self.verify_build_ac(ac)
        elif self.ac_type == "file":
            return self.verify_file_ac(ac)
        else:
            return {
                "ac_number": ac.ac_number,
                "result": "FAIL",
                "details": {"error": f"Unknown AC type: {self.ac_type}"}
            }

    def run(self) -> int:
        """Execute verification for all matching ACs.

        Returns:
            Exit code: 0 for all pass, 1 for any failures
        """
        # Parse feature markdown
        acs = self.parse_feature_markdown()

        if not acs:
            print(f"SKIP: No {self.ac_type} type ACs found in feature-{self.feature_id}.md",
                  file=sys.stderr)
            return 0

        # Verify each AC
        results = []
        for ac in acs:
            result = self.verify_ac(ac)
            results.append(result)

        # Calculate summary
        total = len(results)
        passed = sum(1 for r in results if r["result"] == "PASS")
        manual = sum(1 for r in results if r["result"] == "MANUAL")
        failed = total - passed - manual

        # Prepare output JSON
        output = {
            "feature": int(self.feature_id),
            "type": self.ac_type,
            "results": results,
            "summary": {
                "total": total,
                "passed": passed,
                "manual": manual,
                "failed": failed
            }
        }

        # Write JSON log
        output_dir = self.repo_root / "_out" / "logs" / "prod" / "ac" / self.ac_type / f"feature-{self.feature_id}"
        output_dir.mkdir(parents=True, exist_ok=True)

        output_file = output_dir / f"{self.ac_type}-result.json"
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(output, f, indent=2, ensure_ascii=False)

        print(f"Verification complete: {passed}/{total} passed, {manual} manual")
        print(f"Log written to: {output_file}")

        return 0 if failed == 0 else 1


def main():
    parser = argparse.ArgumentParser(
        description="AC Static Verifier - Verifies code/build/file type ACs"
    )
    parser.add_argument(
        "--feature",
        required=True,
        help="Feature ID (e.g., 268)"
    )
    parser.add_argument(
        "--ac-type",
        required=True,
        choices=["code", "build", "file"],
        help="AC type to verify"
    )
    parser.add_argument(
        "--repo-root",
        default=".",
        help="Repository root directory (default: current directory)"
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Print verbose output including binary file warnings (default: silent)"
    )

    args = parser.parse_args()

    repo_root = Path(args.repo_root).resolve()
    verifier = ACVerifier(args.feature, args.ac_type, repo_root, verbose=args.verbose)

    try:
        exit_code = verifier.run()
        sys.exit(exit_code)
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
