# Géométrie : apprendre les aires et périmètres en jouant

[![CI](https://github.com/mohamed-ali-akir/app-geometry-kids/actions/workflows/ci.yml/badge.svg)](https://github.com/mohamed-ali-akir/app-geometry-kids/actions/workflows/ci.yml)

Application de bureau **WPF (C# / .NET 10)** pour les élèves de CM1/CM2 : l'enfant choisit une forme (carré, rectangle,
cercle, triangle, ellipse), génère un exercice ou saisit ses propres dimensions, puis calcule le périmètre et la surface.
L'application corrige, affiche la formule et récompense la progression.

*Desktop app (WPF / .NET 10) that teaches kids how to compute area and perimeter, with XP, badges, an adventure mode and a hidden Snake game.*

## Fonctionnalités

- **Exercices** sur 5 formes, dessinées à l'échelle sur un `Canvas`, avec correction et formule (périmètre de l'ellipse par l'approximation de Ramanujan).
- **Triangles toujours réalistes** : les exercices générés ont des dimensions entières dont la hauteur correspond vraiment aux côtés, et une hauteur impossible saisie à la main est refusée avec la bonne valeur.
- **Correction adaptée à l'école** : une réponse arrondie au centième est juste, et pour le cercle et l'ellipse une réponse calculée avec π ≈ 3,14 est acceptée.
- **Validation des saisies** : champs vides, lettres, nombres négatifs, « NaN », exposants et valeurs démesurées refusés ; virgule ou point acceptés.
- **Gamification** : score, XP, niveaux, séries, record, défi rapide et 12 badges.
- **Bouton Formule** : un indice sans la réponse, en échange de la moitié de l'XP.
- **Mode Aventure** : 5 mondes à débloquer avec des étoiles, jusqu'au défi final du Château des Formes.
- **Easter egg** : un mini-jeu Snake caché, fait en WPF pur (`Canvas` et `DispatcherTimer`).
- **Sauvegarde locale en JSON qui ne se perd pas** : écriture atomique, fichier abîmé mis de côté au lieu d'être écrasé, anciennes sauvegardes migrées.

## Architecture

```
src/
  Geometrie.App/     Application WPF : fenêtres, saisies, dessin (aucune règle du jeu)
  Geometrie.Core/    Bibliothèque métier, sans dépendance à Windows
tests/
  Geometrie.Core.Tests/   Tests xUnit du Core
```

| Élément du Core | Rôle |
|---|---|
| `ClasseCalcul` → `ClasseAvecAngle` / `ClasseRond` → formes | Hiérarchie de classes abstraites : chaque forme calcule son périmètre et sa surface, se génère au hasard et valide ses dimensions |
| `CatalogueFormes`, `CatalogueBadges`, `Aventure` | Noms, formules, icônes, badges et mondes au même endroit (plus de chaînes recopiées dans l'interface) |
| `SessionExercice` | Déroulement d'une séance : exercice en cours, défi rapide, Mode Aventure, indice, anti-triche |
| `Progression` | Score, XP, niveaux, badges, étoiles ; `Normaliser()` répare une sauvegarde incohérente |
| `DepotProgression` | Lecture et écriture sûres de la sauvegarde |
| `Correcteur`, `LecteurNombre` | Correction des réponses et lecture des nombres tapés par l'enfant |

L'interface ne contient plus de règle du jeu : tout ce qui décide d'un score, d'un badge ou d'une étoile est dans le Core et couvert par les tests.

## Lancer le projet

Prérequis : Windows et le [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
dotnet run --project src\Geometrie.App
```

Tests (ils tournent aussi sous Linux et macOS, le Core ne dépend pas de Windows) :

```powershell
dotnet test
```

La CI GitHub Actions compile la solution en traitant les avertissements comme des erreurs et lance les tests à chaque push et pull request.

## Documentation

Les choix techniques, l'interface et les règles de jeu sont détaillés dans le [dossier technique](DOSSIER_TECHNIQUE.md).

Projet réalisé dans le cadre du BTS SIO option SLAM.
