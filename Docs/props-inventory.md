# Inventaire Props — après Entrance

## Déjà sous `_Project`
- `Runtime/World/Props/Entrance/` — prefab `vv_entrance` + mats/textures exclusifs (PR #11).
- `Runtime/World/Props/Lighting/ValVertStreetLights.cs` — logique lampadaires (pas les meshes).

## Procédural (éditeur)
Créés dans `ValVertWoods` / helpers : bollards, EV Aldi, bin store door, log fences, boulders, stump, street lamps (`globe_lamp2`), plaques de rue, etc. Ownership matériaux à auditer **par groupe** avant tout `git mv` (ex. `bollard*` vs `aldi_bollard`, `metal_grey` partagé).

## Prefabs tracked hors Entrance
`Assets/ValVert/Prefabs/{fern_b,fern_c,laurel_cc0,shrub_mix_green}` — végétation / terrain trees, hors chantier Props urbain.

## Prochaine extraction Props (proposition)
Choisir un groupe de **matériaux exclusifs** (ex. `bin_*` + `bench_wood` si non partagés) avec mise à jour `MatRelocs`, sans réécrire la géométrie procédurale.
