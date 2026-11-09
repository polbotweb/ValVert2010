#!/usr/bin/env python3
"""Focused tests for grass field config + preflight (no Unity, no invented pack assets)."""
from __future__ import annotations
import json, shutil, subprocess, sys, tempfile, unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
PRE = ROOT / "Tools/vegetation/preflight.py"
CFG = ROOT / "Assets/_Project/Generated/Vegetation/grass_field_config.json"
sys.path.insert(0, str(ROOT / "Tools/vegetation"))
import preflight as PF  # noqa

class GrassConfigTests(unittest.TestCase):
    def test_config_schema_and_pack_id(self):
        cfg = json.loads(CFG.read_text())
        self.assertEqual(cfg["schemaVersion"], "1")
        self.assertEqual(cfg["seed"], 5)
        self.assertEqual(cfg["chunkSize"], 32.0)
        self.assertEqual(cfg["pack"]["assetStoreId"], "138810")
        self.assertEqual(cfg["materialMode"], "legacy_homemade")
        self.assertEqual(cfg["pack"]["status"], "awaiting_acquisition")
        self.assertFalse(cfg["pack"].get("albedoRelative"), "must not invent pack texture path")
        self.assertFalse(cfg["pack"].get("localRoot"), "must not invent localRoot before acquisition")

    def test_preflight_legacy_allows_missing_albedo_flag(self):
        r = subprocess.run([sys.executable, str(PRE), "--allow-missing-legacy-albedo"], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(r.returncode, 0, r.stdout + r.stderr)

    def test_require_pack_fails_while_awaiting(self):
        r = subprocess.run([sys.executable, str(PRE), "--require-pack", "--allow-missing-legacy-albedo"], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(r.returncode, 2, r.stdout)
        self.assertIn("require_pack", r.stdout)

    def test_pack_cutout_without_albedo_fails(self):
        cfg = json.loads(CFG.read_text())
        cfg["materialMode"] = "pack_cutout"
        cfg["pack"] = dict(cfg["pack"])
        cfg["pack"]["localRoot"] = "Assets/ThirdParty/DoesNotExist"
        cfg["pack"]["albedoRelative"] = ""  # invent nothing
        errs = PF.check(cfg)
        self.assertTrue(any("albedoRelative" in e for e in errs), errs)

    def test_woods_delegates_to_grass_field(self):
        woods = (ROOT / "Assets/_Project/Editor/Generators/ValVertWoods.cs").read_text()
        self.assertIn("ValVertGrassField.Build", woods)
        self.assertNotIn("self-made crossed-quad grass clumps, chunked", woods)
        field = (ROOT / "Assets/_Project/Editor/Generators/Vegetation/ValVertGrassField.cs").read_text()
        self.assertIn("pack_cutout", field)
        self.assertIn("138810", json.loads(CFG.read_text())["pack"]["assetStoreUrl"])

if __name__ == "__main__":
    unittest.main()
