using System.Globalization;

namespace Geometrie.Core;

/// <summary>Lecture des nombres tapés par l'enfant, avec une virgule ou un point.</summary>
public static class LecteurNombre
{
    // Pas d'exposant, de signe ni de séparateur de milliers : « NaN », « 1e400 » ou « -3 » sont refusés.
    private const NumberStyles Styles = NumberStyles.AllowDecimalPoint
        | NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite;

    /// <summary>Vrai si <paramref name="texte"/> est un nombre strictement positif, au plus égal à <paramref name="maximum"/>.</summary>
    public static bool EssayerLirePositif(string? texte, double maximum, out double valeur)
    {
        string normalise = (texte ?? string.Empty).Trim().Replace(',', '.');

        if (!double.TryParse(normalise, Styles, CultureInfo.InvariantCulture, out valeur)
            || !double.IsFinite(valeur)
            || valeur <= 0
            || valeur > maximum)
        {
            valeur = 0;
            return false;
        }

        return true;
    }
}
