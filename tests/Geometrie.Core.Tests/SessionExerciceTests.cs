using Geometrie.Core;

namespace Geometrie.Core.Tests;

public class SessionExerciceTests
{
    private static SessionExercice NouvelleSession(Progression? progression = null) =>
        new(progression ?? new Progression(), new Random(2024));

    private static ResultatValidation RepondreJuste(SessionExercice session, ClasseCalcul forme) =>
        session.Valider(forme, forme.Perimetre(), forme.Surface());

    [Fact]
    public void GenererSansFormeChoisieNeDonneRien()
    {
        Assert.Null(NouvelleSession().Generer());
    }

    [Fact]
    public void UneBonneReponseEstCompteeUneSeuleFois()
    {
        SessionExercice session = NouvelleSession();
        session.ChoisirForme(TypeForme.Carre);
        ClasseCalcul exercice = session.Generer()!;

        ResultatValidation premiere = RepondreJuste(session, exercice);
        ResultatValidation deuxieme = RepondreJuste(session, exercice);

        Assert.True(premiere.ToutCorrect);
        Assert.False(premiere.DejaCompte);
        Assert.True(deuxieme.DejaCompte);
        Assert.Equal(10, session.Progression.Score);
    }

    [Fact]
    public void LesDimensionsSaisiesParLEnfantSontCorrigees()
    {
        SessionExercice session = NouvelleSession();
        session.ChoisirForme(TypeForme.Rectangle);

        ResultatValidation resultat = session.Valider(new ClasseRectangle { UneValeur = 3, Largeur = 2 }, 10, 7);

        Assert.True(resultat.PerimetreCorrect);
        Assert.False(resultat.SurfaceCorrect);
        Assert.Equal(6, resultat.SurfaceAttendue);
        Assert.False(resultat.DimensionsModifiees);
    }

    [Fact]
    public void LeDefiRapideDonneSonBonusPuisSArrete()
    {
        SessionExercice session = NouvelleSession();
        ClasseCalcul defi = session.LancerDefiRapide();

        Assert.Equal(ModeExercice.DefiRapide, session.Mode);
        Assert.Equal(defi.Type, session.FormeChoisie);

        RepondreJuste(session, defi);

        Assert.Equal(15, session.Progression.Score);
        Assert.Equal(ModeExercice.Libre, session.Mode);
    }

    [Fact]
    public void LeBonusDuDefiNeComptePasSiLEnfantChangeLesDimensions()
    {
        SessionExercice session = NouvelleSession();
        ClasseCalcul defi = session.LancerDefiRapide();
        ClasseCalcul facile = CatalogueFormes.Infos(defi.Type).Creer();
        facile.Init(new Random(99));
        facile.UneValeur = defi.UneValeur + 1;

        ResultatValidation resultat = RepondreJuste(session, facile);

        Assert.True(resultat.DimensionsModifiees);
        Assert.Equal(10, session.Progression.Score);
    }

    [Fact]
    public void UtiliserLaFormuleDiviseLXpDeLExerciceEnCours()
    {
        SessionExercice session = NouvelleSession();
        session.ChoisirForme(TypeForme.Cercle);
        ClasseCalcul exercice = session.Generer()!;
        session.UtiliserFormule();

        ResultatValidation resultat = RepondreJuste(session, exercice);

        Assert.Equal(10, resultat.Progression!.XpGagne);

        session.Generer();
        Assert.False(session.FormuleUtilisee);
    }

    [Fact]
    public void UnMondeVerrouilleNePeutPasEtreLance()
    {
        SessionExercice session = NouvelleSession();

        Assert.Null(session.LancerAventure(2));
        Assert.Null(session.NiveauAventure);
    }

    [Fact]
    public void LAventurePasseAuMondeSuivantApresTroisReussites()
    {
        SessionExercice session = NouvelleSession();
        ClasseCalcul exercice = session.LancerAventure(1)!;

        Assert.Equal(TypeForme.Carre, exercice.Type);

        for (int i = 0; i < Aventure.ReussitesParMonde; i++)
        {
            RepondreJuste(session, exercice);
            exercice = session.Generer()!;
        }

        Assert.Equal(2, session.NiveauAventure);
        Assert.Equal(TypeForme.Rectangle, exercice.Type);
        Assert.True(session.Progression.NiveauAventureDebloque(2));
    }

    [Fact]
    public void ChangerLesDimensionsEnAventureNeDonnePasDEtoile()
    {
        SessionExercice session = NouvelleSession();
        ClasseCalcul exercice = session.LancerAventure(1)!;

        ResultatValidation resultat = RepondreJuste(session, new ClasseCarre { UneValeur = exercice.UneValeur + 1 });

        Assert.True(resultat.ToutCorrect);
        Assert.True(resultat.DimensionsModifiees);
        Assert.Null(resultat.Aventure);
        Assert.Equal(0, session.Progression.EtoilesAventure(1));
        Assert.Equal(1, session.NiveauAventure);
    }

    [Fact]
    public void TerminerLeDernierMondeQuitteLAventure()
    {
        var progression = new Progression();
        for (int niveau = 1; niveau < Aventure.NombreMondes; niveau++)
        {
            for (int i = 0; i < Aventure.ReussitesParMonde; i++)
            {
                progression.EnregistrerReussiteAventure(niveau);
            }
        }

        SessionExercice session = NouvelleSession(progression);
        ClasseCalcul exercice = session.LancerAventure(Aventure.NombreMondes)!;
        ResultatValidation? resultat = null;

        for (int i = 0; i < Aventure.ReussitesParMonde; i++)
        {
            resultat = RepondreJuste(session, exercice);
            exercice = session.NiveauAventure is null ? exercice : session.Generer()!;
        }

        Assert.True(resultat!.Aventure!.AventureTerminee);
        Assert.Null(session.NiveauAventure);
        Assert.Equal(ModeExercice.Libre, session.Mode);
        Assert.Contains(CatalogueBadges.AventurierDesFormes, progression.Badges);
    }

    [Fact]
    public void ChangerDeFormeAbandonneLAventure()
    {
        SessionExercice session = NouvelleSession();
        session.LancerAventure(1);

        Assert.False(session.ChoisirForme(TypeForme.Carre));
        Assert.Equal(1, session.NiveauAventure);

        Assert.True(session.ChoisirForme(TypeForme.Cercle));
        Assert.Null(session.NiveauAventure);
        Assert.Null(session.ExerciceEnCours);
        Assert.Equal(ModeExercice.Libre, session.Mode);
    }
}
