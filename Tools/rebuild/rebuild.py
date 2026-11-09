#!/usr/bin/env python3
"""Val Vert generated-data rebuild entry point.

Usage (from repo root):
  python3 Tools/rebuild/rebuild.py list
  python3 Tools/rebuild/rebuild.py preflight [--target NAME] [--allow-unpinned-env]
  python3 Tools/rebuild/rebuild.py rebuild --target NAME [--staging DIR] [--apply] [--allow-unpinned-env]
  python3 Tools/rebuild/rebuild.py determinism --target NAME [--staging DIR] [--allow-unpinned-env]
  python3 Tools/rebuild/rebuild.py verify --target NAME [--staging DIR] [--allow-unpinned-env]
  python3 Tools/rebuild/rebuild.py rebuild-proven [--staging DIR] [--apply] [--allow-unpinned-env]
"""
from __future__ import annotations
import argparse, json, shutil, sys, time
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from lib import (  # noqa: E402
    ROOT, load_manifest, load_baselines, preflight_target, compare_outputs, sha256_file,
    staging_dir, write_report, resolve_input, current_environment, validate_against_baselines,
    environment_errors,
)

def _versions() -> dict:
    return current_environment()

def cmd_list(_: argparse.Namespace) -> int:
    m = load_manifest()
    for name, t in m["targets"].items():
        print(f"{name:16s}  status={t['status']:28s}  producer={t['producer']}")
    print("--- unsupported ---")
    for k, v in m["unsupported"].items():
        print(f"{k}: {v}")
    print("--- pinned env ---")
    print(json.dumps(load_baselines()["environment"], indent=2))
    return 0

def cmd_preflight(args: argparse.Namespace) -> int:
    m = load_manifest()
    names = [args.target] if args.target else list(m["targets"])
    rc = 0
    for n in names:
        errs = preflight_target(n, m, allow_unpinned_env=args.allow_unpinned_env)
        if errs:
            rc = 2
            for e in errs:
                print(f"FAIL {e}")
        else:
            print(f"OK   {n}")
    return rc

def _run_producer(name: str, out_dir: Path, m: dict, allow_unpinned_env: bool) -> list[Path]:
    t = m["targets"][name]
    errs = preflight_target(name, m, allow_unpinned_env=allow_unpinned_env)
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
    t = m["targets"][name]
    by_base = {p.name: p for p in produced}
    mapping = {}
    for o in t["outputs"]:
        base = Path(o["path"]).name
        if base not in by_base:
            raise FileNotFoundError(f"producer for {name} did not emit {base}; got {list(by_base)}")
        mapping[o["path"]] = by_base[base]
    return mapping

def _atomic_apply(name: str, mapping: dict[str, Path], m: dict) -> None:
    """Validate ALL outputs vs baselines, then copy all; on any failure copy nothing."""
    t = m["targets"][name]
    if t["status"] != "proven":
        raise SystemExit(f"refusing apply for status={t['status']}")
    results = {}
    ok = True
    for repo_rel, src in mapping.items():
        match, reason = validate_against_baselines(repo_rel, src)
        results[repo_rel] = {"ok": match, "reason": reason}
        ok = ok and match
    if not ok:
        raise SystemExit(
            "baseline validation failed; refusing apply (live outputs unchanged):\n"
            + "\n".join(f"  {k}: {v}" for k, v in results.items())
        )
    # all ok — copy
    for repo_rel, src in mapping.items():
        dst = ROOT / repo_rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)
        meta = None
        for o in t["outputs"]:
            if o["path"] == repo_rel:
                meta = ROOT / o["meta"]
                break
        if meta and not meta.is_file():
            print(f"WARNING missing meta {meta}")

