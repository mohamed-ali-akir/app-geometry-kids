using System.Globalization;

namespace Geometrie.Core;

/// <summary>
/// Triangle décrit par sa base (<see cref="ClasseCalcul.UneValeur"/>), ses deux autres côtés
/// (<see cref="C1"/> à gauche, <see cref="C2"/> à droite) et sa hauteur relative à la base (<see cref="Haut"/>).
/// </summary>
public class ClasseTriangle : ClasseAvecAngle
{
    /// <summary>Écart relatif accepté entre la hauteur saisie et la vraie hauteur (pour les valeurs arrondies).</summary>
    public const double ToleranceHauteur = 0.02;

    private const int BaseMax = 20;

    /// <summary>
    /// Triangles dont toutes les dimensions sont entières et cohérentes entre elles, obtenus en accolant
    /// deux triangles rectangles à côtés entiers qui ont la même hauteur.
    /// </summary>
    private static readonly (int Base, int C1, int C2, int Haut)[] TrianglesEntiers = ConstruireTrianglesEntiers();

    public double C1 { get; set; }

    public double C2 { get; set; }

    public double Haut { get; set; }

    public override TypeForme Type => TypeForme.Triangle;

    public override IReadOnlyList<double> Dimensions => [UneValeur, C1, C2, Haut];

    public override void Init(Random hasard)
    {
        (int baseTriangle, int c1, int c2, int haut) = TrianglesEntiers[hasard.Next(TrianglesEntiers.Length)];
        UneValeur = baseTriangle;
        C1 = c1;
        C2 = c2;
        Haut = haut;
    }

    /// <summary>Vrai si les trois côtés peuvent former un triangle (inégalité triangulaire).</summary>
    public bool EstCoherent()
    {
        return UneValeur > 0
            && C1 > 0
            && C2 > 0
            && UneValeur + C1 > C2
            && UneValeur + C2 > C1
            && C1 + C2 > UneValeur;
    }

    /// <summary>Hauteur imposée par les trois côtés (formule de Héron).</summary>
    public double HauteurReelle()
    {
        double a = UneValeur;
        double demiPerimetre = (a + C1 + C2) / 2;
        double produit = demiPerimetre * (demiPerimetre - a) * (demiPerimetre - C1) * (demiPerimetre - C2);
        double aire = Math.Sqrt(Math.Max(0, produit));

        return 2 * aire / a;
    }

    /// <summary>Distance horizontale entre le coin gauche de la base et le pied de la hauteur (peut sortir de la base).</summary>
    public double DecalageSommet()
    {
        double a = UneValeur;
        return (a * a + C1 * C1 - C2 * C2) / (2 * a);
    }

    public override double Perimetre() => UneValeur + C1 + C2;

    public override double Surface() => UneValeur * Haut / 2;

    public override string? Valider()
    {
        string? erreur = base.Valider();

        if (erreur is not null)
        {
            return erreur;
        }

        if (!EstCoherent())
        {
            return "Les trois côtés ne peuvent pas former un triangle.";
        }

        double hauteurReelle = HauteurReelle();

        if (Math.Abs(Haut - hauteurReelle) > hauteurReelle * ToleranceHauteur)
        {
            string attendue = Math.Round(hauteurReelle, 2).ToString("0.##", CultureInfo.CurrentCulture);
            return $"Avec ces trois côtés, la hauteur doit être d'environ {attendue}.";
        }

        return null;
    }

    private static (int Base, int C1, int C2, int Haut)[] ConstruireTrianglesEntiers()
    {
        // Demi-triangles rectangles (hauteur, morceau de base, côté) ; un morceau de 0 donne un angle droit.
        var demis = new List<(int Haut, int Morceau, int Cote)>();

        for (int c = 1; c <= 15; c++)
        {
            for (int a = 1; a < c; a++)
            {
                for (int b = a; b < c; b++)
                {
                    if (a * a + b * b == c * c)
                    {
                        demis.Add((a, b, c));
                        demis.Add((b, a, c));
                    }
                }
            }
        }

        foreach (int haut in demis.Select(demi => demi.Haut).Distinct().ToList())
        {
            demis.Add((haut, 0, haut));
        }

        return [.. from gauche in demis
                   from droite in demis
                   where gauche.Haut == droite.Haut
                   let baseTriangle = gauche.Morceau + droite.Morceau
                   where baseTriangle > 0 && baseTriangle <= BaseMax
                   select (baseTriangle, gauche.Cote, droite.Cote, gauche.Haut)];
    }
}
