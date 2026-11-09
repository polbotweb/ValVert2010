#!/usr/bin/env python3
"""Val Vert generated-data rebuild entry point.

Usage (from repo root):
  python3 Tools/rebuild/rebuild.py list
  python3 Tools/rebuild/rebuild.py preflight [--target NAME]
  python3 Tools/rebuild/rebuild.py rebuild --target NAME [--staging DIR] [--apply]
  python3 Tools/rebuild/rebuild.py determinism --target NAME [--staging DIR]
  python3 Tools/rebuild/rebuild.py verify --target NAME [--staging DIR]
  python3 Tools/rebuild/rebuild.py rebuild-proven [--staging DIR] [--apply]
"""
from __future__ import annotations
import argparse, importlib.util, json, os, shutil, sys, time
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from lib import (  # noqa: E402
    ROOT, load_manifest, preflight_target, compare_outputs, sha256_file,
    staging_dir, write_report, resolve_input,
)

def _versions() -> dict:
    import numpy, PIL
    return {
        "python": sys.version.split()[0],
        "numpy": numpy.__version__,
        "pillow": PIL.__version__,
    }

def cmd_list(_: argparse.Namespace) -> int:
    m = load_manifest()
    for name, t in m["targets"].items():
        print(f"{name:16s}  status={t['status']:28s}  producer={t['producer']}")
    print("--- unsupported ---")
    for k, v in m["unsupported"].items():
        print(f"{k}: {v}")
    return 0

def cmd_preflight(args: argparse.Namespace) -> int:
    m = load_manifest()
    names = [args.target] if args.target else list(m["targets"])
    rc = 0
    for n in names:
        errs = preflight_target(n, m)
        if errs:
            rc = 2
            for e in errs:
                print(f"FAIL {e}")
        else:
            print(f"OK   {n}")
    return rc

def _run_producer(name: str, out_dir: Path, m: dict) -> list[Path]:
    t = m["targets"][name]
    errs = preflight_target(name, m)
    if errs:
        raise SystemExit("preflight failed:\n  " + "\n  ".join(errs))
    out_dir.mkdir(parents=True, exist_ok=True)
    if name == "sign_danger":
        from producers.signs import build
        return build(out_dir)
    if name == "asphalt_ww":
        from producers.asphalt import build_asphalt_ww
        return build_asphalt_ww(out_dir)
    if name == "parking":
        from producers.asphalt import build_parking
        return build_parking(out_dir)
    if name == "facade_atlas":
        from producers.facade_atlas import build
        plaster = resolve_input(t["inputs"][0])
        return build(out_dir, plaster)
    raise SystemExit(f"no producer wiring for {name}")

def _map_staging_to_repo_outputs(name: str, produced: list[Path], m: dict) -> dict[str, Path]:
    """Map produced basenames to repo-relative output paths from manifest."""
    t = m["targets"][name]
    by_base = {p.name: p for p in produced}
    mapping = {}
    for o in t["outputs"]:
        base = Path(o["path"]).name
        if base not in by_base:
            raise FileNotFoundError(f"producer for {name} did not emit {base}; got {list(by_base)}")
        mapping[o["path"]] = by_base[base]
    return mapping

def cmd_rebuild(args: argparse.Namespace) -> int:
    m = load_manifest()
    name = args.target
    if name not in m["targets"]:
        print(f"unknown target {name}", file=sys.stderr)
        return 2
    base = Path(args.staging or m["staging_default"])
    run = staging_dir(base, f"{name}-{int(time.time())}")
    produced = _run_producer(name, run, m)
    mapping = _map_staging_to_repo_outputs(name, produced, m)
    report = {
        "target": name,
        "status": m["targets"][name]["status"],
        "versions": _versions(),
        "staging": str(run),
        "outputs": {k: {"staging": str(v), "sha256": sha256_file(v)} for k, v in mapping.items()},
        "applied": False,
    }
    if args.apply:
        if m["targets"][name]["status"] not in ("proven",):
            print(f"refusing --apply for status={m['targets'][name]['status']}", file=sys.stderr)
            return 2
        for repo_rel, src in mapping.items():
            dst = ROOT / repo_rel
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, dst)
            meta = ROOT / m["targets"][name]["outputs"][[o["path"] for o in m["targets"][name]["outputs"]].index(repo_rel)]["meta"]
            # keep meta untouched
            if not meta.is_file():
                print(f"WARNING missing meta {meta}")
        report["applied"] = True
    write_report(run / "report.json", report)
    print(json.dumps(report, indent=2))
    return 0

