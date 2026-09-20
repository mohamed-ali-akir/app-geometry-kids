namespace Geometrie.Core;

public class ClasseRectangle : Classe4Angles
{
    public double Largeur { get; set; }

    public override void Init()
    {
        base.Init();
        Largeur = Random.Shared.Next(1, 11);
    }

    public double Perimetre()
    {
        return 2 * (UneValeur + Largeur);
    }

    public double Surface()
    {
        return UneValeur * Largeur;
    }

    public string Dessin()
    {
        return "Rectangle";
    }
}
