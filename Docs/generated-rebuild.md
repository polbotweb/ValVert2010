# Pipeline de reconstruction des données générées

Date : 2026-11-09 (Europe/Paris). Milestone architecture après PR #12.

## Commandes (depuis la racine du dépôt)

```bash
python3 Tools/rebuild/rebuild.py list
python3 Tools/rebuild/rebuild.py preflight
python3 Tools/rebuild/rebuild.py determinism --target sign_danger
python3 Tools/rebuild/rebuild.py determinism --target asphalt_ww
python3 Tools/rebuild/rebuild.py determinism --target parking
python3 Tools/rebuild/rebuild.py verify --target sign_danger
python3 Tools/rebuild/rebuild.py verify --target asphalt_ww
python3 Tools/rebuild/rebuild.py verify --target parking
python3 Tools/rebuild/rebuild.py rebuild --target sign_danger --staging /workspace/valvert/rebuild-staging
python3 Tools/rebuild/rebuild.py rebuild-proven --apply   # écrit les sorties proven dans le dépôt
python3 Tools/rebuild/tests/test_rebuild.py
```

Manifeste machine : `Tools/rebuild/manifest.json`.

## Cibles prouvées (éligibles à l’ignorance git des binaires)

| Cible | Sorties | Preuve |
|---|---|---|
| `sign_danger` | `Assets/ValVert/Gen/sign_danger.png` | 2 runs identiques + égalité octets vs asset suivi |
| `asphalt_ww` | `Assets/ValVert/Ground/asphalt_ww_{diff,nor,ms}.*` | idem, seed 11 |
| `parking` | `Assets/ValVert/Gen/parking.png` (+ `_nor.jpg`, `_ms.png`) | 2 runs ; albedo via `pixels_rgb` (PNG) ; nor/ms octets |

Les fichiers `.meta` restent **suivis** (GUID stables). Après clone : `preflight` puis `rebuild-proven --apply`.

## Déterministe mais non équivalent (non désuivi)

- `facade_atlas` : 2 runs identiques entre eux, mais taille/contenu ≠ atlas actuel (4096×1536 vs 3072×1536).

## Non prouvé / hors scope (restent suivis ou déjà ignorés)

- `grass_*.asset` : déjà gitignorés ; `ValVertWoods.Grass` non sandboxé ici.
- `m_Bld_*.asset` / `m_OuterRing` : déjà gitignorés ; `ValVertSetup.Build` mutant.
- `demfix_*`, snow vendor-derived, rebuild ville complète Unity.

## Politique GUID

Ne jamais régénérer/écraser les `.meta` des sorties. Le producteur n’écrit que le binaire.

## Entrées locales requises

- Police système : `/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf` (`sign_danger`).
- `facade_atlas` : `Assets/ValVert/Gen/grey_plaster_diff_1k.jpg` (source suivie).
- Paquets Asset Store locaux : non requis pour les cibles proven.

## Emplacement architectural

`Assets/_Project/Generated/` : point d’ancrage documentaire pour de futures catégories migrées. Les sorties proven actuelles restent aux chemins historiques consommés par la scène (Ground/Gen) pour stabilité des références.
