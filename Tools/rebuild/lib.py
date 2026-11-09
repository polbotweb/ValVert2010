"""Shared helpers for Val Vert rebuild pipeline."""
from __future__ import annotations
import hashlib, json, os, shutil, sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]  # repo root
MANIFEST_PATH = Path(__file__).resolve().parent / "manifest.json"

def load_manifest() -> dict[str, Any]:
    return json.loads(MANIFEST_PATH.read_text())

def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

def resolve_input(spec: dict[str, Any], root: Path = ROOT) -> Path:
    p = Path(spec["path"])
    if not p.is_absolute():
        p = root / p
    return p

def preflight_target(name: str, m: dict[str, Any] | None = None) -> list[str]:
    m = m or load_manifest()
    if name not in m["targets"]:
        return [f"unknown target: {name}"]
    t = m["targets"][name]
    errs = []
    for inp in t.get("inputs", []):
        p = resolve_input(inp)
        if inp.get("required", True) and not p.is_file():
            errs.append(f"missing input for {name}: {p}")
    prod = ROOT / t["producer"]
    if not prod.is_file():
        errs.append(f"missing producer: {prod}")
    return errs

def compare_outputs(mode: str, a: Path, b: Path) -> tuple[bool, str]:
    if not a.is_file() or not b.is_file():
        return False, f"missing file a={a.exists()} b={b.exists()}"
    if mode == "bytes":
        ok = sha256_file(a) == sha256_file(b)
        return ok, "bytes" if ok else f"sha {sha256_file(a)[:12]}!={sha256_file(b)[:12]}"
    if mode == "pixels_rgb":
        from PIL import Image
        import numpy as np
        ia = np.asarray(Image.open(a).convert("RGB"))
        ib = np.asarray(Image.open(b).convert("RGB"))
        if ia.shape != ib.shape:
            return False, f"shape {ia.shape}!={ib.shape}"
        ok = bool((ia == ib).all())
        return ok, "pixels_rgb" if ok else "pixel mismatch"
    return False, f"unknown compare mode {mode}"

def staging_dir(base: Path, run_id: str) -> Path:
    d = base / run_id
    d.mkdir(parents=True, exist_ok=True)
    return d

def write_report(path: Path, data: dict[str, Any]) -> None:
    path.write_text(json.dumps(data, indent=2, sort_keys=True) + "\n")
