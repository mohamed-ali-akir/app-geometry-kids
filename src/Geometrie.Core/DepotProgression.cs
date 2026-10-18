using System.Globalization;
using System.Text.Json;

namespace Geometrie.Core;

public enum EtatChargement
{
    /// <summary>Aucune sauvegarde : première utilisation.</summary>
    Nouvelle,

    /// <summary>La sauvegarde a été relue normalement.</summary>
    Chargee,

    /// <summary>La sauvegarde était abîmée : une copie a été mise de côté et l'enfant repart de zéro.</summary>
    Recuperee,

    /// <summary>La sauvegarde n'a pas pu être lue ni copiée : on ne l'écrasera pas pendant cette session.</summary>
    Illisible
}

public sealed record ResultatChargement(Progression Progression, EtatChargement Etat, string? CopieDeSecours);

/// <summary>
/// Lit et écrit la progression sur le disque, sans jamais perdre une sauvegarde : l'écriture passe par un
/// fichier temporaire (une coupure de courant ne laisse pas un fichier à moitié écrit) et un fichier abîmé
/// est mis de côté au lieu d'être écrasé.
/// </summary>
public sealed class DepotProgression
{
    private static readonly JsonSerializerOptions OptionsJson = new() { WriteIndented = true };

    private bool ecritureInterdite;

    public DepotProgression(string fichier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fichier);
        Fichier = fichier;
    }

    public static string FichierParDefaut { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Geometrie",
        "progression.json");

    public string Fichier { get; }

    public ResultatChargement Charger()
    {
        if (!File.Exists(Fichier))
        {
            return new ResultatChargement(new Progression(), EtatChargement.Nouvelle, null);
        }

        try
        {
            string json = File.ReadAllText(Fichier);
            Progression progression = JsonSerializer.Deserialize<Progression>(json)
                ?? throw new JsonException("La sauvegarde est vide.");
            progression.Normaliser();
            return new ResultatChargement(progression, EtatChargement.Chargee, null);
        }
        catch (Exception erreur) when (erreur is JsonException or IOException or UnauthorizedAccessException)
        {
            string? copie = MettreDeCote();

            if (copie is null)
            {
                ecritureInterdite = true;
                return new ResultatChargement(new Progression(), EtatChargement.Illisible, null);
            }

            return new ResultatChargement(new Progression(), EtatChargement.Recuperee, copie);
        }
    }

    /// <summary>Enregistre la progression. Renvoie <c>false</c> si le disque a refusé l'écriture.</summary>
    public bool Sauvegarder(Progression progression)
    {
        ArgumentNullException.ThrowIfNull(progression);

        if (ecritureInterdite)
        {
            return false;
        }

        string temporaire = Fichier + ".tmp";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Fichier))!);

            using (var flux = new FileStream(temporaire, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(flux, progression, OptionsJson);
                flux.Flush(flushToDisk: true);
            }

            File.Move(temporaire, Fichier, overwrite: true);
            return true;
        }
        catch (Exception erreur) when (erreur is IOException or UnauthorizedAccessException)
        {
            TryDelete(temporaire);
            return false;
        }
    }

    private string? MettreDeCote()
    {
        string horodatage = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string copie = Path.ChangeExtension(Fichier, $".abimee-{horodatage}.json");

        try
        {
            File.Copy(Fichier, copie, overwrite: true);
            return copie;
        }
        catch (Exception erreur) when (erreur is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static void TryDelete(string fichier)
    {
        try
        {
            File.Delete(fichier);
        }
        catch (Exception erreur) when (erreur is IOException or UnauthorizedAccessException)
        {
            // Un fichier temporaire oublié ne gêne pas : il sera écrasé à la prochaine sauvegarde.
        }
    }
}
