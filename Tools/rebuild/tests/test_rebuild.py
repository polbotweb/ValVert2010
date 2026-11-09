#!/usr/bin/env python3
"""Focused automated tests for the rebuild pipeline (pins + baselines + atomic apply)."""
from __future__ import annotations
import json, os, shutil, subprocess, sys, tempfile, unittest
from pathlib import Path
from unittest import mock

ROOT = Path(__file__).resolve().parents[3]
REBUILD = ROOT / "Tools" / "rebuild" / "rebuild.py"
sys.path.insert(0, str(ROOT / "Tools" / "rebuild"))
from lib import (  # noqa: E402
    load_manifest, load_baselines, preflight_target, sha256_file,
    environment_errors, validate_against_baselines, current_environment,
)

class RebuildTests(unittest.TestCase):
    def test_manifest_targets(self):
        m = load_manifest()
        self.assertIn("sign_danger", m["targets"])
        self.assertIn("asphalt_ww", m["targets"])
        self.assertIn("parking", m["targets"])
        self.assertEqual(m["targets"]["sign_danger"]["status"], "proven")
        self.assertEqual(m["targets"]["facade_atlas"]["status"], "deterministic_not_equivalent")
        self.assertTrue((ROOT / "Tools/rebuild/requirements.txt").is_file())
        self.assertTrue((ROOT / "Tools/rebuild/expected_baselines.json").is_file())

    def test_baselines_cover_seven_proven_outputs(self):
        m = load_manifest()
        b = load_baselines()
        outs = []
        for name, t in m["targets"].items():
            if t["status"] != "proven":
                continue
            for o in t["outputs"]:
                outs.append(o["path"])
                self.assertIn(o["path"], b["outputs"], o["path"])
        self.assertEqual(len(outs), 7)
        self.assertEqual(b["environment"]["numpy"], "2.5.4")
        self.assertEqual(b["environment"]["pillow"], "13.0.0")

    def test_preflight_proven_ok(self):
        for n in ("sign_danger", "asphalt_ww", "parking"):
            errs = preflight_target(n)
            self.assertEqual(errs, [], errs)

    def test_env_mismatch_fails_preflight(self):
        # Corrupt env check by mocking current_environment
        import lib as L
        fake = current_environment()
        fake = dict(fake)
        fake["numpy"] = "0.0.0"
        with mock.patch.object(L, "current_environment", return_value=fake):
            errs = L.environment_errors(allow_unpinned=False)
            self.assertTrue(any("numpy" in e for e in errs), errs)
            errs2 = L.preflight_target("sign_danger", allow_unpinned_env=False)
            self.assertTrue(any("numpy" in e for e in errs2), errs2)
            # override skips
            self.assertEqual(L.environment_errors(allow_unpinned=True), [])

    def test_missing_input_fails_preflight(self):
        m = load_manifest()
        t = dict(m["targets"]["facade_atlas"])
        t["inputs"] = [{"path": "/no/such/plaster.jpg", "required": True}]
        m2 = {"targets": {"facade_atlas": t}}
        errs = preflight_target("facade_atlas", m2)
        self.assertTrue(any("missing input" in e for e in errs), errs)

    def test_baseline_mismatch_rejects(self):
        staging = Path(tempfile.mkdtemp(prefix="vv-bad-"))
        try:
            bad = staging / "sign_danger.png"
            bad.write_bytes(b"not-a-real-png")
            ok, reason = validate_against_baselines("Assets/ValVert/Gen/sign_danger.png", bad)
            self.assertFalse(ok)
            self.assertIn("!=", reason)
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def test_atomic_no_partial_apply_on_mismatch(self):
        """If baseline validation fails, live targets must stay unchanged."""
        m = load_manifest()
        # Pick asphalt_ww_diff as canary; snapshot hash if present
        canary_rel = "Assets/ValVert/Ground/asphalt_ww_diff.jpg"
        canary = ROOT / canary_rel
        before = sha256_file(canary) if canary.is_file() else None
        staging = Path(tempfile.mkdtemp(prefix="vv-atomic-"))
        try:
            # Build a fake mapping with one good-looking but wrong hash file for ALL outputs
            # so apply refuses entirely.
            t = m["targets"]["asphalt_ww"]
            mapping = {}
            for o in t["outputs"]:
                p = staging / Path(o["path"]).name
                p.write_bytes(b"\xff\xd8\xff\xe0" + b"BOGUSJPEG" * 100)  # fake jpeg-ish
                mapping[o["path"]] = p
            # Import the private helper via rebuild module
            sys.path.insert(0, str(ROOT / "Tools" / "rebuild"))
            import rebuild as R
            with self.assertRaises(SystemExit) as cm:
                R._atomic_apply("asphalt_ww", mapping, m)
            self.assertIn("baseline validation failed", str(cm.exception))
            after = sha256_file(canary) if canary.is_file() else None
            self.assertEqual(before, after, "partial apply mutated live output")
            # metas untouched
            for o in t["outputs"]:
                meta = ROOT / o["meta"]
                self.assertTrue(meta.is_file())
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def test_determinism_sign(self):
        staging = Path(tempfile.mkdtemp(prefix="vv-det-sign-"))
        try:
            r = subprocess.run([sys.executable, str(REBUILD), "determinism", "--target", "sign_danger", "--staging", str(staging)], cwd=ROOT, capture_output=True, text=True)
            self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
        finally:
            shutil.rmtree(staging, ignore_errors=True)

    def test_verify_sign_against_baselines(self):
        staging = Path(tempfile.mkdtemp(prefix="vv-ver-sign-"))
        try:
            r = subprocess.run([sys.executable, str(REBUILD), "verify", "--target", "sign_danger", "--staging", str(staging)], cwd=ROOT, capture_output=True, text=True)
            self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
            self.assertIn("OK   baseline", r.stdout)
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
