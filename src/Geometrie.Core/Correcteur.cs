namespace Geometrie.Core;

/// <summary>Décide si une réponse de l'enfant est juste.</summary>
public static class Correcteur
{
    /// <summary>Écart accepté avec la bonne valeur (une réponse arrondie au centième est juste).</summary>
    public const double Tolerance = 0.01;

    /// <summary>La valeur de π que l'on apprend à l'école.</summary>
    public const double PiEcole = 3.14;

    /// <summary>
    /// Vrai si <paramref name="reponse"/> est proche de <paramref name="attendu"/>. Pour les formes rondes,
    /// une réponse calculée avec π ≈ 3,14 est aussi acceptée, comme on l'enseigne en classe.
    /// </summary>
    public static bool EstCorrect(double reponse, double attendu, bool utilisePi)
    {
        if (!double.IsFinite(reponse))
        {
            return false;
        }

        if (Math.Abs(reponse - attendu) <= Tolerance)
        {
            return true;
        }

        // Les périmètres et les surfaces des formes rondes sont proportionnels à π.
        return utilisePi && Math.Abs(reponse - attendu * PiEcole / Math.PI) <= Tolerance;
    }
}
