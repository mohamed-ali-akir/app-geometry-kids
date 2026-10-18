using Geometrie.Core;

namespace Geometrie.Core.Tests;

public class FormesTests
{
    private const int Precision = 6;

    [Fact]
    public void Carre()
    {
        var carre = new ClasseCarre { UneValeur = 5 };

        Assert.Equal(20, carre.Perimetre(), Precision);
        Assert.Equal(25, carre.Surface(), Precision);
    }

    [Fact]
    public void Rectangle()
    {
        var rectangle = new ClasseRectangle { UneValeur = 5, Largeur = 4 };

        Assert.Equal(18, rectangle.Perimetre(), Precision);
        Assert.Equal(20, rectangle.Surface(), Precision);
    }

    [Fact]
    public void Cercle()
    {
        var cercle = new ClasseCercle { UneValeur = 3 };

        Assert.Equal(2 * Math.PI * 3, cercle.Perimetre(), Precision);
        Assert.Equal(Math.PI * 9, cercle.Surface(), Precision);
    }

    [Fact]
    public void Triangle()
    {
        var triangle = new ClasseTriangle { UneValeur = 5, C1 = 3, C2 = 4, Haut = 2.4 };

        Assert.Equal(12, triangle.Perimetre(), Precision);
        Assert.Equal(6, triangle.Surface(), Precision);
    }

    [Fact]
    public void Ellipse()
    {
        var ellipse = new ClasseEllipse { UneValeur = 5, PR = 3 };

        Assert.Equal(Math.PI * 15, ellipse.Surface(), Precision);
    }

    [Fact]
    public void LePerimetreDUneEllipseAuxRayonsEgauxEstCeluiDuCercle()
    {
        var ellipse = new ClasseEllipse { UneValeur = 4, PR = 4 };

        Assert.Equal(new ClasseCercle { UneValeur = 4 }.Perimetre(), ellipse.Perimetre(), Precision);
    }

    [Fact]
    public void LesFormesSontPolymorphes()
    {
        ClasseCalcul[] formes =
        [
            new ClasseCarre { UneValeur = 2 },
            new ClasseRectangle { UneValeur = 2, Largeur = 3 },
            new ClasseCercle { UneValeur = 1 },
            new ClasseTriangle { UneValeur = 6, C1 = 5, C2 = 5, Haut = 4 },
            new ClasseEllipse { UneValeur = 2, PR = 1 },
        ];

        Assert.Equal([8, 10, 2 * Math.PI, 16, new ClasseEllipse { UneValeur = 2, PR = 1 }.Perimetre()], formes.Select(forme => forme.Perimetre()));
        Assert.Equal(["Carre", "Rectangle", "Cercle", "Triangle", "Ellipse"], formes.Select(forme => forme.Dessin()));
    }

    [Fact]
    public void LaSignatureDependDeLaFormeEtDesDimensions()
    {
        Assert.Equal(new ClasseCarre { UneValeur = 3 }.Signature(), new ClasseCarre { UneValeur = 3 }.Signature());
        Assert.NotEqual(new ClasseCarre { UneValeur = 3 }.Signature(), new ClasseCarre { UneValeur = 4 }.Signature());
        Assert.NotEqual(new ClasseCarre { UneValeur = 3 }.Signature(), new ClasseCercle { UneValeur = 3 }.Signature());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(ClasseCalcul.DimensionMax + 1)]
    public void UneDimensionImpossibleEstRefusee(double valeur)
    {
        Assert.NotNull(new ClasseCarre { UneValeur = valeur }.Valider());
    }

    [Fact]
    public void LePetitRayonNePeutPasDepasserLeGrandRayon()
    {
        Assert.NotNull(new ClasseEllipse { UneValeur = 2, PR = 5 }.Valider());
        Assert.Null(new ClasseEllipse { UneValeur = 5, PR = 2 }.Valider());
    }

    [Theory]
    [InlineData(TypeForme.Carre)]
    [InlineData(TypeForme.Rectangle)]
    [InlineData(TypeForme.Cercle)]
    [InlineData(TypeForme.Triangle)]
    [InlineData(TypeForme.Ellipse)]
    public void LesExercicesGeneresSontToujoursValides(TypeForme type)
    {
        var hasard = new Random(1234);

        for (int essai = 0; essai < 500; essai++)
        {
            ClasseCalcul forme = CatalogueFormes.CreerAleatoire(type, hasard);

            Assert.Equal(type, forme.Type);
            Assert.Null(forme.Valider());
        }
    }
}
