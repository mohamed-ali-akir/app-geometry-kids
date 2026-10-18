using System.Globalization;

namespace Geometrie.Core;

/// <summary>
/// Base commune de toutes les formes : chaque forme sait calculer son périmètre et sa surface,
/// se générer au hasard et vérifier que ses dimensions sont possibles.
/// </summary>
public abstract class ClasseCalcul
{
    /// <summary>Plus grande dimension acceptée : au-delà, le dessin et les nombres n'ont plus de sens pour un enfant.</summary>
    public const double DimensionMax = 1000;

    public double UneValeur { get; set; }

    public abstract TypeForme Type { get; }

    /// <summary>Vrai si les formules de la forme utilisent π (on accepte alors aussi les réponses calculées avec π ≈ 3,14).</summary>
    public virtual bool UtilisePi => false;

    /// <summary>Toutes les dimensions de la forme, dans un ordre fixe (sert à reconnaître un exercice).</summary>
    public abstract IReadOnlyList<double> Dimensions { get; }

    public abstract double Perimetre();

    public abstract double Surface();

    public string Dessin() => Type.ToString();

    public void Init() => Init(Random.Shared);

    public virtual void Init(Random hasard)
    {
        UneValeur = hasard.Next(1, 11);
    }

    /// <summary>Renvoie un message d'erreur pour l'enfant si les dimensions sont impossibles, sinon <c>null</c>.</summary>
    public virtual string? Valider()
    {
        foreach (double dimension in Dimensions)
        {
            if (!EstDimensionValide(dimension))
            {
                return $"Les dimensions doivent être des nombres positifs, jusqu'à {DimensionMax.ToString(CultureInfo.CurrentCulture)}.";
            }
        }

        return null;
    }

    /// <summary>Identifiant stable d'un exercice : même forme et mêmes dimensions donnent la même signature.</summary>
    public string Signature()
    {
        IEnumerable<string> valeurs = Dimensions.Select(dimension => dimension.ToString("R", CultureInfo.InvariantCulture));
        return $"{Type}:{string.Join(':', valeurs)}";
    }

    public static bool EstDimensionValide(double valeur) => double.IsFinite(valeur) && valeur > 0 && valeur <= DimensionMax;

    public static double Addition(double a, double b) => a + b;

    public static double Multiplication(double a, double b) => a * b;
}
