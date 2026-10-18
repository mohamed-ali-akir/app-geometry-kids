namespace Geometrie.Core;

public class ClasseCercle : ClasseRond
{
    public override TypeForme Type => TypeForme.Cercle;

    public override IReadOnlyList<double> Dimensions => [UneValeur];

    public override double Perimetre() => 2 * Math.PI * UneValeur;

    public override double Surface() => Math.PI * UneValeur * UneValeur;
}