def cmd_determinism(args: argparse.Namespace) -> int:
    m = load_manifest()
    name = args.target
    base = Path(args.staging or m["staging_default"]) / f"det-{name}"
    if base.exists():
        shutil.rmtree(base)
    r1 = staging_dir(base, "run1")
    r2 = staging_dir(base, "run2")
    p1 = _map_staging_to_repo_outputs(name, _run_producer(name, r1, m), m)
    p2 = _map_staging_to_repo_outputs(name, _run_producer(name, r2, m), m)
    ok = True
    details = {}
    for rel in p1:
        mode = next(o["compare"] for o in m["targets"][name]["outputs"] if o["path"] == rel)
        # for determinism always use bytes on staging artefacts
        same = sha256_file(p1[rel]) == sha256_file(p2[rel])
        details[rel] = {"equal": same, "sha": sha256_file(p1[rel])}
        ok = ok and same
        print(("OK  " if same else "FAIL"), rel, details[rel]["sha"][:16])
    write_report(base / "determinism.json", {"target": name, "ok": ok, "versions": _versions(), "details": details})
    return 0 if ok else 1

def cmd_verify(args: argparse.Namespace) -> int:
    """Compare staging rebuild (or fresh rebuild) against intended repo assets."""
    m = load_manifest()
    name = args.target
    t = m["targets"][name]
    base = Path(args.staging or m["staging_default"])
    run = staging_dir(base, f"verify-{name}-{int(time.time())}")
    mapping = _map_staging_to_repo_outputs(name, _run_producer(name, run, m), m)
    ok = True
    details = {}
    for o in t["outputs"]:
        rel = o["path"]
        intended = ROOT / rel
        got = mapping[rel]
        if not intended.is_file():
            details[rel] = {"ok": False, "reason": "intended missing (maybe already untracked — rebuild then verify)"}
            ok = False
            print("FAIL", rel, details[rel]["reason"])
            continue
        match, reason = compare_outputs(o["compare"], got, intended)
        details[rel] = {"ok": match, "reason": reason, "compare": o["compare"]}
        ok = ok and match
        print(("OK  " if match else "FAIL"), rel, reason)
    write_report(run / "verify.json", {"target": name, "ok": ok, "status": t["status"], "details": details, "versions": _versions()})
    if t["status"] != "proven" and ok:
        print("NOTE: status is not proven; do not untrack solely from this run")
    return 0 if ok else 1

def cmd_rebuild_proven(args: argparse.Namespace) -> int:
    m = load_manifest()
    rc = 0
    for name, t in m["targets"].items():
        if t["status"] != "proven":
            continue
        ns = argparse.Namespace(target=name, staging=args.staging, apply=args.apply)
        r = cmd_rebuild(ns)
        if r != 0:
            rc = r
    return rc

def main() -> int:
    ap = argparse.ArgumentParser(description="Val Vert generated rebuild pipeline")
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("list")
    p = sub.add_parser("preflight"); p.add_argument("--target")
    p = sub.add_parser("rebuild"); p.add_argument("--target", required=True); p.add_argument("--staging"); p.add_argument("--apply", action="store_true")
    p = sub.add_parser("determinism"); p.add_argument("--target", required=True); p.add_argument("--staging")
    p = sub.add_parser("verify"); p.add_argument("--target", required=True); p.add_argument("--staging")
    p = sub.add_parser("rebuild-proven"); p.add_argument("--staging"); p.add_argument("--apply", action="store_true")
    args = ap.parse_args()
    return {
        "list": cmd_list,
        "preflight": cmd_preflight,
        "rebuild": cmd_rebuild,
        "determinism": cmd_determinism,
        "verify": cmd_verify,
        "rebuild-proven": cmd_rebuild_proven,
    }[args.cmd](args)

if __name__ == "__main__":
    raise SystemExit(main())
