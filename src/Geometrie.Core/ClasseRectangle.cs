namespace Geometrie.Core;

public class ClasseRectangle : Classe4Angles
{
    public double Largeur { get; set; }

    public override TypeForme Type => TypeForme.Rectangle;

    public override IReadOnlyList<double> Dimensions => [UneValeur, Largeur];

    public override void Init(Random hasard)
    {
        base.Init(hasard);
        Largeur = hasard.Next(1, 11);
    }

    public override double Perimetre() => 2 * (UneValeur + Largeur);

    public override double Surface() => UneValeur * Largeur;
}
