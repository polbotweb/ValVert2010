"""Rebuild facade_atlas.png (deterministic, not yet equivalent to tracked atlas)."""
from __future__ import annotations
from pathlib import Path
import importlib.util

def build(out_dir: Path, plaster: Path) -> list[Path]:
    out_dir.mkdir(parents=True, exist_ok=True)
    # Execute legacy generator with redirected paths via a thin wrapper
    from PIL import Image, ImageDraw
    import numpy as np, random, shutil
    # Ensure plaster available under a temp G that only contains needed input
    gdir = out_dir / "_in"
    gdir.mkdir(exist_ok=True)
    dst = gdir / "grey_plaster_diff_1k.jpg"
    if not dst.exists():
        shutil.copy2(plaster, dst)
    # Import by loading legacy file with patched G — use local reimplementation call
    # Prefer calling Tools/generators/facade_atlas logic by exec with patched globals
    legacy = Path(__file__).resolve().parents[2] / "generators" / "facade_atlas.py"
    code = legacy.read_text()
    code = code.replace("G='/workspace/ValVertGame/Assets/ValVert/Gen/'", f"G=r'{gdir.as_posix()}/'")
    code = code.replace("A.save(G+'facade_atlas.png')", f"A.save(r'{(out_dir / 'facade_atlas.png').as_posix()}')")
    ns = {"__name__": "__main__"}
    exec(compile(code, str(legacy), "exec"), ns)
    out = out_dir / "facade_atlas.png"
    if not out.is_file():
        raise RuntimeError("facade_atlas producer failed to write output")
    return [out]
