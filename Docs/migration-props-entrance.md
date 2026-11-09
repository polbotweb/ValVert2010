# Migration Props — groupe 1 : entrée Val Vert (`vv_entrance`)

Date : 2026-11-09 (Europe/Paris). Base : `origin/main` @ `dbcc2c8` (après phase 1).
Décision utilisateur : « Vazy » — migrer uniquement les accessoires du décor, un groupe à la fois.

## Périmètre retenu

**Groupe :** entrée d’immeuble Val Vert (propre, non vendor).

Constat d’audit : la plupart des « props » urbains (bollards, bancs, lampadaires, clôtures) sont des **primitives procédurales** (`CreatePrimitive`) + matériaux sous `Assets/ValVert/Mats/`, pas des fichiers prefab. Premier lot sûr = le seul prefab d’accessoire own-authored clairement isolé : `vv_entrance`, avec ses matériaux et textures **exclusifs**.

**Inclus (19 assets + metas) :**

| Ancien chemin | Nouveau chemin | GUID |
|---|---|---|
| `Assets/ValVert/Prefabs/vv_entrance.prefab` | `Assets/_Project/Runtime/World/Props/Entrance/Prefabs/vv_entrance.prefab` | `1005397838e350487ba5dea45715821c` |
| `Assets/ValVert/Mats/door_alum_grey.mat` | `…/Entrance/Materials/door_alum_grey.mat` | `5636d3876a34ecc47849d2f79b90d3fd` |
| `Assets/ValVert/Mats/door_glass.mat` | `…/Entrance/Materials/door_glass.mat` | `dd42d158d65854c038583542de8d662c` |
| `Assets/ValVert/Mats/handle_brushed.mat` | `…/Entrance/Materials/handle_brushed.mat` | `98ec511653ccdf4438069fddca80280c` |
| `Assets/ValVert/Mats/shutter_grey.mat` | `…/Entrance/Materials/shutter_grey.mat` | `a717c5df0c3d56e9d8dd7d59d9945793` |
| `Assets/ValVert/Mats/interphone.mat` | `…/Entrance/Materials/interphone.mat` | `e1f5ad1d561242472be652443a3586c5` |
| `Assets/ValVert/Mats/globe_lamp.mat` | `…/Entrance/Materials/globe_lamp.mat` | `03f587e820e7a356abfb0d9308a29903` |
| `Assets/ValVert/Mats/plaque_{7,9,11,13,14}.mat` | `…/Entrance/Materials/plaque_*.mat` | (inchangés, voir commit) |
| `Assets/ValVert/Gen/intercom_panel.png` | `…/Entrance/Textures/intercom_panel.png` | `7997f8223b18a6e488d748da3df12817` |
| `Assets/ValVert/Gen/roller_shutter.png` | `…/Entrance/Textures/roller_shutter.png` | `cd471f1366f594c2f93243db1da7cb2a` |
| `Assets/ValVert/Gen/plaque_{7,9,11,13,14}.png` | `…/Entrance/Textures/plaque_*.png` | (inchangés) |

**Exclus volontairement :**

- `canopy.mat` + `concrete_canopy.png` — partagés avec `Bld_canopy` en scène et le dictionnaire MATS woods.
- `globe_lamp2.mat` — lampadaires de rue procéduraux.
- `aldi_*`, tout `Assets/Cars/**`, `car2_*`.
- Végétation (`fern_*`, `laurel_*`, wrappers EF/UNS).
- Bollards / bancs / bins (matériaux partagés ou procéduraux ; prochaines passes).

## Code mis à jour

- `ValVertDress.Mat` / `MatPath` : petite table `MatRelocs` pour les 11 matériaux déplacés (évite de recréer l’ancien chemin).
- `ValVertWoods.BuildEntrancePrefab` / `PlaceValVertEntrances` : chemins prefab + textures d’entrée.

Aucune autre référence Python/générateur pour ces assets.

## Validation

- GUIDs et hashes de contenu des 19 assets : identiques avant/après `git mv`.
- Scène `ValVert.unity` : non modifiée (pas de changement de placement).
- Compile Unity batchmode `-quit` : voir log joint au PR / rapport.
- Pas d’appel à Dress/Setup/Review (mutants). Comparaison visuelle caméra : non effectuée (évite hooks météo/saison) ; invariance GUID/contenu/scène à la place.

## Défauts visuels préexistants (non traités)

Feuillage retiré temporairement, troncs surexposés, matching photo incomplet. Aldi et voitures gelés.
