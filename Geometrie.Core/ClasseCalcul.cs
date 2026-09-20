namespace Geometrie.Core;

public abstract class ClasseCalcul
{
    public double UneValeur { get; set; }

    public virtual void Init()
    {
        UneValeur = Random.Shared.Next(1, 11);
    }

    public double Addition(double a, double b) => a + b;

    public double Multiplication(double a, double b) => a * b;
}
