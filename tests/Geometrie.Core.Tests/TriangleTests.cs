using Geometrie.Core;

namespace Geometrie.Core.Tests;

public class TriangleTests
{
    [Fact]
    public void LesTrianglesGeneresOntUneHauteurCoherenteAvecLeursCotes()
    {
        var hasard = new Random(42);

        for (int essai = 0; essai < 1000; essai++)
        {
            var triangle = new ClasseTriangle();
            triangle.Init(hasard);

            Assert.True(triangle.EstCoherent());
            Assert.Equal(triangle.HauteurReelle(), triangle.Haut, 9);
            Assert.All(triangle.Dimensions, dimension => Assert.Equal(Math.Round(dimension), dimension));
        }
    }

    [Fact]
    public void LesTrianglesGeneresSontVaries()
    {
        var hasard = new Random(7);
        var signatures = new HashSet<string>();

        for (int essai = 0; essai < 200; essai++)
        {
            var triangle = new ClasseTriangle();
            triangle.Init(hasard);
            signatures.Add(triangle.Signature());
        }

        Assert.True(signatures.Count >= 10, $"Seulement {signatures.Count} triangles différents.");
    }

    [Fact]
    public void LaHauteurSeDeduitDesTroisCotes()
    {
        // Triangle 13-14-15 : la hauteur sur la base 14 vaut 12.
        var triangle = new ClasseTriangle { UneValeur = 14, C1 = 13, C2 = 15 };

        Assert.Equal(12, triangle.HauteurReelle(), 9);
        Assert.Equal(5, triangle.DecalageSommet(), 9);
    }

    [Fact]
    public void DesCotesImpossiblesSontRefuses()
    {
        var triangle = new ClasseTriangle { UneValeur = 10, C1 = 2, C2 = 3, Haut = 1 };

        Assert.False(triangle.EstCoherent());
        Assert.Equal("Les trois côtés ne peuvent pas former un triangle.", triangle.Valider());
    }

    [Fact]
    public void UneHauteurQuiNeCorrespondPasAuxCotesEstRefusee()
    {
        // Avec les côtés 3-4-5, la hauteur sur la base 5 vaut 2,4 : une hauteur de 10 est impossible.
        var triangle = new ClasseTriangle { UneValeur = 5, C1 = 3, C2 = 4, Haut = 10 };

        string? erreur = triangle.Valider();

        Assert.NotNull(erreur);
        Assert.Contains("hauteur", erreur, StringComparison.Ordinal);
    }

    [Fact]
    public void UneHauteurArrondieEstAcceptee()
    {
        // Triangle équilatéral de côté 5 : hauteur ≈ 4,33.
        Assert.Null(new ClasseTriangle { UneValeur = 5, C1 = 5, C2 = 5, Haut = 4.33 }.Valider());
        Assert.Null(new ClasseTriangle { UneValeur = 5, C1 = 5, C2 = 5, Haut = 4.3 }.Valider());
    }
}
