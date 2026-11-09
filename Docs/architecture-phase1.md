# Architecture Val Vert 2010 — Phase 1 (audit + squelette)

Date : 2026-11-09 (Europe/Paris). Base : `origin/main` @ `0143568` (merge PR #8).
Périmètre autorisé : audit en lecture seule + dossiers vides. Aucun déplacement d’asset, aucune migration, aucun correctif visuel.

## 1. Constat d’audit (mesuré, pas hypothèses)

### 1.1 Dépôt suivi (git)

| Zone | Fichiers suivis | Notes |
|---|---:|---|
| Total | 2422 | `git ls-files` sur `origin/main` |
| `Assets/ValVert/` | 2101 | Terrains, Gen, Mats, Prefabs partiels, demfix, Sky, Veg, Ground |
| `Assets/Cars/` | 104 | Gelé — ne pas modifier |
| `Assets/Editor/` | 33 | 16 scripts C# éditeur + metas / notes |
| `Assets/Scripts/` | 13 | 6 scripts runtime (+ metas / dossier) |
| `Assets/Settings/` | 14 | URP / volumes |
| `Assets/Scenes/` | 4 | `ValVert.unity` (GUID `f3a1fd11dbb77e25d899bcc1222d8a05`) + SampleScene |
| `Captures/` | 83 | À la racine du projet, **pas** sous `Assets/` |
| `Tools/generators/` | 17 | Générateurs Python hors Unity |
| Autres | ProjectSettings, Packages, README, CREDITS, TutorialInfo | |

Hypothèses du plan utilisateur confrontées aux faits :

- **« zero runtime »** → **FAUX**. Runtime présent : `Assets/Scripts/SimpleFPS.cs`, `ValVertSmoke.cs`, `Environment/ValVertClock.cs`, `ValVertSeasons.cs`, `ValVertWeather.cs`, `ValVertStreetLights.cs`.
- **« no prefabs »** → **FAUX**. 5 prefabs suivis sous `Assets/ValVert/Prefabs/` (`fern_b`, `fern_c`, `laurel_cc0`, `shrub_mix_green`, `vv_entrance`). ~119 prefabs sur disque au total (vendor + wrappers ignorés).
- **« Captures in Assets »** → **FAUX**. `Captures/` est à la racine du dépôt (83 fichiers suivis). Les planches récentes de review vivent hors dépôt sous `/workspace/valvert/review/`.

### 1.2 Contenu local ignoré (vendor / généré)

| Chemin | Taille approx. | Statut |
|---|---|---|
| `Assets/InnerverseInteractive/` (Ultimate Nature Starter) | ~263 Mo | gitignored ; ~27 prefabs locaux |
| `Assets/Realistic Tree/` (European Forests) | ~202 Mo | gitignored ; ~51 prefabs locaux |
| `Assets/ValVert/Prefabs/ef_*`, `uns_*` | wrappers terrain | gitignored (dérivés vendor) |
| `Assets/ValVert/Gen/grass_*.asset` | ~767 fichiers locaux | gitignored ; régénérés par `ValVertWoods.Grass` |
| `Assets/ValVert/Gen/snowtrees/` | prefabs neige | gitignored |
| Gros maillages `m_Bld_*` / `m_OuterRing` | — | gitignored ; régénérés par `ValVertSetup.Build` |

`Assets/ValVert/Gen/` : ~1195 fichiers suivis vs ~2846 sur disque.  
`demfix_*` : 144 suivis vs 192 sur disque (scratch local non suivi).

### 1.3 Scripts éditeur (générateurs / review)

Suivis sous `Assets/Editor/` : `ValVertSetup`, `ValVertWoods`, `ValVertDress`, `ValVertSeason`, `ValVertReview`, `ValVertPOV`, `ValVertPhotoCompare`, `ValVertSiteCompare`, `ValVertRampCompare`, `ValVertAldi`, `ValVertAldiVerify`, `ValVertEF`, `ValVertUNS`, `ValVertFoliageVariants`, `ValVertFoliageEvidence`, `ValVertPlayer`.

Méthodes batchmode mutantes connues : `ValVertSetup.Build`, `ValVertWoods.Grass`, `ValVertDress`, `ValVertSeason`, `ValVertReview.Run`, comparaisons photo. **Ne pas les lancer** pendant la phase 1.

### 1.4 Outils hors Assets

`Tools/generators/*.py` (asphalt, buildings, woods, cars, signs, pilot6, etc.) restent à `Tools/` — hors du squelette `_Project` pour l’instant.

### 1.5 État de travail local (hors cette phase)

Au moment de l’audit, le checkout `/workspace/ValVertGame` (branche `fix-foliage-cameras-entrance` @ `5052e67`, déjà mergée en #8) contenait ~92 fichiers **non suivis** (probes feuillage, textures snow scratch, demfix locaux). Aucun de ces fichiers n’entre dans le commit phase 1.

## 2. Squelette ajouté (vide)

Aucun fichier existant déplacé, renommé ou modifié. Dossiers vides + `.meta` Unity (`folderAsset: yes`, GUID déterministe MD5 du chemin) + `.gitkeep` sur les feuilles.

```
Assets/_Project/
  Boot/
  Scenes/
  Settings/
  Input/
  Runtime/
    World/{Streaming,Terrain,Roads,Buildings,Vegetation,Props,Water,Sky,Audio}/
    Characters/{Player,NPCs}/
    Vehicles/{Cars,Traffic}/
    UI/
    Gameplay/{Season,Weather,DayNight}/
  Editor/{Tools,Generators,Validation,Debug}/
  Art/
Assets/ThirdParty/
Docs/architecture-phase1.md   (ce document)
```

Compte : **34** dossiers, **34** `.meta` de dossiers, **27** `.gitkeep` (feuilles).

`Captures/` et `Tools/` restent à leur emplacement actuel. `Assets/Cars/`, Aldi et la scène validée sont gelés.

## 3. Limites de phase / prochaines étapes (propositions seulement)

**Dans cette phase :** squelette + doc. Rien d’autre.

**Hors phase 1 (à décider avec l’utilisateur, une migration à la fois) :**

1. Props d’abord (proposition) : déplacer progressivement les props non vendor vers `Assets/_Project/Runtime/World/Props` avec mise à jour GUID/références.
2. Puis scripts runtime Environment → `Runtime/Gameplay/{Season,Weather,DayNight}`.
3. Puis Editor tools → `_Project/Editor/...`.
4. Vendor : rester dans `Assets/ThirdParty/` **sans** republier le contenu Asset Store brut.
5. Voitures / Aldi : **gelés** jusqu’à ordre explicite.

Pas de bus d’événements, streaming, contrôleurs, conversion voitures, ni désuivi massif de Gen dans cette phase.

## 4. Défauts visuels connus (non corrigés ici)

- Suppression temporaire du feuillage EF/UNS défectueux (cartes feuilles en gros carrés) — PR #8 ; les troncs restent, parfois blancs / surexposés.
- Caméras de comparaison photo encore incomplètes / mal calées sur certaines vues.
- Matching photo Val Vert non terminé (bandeaux, détails de porte « boîte », etc.).

Ces points sont documentés, **pas** traités en phase 1.

## 5. Règles permanentes rappelées

- 100 % gratuit ; crédits OSM / IGN.
- Ne jamais committer photos utilisateur, contenu brut Asset Store, credentials.
- Pas de force-push / hard reset.
- Après chaque passe validée : commit + merge normal, puis accord sur la suite.
