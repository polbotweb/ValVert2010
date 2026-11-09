# Val Vert 2010 — Seynod

Reconstitution en 3D du quartier du **Val Vert / Vovray à Seynod (Annecy)** tel qu'il était vers **2010**, faite avec **Unity 6000.3.27f2 (URP)**.
Les bâtiments, les rues et les chemins viennent d'OpenStreetMap. Le relief, la hauteur des bâtiments et les couleurs des toits viennent de l'IGN (ortho 2006-2010).

## Ouvrir le projet
1. Installer **Unity Hub** et l'éditeur **Unity 6000.3.27f2**.
2. Télécharger ce dépôt : bouton *Code > Download ZIP*, ou `git lfs install && git clone https://github.com/polbotweb/ValVert2010`. Git LFS est nécessaire pour les gros fichiers.
3. Unity Hub > **Add** > choisir le dossier du projet.
4. Ouvrir la scène **Assets/Scenes/ValVert.unity**.

> Note : les gros maillages générés (`Assets/ValVert/Gen/m_Bld_kerb`, `m_Bld_white_trim`, `m_Bld_facade`, `m_Bld_metal`, `m_OuterRing` en `.asset`, de 20 à 135 Mo chacun) ne sont pas dans le dépôt, pour rester dans le quota gratuit. Ils sont régénérés par `ValVertSetup.Build()`. Leurs fichiers `.meta` sont gardés, pour que la scène retrouve ses références.
>
> De même, l'herbe générée (`Assets/ValVert/Gen/grass_*.asset`, environ 400 Mo) n'est pas dans le dépôt. Elle est recréée par `ValVertWoods.Grass()`, appelé depuis `ValVertSetup.Build()`. Sans elle, la scène s'ouvre quand même, mais sans les touffes d'herbe.

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

## Environnement dynamique (runtime)
`ValVertEnvironment` (scène) porte : **ValVertClock** (date/heure 2010, position solaire NOAA pour Seynod, heure CET/CEST, lune — phase et position calculées, lumière ambiante/brouillard/ciel), **ValVertSeasons** (phénologie continue : débourrement, été, couleurs d'automne progressives par essence, chute des feuilles, litière au sol, humidité), **ValVertWeather** (météo **synthétique** reproductible, tirée au sort par date selon un climat typique du bassin annécien — ce ne sont PAS des observations historiques : ciel clair/couvert/bruine/pluie/brouillard/neige, transitions douces, accumulation/fonte de neige, sol mouillé ; touche F6 = forcer un état) et **ValVertStreetLights** (349 lampadaires allumés au crépuscule).
La neige se dépose sur les faces tournées vers le ciel (branches, sapins, buissons) via des « coques » générées dans l'éditeur (`ValVertWeatherWire`), en plus des textures de neige au sol/toits/routes (gadoue).

## Paquets Asset Store (locaux, non redistribués)
Deux paquets gratuits de l'Asset Store (EULA standard) sont utilisés localement et **git-ignorés** : *Ultimate Nature – Starter* (Innerverse Interactive) et *European Forests – Realistic Trees* (MysticForge), ainsi que leurs dérivés (`Assets/ValVert/Prefabs/uns_*`, `ef_*`, `Assets/ValVert/Gen/snowtrees/`, masques de neige issus des feuilles UNS).
Pour les retrouver : les ajouter à votre compte Unity, les importer via Package Manager (sans `Packages/manifest.json`, `Demo`, `Settings`, HDRP), puis lancer `ValVertSetup.Build`.
**Sans eux**, `ValVertSetup.Build` retombe sur les arbres/buissons CC0 Poly Haven (`tree_small_02`, `fir_tree_01`, `shrub_02`) — rendu moins riche mais complet (repli présent dans le code, non re-testé sur un clone vierge).

## Construire / tester
- Régénérer la scène : `Unity -batchmode -quit -projectPath . -executeMethod ValVertSetup.Build`
- Test environnement : `-executeMethod ValVertEnvWire.Test` (attend `[VV] ENVTEST DONE`)
- Rendus POV saisons/météo : `-executeMethod ValVertPOV.Run`
- Jeu Linux autonome : `-executeMethod ValVertPlayer.BuildLinux` ; test fumée : `./ValVert2010.x86_64 -vvsmoke -vvout <dossier>` (parcours scripté, FPS, captures, puis quitte).
