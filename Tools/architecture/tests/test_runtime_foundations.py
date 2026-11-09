#!/usr/bin/env python3
"""Static tests for runtime foundations + migration (no Unity playmode)."""
from __future__ import annotations
import re, subprocess, sys, unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]

class ArchitectureTests(unittest.TestCase):
    def test_migration_verify(self):
        r = subprocess.run([sys.executable, str(ROOT / "Tools/architecture/verify_migration.py")], cwd=ROOT, capture_output=True, text=True)
        self.assertEqual(r.returncode, 0, r.stdout + r.stderr)

    def test_event_bus_api(self):
        t = (ROOT / "Assets/_Project/Runtime/Boot/ValVertEventBus.cs").read_text()
        for s in ("Subscribe", "Unsubscribe", "Publish", "ValVertTimeChanged", "ValVertWeatherChanged"):
            self.assertIn(s, t)

    def test_services_api(self):
        t = (ROOT / "Assets/_Project/Runtime/Boot/ValVertServices.cs").read_text()
        self.assertIn("Register", t)
        self.assertIn("TryGet", t)

    def test_streaming_not_lod_claim(self):
        t = (ROOT / "Assets/_Project/Runtime/World/Streaming/ValVertWorldStreaming.cs").read_text()
        self.assertIn("NOT render-LOD", t)
        self.assertIn("Register", t)
        self.assertIn("SetActive", t)

    def test_bootstrap_wires_env(self):
        t = (ROOT / "Assets/_Project/Runtime/Boot/ValVertRuntimeBootstrap.cs").read_text()
        self.assertIn("ValVertClock", t)
        self.assertIn("ValVertEventBus.Publish", t)
        self.assertIn("ValVertWorldStreaming", t)
        season = (ROOT / "Assets/_Project/Editor/Generators/ValVertSeason.cs").read_text()
        self.assertIn("ValVertRuntimeBootstrap.EnsureOn", season)

    def test_sky_prefab_guids_stable_files(self):
        for rel in (
            "Assets/_Project/Runtime/World/Sky/kloofendal_overcast_puresky_2k.hdr",
            "Assets/_Project/Runtime/World/Vegetation/Prefabs/fern_b.prefab",
        ):
            self.assertTrue((ROOT / rel).is_file(), rel)
            self.assertTrue((ROOT / (rel + ".meta")).is_file(), rel + ".meta")

if __name__ == "__main__":
    unittest.main()
