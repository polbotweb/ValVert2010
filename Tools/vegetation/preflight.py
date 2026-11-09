#!/usr/bin/env python3
"""Preflight for Val Vert grass field config (no Unity required).

Does NOT claim deterministic grass rebuild. Fails pack_cutout mode when pack
paths are empty/missing. Refuses invented pack texture names.
"""
from __future__ import annotations
import argparse, json, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CFG = ROOT / "Assets/_Project/Generated/Vegetation/grass_field_config.json"

REQUIRED = [
    "schemaVersion", "materialMode", "legacyAlbedoPath", "rootName",
    "meshAssetPrefix", "materialName", "seed", "chunkSize", "pack",
]

def load():
    return json.loads(CFG.read_text())

def check(cfg: dict, require_pack: bool = False) -> list[str]:
    errs = []
    for k in REQUIRED:
        if k not in cfg:
            errs.append(f"missing key {k}")
    pack = cfg.get("pack") or {}
    if pack.get("assetStoreId") != "138810":
        errs.append("pack.assetStoreId must be 138810 (authoritative user pack)")
    mode = cfg.get("materialMode")
    if mode == "legacy_homemade":
        leg = ROOT / cfg.get("legacyAlbedoPath", "")
        # Texture may be absent in bare clone; warn but only error if require_pack false and file expected tracked
        if not leg.is_file():
            errs.append(f"legacy albedo not on disk (ok in bare clone without Gen binaries): {leg}")
    elif mode == "pack_cutout":
        if not pack.get("localRoot"):
            errs.append("pack_cutout requires pack.localRoot (official path not set yet)")
        if not pack.get("albedoRelative"):
            errs.append("pack_cutout requires pack.albedoRelative from genuine inspect — do not invent")
        else:
            full = ROOT / pack["localRoot"] / pack["albedoRelative"]
            if not full.is_file():
                errs.append(f"pack albedo missing: {full}")
    else:
        errs.append(f"unknown materialMode {mode}")
    if require_pack:
        if pack.get("status") == "awaiting_acquisition" or not pack.get("localRoot"):
            errs.append("require_pack: Asset Store 138810 local package not configured")
    # Never claim grass byte-equivalence
    if cfg.get("notes") and "deterministic" in cfg.get("notes", "").lower() and "proven" in cfg.get("notes", "").lower():
        errs.append("config must not claim proven deterministic grass without tests")
    return errs

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--require-pack", action="store_true",
                    help="Fail unless pack 138810 localRoot is configured (integration gate)")
    ap.add_argument("--allow-missing-legacy-albedo", action="store_true",
                    help="Ignore missing legacy grass_blades.png (bare clone)")
    args = ap.parse_args()
    cfg = load()
    errs = check(cfg, require_pack=args.require_pack)
    if args.allow_missing_legacy_albedo:
        errs = [e for e in errs if "legacy albedo not on disk" not in e]
    if errs:
        for e in errs:
            print("FAIL", e)
        return 2
    print("OK materialMode=", cfg["materialMode"], "pack.status=", cfg["pack"].get("status"))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