def cmd_rebuild(args: argparse.Namespace) -> int:
    m = load_manifest()
    name = args.target
    if name not in m["targets"]:
        print(f"unknown target {name}", file=sys.stderr)
        return 2
    base = Path(args.staging or m["staging_default"])
    run = staging_dir(base, f"{name}-{int(time.time())}")
    produced = _run_producer(name, run, m, args.allow_unpinned_env)
    mapping = _map_staging_to_repo_outputs(name, produced, m)
    baseline_check = {rel: validate_against_baselines(rel, src) for rel, src in mapping.items()}
    report = {
        "target": name,
        "status": m["targets"][name]["status"],
        "versions": _versions(),
        "staging": str(run),
        "outputs": {k: {"staging": str(v), "sha256": sha256_file(v)} for k, v in mapping.items()},
        "baseline_check": {k: {"ok": v[0], "reason": v[1]} for k, v in baseline_check.items()},
        "applied": False,
    }
    if args.apply:
        try:
            _atomic_apply(name, mapping, m)
            report["applied"] = True
        except SystemExit as e:
            write_report(run / "report.json", report)
            print(json.dumps(report, indent=2))
            print(str(e), file=sys.stderr)
            return 2
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
    p1 = _map_staging_to_repo_outputs(name, _run_producer(name, r1, m, args.allow_unpinned_env), m)
    p2 = _map_staging_to_repo_outputs(name, _run_producer(name, r2, m, args.allow_unpinned_env), m)
    ok = True
    details = {}
    for rel in p1:
        same = sha256_file(p1[rel]) == sha256_file(p2[rel])
        details[rel] = {"equal": same, "sha": sha256_file(p1[rel])}
        ok = ok and same
        print(("OK  " if same else "FAIL"), rel, details[rel]["sha"][:16])
    write_report(base / "determinism.json", {"target": name, "ok": ok, "versions": _versions(), "details": details})
    return 0 if ok else 1

def cmd_verify(args: argparse.Namespace) -> int:
    """Rebuild to staging and compare against committed expected_baselines (primary),
    and against on-disk intended file when present (secondary)."""
    m = load_manifest()
    name = args.target
    t = m["targets"][name]
    base = Path(args.staging or m["staging_default"])
    run = staging_dir(base, f"verify-{name}-{int(time.time())}")
    mapping = _map_staging_to_repo_outputs(name, _run_producer(name, run, m, args.allow_unpinned_env), m)
    ok = True
    details = {}
    for o in t["outputs"]:
        rel = o["path"]
        got = mapping[rel]
        match_b, reason_b = validate_against_baselines(rel, got)
        details[rel] = {"baseline_ok": match_b, "baseline_reason": reason_b}
        if not match_b:
            ok = False
            print("FAIL baseline", rel, reason_b)
        else:
            print("OK   baseline", rel, reason_b)
        intended = ROOT / rel
        if intended.is_file():
            match_i, reason_i = compare_outputs(o["compare"], got, intended)
            details[rel]["intended_ok"] = match_i
            details[rel]["intended_reason"] = reason_i
            if not match_i:
                ok = False
                print("FAIL intended", rel, reason_i)
    write_report(run / "verify.json", {"target": name, "ok": ok, "status": t["status"], "details": details, "versions": _versions()})
    return 0 if ok else 1

def cmd_rebuild_proven(args: argparse.Namespace) -> int:
    m = load_manifest()
    rc = 0
    for name, t in m["targets"].items():
        if t["status"] != "proven":
            continue
        ns = argparse.Namespace(target=name, staging=args.staging, apply=args.apply, allow_unpinned_env=args.allow_unpinned_env)
        r = cmd_rebuild(ns)
        if r != 0:
            rc = r
    return rc

def main() -> int:
    ap = argparse.ArgumentParser(description="Val Vert generated rebuild pipeline")
    sub = ap.add_subparsers(dest="cmd", required=True)

    def add_env(p):
        p.add_argument("--allow-unpinned-env", action="store_true",
                       help="UNSAFE: skip pinned Python/numpy/Pillow/font checks. Not used by default rebuild-proven.")

    sub.add_parser("list")
    p = sub.add_parser("preflight"); p.add_argument("--target"); add_env(p)
    p = sub.add_parser("rebuild"); p.add_argument("--target", required=True); p.add_argument("--staging"); p.add_argument("--apply", action="store_true"); add_env(p)
    p = sub.add_parser("determinism"); p.add_argument("--target", required=True); p.add_argument("--staging"); add_env(p)
    p = sub.add_parser("verify"); p.add_argument("--target", required=True); p.add_argument("--staging"); add_env(p)
    p = sub.add_parser("rebuild-proven"); p.add_argument("--staging"); p.add_argument("--apply", action="store_true"); add_env(p)
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
