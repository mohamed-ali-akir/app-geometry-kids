namespace Geometrie.Core;

public sealed record InfosBadge(string Nom, string Icone, string Description);

/// <summary>
/// Liste officielle des badges. Les noms sont enregistrés dans la sauvegarde de l'enfant :
/// ne pas les renommer sans prévoir une migration dans <see cref="Progression.Normaliser"/>.
/// </summary>
public static class CatalogueBadges
{
    public const string PremierExercice = "Premier exercice";
    public const string TroisBonnesReponses = "3 bonnes réponses";
    public const string Niveau5 = "Niveau 5";
    public const string Serie3 = "Série de 3";
    public const string Serie5 = "Série de 5";
    public const string PetitFilou = "Petit filou";
    public const string AventurierDesFormes = "Aventurier des formes";

    public static IReadOnlyList<InfosBadge> Tous { get; } =
    [
        new(PremierExercice, "🎯", "Valide ton premier exercice."),
        new(TroisBonnesReponses, "✅", "Réussis entièrement 3 exercices."),
        .. CatalogueFormes.Toutes.Select(forme =>
            new InfosBadge(Expert(forme.Type), forme.Icone, $"Réussis 3 {forme.Nom.ToLowerInvariant()}s.")),
        new(Niveau5, "🏆", "Atteins le niveau 5."),
        new(Serie3, "🔥", "Réussis 3 exercices d'affilée."),
        new(Serie5, "⚡", "Réussis 5 exercices d'affilée."),
        new(PetitFilou, "\U0001F575️", "Trouve le mini-jeu secret."),
        new(AventurierDesFormes, "🗺️", "Tu as terminé le Mode Aventure."),
    ];

    public static string Expert(TypeForme forme) => $"Expert {CatalogueFormes.Infos(forme).Nom.ToLowerInvariant()}";

    public static string Icone(string nom) => Tous.FirstOrDefault(badge => badge.Nom == nom)?.Icone ?? "🏅";
}
