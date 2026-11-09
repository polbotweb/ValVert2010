"""Shared helpers for Val Vert rebuild pipeline."""
from __future__ import annotations
import hashlib, json, os, sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]  # repo root
MANIFEST_PATH = Path(__file__).resolve().parent / "manifest.json"
BASELINES_PATH = Path(__file__).resolve().parent / "expected_baselines.json"
REQUIREMENTS_PATH = Path(__file__).resolve().parent / "requirements.txt"

def load_manifest() -> dict[str, Any]:
    return json.loads(MANIFEST_PATH.read_text())

def load_baselines() -> dict[str, Any]:
    return json.loads(BASELINES_PATH.read_text())

def sha256_file(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

def sha256_bytes(b: bytes) -> str:
    return hashlib.sha256(b).hexdigest()

def resolve_input(spec: dict[str, Any], root: Path = ROOT) -> Path:
    p = Path(spec["path"])
    if not p.is_absolute():
        p = root / p
    return p

def pixel_digest_rgb(path: Path) -> dict[str, Any]:
    from PIL import Image
    im = Image.open(path).convert("RGB")  # alpha dropped intentionally
    raw = im.tobytes()
    return {
        "mode": "RGB",
        "width": im.size[0],
        "height": im.size[1],
        "alpha_policy": "convert('RGB') drops any alpha; digest is SHA256 of contiguous RGB byte buffer row-major",
        "sha256_rgb_bytes": sha256_bytes(raw),
    }

def current_environment() -> dict[str, Any]:
    import numpy, PIL
    font = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf")
    return {
        "python": f"{sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}",
        "python_major": sys.version_info.major,
        "python_minor": sys.version_info.minor,
        "numpy": numpy.__version__,
        "pillow": PIL.__version__,
        "font_path": str(font),
        "font_sha256": sha256_file(font) if font.is_file() else None,
        "font_present": font.is_file(),
    }

def environment_errors(allow_unpinned: bool = False) -> list[str]:
    """Return human-readable errors if host env cannot claim byte-equivalence."""
    if allow_unpinned:
        return []
    base = load_baselines()["environment"]
    cur = current_environment()
    errs = []
    if cur["python_major"] != base["python"]["major"] or cur["python_minor"] != base["python"]["minor"]:
        errs.append(
            f"Python {cur['python']} != pinned {base['python']['major']}.{base['python']['minor']}.x "
            f"(install matching interpreter; see Docs/generated-rebuild.md)"
        )
    if cur["numpy"] != base["numpy"]:
        errs.append(f"numpy {cur['numpy']} != pinned {base['numpy']} (pip install -r Tools/rebuild/requirements.txt)")
    if cur["pillow"] != base["pillow"]:
        errs.append(f"Pillow {cur['pillow']} != pinned {base['pillow']} (pip install -r Tools/rebuild/requirements.txt)")
    if not cur["font_present"]:
        errs.append(f"missing font {cur['font_path']}")
    elif cur["font_sha256"] != base["font_dejavu_sans_bold_sha256"]:
        errs.append(
            f"font checksum {cur['font_sha256']} != pinned {base['font_dejavu_sans_bold_sha256']}"
        )
    return errs

def preflight_target(name: str, m: dict[str, Any] | None = None, allow_unpinned_env: bool = False) -> list[str]:
    m = m or load_manifest()
    if name not in m["targets"]:
        return [f"unknown target: {name}"]
    t = m["targets"][name]
    errs = []
    errs.extend(environment_errors(allow_unpinned=allow_unpinned_env))
    for inp in t.get("inputs", []):
        p = resolve_input(inp)
        if inp.get("required", True) and not p.is_file():
            errs.append(f"missing input for {name}: {p}")
            continue
        # optional input checksum recording when declared on baseline env-adjacent inputs
        if p.is_file() and inp.get("sha256"):
            got = sha256_file(p)
            if got != inp["sha256"]:
                errs.append(f"input checksum mismatch for {p}: {got} != {inp['sha256']}")
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
        da = pixel_digest_rgb(a)
        db = pixel_digest_rgb(b)
        ok = da == db
        return ok, "pixels_rgb" if ok else f"pixel digest {da['sha256_rgb_bytes'][:12]}!={db['sha256_rgb_bytes'][:12]}"
    return False, f"unknown compare mode {mode}"

def validate_against_baselines(repo_rel: str, produced: Path) -> tuple[bool, str]:
    """Validate a produced file against committed expected_baselines.json."""
    base = load_baselines()["outputs"].get(repo_rel)
    if not base:
        return False, f"no baseline entry for {repo_rel}"
    if not produced.is_file():
        return False, "produced missing"
    mode = base["compare"]
    if mode == "bytes":
        got = sha256_file(produced)
        ok = got == base["sha256"]
        return ok, "bytes-baseline" if ok else f"sha {got} != expected {base['sha256']}"
    if mode == "pixels_rgb":
        dig = pixel_digest_rgb(produced)
        exp = base["pixel_digest"]
        ok = (
            dig["mode"] == exp["mode"]
            and dig["width"] == exp["width"]
            and dig["height"] == exp["height"]
            and dig["sha256_rgb_bytes"] == exp["sha256_rgb_bytes"]
        )
        # also require file sha256 match when provided (PNG encoding of those pixels)
        if ok and base.get("sha256"):
            got = sha256_file(produced)
            if got != base["sha256"]:
                return False, f"pixels ok but file sha {got} != expected {base['sha256']}"
        return ok, "pixels-baseline" if ok else f"pixel digest mismatch got={dig} expected={exp}"
    return False, f"unknown baseline compare {mode}"

def staging_dir(base: Path, run_id: str) -> Path:
    d = base / run_id
    d.mkdir(parents=True, exist_ok=True)
    return d

def write_report(path: Path, data: dict[str, Any]) -> None:
    path.write_text(json.dumps(data, indent=2, sort_keys=True) + "\n")
