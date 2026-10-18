namespace Geometrie.Core;

public class ClasseCarre : Classe4Angles
{
    public override TypeForme Type => TypeForme.Carre;

    public override IReadOnlyList<double> Dimensions => [UneValeur];

    public override double Perimetre() => 4 * UneValeur;

    public override double Surface() => UneValeur * UneValeur;
}
