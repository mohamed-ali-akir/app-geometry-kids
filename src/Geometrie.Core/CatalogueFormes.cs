namespace Geometrie.Core;

/// <summary>Tout ce que l'application affiche sur une forme, au même endroit.</summary>
public sealed record InfosForme(
    TypeForme Type,
    string Nom,
    string Icone,
    string FormuleCourte,
    string FormuleAide,
    string Conseil,
    Func<ClasseCalcul> Creer);

public static class CatalogueFormes
{
    public static IReadOnlyList<InfosForme> Toutes { get; } =
    [
        new(TypeForme.Carre, "Carré", "◼",
            "Carré : P = 4 × côté, S = côté × côté",
            "Périmètre = 4 × côté\nSurface = côté × côté",
            "Pour un carré, tous les côtés sont égaux.",
            () => new ClasseCarre()),
        new(TypeForme.Rectangle, "Rectangle", "▭",
            "Rectangle : P = 2 × (longueur + largeur), S = longueur × largeur",
            "Périmètre = 2 × (longueur + largeur)\nSurface = longueur × largeur",
            "Pour un rectangle, les côtés opposés ont la même longueur.",
            () => new ClasseRectangle()),
        new(TypeForme.Cercle, "Cercle", "⚪",
            "Cercle : P = 2 × π × rayon, S = π × rayon²",
            "Périmètre = 2 × π × rayon\nSurface = π × rayon²",
            "Pour un cercle, pense à π ≈ 3,14.",
            () => new ClasseCercle()),
        new(TypeForme.Triangle, "Triangle", "🔺",
            "Triangle : P = base + côté 1 + côté 2, S = (base × hauteur) / 2",
            "Périmètre = base + côté 1 + côté 2\nSurface = (base × hauteur) / 2",
            "Pour un triangle, la surface se calcule avec base × hauteur ÷ 2.",
            () => new ClasseTriangle()),
        new(TypeForme.Ellipse, "Ellipse", "🥚",
            "Ellipse : P ≈ formule de Ramanujan, S = π × grand rayon × petit rayon",
            "Périmètre ≈ formule de Ramanujan\nSurface = π × grand rayon × petit rayon",
            "Pour une ellipse, la surface utilise le grand rayon et le petit rayon.",
            () => new ClasseEllipse()),
    ];

    public static InfosForme Infos(TypeForme type) => Toutes.First(infos => infos.Type == type);

    public static TypeForme Hasard(Random hasard) => Toutes[hasard.Next(Toutes.Count)].Type;

    /// <summary>Nouvelle forme du type demandé, avec des dimensions tirées au hasard.</summary>
    public static ClasseCalcul CreerAleatoire(TypeForme type, Random hasard)
    {
        ClasseCalcul forme = Infos(type).Creer();
        forme.Init(hasard);
        return forme;
    }
}
