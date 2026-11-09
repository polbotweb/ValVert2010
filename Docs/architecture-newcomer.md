# Val Vert — orientation (≈10 minutes)

## What this project is
Unity 6 URP recreation of Seynod “Val Vert” (2010). Free assets only; OSM/IGN credits.

## Where things live
| Area | Path |
|---|---|
| Playable systems | `Assets/_Project/Runtime/` (World, Gameplay, Characters…) |
| Editor generators / validation | `Assets/_Project/Editor/{Generators,Validation,Tools,Debug}/` |
| **Source** config | `Assets/_Project/Settings/` (not Generated) |
| Generated rebuild pipeline | `Tools/rebuild/` + `Docs/generated-rebuild.md` |
| Migration ledger | `Assets/_Project/Settings/Architecture/migration_manifest.json` |
| Historical art still co-located | `Assets/ValVert/` (Mats/Ground/Gen…) — migrating by system |
| Vendor (ignored) | `Assets/ThirdParty/`, `InnerverseInteractive/`, `Realistic Tree/` |

## Runtime foundations (real, minimal)
- `ValVertEventBus` — time / season / weather events
- `ValVertServices` — register clock, seasons, weather, streaming
- `ValVertRuntimeBootstrap` — wired from `ValVertEnvWire`
- `ValVertWorldStreaming` — **region** activate/deactivate by distance (not LOD)

## Rebuild proven today
`sign_danger`, `asphalt_ww`, `parking` only. Grass field generator exists but pack 138810 not wired; not byte-proven.

## Don’t
- Commit Asset Store binaries or user photos
- Force-push / hard-reset
- Claim empty folders are implemented systems
