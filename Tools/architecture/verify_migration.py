#!/usr/bin/env python3
"""Verify migration_manifest.json: destinations exist, sources gone, .meta GUIDs present."""
from __future__ import annotations
import json, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MAN = ROOT / "Assets/_Project/Settings/Architecture/migration_manifest.json"

def meta_guid(asset: Path) -> str | None:
    m = Path(str(asset) + ".meta")
    if not m.is_file():
        return None
    for line in m.read_text().splitlines():
        if line.startswith("guid:"):
            return line.split(":", 1)[1].strip()
    return None

def main() -> int:
    data = json.loads(MAN.read_text())
    errs = []
    for mv in data.get("moves", []):
        dest = ROOT / mv["to"]
        src = ROOT / mv["from"]
        if not dest.is_file():
            errs.append(f"missing dest {mv['to']}")
            continue
        if src.is_file():
            errs.append(f"source still present {mv['from']}")
        g = meta_guid(dest)
        if mv.get("preserveMetaGuid") and not g:
            errs.append(f"missing meta guid for {mv['to']}")
        print("OK", mv["system"], mv["to"], "guid=", (g or "")[:12])
    for rf in data.get("runtimeFoundations", []):
        if not (ROOT / rf).is_file():
            errs.append(f"missing runtime foundation {rf}")
        else:
            print("OK runtime", rf)
    if errs:
        for e in errs:
            print("FAIL", e)
        return 2
    print("ALL_OK moves=", len(data.get("moves", [])))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
