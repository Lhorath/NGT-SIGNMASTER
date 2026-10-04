#!/usr/bin/env python3
from __future__ import annotations

import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TSV = ROOT / "src" / "NGT.SignMaster" / "Resources" / "default-signs.tsv"

ALLOWED_ROLES = {"Storage", "Portal", "Header", "Trophy"}
COLOR_RE = re.compile(r"^<#[0-9a-fA-F]{3}>")
TAG_RE = re.compile(r"<[^>]+>")


def parse_tsv() -> list[tuple[str, str, str]]:
    records: list[tuple[str, str, str]] = []
    for number, line in enumerate(TSV.read_text(encoding="utf-8").splitlines(), start=1):
        if not line or line.startswith("#"):
            continue

        parts = line.split("\t", 2)
        if len(parts) != 3:
            raise AssertionError(f"TSV line {number} does not contain exactly three columns")

        records.append((parts[0], parts[1], parts[2]))

    return records


def visible_label(styled: str) -> str:
    first_line = styled.split(r"\n", 1)[0]
    return TAG_RE.sub("", first_line).strip()


def main() -> None:
    records = parse_tsv()

    assert len(records) == 285, f"expected 285 unique styles, found {len(records)}"

    keys = [(role, label.upper()) for role, label, _ in records]
    duplicates = [key for key, count in Counter(keys).items() if count > 1]
    assert not duplicates, f"duplicate role/label rules: {duplicates}"

    labels = {label.upper() for _, label, _ in records}
    assert len(labels) == 256, f"expected 256 unique labels, found {len(labels)}"

    for role, label, styled in records:
        assert role in ALLOWED_ROLES, f"unknown role: {role}"
        assert label, "empty label"
        assert styled, f"empty style for {role}:{label}"
        assert COLOR_RE.match(styled), f"{role}:{label} does not start with a compact 3-digit color"
        assert r"\n" in styled, f"{role}:{label} is missing literal \\n"
        assert "</" not in styled, f"{role}:{label} contains a closing tag"
        assert "<b>" not in styled.lower(), f"{role}:{label} uses unsupported bold markup"
        assert len(styled) <= 50, f"{role}:{label} is {len(styled)} characters"
        assert visible_label(styled) == label, (
            f"{role}:{label} visible first-line label does not match the catalog key"
        )

    by_label = Counter(label.upper() for _, label, _ in records)
    ambiguous = sum(1 for count in by_label.values() if count > 1)
    assert ambiguous == 22, f"expected 22 ambiguous labels, found {ambiguous}"

    lookup = {(role, label): styled for role, label, styled in records}
    assert lookup[("Storage", "WOOD")] == r"<#bf8><size=3>WOOD\n<size=5>🌳"
    assert lookup[("Portal", "MEADOWS")] == r"<#bf8><size=3>MEADOWS\n<size=5>🌱"
    assert lookup[("Trophy", "MEADOWS")] == r"<#bf8><size=3>MEADOWS\n<size=5>🏆🌱"
    assert lookup[("Header", "MEADOWS")].startswith("<#bf8><u><cspace=6><size=5>")
    assert "<i>" in lookup[("Portal", "FROST CAVE")]
    assert "<u>" in lookup[("Portal", "MAIN HUB")]

    print(
        f"Validated {len(records)} styles / {len(labels)} labels; "
        f"{ambiguous} ambiguous labels; max length {max(len(style) for _, _, style in records)}."
    )


if __name__ == "__main__":
    main()
