namespace Geometrie.Core;

/// <summary>
/// Progression de l'enfant (score, niveau, badges, Mode Aventure). Elle est enregistrée telle quelle en JSON :
/// garder les noms des propriétés, et passer par <see cref="Normaliser"/> après chaque chargement.
/// </summary>
public class Progression
{
    public const int VersionActuelle = 2;
    public const int XpParNiveau = 100;
    public const int ReussitesPourExpert = 3;

    /// <summary>Version du format de sauvegarde, pour pouvoir migrer les anciennes sauvegardes.</summary>
    public int Version { get; set; } = VersionActuelle;
    public int Score { get; set; }
    public int Niveau { get; set; } = 1;
    public int XP { get; set; }
    public int SerieActuelle { get; set; }
    public int MeilleureSerie { get; set; }
    public int ExercicesValides { get; set; }
    public int ReussitesParfaites { get; set; }
    public List<string> Badges { get; set; } = [];

    /// <summary>Réussites parfaites par forme, indexées par le nom affiché de la forme (« Carré »…).</summary>
    public Dictionary<string, int> ReussitesParForme { get; set; } = [];
    public Dictionary<int, int> ReussitesAventure { get; set; } = [];

    public ProgressionResult EnregistrerResultat(
        TypeForme forme,
        bool perimetreCorrect,
        bool surfaceCorrect,
        bool defiRapide,
        bool formuleUtilisee = false)
    {
        bool toutCorrect = perimetreCorrect && surfaceCorrect;
        bool uneReponseCorrecte = perimetreCorrect || surfaceCorrect;
        int points = toutCorrect ? 10 : uneReponseCorrecte ? 5 : 0;
        int xpGagne = toutCorrect ? 20 : uneReponseCorrecte ? 10 : 0;

        if (formuleUtilisee)
        {
            xpGagne /= 2;
        }

        if (toutCorrect && defiRapide)
        {
            points += 5;
        }

        Score += points;
        XP += xpGagne;
        ExercicesValides++;

        if (toutCorrect)
        {
            string nom = CatalogueFormes.Infos(forme).Nom;
            SerieActuelle++;
            MeilleureSerie = Math.Max(MeilleureSerie, SerieActuelle);
            ReussitesParfaites++;
            ReussitesParForme[nom] = ReussitesParForme.GetValueOrDefault(nom) + 1;
        }
        else
        {
            SerieActuelle = 0;
        }

        bool niveauGagne = false;
        while (XP >= XpParNiveau)
        {
            XP -= XpParNiveau;
            Niveau++;
            niveauGagne = true;
        }

        List<string> nouveauxBadges = DebloquerBadges();

        return new ProgressionResult(points, xpGagne, niveauGagne, nouveauxBadges);
    }

    public void Reinitialiser()
    {
        Score = 0;
        Niveau = 1;
        XP = 0;
        SerieActuelle = 0;
        MeilleureSerie = 0;
        ExercicesValides = 0;
        ReussitesParfaites = 0;
        Badges.Clear();
        ReussitesParForme.Clear();
        ReussitesAventure.Clear();
    }

    /// <summary>Badge du mini-jeu secret : il donne aussi un niveau, une seule fois.</summary>
    public bool DebloquerPetitFilou()
    {
        if (Badges.Contains(CatalogueBadges.PetitFilou))
        {
            return false;
        }

        Badges.Add(CatalogueBadges.PetitFilou);
        Niveau++;
        DebloquerBadges();
        return true;
    }

    public bool NiveauAventureDebloque(int niveau)
    {
        if (!Aventure.Existe(niveau))
        {
            return false;
        }

        return niveau == 1 || ReussitesNiveauAventure(niveau - 1) >= Aventure.ReussitesParMonde;
    }

    public int ReussitesNiveauAventure(int niveau)
    {
        if (!Aventure.Existe(niveau))
        {
            return 0;
        }

        return Math.Clamp(ReussitesAventure.GetValueOrDefault(niveau), 0, Aventure.ReussitesParMonde);
    }

    public int EtoilesAventure(int niveau) => ReussitesNiveauAventure(niveau);

