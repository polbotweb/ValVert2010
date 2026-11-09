#!/usr/bin/env python3
from __future__ import annotations
import json, subprocess, sys, unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
PRE = ROOT / "Tools/vegetation/preflight.py"
CFG = ROOT / "Assets/_Project/Settings/World/Vegetation/grass_field_config.json"
sys.path.insert(0, str(ROOT / "Tools/vegetation"))
import preflight as PF  # noqa

class GrassConfigTests(unittest.TestCase):
    def test_config_is_under_settings_not_generated(self):
        self.assertTrue(CFG.is_file())
        self.assertIn("/Settings/World/Vegetation/", str(CFG))
        self.assertFalse((ROOT / "Assets/_Project/Generated/Vegetation/grass_field_config.json").exists())

    def test_config_schema_and_pack_id(self):
        cfg = json.loads(CFG.read_text())
        self.assertEqual(cfg["schemaVersion"], "1")
        self.assertEqual(cfg["seed"], 5)
        self.assertEqual(cfg["chunkSize"], 32.0)
        self.assertEqual(cfg["pack"]["assetStoreId"], "138810")
        self.assertEqual(cfg["materialMode"], "legacy_homemade")
        self.assertEqual(cfg["pack"]["status"], "awaiting_acquisition")
        self.assertFalse(cfg["pack"].get("albedoRelative"))
        self.assertFalse(cfg["pack"].get("localRoot"))

    def test_preflight_legacy_ok(self):
        r = subprocess.run([sys.executable, str(PRE), "--allow-missing-legacy-albedo"], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(r.returncode, 0, r.stdout + r.stderr)

    def test_require_pack_fails_unless_pack_cutout(self):
        r = subprocess.run([sys.executable, str(PRE), "--require-pack", "--allow-missing-legacy-albedo"], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(r.returncode, 2, r.stdout)
        self.assertIn("pack_cutout", r.stdout)

    def test_require_pack_not_fooled_by_localroot_alone(self):
        cfg = json.loads(CFG.read_text())
        cfg["materialMode"] = "legacy_homemade"
        cfg["pack"] = dict(cfg["pack"])
        cfg["pack"]["localRoot"] = "Assets/ThirdParty/Fake"
        cfg["pack"]["albedoRelative"] = "x.png"
        cfg["pack"]["status"] = "local_present_unwired"
        errs = PF.check(cfg, require_pack=True)
        self.assertTrue(any("pack_cutout" in e for e in errs), errs)

    def test_pack_cutout_without_albedo_no_keyerror(self):
        cfg = json.loads(CFG.read_text())
        cfg["materialMode"] = "pack_cutout"
        cfg["pack"] = {"assetStoreId": "138810", "localRoot": "Assets/ThirdParty/X", "albedoRelative": ""}
        errs = PF.check(cfg)
        self.assertTrue(any("albedoRelative" in e for e in errs), errs)

    def test_woods_delegates_and_cs_config_path(self):
        woods = (ROOT / "Assets/_Project/Editor/Generators/ValVertWoods.cs").read_text()
        self.assertIn("ValVertGrassField.Build", woods)
        field = (ROOT / "Assets/_Project/Editor/Generators/Vegetation/ValVertGrassField.cs").read_text()
        self.assertIn("Settings/World/Vegetation/grass_field_config.json", field)
        self.assertIn("ExpectedPackId", field)
        self.assertIn("FinitePositive", field)
        self.assertNotIn("ShotClear(px, pz) && new Vector2(px, pz).magnitude < 0", field)

if __name__ == "__main__":
    unittest.main()
