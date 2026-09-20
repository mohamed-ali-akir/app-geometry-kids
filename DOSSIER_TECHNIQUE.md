# Dossier technique - Géométrie

## Présentation

Géométrie est une application WPF autonome destinée à des élèves de CM1/CM2. Elle aide les enfants à s'entraîner sur les périmètres et les surfaces de figures simples : carré, rectangle, cercle, triangle et ellipse.

L'enfant choisit une forme, génère un exercice ou saisit ses propres dimensions, puis entre ses réponses. L'application affiche un message clair, la correction, la formule utilisée et une progression motivante.

## Choix techniques

- C# avec .NET 8.
- WPF pour l'interface graphique.
- Projet `Geometrie.Core` pour la bibliothèque de classes métier.
- Projet `Geometrie` pour l'application WPF.
- Projet `Geometrie.Tests` sous forme de console de vérification, sans dépendance NuGet externe.
- Sauvegarde locale en JSON pour la progression.

## Structure de la solution

- `Geometrie.sln` : solution Visual Studio.
- `Geometrie.csproj` : application WPF.
- `Geometrie.Core/` : bibliothèque métier.
- `Geometrie.Tests/` : tests simples des formules et de la progression.
- `MainWindow.xaml` : interface enfant.
- `MainWindow.xaml.cs` : interactions utilisateur, validation, dessin Canvas, badges visuels et sauvegarde.

## Formules utilisées

- Carré : périmètre = `4 × côté`, surface = `côté × côté`.
- Rectangle : périmètre = `2 × (longueur + largeur)`, surface = `longueur × largeur`.
- Cercle : périmètre = `2 × π × rayon`, surface = `π × rayon²`.
- Triangle : périmètre = `base + côté 1 + côté 2`, surface = `(base × hauteur) / 2`.
- Ellipse : surface = `π × grand rayon × petit rayon`.
- Ellipse : périmètre approximé avec la formule de Ramanujan.

## Gestion des erreurs

- Les champs vides sont refusés.
- Les lettres sont refusées.
- Les nombres avec virgule ou point sont acceptés.
- Les nombres négatifs ou égaux à 0 sont refusés.
- Les trois côtés du triangle sont vérifiés.
- Les réponses sont comparées avec une tolérance de `0,01`.
- Les erreurs sont affichées dans la zone de résultat, sans message technique.

## Fonctionnalités ludiques

- Score : +10 points si le périmètre et la surface sont corrects, +5 points si une seule réponse est correcte.
- XP : +20 XP pour une réussite complète, +10 XP pour une réussite partielle.
- Niveaux : tous les 100 XP, l'enfant passe au niveau suivant.
- Série : augmente seulement quand l'exercice est entièrement réussi.
- Record : meilleure série conservée.
- Défi rapide : génère une forme aléatoire avec un bonus de +5 points si tout est juste.
- Sauvegarde locale : score, niveau, XP, série, meilleure série, badges et réussites par forme.
- Réinitialisation de progression : remet tout à zéro après confirmation.

## Amélioration de l'interface enfant

L'interface a été retravaillée pour être plus attirante et moins intimidante :

- fond clair avec dégradé bleu, violet et jaune pastel ;
- cartes blanches arrondies avec ombre légère ;
- boutons arrondis, grands et colorés ;
- choix des formes par boutons visuels au lieu d'une ComboBox visible ;
- forme sélectionnée mise en avant en jaune ;
- zone de dessin encadrée, douce et lumineuse ;
- tableau de bord enfant avec score, niveau, XP, série et record ;
- barre d'XP verte et lisible ;
- zone résultat très visible, avec couleur selon réussite, erreur, réussite partielle ou badge ;
- petite animation d'apparition sur le résultat ;
- mascotte "Géo le robot" avec icône et bulle de conseil ;
- badges affichés sous forme de petites cartes visuelles.

Ces choix sont adaptés à des enfants parce qu'ils donnent des repères visuels simples, rendent l'erreur moins stressante, encouragent la progression et transforment les réussites en récompenses claires sans rendre l'application manipulatrice.

## Badges visuels

Chaque badge est affiché dans une carte arrondie avec une icône, un nom court et une infobulle. Les badges verrouillés sont grisés avec un cadenas. Les badges débloqués sont dorés et plus visibles.

- 🎯 Premier exercice
- ✅ 3 bonnes réponses
- ◼ Expert carré
- ▭ Expert rectangle
- ⚪ Expert cercle
- 🔺 Expert triangle
- 🥚 Expert ellipse
- 🏆 Niveau 5
- 🔥 Série de 3
- ⚡ Série de 5
- 🕵️ Petit filou
- 🗺️ Aventurier des formes

## Bouton Formule

Le bouton `💡 Formule` aide l'enfant sans donner la réponse numérique finale.

