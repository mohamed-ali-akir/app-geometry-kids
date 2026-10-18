namespace Geometrie.Core;

public sealed record MondeAventure(int Niveau, string Icone, string Nom, string Description, TypeForme? Forme)
{
    /// <summary>Le dernier monde mélange toutes les formes.</summary>
    public TypeForme FormeExercice(Random hasard) => Forme ?? CatalogueFormes.Hasard(hasard);
}

/// <summary>Règles et contenu du Mode Aventure.</summary>
public static class Aventure
{
    public const int ReussitesParMonde = 3;

    public static IReadOnlyList<MondeAventure> Mondes { get; } =
    [
        new(1, "🟦", "Village du Carré", "Exercices sur le carré", TypeForme.Carre),
        new(2, "▭", "Forêt du Rectangle", "Exercices sur le rectangle", TypeForme.Rectangle),
        new(3, "⚪", "Lac du Cercle", "Exercices sur le cercle", TypeForme.Cercle),
        new(4, "🔺", "Montagne du Triangle", "Exercices sur le triangle", TypeForme.Triangle),
        new(5, "🏰", "Château des Formes", "Défi final avec formes mélangées", null),
    ];

    public static int NombreMondes => Mondes.Count;

    public static bool Existe(int niveau) => niveau >= 1 && niveau <= NombreMondes;

    public static MondeAventure Monde(int niveau) => Mondes[niveau - 1];

    /// <summary>Nom du monde avec son article, pour les phrases (« le Village du Carré »).</summary>
    public static string NomDansUnePhrase(int niveau) => niveau switch
    {
        2 or 4 => $"la {Monde(niveau).Nom}",
        _ when Existe(niveau) => $"le {Monde(niveau).Nom}",
        _ => "le Mode Aventure"
    };

    public static string Etoiles(int reussites)
    {
        int etoiles = Math.Clamp(reussites, 0, ReussitesParMonde);
        return new string('★', etoiles) + new string('☆', ReussitesParMonde - etoiles);
    }
}
