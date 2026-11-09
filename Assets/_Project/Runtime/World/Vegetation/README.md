# Runtime / World / Vegetation

Field grass is **editor-generated** (chunked meshes under `Assets/ValVert/Gen/grass_*.asset`), not a runtime spawner.

- Generator: `Assets/_Project/Editor/Generators/Vegetation/ValVertGrassField.cs`
- Config: `Assets/_Project/Generated/Vegetation/grass_field_config.json`
- Call chain: `ValVertSetup.Build` → `ValVertWoods.Grass` → `ValVertGrassField.Build`

Pack 138810 (Grass Flowers, 2D textures) slots into config `pack_*` once the genuine local package is inspected. Geometry/batching stays necessary.
