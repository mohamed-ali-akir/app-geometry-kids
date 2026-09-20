namespace Geometrie.Core;

public class ClasseCarre : Classe4Angles
{
    public double Perimetre()
    {
        return 4 * UneValeur;
    }

    public double Surface()
    {
        return UneValeur * UneValeur;
    }

    public string Dessin()
    {
        return "Carre";
    }
}
