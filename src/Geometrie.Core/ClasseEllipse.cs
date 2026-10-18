namespace Geometrie.Core;

public class ClasseEllipse : ClasseRond
{
    /// <summary>Petit rayon (le grand rayon est <see cref="ClasseCalcul.UneValeur"/>).</summary>
    public double PR { get; set; }

    public override TypeForme Type => TypeForme.Ellipse;

    public override IReadOnlyList<double> Dimensions => [UneValeur, PR];

    public override void Init(Random hasard)
    {
        UneValeur = hasard.Next(2, 11);
        PR = hasard.Next(1, (int)UneValeur + 1);
    }

    /// <summary>Approximation de Ramanujan (il n'existe pas de formule exacte simple).</summary>
    public override double Perimetre()
    {
        double a = UneValeur;
        double b = PR;

        return Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
    }

    public override double Surface() => Math.PI * UneValeur * PR;

    public override string? Valider()
    {
        string? erreur = base.Valider();

        if (erreur is null && PR > UneValeur)
        {
            return "Le petit rayon ne peut pas être plus grand que le grand rayon.";
        }

        return erreur;
    }
}
