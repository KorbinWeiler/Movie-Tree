#!/usr/bin/env python3
"""Filter movie records to keep only English original_title entries.

Supports:
- NDJSON input (one JSON object per line)
- JSON array input ([{...}, {...}, ...])

Usage:
    python filter_english_titles.py "movie_ids_03_05_2026 copy 2.json"
    python filter_english_titles.py input.json -o output.json --threshold 0.70

Optional dependency for better language detection:
    pip install langdetect
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any

try:
    from langdetect import DetectorFactory, detect_langs

    DetectorFactory.seed = 0
    HAS_LANGDETECT = True
except Exception:
    HAS_LANGDETECT = False


COMMON_ENGLISH_WORDS = {
    "the",
    "a",
    "an",
    "of",
    "to",
    "and",
    "in",
    "for",
    "with",
    "on",
    "at",
    "from",
    "is",
    "you",
    "love",
    "my",
    "your",
    "night",
    "day",
    "life",
    "man",
    "woman",
    "war",
    "world",
    "home",
    "dead",
    "dark",
    "sun",
    "moon",
    "summer",
    "winter",
    "history",
    "future",
}


def detect_input_format(path: Path) -> str:
    """Return 'ndjson' or 'array' based on first non-whitespace char."""
    with path.open("r", encoding="utf-8-sig") as f:
        while True:
            ch = f.read(1)
            if not ch:
                raise ValueError("Input file is empty.")
            if not ch.isspace():
                return "array" if ch == "[" else "ndjson"


def english_score_heuristic(title: str) -> float:
    """Simple fallback score when langdetect is unavailable or uncertain."""
    if not title:
        return 0.0

    # Favor titles made mostly from Latin letters and common separators.
    allowed = re.findall(r"[A-Za-z0-9\s'\-:,.!?&()]+", title)
    allowed_len = sum(len(part) for part in allowed)
    char_ratio = allowed_len / max(len(title), 1)

    words = [w.lower() for w in re.findall(r"[A-Za-z']+", title)]
    if not words:
        return char_ratio * 0.4

    stopword_hits = sum(1 for w in words if w in COMMON_ENGLISH_WORDS)
    stopword_ratio = stopword_hits / len(words)

    # Weighted blend for a rough English-likeness score.
    return (0.7 * char_ratio) + (0.3 * min(stopword_ratio * 3.0, 1.0))


def is_english_title(title: Any, threshold: float) -> bool:
    if not isinstance(title, str) or not title.strip():
        return False

    title = title.strip()
    heuristic = english_score_heuristic(title)

    if HAS_LANGDETECT and len(title) >= 4:
        try:
            langs = detect_langs(title)
            en_prob = next((lang.prob for lang in langs if lang.lang == "en"), 0.0)

            # Blend detector probability with heuristic for short/ambiguous titles.
            blended = (0.7 * en_prob) + (0.3 * heuristic)
            return blended >= threshold
        except Exception:
            # Fall back to heuristic on detection errors.
            return heuristic >= threshold

    return heuristic >= threshold


def process_ndjson(
    input_path: Path, output_path: Path, threshold: float, progress_every: int
) -> tuple[int, int, int]:
    total = 0
    kept = 0

    with input_path.open("r", encoding="utf-8-sig") as src, output_path.open(
        "w", encoding="utf-8"
    ) as out:
        for line_no, line in enumerate(src, start=1):
            line = line.strip()
            if not line:
                continue

            try:
                obj = json.loads(line)
            except json.JSONDecodeError as exc:
                raise ValueError(f"Invalid JSON on line {line_no}: {exc}") from exc

            if not isinstance(obj, dict):
                continue

            total += 1
            if is_english_title(obj.get("original_title"), threshold):
                out.write(json.dumps(obj, ensure_ascii=False))
                out.write("\n")
                kept += 1

            if progress_every > 0 and total % progress_every == 0:
                print(f"Processed {total} records... kept {kept}")

    return total, kept, total - kept


def process_json_array(
    input_path: Path, output_path: Path, threshold: float, progress_every: int
) -> tuple[int, int, int]:
    with input_path.open("r", encoding="utf-8-sig") as f:
        data = json.load(f)

    if not isinstance(data, list):
        raise ValueError("Expected a JSON array at top-level.")

    total = 0
    kept = 0
    first = True

    with output_path.open("w", encoding="utf-8") as out:
        out.write("[")
        for item in data:
            if not isinstance(item, dict):
                continue

            total += 1
            if is_english_title(item.get("original_title"), threshold):
                if not first:
                    out.write(",")
                out.write(json.dumps(item, ensure_ascii=False))
                first = False
                kept += 1

            if progress_every > 0 and total % progress_every == 0:
                print(f"Processed {total} records... kept {kept}")

        out.write("]")

    return total, kept, total - kept


def default_output_path(input_path: Path) -> Path:
    return input_path.with_name(f"{input_path.stem}_english{input_path.suffix}")


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Filter out movies whose original_title is not English."
    )
    parser.add_argument("input_file", type=Path, help="Path to input JSON/NDJSON file")
    parser.add_argument("-o", "--output", type=Path, default=None, help="Output file path")
    parser.add_argument(
        "-t",
        "--threshold",
        type=float,
        default=0.65,
        help="English confidence threshold from 0 to 1 (default: 0.65)",
    )
    parser.add_argument(
        "--progress-every",
        type=int,
        default=1000,
        help="Print progress every N processed records (default: 1000)",
    )

    args = parser.parse_args()

    if not args.input_file.exists():
        raise FileNotFoundError(f"Input file not found: {args.input_file}")
    if not (0.0 <= args.threshold <= 1.0):
        raise ValueError("Threshold must be between 0 and 1.")
    if args.progress_every < 0:
        raise ValueError("--progress-every must be 0 or greater.")

    output_path = args.output or default_output_path(args.input_file)
    in_format = detect_input_format(args.input_file)

    if in_format == "ndjson":
        total, kept, removed = process_ndjson(
            args.input_file, output_path, args.threshold, args.progress_every
        )
    else:
        total, kept, removed = process_json_array(
            args.input_file, output_path, args.threshold, args.progress_every
        )

    print(f"Input format      : {in_format}")
    print(f"Total movies      : {total}")
    print(f"Kept (English)    : {kept}")
    print(f"Removed (non-EN)  : {removed}")
    print(f"Output written to : {output_path}")
    if not HAS_LANGDETECT:
        print("Hint: install 'langdetect' for more accurate language filtering.")


if __name__ == "__main__":
    main()