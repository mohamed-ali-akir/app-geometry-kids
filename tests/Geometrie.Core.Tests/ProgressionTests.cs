using Geometrie.Core;

namespace Geometrie.Core.Tests;

public class ProgressionTests
{
    [Fact]
    public void UneReussiteParfaiteDonneDixPointsEtVingtXp()
    {
        var progression = new Progression();

        ProgressionResult resultat = progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);

        Assert.Equal(10, progression.Score);
        Assert.Equal(20, progression.XP);
        Assert.Equal(1, progression.SerieActuelle);
        Assert.Equal(10, resultat.PointsGagnes);
        Assert.Contains(CatalogueBadges.PremierExercice, progression.Badges);
    }

    [Fact]
    public void UneReussitePartielleDonneLaMoitieEtCasseLaSerie()
    {
        var progression = new Progression();
        progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);

        progression.EnregistrerResultat(TypeForme.Carre, true, false, defiRapide: false);

        Assert.Equal(15, progression.Score);
        Assert.Equal(30, progression.XP);
        Assert.Equal(0, progression.SerieActuelle);
        Assert.Equal(1, progression.MeilleureSerie);
    }

    [Fact]
    public void LaFormuleDiviseLXpParDeuxMaisPasLeScore()
    {
        var progression = new Progression();

        ProgressionResult resultat = progression.EnregistrerResultat(TypeForme.Cercle, true, true, defiRapide: false, formuleUtilisee: true);

        Assert.Equal(10, progression.Score);
        Assert.Equal(10, resultat.XpGagne);
    }

    [Fact]
    public void LeDefiRapideDonneCinqPointsBonusSeulementSiToutEstJuste()
    {
        var progression = new Progression();

        progression.EnregistrerResultat(TypeForme.Carre, true, false, defiRapide: true);
        Assert.Equal(5, progression.Score);

        progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: true);
        Assert.Equal(20, progression.Score);
    }

    [Fact]
    public void CentXpFontGagnerUnNiveauEtLeSurplusEstGarde()
    {
        var progression = new Progression();

        for (int i = 0; i < 6; i++)
        {
            progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);
        }

        Assert.Equal(2, progression.Niveau);
        Assert.Equal(20, progression.XP);
    }

    [Fact]
    public void LesBadgesDeSerieEtDExpertSeDebloquent()
    {
        var progression = new Progression();

        for (int i = 0; i < 3; i++)
        {
            progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);
        }

        Assert.Contains(CatalogueBadges.TroisBonnesReponses, progression.Badges);
        Assert.Contains("Expert carré", progression.Badges);
        Assert.Contains(CatalogueBadges.Serie3, progression.Badges);
        Assert.DoesNotContain(CatalogueBadges.Serie5, progression.Badges);

        progression.EnregistrerResultat(TypeForme.Triangle, true, true, defiRapide: false);
        progression.EnregistrerResultat(TypeForme.Rectangle, true, true, defiRapide: false);

        Assert.Contains(CatalogueBadges.Serie5, progression.Badges);
        Assert.DoesNotContain("Expert triangle", progression.Badges);
    }

    [Fact]
    public void LesReussitesParFormeGardentLeNomAfficheCommeCle()
    {
        // Les anciennes sauvegardes utilisent « Carré » comme clé : il ne faut pas en changer.
        var progression = new Progression();

        progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);

        Assert.Equal(1, progression.ReussitesParForme["Carré"]);
    }

    [Fact]
    public void LeBadgeSecretDonneUnNiveauUneSeuleFois()
    {
        var progression = new Progression();

        Assert.True(progression.DebloquerPetitFilou());
        Assert.False(progression.DebloquerPetitFilou());
        Assert.Equal(2, progression.Niveau);
        Assert.Contains(CatalogueBadges.PetitFilou, progression.Badges);
    }

    [Fact]
    public void LeNiveauGagneParLeBadgeSecretPeutDebloquerLeBadgeNiveau5()
    {
        var progression = new Progression { Niveau = 4 };

        progression.DebloquerPetitFilou();

        Assert.Contains(CatalogueBadges.Niveau5, progression.Badges);
    }

    [Fact]
    public void LAventureSeDebloqueMondeParMonde()
    {
        var progression = new Progression();

        Assert.True(progression.NiveauAventureDebloque(1));
        Assert.False(progression.NiveauAventureDebloque(2));
        Assert.False(progression.NiveauAventureDebloque(0));
        Assert.False(progression.NiveauAventureDebloque(6));

        AventureResult premiere = progression.EnregistrerReussiteAventure(1);
        Assert.True(premiere.EtoileGagnee);
        Assert.Equal(1, progression.EtoilesAventure(1));

        progression.EnregistrerReussiteAventure(1);
        AventureResult troisieme = progression.EnregistrerReussiteAventure(1);

        Assert.True(troisieme.NiveauSuivantDebloque);
        Assert.True(progression.NiveauAventureDebloque(2));

        AventureResult enTrop = progression.EnregistrerReussiteAventure(1);
        Assert.False(enTrop.EtoileGagnee);
        Assert.Equal(3, progression.EtoilesAventure(1));
    }

    [Fact]
    public void UnMondeVerrouilleNeDonnePasDEtoile()
    {
        var progression = new Progression();

        AventureResult resultat = progression.EnregistrerReussiteAventure(3);

        Assert.False(resultat.EtoileGagnee);
        Assert.Equal(0, progression.EtoilesAventure(3));
    }

    [Fact]
    public void TerminerLAventureDonneLeBadgeFinal()
    {
        var progression = new Progression();

        AventureResult? dernier = null;
        for (int niveau = 1; niveau <= Aventure.NombreMondes; niveau++)
        {
            for (int i = 0; i < Aventure.ReussitesParMonde; i++)
            {
                dernier = progression.EnregistrerReussiteAventure(niveau);
            }
        }

        Assert.NotNull(dernier);
        Assert.True(dernier.AventureTerminee);
        Assert.True(dernier.BadgeDebloque);
        Assert.Contains(CatalogueBadges.AventurierDesFormes, progression.Badges);
    }

    [Fact]
    public void ReinitialiserRemetToutAZero()
    {
        var progression = new Progression();
        progression.EnregistrerResultat(TypeForme.Carre, true, true, defiRapide: false);
        progression.EnregistrerReussiteAventure(1);
        progression.DebloquerPetitFilou();

        progression.Reinitialiser();

        Assert.Equal(0, progression.Score);
        Assert.Equal(1, progression.Niveau);
        Assert.Equal(0, progression.XP);
        Assert.Equal(0, progression.SerieActuelle);
        Assert.Empty(progression.Badges);
        Assert.Equal(0, progression.EtoilesAventure(1));
        Assert.False(progression.NiveauAventureDebloque(2));
    }

    [Fact]
    public void NormaliserRepareUneProgressionIncoherente()
    {
        var progression = new Progression
        {
            Version = 1,
            Score = -5,
            Niveau = 0,
            XP = 250,
            SerieActuelle = 4,
            MeilleureSerie = 2,
            ExercicesValides = 3,
            ReussitesParfaites = 10,
            Badges = ["3 bonnes rÃ©ponses", "Badge inventé", CatalogueBadges.Serie3, CatalogueBadges.Serie3],
            ReussitesParForme = new() { ["Carré"] = -2 },
            ReussitesAventure = new() { [1] = 9, [42] = 3 },
        };

        progression.Normaliser();

        Assert.Equal(Progression.VersionActuelle, progression.Version);
        Assert.Equal(0, progression.Score);
        Assert.Equal(3, progression.Niveau);
        Assert.Equal(50, progression.XP);
        Assert.Equal(4, progression.MeilleureSerie);
        Assert.Equal(3, progression.ReussitesParfaites);
        Assert.Equal([CatalogueBadges.TroisBonnesReponses, CatalogueBadges.Serie3], progression.Badges);
        Assert.Equal(0, progression.ReussitesParForme["Carré"]);
        Assert.Equal(3, progression.ReussitesAventure[1]);
        Assert.False(progression.ReussitesAventure.ContainsKey(42));
    }

    [Fact]
    public void LeCatalogueDesBadgesNAPasDeDoublon()
    {
        Assert.Equal(12, CatalogueBadges.Tous.Count);
        Assert.Equal(CatalogueBadges.Tous.Count, CatalogueBadges.Tous.Select(badge => badge.Nom).Distinct().Count());
        Assert.Contains(CatalogueBadges.Tous, badge => badge.Nom == "Expert ellipse" && badge.Description == "Réussis 3 ellipses.");
    }
}