- Il affiche uniquement la formule à utiliser pour la forme sélectionnée.
- Il indique clairement que l'aide a été utilisée.
- En échange, l'XP gagnée sur l'exercice est divisée par deux.
- Le score reste normal.
- La pénalité ne s'applique qu'une seule fois par exercice, même si l'enfant clique plusieurs fois.
- La pénalité est réinitialisée quand un nouvel exercice est généré, quand la forme change, avec le bouton Réinitialiser ou avec le Défi rapide.

Ce choix encourage l'apprentissage : l'enfant peut demander un indice, comprendre la méthode, puis garder un petit défi pour progresser.

## Easter egg : mini-jeu secret

Un petit bouton `?` discret est placé dans l'en-tête de l'application. Il ouvre un mini-jeu Snake simple, réalisé uniquement avec WPF, un `Canvas` et un `DispatcherTimer`.

- Le mini-jeu se joue avec les flèches du clavier.
- Le serpent est vert, avec une tête un peu différente.
- Les objectifs sont des pommes rouges, plusieurs pommes restant visibles en même temps.
- Le bouton `Quitter` est toujours visible dans la fenêtre du mini-jeu.
- Quand l'enfant quitte le mini-jeu, le badge secret `Petit filou` est débloqué.
- Le badge `Petit filou` apparaît dans la grille des badges comme les autres badges, verrouillé puis débloqué.
- Le premier déblocage donne aussi +1 niveau.
- Le bonus de niveau est donné une seule fois : si le badge existe déjà dans la sauvegarde, quitter le mini-jeu ne redonne pas de niveau.
- La progression est sauvegardée en JSON comme le score, l'XP et les autres badges.

Ce clin d'oeil reste positif et enfant-friendly : il récompense la curiosité sans modifier les calculs, le score, les formules ni les exercices.

## Mode Aventure

Le bouton `🗺️ Mode Aventure` ouvre une fenêtre dédiée appelée `ModeAventureWindow`. Elle affiche une petite carte de progression avec des mondes, des cadenas, des étoiles et un bouton `Jouer` par monde.

Parcours :

- 🟦 Village du Carré : exercices sur le carré.
- ▭ Forêt du Rectangle : exercices sur le rectangle.
- ⚪ Lac du Cercle : exercices sur le cercle.
- 🔺 Montagne du Triangle : exercices sur le triangle.
- 🏰 Château des Formes : défi final avec formes mélangées, y compris l'ellipse.

Au départ, seul le premier monde est débloqué. Chaque monde demande 3 réussites complètes pour ouvrir le suivant. Les étoiles appartiennent au Mode Aventure et ne remplacent pas les badges :

- 1 réussite : `★☆☆`
- 2 réussites : `★★☆`
- 3 réussites : `★★★`

Quand l'enfant clique sur `Jouer`, l'application revient sur l'exercice principal avec une forme adaptée au monde choisi. Le bouton `Générer` devient alors `Suivant` : l'enfant peut enchaîner les exercices du Mode Aventure sans rouvrir la carte après chaque réponse. Si le périmètre et la surface sont tous les deux corrects, une étoile est ajoutée au monde actif. Après 3 réussites, le monde suivant est déverrouillé et `Suivant` prépare automatiquement ce nouveau monde.

Le Château des Formes génère aléatoirement un carré, un rectangle, un cercle, un triangle ou une ellipse. Quand il atteint 3 réussites, l'application affiche un message spécial de fin d'aventure et débloque le badge `Aventurier des formes`.

La sauvegarde JSON existante est étendue avec les réussites du Mode Aventure. Une ancienne sauvegarde reste compatible : si les données d'aventure n'existent pas encore, elles démarrent vides, avec seulement le niveau 1 débloqué. Le reset global remet aussi l'aventure à zéro, retire ses étoiles et supprime le badge final.

L'intérêt pédagogique est de donner un objectif court et lisible : l'enfant travaille une famille de formes à la fois, voit sa progression avec des étoiles, puis passe à un défi final mélangé.

## Tests réalisés

Le projet `Geometrie.Tests` vérifie :

- périmètre et surface des formes ;
- augmentation du score ;
- augmentation de l'XP ;
- division par deux de l'XP quand la formule est utilisée ;
- bonus du défi rapide ;
- passage de niveau à 100 XP ;
- série de bonnes réponses ;
- remise à zéro de la série après une erreur ;
- déblocage des badges ;
- badges de série 3 et 5 ;
- badge secret Petit filou et bonus de niveau unique ;
- progression du Mode Aventure ;
- déblocage des mondes après 3 réussites ;
- badge final Aventurier des formes ;
- remise à zéro du Mode Aventure ;
- réinitialisation complète de la progression.

Commande :

```powershell
dotnet run --project Geometrie.Tests\Geometrie.Tests.csproj
```

La solution complète se compile avec :

```powershell
dotnet build Geometrie.sln
```
