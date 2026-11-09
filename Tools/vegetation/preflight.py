#!/usr/bin/env python3
"""Preflight for Val Vert grass field SOURCE config (no Unity required)."""
from __future__ import annotations
import argparse, json, math, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CFG = ROOT / "Assets/_Project/Settings/World/Vegetation/grass_field_config.json"
EXPECTED_PACK_ID = "138810"
EXPECTED_SCHEMA = "1"

def load():
    return json.loads(CFG.read_text())

def finite_positive(v) -> bool:
    try:
        f = float(v)
    except (TypeError, ValueError):
        return False
    return math.isfinite(f) and f > 0

def check(cfg: dict, require_pack: bool = False) -> list[str]:
    errs: list[str] = []
    if cfg.get("schemaVersion") != EXPECTED_SCHEMA:
        errs.append(f"schemaVersion {cfg.get('schemaVersion')!r} != {EXPECTED_SCHEMA}")
    for key in ("chunkSize", "parkSpacing", "woodsSpacing"):
        if not finite_positive(cfg.get(key)):
            errs.append(f"{key} must be finite > 0")
    pack = cfg.get("pack")
    if not isinstance(pack, dict):
        errs.append("pack slot missing")
        pack = {}
    if pack.get("assetStoreId") != EXPECTED_PACK_ID:
        errs.append(f"pack.assetStoreId must be {EXPECTED_PACK_ID}")

    mode = cfg.get("materialMode")
    if mode == "legacy_homemade":
        leg = ROOT / (cfg.get("legacyAlbedoPath") or "")
        if not leg.is_file():
            errs.append(f"legacy albedo not on disk (ok in bare clone without Gen binaries): {leg}")
    elif mode == "pack_cutout":
        local = pack.get("localRoot") or ""
        albedo = pack.get("albedoRelative") or ""
        if not local:
            errs.append("pack_cutout requires pack.localRoot")
        elif not (ROOT / local).is_dir() and not Path(local).is_dir():
            errs.append(f"pack localRoot absent: {local}")
        if not albedo:
            errs.append("pack_cutout requires pack.albedoRelative from genuine inspect — do not invent")
        elif local:
            full = Path(local) / albedo
            if not full.is_file() and not (ROOT / local / albedo).is_file():
                errs.append(f"pack albedo missing: {local}/{albedo}")
    else:
        errs.append(f"unknown materialMode {mode}")

    if require_pack:
        # Assert actual pack mode, not merely nonempty paths/status
        if mode != "pack_cutout":
            errs.append("require_pack: materialMode must be pack_cutout (not merely localRoot set)")
        if pack.get("status") not in ("wired", "local_present_unwired"):
            # wired preferred; local_present_unwired allowed only if mode already pack_cutout with files
            if mode != "pack_cutout":
                errs.append("require_pack: pack.status awaiting_acquisition / not pack mode")
        if not (pack.get("localRoot") and pack.get("albedoRelative")):
            errs.append("require_pack: need both localRoot and albedoRelative from genuine inspect")
    return errs

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--require-pack", action="store_true",
                    help="Fail unless materialMode is pack_cutout with real localRoot+albedoRelative")
    ap.add_argument("--allow-missing-legacy-albedo", action="store_true")
    args = ap.parse_args()
    cfg = load()
    errs = check(cfg, require_pack=args.require_pack)
    if args.allow_missing_legacy_albedo:
        errs = [e for e in errs if "legacy albedo not on disk" not in e]
    if errs:
        for e in errs:
            print("FAIL", e)
        return 2
    print("OK materialMode=", cfg["materialMode"], "pack.status=", (cfg.get("pack") or {}).get("status"))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
