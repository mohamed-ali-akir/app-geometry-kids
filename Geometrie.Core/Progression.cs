namespace Geometrie.Core;

public class Progression
{
    private const int XpParNiveau = 100;

    public int Score { get; set; }
    public int Niveau { get; set; } = 1;
    public int XP { get; set; }
    public int SerieActuelle { get; set; }
    public int MeilleureSerie { get; set; }
    public int ExercicesValides { get; set; }
    public int ReussitesParfaites { get; set; }
    public List<string> Badges { get; set; } = [];
    public Dictionary<string, int> ReussitesParForme { get; set; } = [];
    public Dictionary<int, int> ReussitesAventure { get; set; } = [];

    public ProgressionResult EnregistrerResultat(
        string forme,
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
            SerieActuelle++;
            MeilleureSerie = Math.Max(MeilleureSerie, SerieActuelle);
            ReussitesParfaites++;
            ReussitesParForme[forme] = ReussitesParForme.GetValueOrDefault(forme) + 1;
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

        List<string> nouveauxBadges = DebloquerBadges(forme);

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

    public bool DebloquerPetitFilou()
    {
        const string badgeSecret = "Petit filou";

        if (Badges.Contains(badgeSecret))
        {
            return false;
        }

        Badges.Add(badgeSecret);
        Niveau++;
        return true;
    }

    public bool NiveauAventureDebloque(int niveau)
    {
        if (niveau == 1)
        {
            return true;
        }

        if (niveau < 1 || niveau > 5)
        {
            return false;
        }

        return ReussitesNiveauAventure(niveau - 1) >= 3;
    }

    public int ReussitesNiveauAventure(int niveau)
    {
        if (niveau < 1 || niveau > 5)
        {
            return 0;
        }

        return Math.Clamp(ReussitesAventure.GetValueOrDefault(niveau), 0, 3);
    }

    public int EtoilesAventure(int niveau)
    {
        return ReussitesNiveauAventure(niveau);
    }

    public AventureResult EnregistrerReussiteAventure(int niveau)
    {
        if (!NiveauAventureDebloque(niveau))
        {
            return new AventureResult(niveau, ReussitesNiveauAventure(niveau), false, false, false, false);
        }

        int avant = ReussitesNiveauAventure(niveau);

        if (avant >= 3)
        {
            return new AventureResult(niveau, avant, false, false, niveau == 5, false);
        }

        int apres = avant + 1;
        ReussitesAventure[niveau] = apres;

        bool niveauSuivantDebloque = apres == 3 && niveau < 5;
        bool aventureTerminee = apres == 3 && niveau == 5;
        bool badgeDebloque = false;

        if (aventureTerminee && !Badges.Contains("Aventurier des formes"))
        {
            Badges.Add("Aventurier des formes");
            badgeDebloque = true;
        }

        return new AventureResult(niveau, apres, true, niveauSuivantDebloque, aventureTerminee, badgeDebloque);
    }

    private List<string> DebloquerBadges(string forme)
    {
        var nouveauxBadges = new List<string>();

        AjouterBadge("Premier exercice", ExercicesValides >= 1, nouveauxBadges);
        AjouterBadge("3 bonnes réponses", ReussitesParfaites >= 3, nouveauxBadges);
        AjouterBadge($"Expert {forme.ToLowerInvariant()}", ReussitesParForme.GetValueOrDefault(forme) >= 3, nouveauxBadges);
        AjouterBadge("Série de 3", MeilleureSerie >= 3, nouveauxBadges);
        AjouterBadge("Série de 5", MeilleureSerie >= 5, nouveauxBadges);
        AjouterBadge("Niveau 5", Niveau >= 5, nouveauxBadges);

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

public sealed record ProgressionResult(int PointsGagnes, int XpGagne, bool NiveauGagne, List<string> NouveauxBadges);
public sealed record AventureResult(
    int Niveau,
    int Reussites,
    bool EtoileGagnee,
    bool NiveauSuivantDebloque,
    bool AventureTerminee,
    bool BadgeDebloque);
