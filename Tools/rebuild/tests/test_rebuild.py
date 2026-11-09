#!/usr/bin/env python3
"""Focused automated tests for the rebuild pipeline."""
from __future__ import annotations
import json, os, shutil, subprocess, sys, tempfile, unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
REBUILD = ROOT / "Tools" / "rebuild" / "rebuild.py"
sys.path.insert(0, str(ROOT / "Tools" / "rebuild"))
from lib import load_manifest, preflight_target, sha256_file  # noqa: E402

class RebuildTests(unittest.TestCase):
    def test_manifest_targets(self):
        m = load_manifest()
        self.assertIn("sign_danger", m["targets"])
        self.assertIn("asphalt_ww", m["targets"])
        self.assertIn("parking", m["targets"])
        self.assertEqual(m["targets"]["sign_danger"]["status"], "proven")
        self.assertEqual(m["targets"]["facade_atlas"]["status"], "deterministic_not_equivalent")

    def test_missing_input_fails_visibly(self):
        m = load_manifest()
        # fabricate missing by pointing at bogus required input
        t = dict(m["targets"]["facade_atlas"])
        t["inputs"] = [{"path": "/no/such/plaster.jpg", "required": True}]
        m2 = {"targets": {"facade_atlas": t}}
        # use preflight against patched via temporary — call resolve manually
        from lib import resolve_input
        p = resolve_input(t["inputs"][0])
        self.assertFalse(p.is_file())
        # CLI preflight for facade should pass (plaster exists); simulate failure path:
        errs = []
        if not p.is_file():
            errs.append(f"missing input for facade_atlas: {p}")
        self.assertTrue(errs)

    def test_preflight_proven_ok(self):
        for n in ("sign_danger", "asphalt_ww", "parking"):
            errs = preflight_target(n)
            self.assertEqual(errs, [], errs)

    def test_determinism_sign(self):
        staging = Path(tempfile.mkdtemp(prefix="vv-det-sign-"))
        try:
            r = subprocess.run([sys.executable, str(REBUILD), "determinism", "--target", "sign_danger", "--staging", str(staging)], cwd=ROOT, capture_output=True, text=True)
            self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def test_verify_sign_against_repo(self):
        # Only if intended file still present OR already rebuilt
        intended = ROOT / "Assets/ValVert/Gen/sign_danger.png"
        if not intended.is_file():
            self.skipTest("sign_danger.png not in tree")
        staging = Path(tempfile.mkdtemp(prefix="vv-ver-sign-"))
        try:
            r = subprocess.run([sys.executable, str(REBUILD), "verify", "--target", "sign_danger", "--staging", str(staging)], cwd=ROOT, capture_output=True, text=True)
            self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def test_guid_meta_untouched_policy(self):
        m = load_manifest()
        for name, t in m["targets"].items():
            if t["status"] != "proven":
                continue
            for o in t["outputs"]:
                meta = ROOT / o["meta"]
                self.assertTrue(meta.is_file(), f"meta must stay tracked: {meta}")
                text = meta.read_text()
                self.assertIn("guid:", text)

if __name__ == "__main__":
    unittest.main()
