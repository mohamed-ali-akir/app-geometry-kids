namespace Geometrie.Core;

public class ClasseEllipse : ClasseRond
{
    public double PR { get; set; }

    public override void Init()
    {
        UneValeur = Random.Shared.Next(2, 11);
        PR = Random.Shared.Next(1, (int)UneValeur + 1);
    }

    public double Perimetre()
    {
        double a = UneValeur;
        double b = PR;

        return Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
    }

    public double Surface()
    {
        return Math.PI * UneValeur * PR;
    }

    public string Dessin()
    {
        return "Ellipse";
    }
}
