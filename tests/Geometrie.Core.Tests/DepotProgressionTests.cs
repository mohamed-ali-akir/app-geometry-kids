using Geometrie.Core;

namespace Geometrie.Core.Tests;

public sealed class DepotProgressionTests : IDisposable
{
    private readonly string dossier = Path.Combine(Path.GetTempPath(), "geometrie-tests-" + Guid.NewGuid().ToString("N"));

    private string Fichier => Path.Combine(dossier, "progression.json");

    public void Dispose()
    {
        if (Directory.Exists(dossier))
        {
            Directory.Delete(dossier, recursive: true);
        }
    }

    [Fact]
    public void SansSauvegardeOnCommenceUneNouvelleProgression()
    {
        ResultatChargement resultat = new DepotProgression(Fichier).Charger();

        Assert.Equal(EtatChargement.Nouvelle, resultat.Etat);
        Assert.Equal(0, resultat.Progression.Score);
    }

    [Fact]
    public void UneProgressionSauvegardeeEstRelueALIdentique()
    {
        var depot = new DepotProgression(Fichier);
        var progression = new Progression();
        progression.EnregistrerResultat(TypeForme.Triangle, true, true, defiRapide: false);
        progression.EnregistrerReussiteAventure(1);

        Assert.True(depot.Sauvegarder(progression));
        ResultatChargement resultat = new DepotProgression(Fichier).Charger();

        Assert.Equal(EtatChargement.Chargee, resultat.Etat);
        Assert.Equal(progression.Score, resultat.Progression.Score);
        Assert.Equal(progression.Badges, resultat.Progression.Badges);
        Assert.Equal(1, resultat.Progression.ReussitesParForme["Triangle"]);
        Assert.Equal(1, resultat.Progression.EtoilesAventure(1));
        Assert.False(File.Exists(Fichier + ".tmp"));
    }

    [Fact]
    public void UneAncienneSauvegardeSansVersionSeChargeEncore()
    {
        Directory.CreateDirectory(dossier);
        File.WriteAllText(Fichier, """
            {
              "Score": 40,
              "Niveau": 2,
              "XP": 20,
              "Badges": ["Premier exercice", "3 bonnes rÃ©ponses"],
              "ReussitesParForme": { "Carré": 3 }
            }
            """);

        ResultatChargement resultat = new DepotProgression(Fichier).Charger();

        Assert.Equal(EtatChargement.Chargee, resultat.Etat);
        Assert.Equal(40, resultat.Progression.Score);
        Assert.Equal([CatalogueBadges.PremierExercice, CatalogueBadges.TroisBonnesReponses], resultat.Progression.Badges);
        Assert.Empty(resultat.Progression.ReussitesAventure);
    }

    [Theory]
    [InlineData("{ ceci n'est pas du JSON")]
    [InlineData("null")]
    [InlineData("{ \"Score\": \"beaucoup\" }")]
    [InlineData("")]
    public void UneSauvegardeAbimeeEstMiseDeCoteAuLieuDEtreEcrasee(string contenu)
    {
        Directory.CreateDirectory(dossier);
        File.WriteAllText(Fichier, contenu);

        ResultatChargement resultat = new DepotProgression(Fichier).Charger();

        Assert.Equal(EtatChargement.Recuperee, resultat.Etat);
        Assert.NotNull(resultat.CopieDeSecours);
        Assert.Equal(contenu, File.ReadAllText(resultat.CopieDeSecours));
        Assert.Equal(0, resultat.Progression.Score);
    }

    [Fact]
    public void DesListesNullesDansLaSauvegardeNeFontPasPlanter()
    {
        Directory.CreateDirectory(dossier);
        File.WriteAllText(Fichier, """{ "Badges": null, "ReussitesParForme": null, "ReussitesAventure": null }""");

        Progression progression = new DepotProgression(Fichier).Charger().Progression;

        Assert.Empty(progression.Badges);
        progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);
        Assert.False(progression.NiveauAventureDebloque(2));
    }

    [Fact]
    public void UneSauvegardeIllisibleNEstJamaisEcrasee()
    {
        Directory.CreateDirectory(dossier);
        File.WriteAllText(Fichier, "{}");

        // Le fichier est verrouillé par un autre programme : impossible de le lire ou de le copier.
        using (new FileStream(Fichier, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var depot = new DepotProgression(Fichier);
            ResultatChargement resultat = depot.Charger();

            if (OperatingSystem.IsWindows())
            {
                Assert.Equal(EtatChargement.Illisible, resultat.Etat);
                Assert.False(depot.Sauvegarder(new Progression { Score = 1 }));
            }
        }

        Assert.Equal("{}", File.ReadAllText(Fichier));
    }

    [Fact]
    public void UnDisqueQuiRefuseLEcritureNeFaitPasPlanter()
    {
        // Le chemin de la sauvegarde est occupé par un dossier : l'écriture échoue.
        Directory.CreateDirectory(Fichier);

        Assert.False(new DepotProgression(Fichier).Sauvegarder(new Progression()));
    }
}
