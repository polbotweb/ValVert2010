# Carte d’architecture Val Vert — réorganisation Runtime + Editor

Date : 2026-11-09 (Europe/Paris). Base : `origin/main` après PR #11 (props Entrance).
Objectif : ranger les **systèmes existants** sous `Assets/_Project` sans changer le gameplay ni l’apparence.

## Runtime (joueur / scène)

| Avant | Après | Rôle |
|---|---|---|
| `Assets/Scripts/Environment/ValVertClock.cs` | `Assets/_Project/Runtime/Gameplay/DayNight/` | Horloge, soleil, cycle jour/nuit |
| `…/ValVertSeasons.cs` | `…/Gameplay/Season/` | Phénologie / saisons |
| `…/ValVertWeather.cs` | `…/Gameplay/Weather/` | Météo synthétique, neige/pluie |
| `…/ValVertStreetLights.cs` | `…/World/Props/Lighting/` | Lampadaires (positions depuis meshes existants) |
| `Assets/Scripts/SimpleFPS.cs` | `…/Characters/Player/` | Contrôleur 1re personne |
| `Assets/Scripts/ValVertSmoke.cs` | `…/Gameplay/QA/` | Smoke test runtime (`-vvsmoke`) — pas un outil Editor |

**Inchangé :** noms de classes, champs sérialisés, GUID `.meta`. `SimpleFPS` reste référencé par la scène (même GUID).
**Pas encore :** EventBus, streaming, DI, assembly definitions, services.

Les scripts runtime **n’utilisent pas** `UnityEditor` et ne sont **pas** sous un dossier `Editor`.

## Editor (classement par rôle)

| Dossier | Scripts | Rôle |
|---|---|---|
| `Editor/Generators/` | `ValVertSetup`, `ValVertWoods`, `ValVertDress`, `ValVertSeason` (+ `ValVertEnvWire`, `ValVertWeatherWire`), `ValVertEF`, `ValVertUNS`, `ValVertFoliageVariants` | Produisent / rebranchent le monde |
| `Editor/Tools/` | `ValVertPlayer`, `ValVertAldi` | Builds / outils de maintenance ciblés |
| `Editor/Validation/` | `ValVertReview`, `ValVertPOV`, `ValVertPhotoCompare`, `ValVertSiteCompare`, `ValVertRampCompare`, `ValVertAldiVerify`, `ValVertFoliageEvidence` | Reviews, comparaisons photo, preuves |
| `Editor/Debug/` | *(vide suivi)* | Probes locaux non suivis (`ValVertProbe`, `ValVertTreeProbe`, `ValVertFoliageProbe`) restent hors dépôt |

`-executeMethod ValVertSetup.Build` etc. inchangé (résolution par nom de classe).

## Props (état)

- **Migré :** groupe Entrance → `Runtime/World/Props/Entrance/` (PR #11).
- **Procédural (pas de prefab asset) :** bollards, bancs, lampadaires de rue, clôtures rondins, bins… créés via `CreatePrimitive` dans `ValVertWoods` + matériaux sous `Assets/ValVert/Mats/`. Extraction future = grouper mats exclusifs + éventuelle capture prefab, **pas** une réécriture géométrie dans cette passe.
- **Partagés / à ne pas déplacer sans audit :** `canopy.mat`, mats routes/bâtiments, vendor `ef_*` / `uns_*`.
- **Végétation tracked :** `fern_*`, `laurel_*`, `shrub_mix_green` restent sous `Assets/ValVert/Prefabs/` pour l’instant (lié terrain trees).

## Données générées (prochain chantier)

## Données générées — pipeline (2026-11-09)

Point d’entrée : `python3 Tools/rebuild/rebuild.py`. Manifeste : `Tools/rebuild/manifest.json`.
Doc : `Docs/generated-rebuild.md`.

Cibles **proven** (binaires gitignorés, metas suivis) : `sign_danger`, `asphalt_ww`, `parking`.
`facade_atlas` : déterministe mais non équivalent → reste suivi.
Grass / m_Bld / Setup.Build : non sandboxés dans ce milestone.

### Périmètre honnête

- **Prouvé + binaires désuivis (metas gardés)** : `sign_danger`, `asphalt_ww`, `parking` (~15.3 Mo checkout).
- **Déjà gitignorés sans preuve sandbox ici** : `grass_*`, `m_Bld_*` / `m_OuterRing` (Unity Setup/Woods).
- **Non désuivi** : `facade_atlas` (déterministe ≠ asset actuel), demfix, snow vendor, rebuild ville complète.

## Limites honnêtes

- Dossiers `Streaming`, `Vehicles`, `UI`, etc. existent comme squelette : **vides**, pas d’implémentation.
- Pas de correction art / feuillage / matching photo ici.

## Vegetation / grass field (en cours)

- Générateur isolé : `Editor/Generators/Vegetation/ValVertGrassField.cs` (extrait de `ValVertWoods.Grass`).
- Config : `Generated/Vegetation/grass_field_config.json` — pack Asset Store 138810 en attente d’acquisition locale.
- Doc : `Docs/architecture-vegetation-grass.md`.
- **Pas** de claim d’intégration pack tant que le package authentique n’est pas inspecté.

## Runtime foundations (2026-11-09)

| Piece | Path | Role |
|---|---|---|
| Event bus | `Runtime/Boot/ValVertEventBus.cs` | Time/season/weather pub-sub |
| Services | `Runtime/Boot/ValVertServices.cs` | Lightweight registry |
| Bootstrap | `Runtime/Boot/ValVertRuntimeBootstrap.cs` | Wired from `ValVertEnvWire` |
| Streaming | `Runtime/World/Streaming/ValVertWorldStreaming.cs` | Region SetActive by distance |
| World config SO | `Settings/World/ValVertWorldConfig.asset` | Radii + pack status notes |

## Owned migrations this pass

Sky HDRIs → `Runtime/World/Sky/`. Tracked veg prefabs → `Runtime/World/Vegetation/Prefabs/`.
Grass SOURCE config → `Settings/World/Vegetation/`. Ledger: `Settings/Architecture/migration_manifest.json`.

Still under `Assets/ValVert/`: shared Mats, Ground, Gen, demtiles (owned/derived mix). Cars frozen.

