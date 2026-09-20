namespace Geometrie.Core;

public class ClasseTriangle : ClasseAvecAngle
{
    public double C1 { get; set; }
    public double C2 { get; set; }
    public double Haut { get; set; }

    public override void Init()
    {
        do
        {
            UneValeur = Random.Shared.Next(2, 11);
            C1 = Random.Shared.Next(2, 11);
            C2 = Random.Shared.Next(2, 11);
        }
        while (!EstCoherent());

        Haut = Random.Shared.Next(1, 11);
    }

    public bool EstCoherent()
    {
        return UneValeur > 0
            && C1 > 0
            && C2 > 0
            && UneValeur + C1 > C2
            && UneValeur + C2 > C1
            && C1 + C2 > UneValeur;
    }

    public double Perimetre()
    {
        return UneValeur + C1 + C2;
    }

    public double Surface()
    {
        return (UneValeur * Haut) / 2;
    }

    public string Dessin()
    {
        return "Triangle";
    }
}
