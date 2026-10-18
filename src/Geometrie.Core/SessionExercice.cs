namespace Geometrie.Core;

public enum ModeExercice
{
    Libre,
    DefiRapide,
    Aventure
}

public sealed record ResultatValidation(
    bool PerimetreCorrect,
    bool SurfaceCorrect,
    double PerimetreAttendu,
    double SurfaceAttendue,
    ProgressionResult? Progression,
    AventureResult? Aventure,
    bool DimensionsModifiees)
{
    public bool ToutCorrect => PerimetreCorrect && SurfaceCorrect;

    public bool UneReponseCorrecte => PerimetreCorrect || SurfaceCorrect;

    /// <summary>Vrai si l'exercice avait déjà été compté : on corrige, mais sans redonner de points.</summary>
    public bool DejaCompte => Progression is null;
}

/// <summary>
/// Déroulement d'une séance : forme choisie, exercice en cours, défi rapide, Mode Aventure et indice.
/// Toutes les règles sont ici pour pouvoir être testées sans l'interface.
/// </summary>
public sealed class SessionExercice
{
    private readonly Random hasard;
    private string? dernierExerciceRecompense;

    public SessionExercice(Progression progression, Random? hasard = null)
    {
        ArgumentNullException.ThrowIfNull(progression);
        Progression = progression;
        this.hasard = hasard ?? Random.Shared;
    }

    public Progression Progression { get; }

    public TypeForme? FormeChoisie { get; private set; }

    /// <summary>Exercice proposé par l'application (null si l'enfant n'en a pas encore généré).</summary>
    public ClasseCalcul? ExerciceEnCours { get; private set; }

    public ModeExercice Mode { get; private set; } = ModeExercice.Libre;

    /// <summary>Monde du Mode Aventure en cours, ou null hors aventure.</summary>
    public int? NiveauAventure { get; private set; }

    public bool FormuleUtilisee { get; private set; }

    /// <summary>Change de forme. Renvoie <c>false</c> si c'était déjà la forme choisie (rien n'est remis à zéro).</summary>
    public bool ChoisirForme(TypeForme? forme)
    {
        if (forme == FormeChoisie)
        {
            return false;
        }

        Reinitialiser();
        FormeChoisie = forme;
        return true;
    }

    /// <summary>Abandonne l'exercice, le défi et l'aventure en cours.</summary>
    public void Reinitialiser()
    {
        FormeChoisie = null;
        ExerciceEnCours = null;
        Mode = ModeExercice.Libre;
        NiveauAventure = null;
        FormuleUtilisee = false;
        dernierExerciceRecompense = null;
    }

    /// <summary>Nouvel exercice sur la forme choisie (ou le suivant du monde en cours, en Mode Aventure).</summary>
    public ClasseCalcul? Generer()
    {
        if (NiveauAventure is int niveau)
        {
            return LancerAventure(niveau);
        }

        if (FormeChoisie is not TypeForme forme)
        {
            return null;
        }

        Mode = ModeExercice.Libre;
        return NouvelExercice(forme);
    }

    public ClasseCalcul LancerDefiRapide()
    {
        TypeForme forme = CatalogueFormes.Hasard(hasard);
        FormeChoisie = forme;
        Mode = ModeExercice.DefiRapide;
        NiveauAventure = null;
        return NouvelExercice(forme);
    }

    /// <summary>Exercice du monde demandé, ou null si ce monde est encore verrouillé.</summary>
    public ClasseCalcul? LancerAventure(int niveau)
    {
        if (!Progression.NiveauAventureDebloque(niveau))
        {
            return null;
        }

        TypeForme forme = Aventure.Monde(niveau).FormeExercice(hasard);
        FormeChoisie = forme;
        Mode = ModeExercice.Aventure;
        NiveauAventure = niveau;
        return NouvelExercice(forme);
    }

    public void UtiliserFormule() => FormuleUtilisee = true;

    /// <summary>
    /// Corrige les réponses de l'enfant pour la forme qu'il a sous les yeux. Les bonus du défi rapide et les
    /// étoiles de l'aventure ne comptent que si les dimensions proposées n'ont pas été changées.
    /// </summary>
    public ResultatValidation Valider(ClasseCalcul forme, double perimetre, double surface)
    {
        ArgumentNullException.ThrowIfNull(forme);

        string signature = forme.Signature();
        bool exerciceImpose = ExerciceEnCours?.Signature() == signature;
        bool dimensionsModifiees = Mode != ModeExercice.Libre && !exerciceImpose;

        double perimetreAttendu = forme.Perimetre();
        double surfaceAttendue = forme.Surface();
        bool perimetreCorrect = Correcteur.EstCorrect(perimetre, perimetreAttendu, forme.UtilisePi);
        bool surfaceCorrect = Correcteur.EstCorrect(surface, surfaceAttendue, forme.UtilisePi);

        ProgressionResult? progression = null;
        AventureResult? aventure = null;

        if (signature != dernierExerciceRecompense)
        {
            bool defiRapide = Mode == ModeExercice.DefiRapide && exerciceImpose;
            progression = Progression.EnregistrerResultat(forme.Type, perimetreCorrect, surfaceCorrect, defiRapide, FormuleUtilisee);
            dernierExerciceRecompense = signature;

            if (perimetreCorrect && surfaceCorrect && exerciceImpose && NiveauAventure is int niveau)
            {
                aventure = Progression.EnregistrerReussiteAventure(niveau);

                if (aventure.NiveauSuivantDebloque)
                {
                    NiveauAventure = niveau + 1;
                }
                else if (aventure.AventureTerminee)
                {
                    NiveauAventure = null;
                    Mode = ModeExercice.Libre;
                }
            }
        }

        // Le bonus du défi rapide ne vaut que pour une tentative.
        if (Mode == ModeExercice.DefiRapide)
        {
            Mode = ModeExercice.Libre;
        }

        return new ResultatValidation(
            perimetreCorrect,
            surfaceCorrect,
            perimetreAttendu,
            surfaceAttendue,
            progression,
            aventure,
            dimensionsModifiees);
    }

    private ClasseCalcul NouvelExercice(TypeForme forme)
    {
        ExerciceEnCours = CatalogueFormes.CreerAleatoire(forme, hasard);
        FormuleUtilisee = false;
        dernierExerciceRecompense = null;
        return ExerciceEnCours;
    }
}
