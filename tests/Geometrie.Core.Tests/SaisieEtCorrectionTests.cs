using Geometrie.Core;

namespace Geometrie.Core.Tests;

public class SaisieEtCorrectionTests
{
    [Theory]
    [InlineData("5", 5)]
    [InlineData("2,5", 2.5)]
    [InlineData("2.5", 2.5)]
    [InlineData("  7 ", 7)]
    [InlineData(",5", 0.5)]
    public void LesNombresDeLEnfantSontLus(string texte, double attendu)
    {
        Assert.True(LecteurNombre.EssayerLirePositif(texte, 1000, out double valeur));
        Assert.Equal(attendu, valeur);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("∞")]
    [InlineData("1e400")]
    [InlineData("1e2")]
    [InlineData("1 000")]
    [InlineData("2,5,1")]
    [InlineData("1001")]
    public void LesSaisiesImpossiblesSontRefusees(string? texte)
    {
        Assert.False(LecteurNombre.EssayerLirePositif(texte, 1000, out double valeur));
        Assert.Equal(0, valeur);
    }

    [Fact]
    public void UneReponseArrondieAuCentiemeEstJuste()
    {
        double attendu = new ClasseCercle { UneValeur = 3 }.Surface(); // 28,2743…

        Assert.True(Correcteur.EstCorrect(28.27, attendu, utilisePi: true));
        Assert.False(Correcteur.EstCorrect(28.2, attendu, utilisePi: true));
    }

    [Theory]
    [InlineData(3, 28.26)]  // 3,14 × 3²
    [InlineData(7, 153.86)] // 3,14 × 7²
    [InlineData(10, 314)]   // 3,14 × 10²
    public void UneReponseCalculeeAvecPiEgalTroisQuatorzeEstJuste(double rayon, double reponse)
    {
        var cercle = new ClasseCercle { UneValeur = rayon };

        Assert.True(Correcteur.EstCorrect(reponse, cercle.Surface(), cercle.UtilisePi));
    }

    [Fact]
    public void PiEgalTroisQuatorzeNeCompteQuePourLesFormesRondes()
    {
        // 314 = 3,14 × 100 : juste pour un cercle de rayon 10, faux pour une forme sans π qui vaut 100π.
        Assert.False(Correcteur.EstCorrect(314, 100 * Math.PI, utilisePi: false));
        Assert.True(Correcteur.EstCorrect(314, 100 * Math.PI, utilisePi: true));
    }

    [Fact]
    public void UneReponseNonFinieEstFausse()
    {
        Assert.False(Correcteur.EstCorrect(double.NaN, double.NaN, utilisePi: false));
        Assert.False(Correcteur.EstCorrect(double.PositiveInfinity, double.PositiveInfinity, utilisePi: true));
    }
}