    public AventureResult EnregistrerReussiteAventure(int niveau)
    {
        bool dernierMonde = niveau == Aventure.NombreMondes;

        if (!NiveauAventureDebloque(niveau))
        {
            return new AventureResult(niveau, ReussitesNiveauAventure(niveau), false, false, false, false);
        }

        int avant = ReussitesNiveauAventure(niveau);

        if (avant >= Aventure.ReussitesParMonde)
        {
            return new AventureResult(niveau, avant, false, false, dernierMonde, false);
        }

        int apres = avant + 1;
        ReussitesAventure[niveau] = apres;

        bool mondeTermine = apres == Aventure.ReussitesParMonde;
        bool niveauSuivantDebloque = mondeTermine && !dernierMonde;
        bool aventureTerminee = mondeTermine && dernierMonde;
        bool badgeDebloque = false;

        if (aventureTerminee && !Badges.Contains(CatalogueBadges.AventurierDesFormes))
        {
            Badges.Add(CatalogueBadges.AventurierDesFormes);
            badgeDebloque = true;
        }

        return new AventureResult(niveau, apres, true, niveauSuivantDebloque, aventureTerminee, badgeDebloque);
    }

    /// <summary>
    /// Remet une progression chargée depuis le disque dans un état cohérent : listes absentes, valeurs négatives
    /// ou trop grandes, badges inconnus ou en double, ancien encodage des accents.
    /// </summary>
    public void Normaliser()
    {
        Badges ??= [];
        ReussitesParForme ??= [];
        ReussitesAventure ??= [];

        Score = Math.Max(0, Score);
        Niveau = Math.Max(1, Niveau);
        SerieActuelle = Math.Max(0, SerieActuelle);
        MeilleureSerie = Math.Max(SerieActuelle, MeilleureSerie);
        ExercicesValides = Math.Max(0, ExercicesValides);
        ReussitesParfaites = Math.Clamp(ReussitesParfaites, 0, ExercicesValides);

        XP = Math.Max(0, XP);
        while (XP >= XpParNiveau)
        {
            XP -= XpParNiveau;
            Niveau++;
        }

        HashSet<string> badgesConnus = [.. CatalogueBadges.Tous.Select(badge => badge.Nom)];
        Badges = [.. Badges
            .OfType<string>()
            .Select(badge => badge.Replace("3 bonnes rÃ©ponses", CatalogueBadges.TroisBonnesReponses, StringComparison.OrdinalIgnoreCase))
            .Where(badgesConnus.Contains)
            .Distinct()];

        foreach (string forme in ReussitesParForme.Keys.ToList())
        {
            ReussitesParForme[forme] = Math.Max(0, ReussitesParForme[forme]);
        }

        foreach (int niveau in ReussitesAventure.Keys.ToList())
        {
            if (Aventure.Existe(niveau))
            {
                ReussitesAventure[niveau] = Math.Clamp(ReussitesAventure[niveau], 0, Aventure.ReussitesParMonde);
            }
            else
            {
                ReussitesAventure.Remove(niveau);
            }
        }

        Version = VersionActuelle;
    }

    private List<string> DebloquerBadges()
    {
        var nouveauxBadges = new List<string>();

        AjouterBadge(CatalogueBadges.PremierExercice, ExercicesValides >= 1, nouveauxBadges);
        AjouterBadge(CatalogueBadges.TroisBonnesReponses, ReussitesParfaites >= 3, nouveauxBadges);

        foreach (InfosForme forme in CatalogueFormes.Toutes)
        {
            bool expert = ReussitesParForme.GetValueOrDefault(forme.Nom) >= ReussitesPourExpert;
            AjouterBadge(CatalogueBadges.Expert(forme.Type), expert, nouveauxBadges);
        }

        AjouterBadge(CatalogueBadges.Serie3, MeilleureSerie >= 3, nouveauxBadges);
        AjouterBadge(CatalogueBadges.Serie5, MeilleureSerie >= 5, nouveauxBadges);
        AjouterBadge(CatalogueBadges.Niveau5, Niveau >= 5, nouveauxBadges);

        return nouveauxBadges;
    }

    private void AjouterBadge(string badge, bool condition, List<string> nouveauxBadges)
    {
        if (!condition || Badges.Contains(badge))
        {
            return;
        }

        Badges.Add(badge);
        nouveauxBadges.Add(badge);
    }
}

public sealed record ProgressionResult(int PointsGagnes, int XpGagne, bool NiveauGagne, IReadOnlyList<string> NouveauxBadges);

public sealed record AventureResult(
    int Niveau,
    int Reussites,
    bool EtoileGagnee,
    bool NiveauSuivantDebloque,
    bool AventureTerminee,
    bool BadgeDebloque);
