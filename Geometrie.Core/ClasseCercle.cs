namespace Geometrie.Core;

public class ClasseCercle : ClasseRond
{
    public double Perimetre()
    {
        return 2 * Math.PI * UneValeur;
    }

    public double Surface()
    {
        return Math.PI * UneValeur * UneValeur;
    }

    public string Dessin()
    {
        return "Cercle";
    }
}
