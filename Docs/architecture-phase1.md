# Architecture Val Vert 2010 — Phase 1 (audit + squelette)

Date : 2026-11-09 (Europe/Paris). Base initiale : `origin/main` @ `0143568` (merge PR #8) ; corrections de formulation après merge PR #9.
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

Points du plan utilisateur (extraits fournis par le parent ; le fichier joint d’origine n’a pas été relu ici) confrontés aux mesures :

- **Voitures / prefabs** — extrait : « les voitures ne sont pas des prefabs, c’est de la donnée brute. » Inspection lecture seule : **0** `.prefab` suivi sous `Assets/Cars/`, **0** prefab véhicule nommé ailleurs dans le dépôt suivi. Le contenu suivi de `Assets/Cars/` est de la donnée brute (`.obj` / `.fbx`, `.mtl`, textures, plaques) ; l’éditeur charge ces maillages directement (ex. `ValVertWoods` → `Assets/Cars/v2/*_lod0.obj`). En contexte seulement : 5 prefabs **non véhicules** sont suivis sous `Assets/ValVert/Prefabs/` (`fern_b`, `fern_c`, `laurel_cc0`, `shrub_mix_green`, `vv_entrance`) ; ~119 prefabs existent aussi sur disque hors suivi (vendor / wrappers ignorés). Cela ne contredit pas l’extrait sur les voitures.
- **Couche runtime / infra** — extrait : « il n’y a aucune couche runtime — pas de streaming, pas de LODs, pas de services, pas d’events. » Un runtime **spécifique** existe déjà (6 scripts sous `Assets/Scripts/` : `SimpleFPS`, `ValVertSmoke`, `Environment/ValVertClock`, `ValVertSeasons`, `ValVertWeather`, `ValVertStreetLights` — saisons, météo, horloge, éclairage). Cela ne signifie pas que l’architecture proposée (streaming, services, bus d’événements, injection de dépendances) soit en place. Recherche ciblée dans les `.cs` suivis : aucune occurrence de Addressables / `SceneManager.LoadScene` / EventBus / ServiceLocator / Zenject / VContainer / MessagePipe. `LODGroup` apparaît dans `ValVertWeather` (gestion neige sur LOD existants), pas comme système de streaming. Streaming / DI / services / events de la cible future : **non implémentés en phase 1** ; toute affirmation plus large reste à vérifier au cas par cas.
- **Captures** — le plan source est **inconsistant** (premier paragraphe à la racine, mention ultérieure sous `Assets/`). Emplacement réel mesuré : `Captures/` à la **racine** du dépôt (83 fichiers suivis), pas sous `Assets/`. Planches de review récentes hors dépôt : `/workspace/valvert/review/`.

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
