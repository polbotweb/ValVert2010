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

`Assets/ValVert/Gen/` (herbe, gros maillages, demfix, snow…) : toujours régénérables via `ValVertSetup.Build` / `ValVertWoods.Grass`. **Pas** désuivi ni pipeline rebuild massif dans cette PR — seulement documenté.

## Limites honnêtes

- Dossiers `Streaming`, `Vehicles`, `UI`, etc. existent comme squelette : **vides**, pas d’implémentation.
- Pas de correction art / feuillage / matching photo ici.
