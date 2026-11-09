"""Rebuild asphalt_ww (Ground) and parking (Gen) textures."""
from __future__ import annotations
from pathlib import Path
import numpy as np
from PIL import Image

def field(h, w, beta, seed):
    r = np.random.default_rng(seed)
    f = np.fft.fft2(r.standard_normal((h, w)))
    ky = np.fft.fftfreq(h)[:, None]
    kx = np.fft.fftfreq(w)[None, :]
    k = np.sqrt(kx * kx + ky * ky)
    k[0, 0] = 1
    g = np.real(np.fft.ifft2(f / k**beta))
    return (g - g.mean()) / g.std()

def asphalt(h, w, seed, lines=None):
    big = field(h, w, 1.6, seed)
    mid = field(h, w, 1.0, seed + 1)
    fine = field(h, w, 0.25, seed + 2)
    r = np.random.default_rng(seed + 3)
    stones = (fine > 1.6).astype(float) * r.uniform(0.6, 1, (h, w))
    base = 0.105 + 0.014 * big + 0.008 * mid + 0.008 * fine
    alb = base + stones * 0.05 - (fine < -1.9) * 0.03
    alb = np.clip(alb, 0.04, 0.35)
    cr = field(h, w, 1.3, seed + 4)
    crack = (np.abs(cr) < 0.025) & (field(h, w, 1.8, seed + 5) > 0.9)
    alb[crack] *= 0.45
    rough = np.clip(0.18 + 0.10 * mid - 0.08 * stones + 0.1 * crack + 0.05 * (big > 1.2), 0.02, 0.6)
    height = 0.6 * fine + 0.3 * stones - 2.5 * crack + 0.2 * mid
    col = np.stack([alb * 0.98, alb * 0.98, alb * 1.0], -1)
    if lines is not None:
        m, wear = lines(h, w)
        wn = np.clip(0.75 + 0.25 * field(h, w, 0.8, seed + 6), 0, 1) * (1 - 0.5 * (fine < -0.8))
        a = (m * wn)[..., None]
        col = col * (1 - a) + np.array([0.62, 0.62, 0.59]) * a
        rough = rough * (1 - m) + 0.25 * m
        height = height * (1 - 0.6 * m)
    gy, gx = np.gradient(height)
    s = 2.0
    n = np.stack([-gx * s, -gy * s, np.ones_like(gx)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return np.clip(col, 0, 1), n, rough

def _save_triple(prefix: Path, col, n, rough, albedo_png: bool = False):
    outs = []
    if albedo_png:
        p = Path(str(prefix) + ".png")
        Image.fromarray((col ** (1 / 2.2) * 255).astype(np.uint8)).save(p)
        outs.append(p)
    else:
        p = Path(str(prefix) + "_diff.jpg")
        Image.fromarray((col ** (1 / 2.2) * 255).astype(np.uint8)).save(p, quality=90)
        outs.append(p)
    p = Path(str(prefix) + "_nor.jpg")
    Image.fromarray(((n * 0.5 + 0.5) * 255).astype(np.uint8)).save(p, quality=92)
    outs.append(p)
    p = Path(str(prefix) + "_ms.png")
    a = np.zeros(col.shape[:2] + (4,), np.uint8)
    a[..., 3] = (rough * 255).astype(np.uint8)
    Image.fromarray(a, "RGBA").save(p)
    outs.append(p)
    return outs

def parking_lines(h, w):
    m = np.zeros((h, w))
    pw = int(0.12 / 2.5 * w)
    m[: int(4.6 / 5 * h), : pw // 2 + 1] = 1
    m[: int(4.6 / 5 * h), w - pw // 2 :] = 1
    return m, None

def build_asphalt_ww(out_dir: Path) -> list[Path]:
    out_dir.mkdir(parents=True, exist_ok=True)
    c, n, r = asphalt(2048, 2048, 11)
    return _save_triple(out_dir / "asphalt_ww", c, n, r, albedo_png=False)

def build_parking(out_dir: Path) -> list[Path]:
    out_dir.mkdir(parents=True, exist_ok=True)
    c, n, r = asphalt(2048, 1024, 21, parking_lines)
    # Historical albedo was JPEG-encoded then stored as PNG of the decoded pixels.
    # Reproduce that path so parking.png matches the intended tracked asset.
    outs = []
    tmp = out_dir / "_parking_diff.jpg"
    Image.fromarray((c ** (1 / 2.2) * 255).astype(np.uint8)).save(tmp, quality=90)
    alb = Image.open(tmp).convert("RGB")
    p = out_dir / "parking.png"
    alb.save(p)
    tmp.unlink(missing_ok=True)
    outs.append(p)
    p = out_dir / "parking_nor.jpg"
    Image.fromarray(((n * 0.5 + 0.5) * 255).astype(np.uint8)).save(p, quality=92)
    outs.append(p)
    p = out_dir / "parking_ms.png"
    a = np.zeros(c.shape[:2] + (4,), np.uint8)
    a[..., 3] = (r * 255).astype(np.uint8)
    Image.fromarray(a, "RGBA").save(p)
    outs.append(p)
    return outs
