# Val Vert 2010 — Seynod

Reconstitution en 3D du quartier du **Val Vert / Vovray à Seynod (Annecy)** tel qu'il était vers **2010**, faite avec **Unity 6000.3.27f2 (URP)**.
Les bâtiments, les rues et les chemins viennent d'OpenStreetMap. Le relief, la hauteur des bâtiments et les couleurs des toits viennent de l'IGN (ortho 2006-2010).

## Ouvrir le projet
1. Installer **Unity Hub** et l'éditeur **Unity 6000.3.27f2**.
2. Télécharger ce dépôt : bouton *Code > Download ZIP*, ou `git lfs install && git clone https://github.com/polbotweb/ValVert2010`. Git LFS est nécessaire pour les gros fichiers.
3. Unity Hub > **Add** > choisir le dossier du projet.
4. Ouvrir la scène **Assets/Scenes/ValVert.unity**.

> Note : l'herbe générée (`Assets/ValVert/Gen/grass_*.asset`, environ 400 Mo) n'est pas dans le dépôt. Elle est recréée par `ValVertWoods.Grass()`, appelé depuis `ValVertSetup.Build()`. Sans elle, la scène s'ouvre quand même, mais sans les touffes d'herbe.

## Contenu
- `Assets/` : le projet Unity (scènes, scripts d'éditeur dans `Assets/Editor`, maillages générés).
- `Tools/generators/` : les scripts Python qui préparent les données (OSM, IGN).
- `Captures/` : les derniers rendus.
- `CREDITS_ASSETS.txt` : les crédits détaillés.

## Crédits et licences
- © les contributeurs d'**OpenStreetMap**, données sous licence **ODbL 1.0** (https://www.openstreetmap.org/copyright).
- **IGN** (relief RGE ALTI, MNS/MNH, BD ORTHO 2006-2010), sous **Licence Ouverte / Etalab 2.0**.
- Textures et modèles **Poly Haven**, en **CC0**.
- La liste complète est dans `CREDITS_ASSETS.txt`.
