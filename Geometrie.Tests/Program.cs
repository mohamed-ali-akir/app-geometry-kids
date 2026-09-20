using Geometrie.Core;

const double tolerance = 0.000001;

Tester("Périmètre du carré", 20, new ClasseCarre { UneValeur = 5 }.Perimetre());
Tester("Surface du carré", 25, new ClasseCarre { UneValeur = 5 }.Surface());
Tester("Périmètre du rectangle", 18, new ClasseRectangle { UneValeur = 5, Largeur = 4 }.Perimetre());
Tester("Surface du rectangle", 20, new ClasseRectangle { UneValeur = 5, Largeur = 4 }.Surface());
Tester("Périmètre du cercle", 2 * Math.PI * 3, new ClasseCercle { UneValeur = 3 }.Perimetre());
Tester("Surface du cercle", Math.PI * 9, new ClasseCercle { UneValeur = 3 }.Surface());
Tester("Périmètre du triangle", 12, new ClasseTriangle { UneValeur = 5, C1 = 3, C2 = 4, Haut = 4 }.Perimetre());
Tester("Surface du triangle", 10, new ClasseTriangle { UneValeur = 5, C1 = 3, C2 = 4, Haut = 4 }.Surface());
Tester("Surface de l'ellipse", Math.PI * 5 * 3, new ClasseEllipse { UneValeur = 5, PR = 3 }.Surface());

TesterProgression();

Console.WriteLine("Tous les tests de calcul et de progression sont réussis.");

static void Tester(string nom, double attendu, double obtenu)
{
    if (Math.Abs(attendu - obtenu) > tolerance)
    {
        throw new InvalidOperationException($"{nom} : attendu {attendu}, obtenu {obtenu}");
    }
}

static void TesterProgression()
{
    var progression = new Progression();

    ProgressionResult premier = progression.EnregistrerResultat("Carré", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false);
    TesterEntier("Score après réussite parfaite", 10, progression.Score);
    TesterEntier("XP après réussite parfaite", 20, progression.XP);
    TesterEntier("Série après réussite parfaite", 1, progression.SerieActuelle);
    TesterVrai("Badge Premier exercice", progression.Badges.Contains("Premier exercice"));
    TesterEntier("Points gagnés", 10, premier.PointsGagnes);

    progression.EnregistrerResultat("Carré", perimetreCorrect: true, surfaceCorrect: false, defiRapide: false);
    TesterEntier("Score après réussite partielle", 15, progression.Score);
    TesterEntier("XP après réussite partielle", 30, progression.XP);
    TesterEntier("Série remise à zéro après erreur", 0, progression.SerieActuelle);

    ProgressionResult aide = progression.EnregistrerResultat("Cercle", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false, formuleUtilisee: true);
    TesterEntier("Score normal avec formule", 25, progression.Score);
    TesterEntier("XP divisée par deux avec formule", 40, progression.XP);
    TesterEntier("XP gagnée avec formule", 10, aide.XpGagne);

    progression.EnregistrerResultat("Carré", perimetreCorrect: true, surfaceCorrect: true, defiRapide: true);
    TesterEntier("Bonus défi rapide", 40, progression.Score);

    progression.EnregistrerResultat("Carré", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false);
    progression.EnregistrerResultat("Carré", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false);
    TesterVrai("Badge 3 bonnes réponses", progression.Badges.Contains("3 bonnes réponses"));
    TesterVrai("Badge Expert carré", progression.Badges.Contains("Expert carré"));
    TesterVrai("Badge Série de 3", progression.Badges.Contains("Série de 3"));

    progression.EnregistrerResultat("Triangle", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false);
    TesterEntier("Niveau après 100 XP", 2, progression.Niveau);
    TesterEntier("Surplus XP conservé", 20, progression.XP);

    progression.EnregistrerResultat("Rectangle", perimetreCorrect: true, surfaceCorrect: true, defiRapide: false);
    TesterVrai("Badge Série de 5", progression.Badges.Contains("Série de 5"));

    int niveauAvantSecret = progression.Niveau;
    bool badgeSecretDebloque = progression.DebloquerPetitFilou();
    TesterVrai("Badge Petit filou débloqué", badgeSecretDebloque);
    TesterVrai("Badge Petit filou présent", progression.Badges.Contains("Petit filou"));
    TesterEntier("Bonus niveau secret", niveauAvantSecret + 1, progression.Niveau);

    bool badgeSecretDejaDebloque = progression.DebloquerPetitFilou();
    TesterVrai("Badge Petit filou unique", !badgeSecretDejaDebloque);
    TesterEntier("Bonus niveau secret unique", niveauAvantSecret + 1, progression.Niveau);

    var aventure = new Progression();
    TesterVrai("Aventure niveau 1 débloqué au départ", aventure.NiveauAventureDebloque(1));
    TesterVrai("Aventure niveau 2 verrouillé au départ", !aventure.NiveauAventureDebloque(2));

    AventureResult premiereEtoile = aventure.EnregistrerReussiteAventure(1);
    TesterVrai("Aventure première étoile gagnée", premiereEtoile.EtoileGagnee);
    TesterEntier("Aventure 1 étoile", 1, aventure.EtoilesAventure(1));
    TesterEntier("Aventure progression 1 sur 3", 1, aventure.ReussitesNiveauAventure(1));

    aventure.EnregistrerReussiteAventure(1);
    AventureResult troisiemeEtoile = aventure.EnregistrerReussiteAventure(1);
    TesterEntier("Aventure 3 étoiles", 3, aventure.EtoilesAventure(1));
    TesterVrai("Aventure niveau suivant débloqué", troisiemeEtoile.NiveauSuivantDebloque);
    TesterVrai("Aventure niveau 2 débloqué", aventure.NiveauAventureDebloque(2));

    aventure.EnregistrerReussiteAventure(1);
    TesterEntier("Aventure étoiles plafonnées", 3, aventure.EtoilesAventure(1));

    for (int niveau = 2; niveau <= 5; niveau++)
    {
        aventure.EnregistrerReussiteAventure(niveau);
        aventure.EnregistrerReussiteAventure(niveau);
        aventure.EnregistrerReussiteAventure(niveau);
    }

    TesterVrai("Badge Aventurier des formes", aventure.Badges.Contains("Aventurier des formes"));

    aventure.Reinitialiser();
    TesterVrai("Aventure reset niveau 1 débloqué", aventure.NiveauAventureDebloque(1));
    TesterVrai("Aventure reset niveau 2 verrouillé", !aventure.NiveauAventureDebloque(2));
    TesterEntier("Aventure reset étoiles", 0, aventure.EtoilesAventure(1));
    TesterVrai("Aventure reset badge final", !aventure.Badges.Contains("Aventurier des formes"));

    progression.Reinitialiser();
    TesterEntier("Score après remise à zéro", 0, progression.Score);
    TesterEntier("Niveau après remise à zéro", 1, progression.Niveau);
    TesterEntier("XP après remise à zéro", 0, progression.XP);
    TesterEntier("Série après remise à zéro", 0, progression.SerieActuelle);
    TesterEntier("Badges après remise à zéro", 0, progression.Badges.Count);
}

static void TesterEntier(string nom, int attendu, int obtenu)
{
    if (attendu != obtenu)
    {
        throw new InvalidOperationException($"{nom} : attendu {attendu}, obtenu {obtenu}");
    }
}

static void TesterVrai(string nom, bool condition)
{
    if (!condition)
    {
        throw new InvalidOperationException($"{nom} : condition fausse");
    }
}
