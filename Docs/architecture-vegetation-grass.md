# Architecture — Vegetation / grass field (milestone en cours)

Date : 2026-11-09 (Europe/Paris). Branche de travail : `arch-vegetation-grass-flowers`.
Pack cible (utilisateur) : Asset Store **138810** — [Grass Flowers Pack Free](https://assetstore.unity.com/packages/2d/textures-materials/nature/grass-flowers-pack-free-138810) (catégorie **2D textures/materials**, pas des modèles 3D).

## Objectif

Isoler le système d’herbe de champ sous l’architecture Vegetation, brancher le pack 138810 à la place de la texture homemade `grass_blades.png`, conserver placement / exclusions / batching, supprimer le chemin blade homemade **seulement** après preuve pack + rendu alpha.

## État actuel (checkpoint)

| Élément | État |
|---|---|
| Extraction placement → `ValVertGrassField` | **fait** (worktree) |
| Config `Settings/World/Vegetation/grass_field_config.json` | **fait** — mode `legacy_homemade` (SOURCE) |
| `ValVertWoods.Grass` | **délègue** à `ValVertGrassField.Build` |
| Pack 138810 sur disque | **absent** — acquisition séparée (DOWNLOAD ONLY) |
| `materialMode: pack_cutout` | **non câblé** — `localRoot` / `albedoRelative` vides volontairement |
| Suppression `grass_blades` / chemin obsolete | **pas encore** (interdit tant que pack non prouvé) |
| Rebuild grass déterministe | **non revendiqué** |
| Merge / PR | **pas encore** — stub incomplet sans pack |

## Emplacements

```
Assets/_Project/Editor/Generators/Vegetation/ValVertGrassField.cs
Assets/_Project/Settings/World/Vegetation/grass_field_config.json
Assets/_Project/Runtime/World/Vegetation/README.md
Tools/vegetation/preflight.py
Tools/vegetation/tests/test_grass_config.py
```

Appel historique inchangé : `ValVertSetup.Build` → `ValVertWoods.Grass(terr)`.

## Politique exclusions (inchangée)

`veg_excl.png`, `label_v3.raw` (Lab≠0), `Hidden(woods_meshes)`, `Built` raycast (stairs/surfaces), densités canopy/meadow, seed **5**, chunks 32 m. Pas d’herbe sur paths/stairs/roads via ces masques.

## Pack 138810 — règles

1. Import **uniquement** sous `Assets/ThirdParty/<dossier vérifié>/` (gitignoré) ; ne pas republier textures/wrappers dans le dépôt public.
2. Remplir `pack.localRoot` + `pack.albedoRelative` **après** inspection du package réel — **ne pas inventer** noms de fichiers / layout UV atlas.
3. Passer `materialMode` à `pack_cutout` ; URP Lit alpha cutout.
4. La géométrie crossed-quad / chunking **reste nécessaire** (pack 2D).
5. Preflight : `python3 Tools/vegetation/preflight.py --require-pack` doit passer avant de retirer le mode legacy.
6. Validation requise avant merge : compile Unity, close-up alpha, exclusions, saisons/jour **étiquetés** si non testés.

## Tests locaux (sans Unity / sans pack)

```bash
python3 Tools/vegetation/preflight.py --allow-missing-legacy-albedo
python3 Tools/vegetation/tests/test_grass_config.py
```

## Suite bloquée sur input parent

Chemin officiel du `.unitypackage` / dossier extrait après acquisition → inspect → fill config → import ignored ThirdParty → switch mode → preuve rendu → retirer legacy → PR.
