# Géométrie : apprendre les aires et périmètres en jouant

Application de bureau **WPF (C# / .NET 8)** pour les élèves de CM1/CM2 : l'enfant choisit une forme (carré, rectangle,
cercle, triangle, ellipse), génère un exercice ou saisit ses propres dimensions, puis calcule le périmètre et la surface.
L'application corrige, affiche la formule et récompense la progression.

*Desktop app (WPF / .NET 8) that teaches kids how to compute area and perimeter, with XP, badges, an adventure mode and a hidden Snake game.*

## Fonctionnalités

- **Exercices** sur 5 formes, dessinées sur un `Canvas`, avec correction et formule (périmètre de l'ellipse par l'approximation de Ramanujan).
- **Validation des saisies** adaptée aux enfants : champs vides, lettres, valeurs négatives et triangles impossibles refusés, virgule ou point acceptés, tolérance de 0,01.
- **Gamification** : score, XP, niveaux, séries, record, défi rapide et 12 badges.
- **Bouton Formule** : un indice sans la réponse, en échange de la moitié de l'XP.
- **Mode Aventure** : 5 mondes à débloquer avec des étoiles, jusqu'au défi final du Château des Formes.
- **Easter egg** : un mini-jeu Snake caché, fait en WPF pur (`Canvas` et `DispatcherTimer`).
- **Sauvegarde locale en JSON**, compatible avec les anciennes sauvegardes.

## Architecture

| Projet | Rôle |
|---|---|
| `Geometrie` | Application WPF (fenêtres, interactions, dessin) |
| `Geometrie.Core` | Bibliothèque métier : formes (héritage depuis une classe de calcul commune), progression, badges |
| `Geometrie.Tests` | Tests des formules, du score, de l'XP, des badges et du Mode Aventure |

La logique métier est séparée de l'interface pour pouvoir la tester sans lancer l'application.

## Lancer le projet

Prérequis : Windows et le [SDK .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet build Geometrie.sln
dotnet run --project Geometrie.csproj
```

Tests :

```powershell
dotnet run --project Geometrie.Tests\Geometrie.Tests.csproj
```

## Documentation

Les choix techniques, l'interface et les règles de jeu sont détaillés dans le [dossier technique](DOSSIER_TECHNIQUE.md).

Projet réalisé dans le cadre du BTS SIO option SLAM.
