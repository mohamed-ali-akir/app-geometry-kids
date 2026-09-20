using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Geometrie.Core;
using IOPath = System.IO.Path;
using RectangleShape = System.Windows.Shapes.Rectangle;

namespace Geometrie
{
    public partial class MainWindow : Window
    {
        private const double Tolerance = 0.01;
        private static readonly string DossierSauvegarde = IOPath.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Geometrie");
        private static readonly string FichierSauvegarde = IOPath.Combine(DossierSauvegarde, "progression.json");

        private readonly string[] messagesReussite =
        [
            "Bravo champion ! Tu progresses super bien !",
            "Excellent travail, continue comme ça !",
            "Tu deviens un expert en géométrie !"
        ];

        private readonly string[] messagesPartiels =
        [
            "Bien joué, tu as trouvé une partie !",
            "Presque ! Tu es sur la bonne voie !",
            "Continue, tu vas y arriver !"
        ];

        private readonly string[] messagesErreur =
        [
            "Ce n'est pas grave, l'erreur aide à apprendre !",
            "Essaie encore, tu peux y arriver !",
            "Regarde la correction puis retente ta chance !"
        ];

        private readonly List<BadgeProgression> badgesModeles =
        [
            new() { Nom = "Premier exercice", Icone = "🎯", Description = "Valide ton premier exercice." },
            new() { Nom = "3 bonnes réponses", Icone = "✅", Description = "Réussis entièrement 3 exercices." },
            new() { Nom = "Expert carré", Icone = "◼", Description = "Réussis 3 carrés." },
            new() { Nom = "Expert rectangle", Icone = "▭", Description = "Réussis 3 rectangles." },
            new() { Nom = "Expert cercle", Icone = "⚪", Description = "Réussis 3 cercles." },
            new() { Nom = "Expert triangle", Icone = "🔺", Description = "Réussis 3 triangles." },
            new() { Nom = "Expert ellipse", Icone = "🥚", Description = "Réussis 3 ellipses." },
            new() { Nom = "Niveau 5", Icone = "🏆", Description = "Atteins le niveau 5." },
            new() { Nom = "Série de 3", Icone = "🔥", Description = "Réussis 3 exercices d'affilée." },
            new() { Nom = "Série de 5", Icone = "⚡", Description = "Réussis 5 exercices d'affilée." },
            new() { Nom = "Petit filou", Icone = "\U0001F575\uFE0F", Description = "Trouve le mini-jeu secret." },
            new() { Nom = "Aventurier des formes", Icone = "🗺️", Description = "Tu as terminé le Mode Aventure." }
        ];

        private ClasseCalcul? formeActuelle;
        private Progression progression = new();
        private bool defiRapideActif;
        private bool formuleUtilisee;
        private string? dernierExerciceRecompense;
        private int? niveauAventureActif;
        private string? mondeAventureActif;

        public MainWindow()
        {
            InitializeComponent();
            progression = ChargerProgression();
            AfficherChampsSelonForme(null);
            MettreAJourBoutonsFormes(null);
            MettreAJourProgression();
            MettreAJourBoutonGenerer();
        }

        private void FormeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bouton && bouton.Tag is string forme)
            {
                SelectionnerForme(forme);
            }
        }

        private void FormeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            formeActuelle = null;
            defiRapideActif = false;
            formuleUtilisee = false;
            dernierExerciceRecompense = null;
            niveauAventureActif = null;
            mondeAventureActif = null;
            MettreAJourBoutonGenerer();
            FormeCanvas.Children.Clear();
            DimensionsTextBlock.Text = string.Empty;
            PerimetreTextBox.Clear();
            SurfaceTextBox.Clear();
            string? forme = FormeChoisie();
            AfficherChampsSelonForme(forme);
            MettreAJourBoutonsFormes(forme);
            MettreAJourMascotte(forme);
            AfficherResultat("Entre tes dimensions ou clique sur Générer un exercice.", "#FFF7D6", "#F3D36B");
        }

        private void GenererButton_Click(object sender, RoutedEventArgs e)
        {
            if (niveauAventureActif is int niveauAventure)
            {
                LancerExerciceAventure(niveauAventure);
                return;
            }

            string? forme = FormeChoisie();

            if (forme is null)
            {
                AfficherResultat("Choisis d'abord une forme.", "#FFE7E7", "#E89A9A");
                return;
            }

            defiRapideActif = false;
            niveauAventureActif = null;
            mondeAventureActif = null;
            MettreAJourBoutonGenerer();
            GenererExercice(forme, "À toi de jouer ! Calcule le périmètre et la surface.");
        }

        private void DefiRapideButton_Click(object sender, RoutedEventArgs e)
        {
            string[] formes = ["Carré", "Rectangle", "Cercle", "Triangle", "Ellipse"];
            string forme = formes[Random.Shared.Next(formes.Length)];
            SelectionnerForme(forme);
            formuleUtilisee = false;
            defiRapideActif = true;
            niveauAventureActif = null;
            mondeAventureActif = null;
            MettreAJourBoutonGenerer();
            GenererExercice(forme, "Défi rapide lancé ! Si tout est juste, tu gagnes 5 points bonus.");
        }

        private void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConstruireFormeDepuisSaisie(out ClasseCalcul? forme) || forme is null)
            {
                return;
            }

            if (!LireNombrePositif(PerimetreTextBox, "le périmètre", out double perimetreUtilisateur)
                || !LireNombrePositif(SurfaceTextBox, "la surface", out double surfaceUtilisateur))
            {
                return;
            }

            formeActuelle = forme;
            DessinerForme(formeActuelle);
            AfficherDimensions(formeActuelle);

            string formeTexte = FormeChoisie() ?? "Forme";
            string signatureExercice = CreerSignatureExercice(formeTexte, formeActuelle);
            double perimetreAttendu = CalculerPerimetre(formeActuelle);
            double surfaceAttendue = CalculerSurface(formeActuelle);
            bool perimetreOk = Math.Abs(perimetreUtilisateur - perimetreAttendu) <= Tolerance;
            bool surfaceOk = Math.Abs(surfaceUtilisateur - surfaceAttendue) <= Tolerance;
            bool toutCorrect = perimetreOk && surfaceOk;
            bool uneReponseCorrecte = perimetreOk || surfaceOk;
            string message = MessageAleatoire(toutCorrect, uneReponseCorrecte);
            string progressionMessage;

            if (signatureExercice != dernierExerciceRecompense)
            {
                ProgressionResult resultat = progression.EnregistrerResultat(formeTexte, perimetreOk, surfaceOk, defiRapideActif, formuleUtilisee);
                dernierExerciceRecompense = signatureExercice;
                progressionMessage = CreerMessageProgression(resultat);

                if (toutCorrect && niveauAventureActif is int niveauAventure)
                {
                    AventureResult aventure = progression.EnregistrerReussiteAventure(niveauAventure);
                    progressionMessage += CreerMessageAventure(aventure);

                    if (aventure.NiveauSuivantDebloque)
                    {
                        niveauAventureActif = aventure.Niveau + 1;
                        mondeAventureActif = NomMondeAventure(niveauAventureActif.Value);
                    }
                    else if (aventure.AventureTerminee)
                    {
                        niveauAventureActif = null;
                        mondeAventureActif = null;
                    }
                }

                SauvegarderProgression();
                MettreAJourProgression();
            }
            else
            {
                progressionMessage = "\nCet exercice a déjà été compté dans ta progression.";
            }

            MettreAJourBoutonGenerer();

            if (toutCorrect)
            {
                string serieMessage = progression.SerieActuelle switch
                {
                    3 => "\nSuper série !",
                    5 => "\nIncroyable, tu es lancé !",
                    _ => string.Empty
                };

                string fond = progressionMessage.Contains("Nouveau badge", StringComparison.OrdinalIgnoreCase)
                    ? "#F7E8FF"
                    : "#E8F8ED";
                string bordure = progressionMessage.Contains("Nouveau badge", StringComparison.OrdinalIgnoreCase)
                    ? "#B984E8"
                    : "#8BCB9A";

                AfficherResultat($"{message}{progressionMessage}{serieMessage}", fond, bordure);
                defiRapideActif = false;
                return;
            }

            string fondErreur = uneReponseCorrecte ? "#FFF4C2" : "#FFE2DD";
            string bordureErreur = uneReponseCorrecte ? "#E5BE4E" : "#E99B8F";

            AfficherResultat(
                $"{message}{progressionMessage}\n" +
                $"Périmètre attendu : {FormatNombre(perimetreAttendu)}\n" +
                $"Surface attendue : {FormatNombre(surfaceAttendue)}\n" +
                $"Formule : {Formule(formeActuelle)}",
                fondErreur,
                bordureErreur);

            defiRapideActif = false;
        }

        private void ReinitialiserButton_Click(object sender, RoutedEventArgs e)
        {
            NettoyerExercice();
        }

        private void ModeAventureButton_Click(object sender, RoutedEventArgs e)
        {
            var aventure = new ModeAventureWindow(progression)
            {
                Owner = this
            };

            if (aventure.ShowDialog() == true && aventure.NiveauChoisi is int niveau)
            {
                LancerExerciceAventure(niveau);
            }
        }

        private void FormuleButton_Click(object sender, RoutedEventArgs e)
        {
            string? forme = FormeChoisie();

            if (forme is null)
            {
                AfficherResultat("Choisis d'abord une forme pour voir la formule.", "#FFE7E7", "#E89A9A");
                return;
            }

            formuleUtilisee = true;
            string formule = FormuleAide(forme);

            MascotteTextBlock.Text =
                $"Géo le robot te donne un indice !\n{formule}\nAvec l'indice, tu gagnes moitié moins d'XP.";

            AfficherResultat(
                $"💡 Indice utilisé ! Tu gagnes 50% d'XP en moins pour cet exercice.\n{formule}",
                "#FFF7D6",
                "#E3BD45");
        }

        private void MiniJeuSecretButton_Click(object sender, RoutedEventArgs e)
        {
            var miniJeu = new MiniJeuSnakeWindow
            {
                Owner = this
            };

            miniJeu.ShowDialog();

            if (!miniJeu.MiniJeuQuitte)
            {
                return;
            }

            bool nouveauBadge = progression.DebloquerPetitFilou();
            SauvegarderProgression();
            MettreAJourProgression();

            if (nouveauBadge)
            {
                MascotteTextBlock.Text = "Géo le robot a vu ton clin d'oeil secret ! Badge Petit filou débloqué.";
                AfficherResultat(
                    "\U0001F575\uFE0F Badge secret débloqué : Petit filou ! Tu gagnes aussi +1 niveau, une seule fois.",
                    "#F7E8FF",
                    "#B984E8");
                return;
            }

            MascotteTextBlock.Text = "Le mini-jeu secret est toujours là si tu veux faire une pause.";
            AfficherResultat(
                "Tu as retrouvé le mini-jeu secret ! Le badge Petit filou est déjà débloqué.",
                "#FFF7D6",
                "#F3D36B");
        }

        private void ResetProgressionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult reponse = MessageBox.Show(
                "Voulez-vous vraiment remettre la progression à zéro ?",
                "Réinitialiser la progression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (reponse != MessageBoxResult.Yes)
            {
                return;
            }

            progression.Reinitialiser();
            SauvegarderProgression();
            MettreAJourProgression();
            AfficherResultat("La progression est remise à zéro. Nouveau départ !", "#FFF7D6", "#F3D36B");
        }

        private void QuitterButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult reponse = MessageBox.Show(
                "Voulez-vous vraiment quitter ?",
                "Quitter",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (reponse == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void GenererExercice(string forme, string message)
        {
            formeActuelle = CreerFormeAleatoire(forme);
            dernierExerciceRecompense = null;
            formuleUtilisee = false;
            RemplirDimensions(formeActuelle);
            DessinerForme(formeActuelle);
            AfficherDimensions(formeActuelle);
            PerimetreTextBox.Clear();
            SurfaceTextBox.Clear();
            AfficherResultat(message, "#E8F8ED", "#8BCB9A");
        }

        private void LancerExerciceAventure(int niveau)
        {
            if (!progression.NiveauAventureDebloque(niveau))
            {
                AfficherResultat("Ce monde est encore verrouillé. Gagne 3 étoiles dans le monde précédent.", "#FFE7E7", "#E89A9A");
                return;
            }

            string forme = FormeAventure(niveau);
            string monde = NomMondeAventure(niveau);
            SelectionnerForme(forme);
            niveauAventureActif = niveau;
            mondeAventureActif = monde;
            defiRapideActif = false;
            formuleUtilisee = false;
            MettreAJourBoutonGenerer();

            GenererExercice(
                forme,
                $"Mode Aventure : {monde} ! Réussis l'exercice pour gagner une étoile. Ensuite, clique sur Suivant.");
            MascotteTextBlock.Text = $"Bienvenue dans {monde}. Objectif : 3 réussites pour ouvrir le monde suivant !";
        }

        private string? FormeChoisie()
        {
            return (FormeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        }

        private void SelectionnerForme(string forme)
        {
            foreach (ComboBoxItem item in FormeComboBox.Items)
            {
                if (item.Content?.ToString() == forme)
                {
                    FormeComboBox.SelectedItem = item;
                    MettreAJourBoutonsFormes(forme);
                    return;
                }
            }
        }

        private void MettreAJourBoutonsFormes(string? forme)
        {
            Button[] boutons = [CarreButton, RectangleButton, CercleButton, TriangleButton, EllipseButton];

            foreach (Button bouton in boutons)
            {
                bool selectionne = bouton.Tag?.ToString() == forme;
                bouton.Background = (Brush)new BrushConverter().ConvertFromString(selectionne ? "#FFD76B" : "#F7FBFF")!;
                bouton.BorderBrush = (Brush)new BrushConverter().ConvertFromString(selectionne ? "#EEA83B" : "#B9DCEB")!;
                bouton.FontWeight = selectionne ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }

        private void MettreAJourBoutonGenerer()
        {
            if (niveauAventureActif is int)
            {
                GenererButton.Content = "Suivant";
                GenererButton.Background = (Brush)new BrushConverter().ConvertFromString("#CFF0D8")!;
                GenererButton.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#79C98A")!;
                return;
            }

            GenererButton.Content = "Générer";
            GenererButton.Background = (Brush)new BrushConverter().ConvertFromString("#BDE7F6")!;
            GenererButton.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#75BBD4")!;
        }

        private void AfficherChampsSelonForme(string? forme)
        {
            CarrePanel.Visibility = forme == "Carré" ? Visibility.Visible : Visibility.Collapsed;
            RectanglePanel.Visibility = forme == "Rectangle" ? Visibility.Visible : Visibility.Collapsed;
            CerclePanel.Visibility = forme == "Cercle" ? Visibility.Visible : Visibility.Collapsed;
            TrianglePanel.Visibility = forme == "Triangle" ? Visibility.Visible : Visibility.Collapsed;
            EllipsePanel.Visibility = forme == "Ellipse" ? Visibility.Visible : Visibility.Collapsed;
        }

        private ClasseCalcul CreerFormeAleatoire(string forme)
        {
            ClasseCalcul exercice = forme switch
            {
                "Carré" => new ClasseCarre(),
                "Rectangle" => new ClasseRectangle(),
                "Cercle" => new ClasseCercle(),
                "Triangle" => new ClasseTriangle(),
                "Ellipse" => new ClasseEllipse(),
                _ => new ClasseCarre()
            };

            exercice.Init();
            return exercice;
        }

        private void RemplirDimensions(ClasseCalcul forme)
        {
            switch (forme)
            {
                case ClasseCarre carre:
                    CoteTextBox.Text = FormatNombre(carre.UneValeur);
                    break;
                case ClasseRectangle rectangle:
                    LongueurTextBox.Text = FormatNombre(rectangle.UneValeur);
                    LargeurTextBox.Text = FormatNombre(rectangle.Largeur);
                    break;
                case ClasseCercle cercle:
                    RayonTextBox.Text = FormatNombre(cercle.UneValeur);
                    break;
                case ClasseTriangle triangle:
                    BaseTextBox.Text = FormatNombre(triangle.UneValeur);
                    C1TextBox.Text = FormatNombre(triangle.C1);
                    C2TextBox.Text = FormatNombre(triangle.C2);
                    HauteurTextBox.Text = FormatNombre(triangle.Haut);
                    break;
                case ClasseEllipse ellipse:
                    GrandRayonTextBox.Text = FormatNombre(ellipse.UneValeur);
                    PetitRayonTextBox.Text = FormatNombre(ellipse.PR);
                    break;
            }
        }

        private bool ConstruireFormeDepuisSaisie(out ClasseCalcul? forme)
        {
            forme = null;
            string? choix = FormeChoisie();

            if (choix is null)
            {
                AfficherResultat("Choisis d'abord une forme.", "#FFE7E7", "#E89A9A");
                return false;
            }

            switch (choix)
            {
                case "Carré":
                    if (!LireNombrePositif(CoteTextBox, "le côté", out double cote))
                    {
                        return false;
                    }

                    forme = new ClasseCarre { UneValeur = cote };
                    return true;

                case "Rectangle":
                    if (!LireNombrePositif(LongueurTextBox, "la longueur", out double longueur)
                        || !LireNombrePositif(LargeurTextBox, "la largeur", out double largeur))
                    {
                        return false;
                    }

                    forme = new ClasseRectangle { UneValeur = longueur, Largeur = largeur };
                    return true;

                case "Cercle":
                    if (!LireNombrePositif(RayonTextBox, "le rayon", out double rayon))
                    {
                        return false;
                    }

                    forme = new ClasseCercle { UneValeur = rayon };
                    return true;

                case "Triangle":
                    if (!LireNombrePositif(BaseTextBox, "la base", out double baseTriangle)
                        || !LireNombrePositif(C1TextBox, "le côté 1", out double c1)
                        || !LireNombrePositif(C2TextBox, "le côté 2", out double c2)
                        || !LireNombrePositif(HauteurTextBox, "la hauteur", out double hauteur))
                    {
                        return false;
                    }

                    var triangle = new ClasseTriangle
                    {
                        UneValeur = baseTriangle,
                        C1 = c1,
                        C2 = c2,
                        Haut = hauteur
                    };

                    if (!triangle.EstCoherent())
                    {
                        AfficherResultat("Les trois côtés ne peuvent pas former un triangle.", "#FFE7E7", "#E89A9A");
                        return false;
                    }

                    forme = triangle;
                    return true;

                case "Ellipse":
                    if (!LireNombrePositif(GrandRayonTextBox, "le grand rayon", out double grandRayon)
                        || !LireNombrePositif(PetitRayonTextBox, "le petit rayon", out double petitRayon))
                    {
                        return false;
                    }

                    forme = new ClasseEllipse { UneValeur = grandRayon, PR = petitRayon };
                    return true;

                default:
                    AfficherResultat("Choisis d'abord une forme.", "#FFE7E7", "#E89A9A");
                    return false;
            }
        }

        private bool LireNombrePositif(TextBox zone, string nom, out double valeur)
        {
            string texte = zone.Text.Trim().Replace(',', '.');

            if (!double.TryParse(texte, NumberStyles.Float, CultureInfo.InvariantCulture, out valeur)
                || valeur <= 0)
            {
                AfficherResultat($"Entre uniquement un nombre positif pour {nom}.", "#FFE7E7", "#E89A9A");
                zone.Focus();
                return false;
            }

            return true;
        }

        private static double CalculerPerimetre(ClasseCalcul forme)
        {
            return forme switch
            {
                ClasseCarre carre => carre.Perimetre(),
                ClasseRectangle rectangle => rectangle.Perimetre(),
                ClasseCercle cercle => cercle.Perimetre(),
                ClasseTriangle triangle => triangle.Perimetre(),
                ClasseEllipse ellipse => ellipse.Perimetre(),
                _ => 0
            };
        }

        private static double CalculerSurface(ClasseCalcul forme)
        {
            return forme switch
            {
                ClasseCarre carre => carre.Surface(),
                ClasseRectangle rectangle => rectangle.Surface(),
                ClasseCercle cercle => cercle.Surface(),
                ClasseTriangle triangle => triangle.Surface(),
                ClasseEllipse ellipse => ellipse.Surface(),
                _ => 0
            };
        }

        private void DessinerForme(ClasseCalcul forme)
        {
            FormeCanvas.Children.Clear();

            double largeurCanvas = FormeCanvas.ActualWidth > 0 ? FormeCanvas.ActualWidth : 560;
            double hauteurCanvas = FormeCanvas.ActualHeight > 0 ? FormeCanvas.ActualHeight : 390;
            double centreX = largeurCanvas / 2;
            double centreY = hauteurCanvas / 2;

            switch (forme)
            {
                case ClasseCarre carre:
                    DessinerRectangleVisuel(carre.UneValeur, carre.UneValeur, centreX, centreY, "#72C6E8");
                    AjouterTexte($"côté = {FormatNombre(carre.UneValeur)}", centreX - 55, centreY + 90);
                    break;

                case ClasseRectangle rectangle:
                    DessinerRectangleVisuel(rectangle.UneValeur, rectangle.Largeur, centreX, centreY, "#9DD8A8");
                    AjouterTexte($"L = {FormatNombre(rectangle.UneValeur)}   l = {FormatNombre(rectangle.Largeur)}", centreX - 80, centreY + 90);
                    break;

                case ClasseCercle cercle:
                    DessinerEllipseVisuel(cercle.UneValeur, cercle.UneValeur, centreX, centreY, "#F5C05F");
                    AjouterTexte($"rayon = {FormatNombre(cercle.UneValeur)}", centreX - 60, centreY + 100);
                    break;

                case ClasseTriangle triangle:
                    DessinerTriangleVisuel(triangle, centreX, centreY);
                    AjouterTexte($"base = {FormatNombre(triangle.UneValeur)}   h = {FormatNombre(triangle.Haut)}", centreX - 85, centreY + 102);
                    break;

                case ClasseEllipse ellipse:
                    DessinerEllipseVisuel(ellipse.UneValeur, ellipse.PR, centreX, centreY, "#D6B4F0");
                    AjouterTexte($"R = {FormatNombre(ellipse.UneValeur)}   r = {FormatNombre(ellipse.PR)}", centreX - 70, centreY + 100);
                    break;
            }
        }

        private void DessinerRectangleVisuel(double longueur, double largeur, double centreX, double centreY, string couleur)
        {
            double echelle = CalculerEchelle(longueur, largeur);
            double dessinLargeur = longueur * echelle;
            double dessinHauteur = largeur * echelle;

            var rectangle = new RectangleShape
            {
                Width = dessinLargeur,
                Height = dessinHauteur,
                Fill = (Brush)new BrushConverter().ConvertFromString(couleur)!,
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 4,
                RadiusX = 10,
                RadiusY = 10
            };

            FormeCanvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, centreX - dessinLargeur / 2);
            Canvas.SetTop(rectangle, centreY - dessinHauteur / 2);
        }

        private void DessinerEllipseVisuel(double rayonHorizontal, double rayonVertical, double centreX, double centreY, string couleur)
        {
            double echelle = CalculerEchelle(rayonHorizontal * 2, rayonVertical * 2);
            double dessinLargeur = rayonHorizontal * 2 * echelle;
            double dessinHauteur = rayonVertical * 2 * echelle;

            var ellipse = new Ellipse
            {
                Width = dessinLargeur,
                Height = dessinHauteur,
                Fill = (Brush)new BrushConverter().ConvertFromString(couleur)!,
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 4
            };

            FormeCanvas.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, centreX - dessinLargeur / 2);
            Canvas.SetTop(ellipse, centreY - dessinHauteur / 2);
        }

        private void DessinerTriangleVisuel(ClasseTriangle triangle, double centreX, double centreY)
        {
            double echelle = CalculerEchelle(triangle.UneValeur, triangle.Haut);
            double baseLargeur = triangle.UneValeur * echelle;
            double hauteur = triangle.Haut * echelle;

            var polygon = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromRgb(248, 167, 143)),
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 4,
                Points =
                {
                    new Point(centreX, centreY - hauteur / 2),
                    new Point(centreX - baseLargeur / 2, centreY + hauteur / 2),
                    new Point(centreX + baseLargeur / 2, centreY + hauteur / 2)
                }
            };

            FormeCanvas.Children.Add(polygon);
        }

        private static double CalculerEchelle(double largeur, double hauteur)
        {
            double plusGrand = Math.Max(largeur, hauteur);
            return plusGrand <= 0 ? 1 : Math.Min(250 / plusGrand, 46);
        }

        private void AjouterTexte(string texte, double gauche, double haut)
        {
            var textBlock = new TextBlock
            {
                Text = texte,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Background = Brushes.White,
                Padding = new Thickness(8, 4, 8, 4)
            };

            FormeCanvas.Children.Add(textBlock);
            Canvas.SetLeft(textBlock, Math.Max(8, gauche));
            Canvas.SetTop(textBlock, Math.Max(8, haut));
        }

        private void AfficherDimensions(ClasseCalcul forme)
        {
            DimensionsTextBlock.Text = forme switch
            {
                ClasseCarre carre => $"Carré : côté {FormatNombre(carre.UneValeur)}",
                ClasseRectangle rectangle => $"Rectangle : longueur {FormatNombre(rectangle.UneValeur)}, largeur {FormatNombre(rectangle.Largeur)}",
                ClasseCercle cercle => $"Cercle : rayon {FormatNombre(cercle.UneValeur)}",
                ClasseTriangle triangle => $"Triangle : base {FormatNombre(triangle.UneValeur)}, côté 1 {FormatNombre(triangle.C1)}, côté 2 {FormatNombre(triangle.C2)}, hauteur {FormatNombre(triangle.Haut)}",
                ClasseEllipse ellipse => $"Ellipse : grand rayon {FormatNombre(ellipse.UneValeur)}, petit rayon {FormatNombre(ellipse.PR)}. Son périmètre est une approximation.",
                _ => string.Empty
            };
        }

        private static string Formule(ClasseCalcul forme)
        {
            return forme switch
            {
                ClasseCarre => "Carré : P = 4 × côté, S = côté × côté",
                ClasseRectangle => "Rectangle : P = 2 × (longueur + largeur), S = longueur × largeur",
                ClasseCercle => "Cercle : P = 2 × π × rayon, S = π × rayon²",
                ClasseTriangle => "Triangle : P = base + côté 1 + côté 2, S = (base × hauteur) / 2",
                ClasseEllipse => "Ellipse : P ≈ formule de Ramanujan, S = π × grand rayon × petit rayon",
                _ => string.Empty
            };
        }

        private static string FormuleAide(string forme)
        {
            return forme switch
            {
                "Carré" => "Périmètre = 4 × côté\nSurface = côté × côté",
                "Rectangle" => "Périmètre = 2 × (longueur + largeur)\nSurface = longueur × largeur",
                "Cercle" => "Périmètre = 2 × π × rayon\nSurface = π × rayon²",
                "Triangle" => "Périmètre = base + côté 1 + côté 2\nSurface = (base × hauteur) / 2",
                "Ellipse" => "Périmètre ≈ formule de Ramanujan\nSurface = π × grand rayon × petit rayon",
                _ => "Choisis une forme pour voir sa formule."
            };
        }

        private void NettoyerExercice()
        {
            formeActuelle = null;
            defiRapideActif = false;
            formuleUtilisee = false;
            dernierExerciceRecompense = null;
            niveauAventureActif = null;
            mondeAventureActif = null;
            MettreAJourBoutonGenerer();
            FormeComboBox.SelectedIndex = -1;
            AfficherChampsSelonForme(null);
            MettreAJourBoutonsFormes(null);
            FormeCanvas.Children.Clear();
            DimensionsTextBlock.Text = string.Empty;
            CoteTextBox.Clear();
            LongueurTextBox.Clear();
            LargeurTextBox.Clear();
            RayonTextBox.Clear();
            BaseTextBox.Clear();
            C1TextBox.Clear();
            C2TextBox.Clear();
            HauteurTextBox.Clear();
            GrandRayonTextBox.Clear();
            PetitRayonTextBox.Clear();
            PerimetreTextBox.Clear();
            SurfaceTextBox.Clear();
            MettreAJourMascotte(null);
            AfficherResultat("Choisis une forme, puis génère un exercice ou entre tes dimensions.", "#FFF7D6", "#F3D36B");
        }

        private static string FormeAventure(int niveau)
        {
            return niveau switch
            {
                1 => "Carré",
                2 => "Rectangle",
                3 => "Cercle",
                4 => "Triangle",
                5 => FormeAventureFinale(),
                _ => "Carré"
            };
        }

        private static string FormeAventureFinale()
        {
            string[] formes = ["Carré", "Rectangle", "Cercle", "Triangle", "Ellipse"];
            return formes[Random.Shared.Next(formes.Length)];
        }

        private static string NomMondeAventure(int niveau)
        {
            return niveau switch
            {
                1 => "le Village du Carré",
                2 => "la Forêt du Rectangle",
                3 => "le Lac du Cercle",
                4 => "la Montagne du Triangle",
                5 => "le Château des Formes",
                _ => "le Mode Aventure"
            };
        }

        private static string EtoilesAventure(int reussites)
        {
            int etoiles = Math.Clamp(reussites, 0, 3);
            return new string('★', etoiles) + new string('☆', 3 - etoiles);
        }

        private void MettreAJourProgression()
        {
            ScoreTextBlock.Text = $"⭐ Score : {progression.Score}";
            NiveauTextBlock.Text = $"🧠 Niveau : {progression.Niveau}";
            XpTextBlock.Text = $"XP : {progression.XP} / 100";
            XpProgressBar.Value = progression.XP;
            SerieTextBlock.Text = $"🔥 Série : {progression.SerieActuelle}";
            RecordTextBlock.Text = $"🏆 Record : {progression.MeilleureSerie}";
            MettreAJourBadgesVisuels();
        }

        private static List<BadgeProgression> BadgesOfficiels()
        {
            return
            [
                new() { Nom = "Premier exercice", Icone = "🎯", Description = "Valide ton premier exercice." },
                new() { Nom = "3 bonnes réponses", Icone = "✅", Description = "Réussis entièrement 3 exercices." },
                new() { Nom = "Expert carré", Icone = "◼", Description = "Réussis 3 carrés." },
                new() { Nom = "Expert rectangle", Icone = "▭", Description = "Réussis 3 rectangles." },
                new() { Nom = "Expert cercle", Icone = "⚪", Description = "Réussis 3 cercles." },
                new() { Nom = "Expert triangle", Icone = "🔺", Description = "Réussis 3 triangles." },
                new() { Nom = "Expert ellipse", Icone = "🥚", Description = "Réussis 3 ellipses." },
                new() { Nom = "Niveau 5", Icone = "🏆", Description = "Atteins le niveau 5." },
                new() { Nom = "Série de 3", Icone = "🔥", Description = "Réussis 3 exercices d'affilée." },
                new() { Nom = "Série de 5", Icone = "⚡", Description = "Réussis 5 exercices d'affilée." },
                new() { Nom = "Petit filou", Icone = "\U0001F575\uFE0F", Description = "Trouve le mini-jeu secret." },
                new() { Nom = "Aventurier des formes", Icone = "🗺️", Description = "Tu as terminé le Mode Aventure." }
            ];
        }

        private void MettreAJourBadgesVisuels()
        {
            BadgesWrapPanel.Children.Clear();

            foreach (BadgeProgression badge in BadgesOfficiels())
            {
                bool estDebloque = progression.Badges.Contains(badge.Nom);
                badge.EstDebloque = estDebloque;

                var carte = new Border
                {
                    Width = 86,
                    Height = 88,
                    Margin = new Thickness(0, 0, 8, 8),
                    CornerRadius = new CornerRadius(14),
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)new BrushConverter().ConvertFromString(estDebloque ? "#E4A72A" : "#C6CED6")!,
                    Background = (Brush)new BrushConverter().ConvertFromString(estDebloque ? "#FFF2B8" : "#EEF1F4")!,
                    Opacity = estDebloque ? 1 : 0.68,
                    ToolTip = badge.Description
                };

                var pile = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                pile.Children.Add(new TextBlock
                {
                    Text = estDebloque ? badge.Icone : "🔒",
                    FontSize = 26,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });

                pile.Children.Add(new TextBlock
                {
                    Text = badge.Nom.Replace("Premier exercice", "Premier exo"),
                    FontSize = 11,
                    FontWeight = estDebloque ? FontWeights.Bold : FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(4, 4, 4, 0)
                });

                carte.Child = pile;
                BadgesWrapPanel.Children.Add(carte);
            }
        }

        private void MettreAJourMascotte(string? forme)
        {
            MascotteTextBlock.Text = forme switch
            {
                "Carré" => "Pour un carré, tous les côtés sont égaux.",
                "Rectangle" => "Pour un rectangle, les côtés opposés ont la même longueur.",
                "Cercle" => "Pour un cercle, pense à π ≈ 3,14.",
                "Triangle" => "Pour un triangle, la surface se calcule avec base × hauteur ÷ 2.",
                "Ellipse" => "Pour une ellipse, la surface utilise le grand rayon et le petit rayon.",
                _ => "Le périmètre, c'est le tour de la figure !"
            };
        }

        private string MessageAleatoire(bool toutCorrect, bool uneReponseCorrecte)
        {
            string[] source = toutCorrect ? messagesReussite : uneReponseCorrecte ? messagesPartiels : messagesErreur;
            return source[Random.Shared.Next(source.Length)];
        }

        private static string CreerMessageProgression(ProgressionResult resultat)
        {
            string message = $"\n+{resultat.PointsGagnes} point{(resultat.PointsGagnes > 1 ? "s" : string.Empty)}";

            if (resultat.XpGagne > 0)
            {
                message += $" et +{resultat.XpGagne} XP";
            }

            if (resultat.NiveauGagne)
            {
                message += "\n🏆 Nouveau niveau atteint !";
            }

            if (resultat.NouveauxBadges.Count > 0)
            {
                string badges = string.Join(", ", resultat.NouveauxBadges.Select(badge => $"{IconeBadge(badge)} {badge}"));
                message += $"\nNouveau badge débloqué ! {badges}";
            }

            return message;
        }

        private static string CreerMessageAventure(AventureResult resultat)
        {
            if (!resultat.EtoileGagnee)
            {
                return $"\n{NomMondeAventure(resultat.Niveau)} a déjà ses 3 étoiles.";
            }

            string message =
                $"\nBravo ! Tu as gagné une étoile dans {NomMondeAventure(resultat.Niveau)} {EtoilesAventure(resultat.Reussites)}";

            if (resultat.NiveauSuivantDebloque)
            {
                message += $"\nSuper ! Nouveau monde débloqué : {NomMondeAventure(resultat.Niveau + 1)} !";
            }

            if (resultat.AventureTerminee)
            {
                message += "\nFélicitations ! Tu as terminé l'aventure des formes ! 🏆";
            }

            if (resultat.BadgeDebloque)
            {
                message += "\nNouveau badge débloqué ! 🗺️ Aventurier des formes";
            }

            return message;
        }

        private static string IconeBadge(string nom)
        {
            return nom switch
            {
                "Premier exercice" => "🎯",
                "3 bonnes réponses" => "✅",
                "Expert carré" => "◼",
                "Expert rectangle" => "▭",
                "Expert cercle" => "⚪",
                "Expert triangle" => "🔺",
                "Expert ellipse" => "🥚",
                "Niveau 5" => "🏆",
                "Série de 3" => "🔥",
                "Série de 5" => "⚡",
                "Petit filou" => "\U0001F575\uFE0F",
                "Aventurier des formes" => "🗺️",
                _ => "🏅"
            };
        }

        private static string CreerSignatureExercice(string forme, ClasseCalcul exercice)
        {
            return exercice switch
            {
                ClasseCarre carre => $"{forme}:{carre.UneValeur}",
                ClasseRectangle rectangle => $"{forme}:{rectangle.UneValeur}:{rectangle.Largeur}",
                ClasseCercle cercle => $"{forme}:{cercle.UneValeur}",
                ClasseTriangle triangle => $"{forme}:{triangle.UneValeur}:{triangle.C1}:{triangle.C2}:{triangle.Haut}",
                ClasseEllipse ellipse => $"{forme}:{ellipse.UneValeur}:{ellipse.PR}",
                _ => forme
            };
        }

        private static Progression ChargerProgression()
        {
            try
            {
                if (!File.Exists(FichierSauvegarde))
                {
                    return new Progression();
                }

                string json = File.ReadAllText(FichierSauvegarde);
                Progression progressionChargee = JsonSerializer.Deserialize<Progression>(json) ?? new Progression();
                NormaliserAnciensBadges(progressionChargee);
                return progressionChargee;
            }
            catch
            {
                return new Progression();
            }
        }

        private static void NormaliserAnciensBadges(Progression progressionChargee)
        {
            for (int index = 0; index < progressionChargee.Badges.Count; index++)
            {
                progressionChargee.Badges[index] = progressionChargee.Badges[index]
                    .Replace("3 bonnes rÃ©ponses", "3 bonnes réponses", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void SauvegarderProgression()
        {
            Directory.CreateDirectory(DossierSauvegarde);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(FichierSauvegarde, JsonSerializer.Serialize(progression, options));
        }

        private void AfficherResultat(string message, string fond, string bordure)
        {
            ResultatTextBlock.Text = message;
            ResultatBorder.Background = (Brush)new BrushConverter().ConvertFromString(fond)!;
            ResultatBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString(bordure)!;

            var animation = new DoubleAnimation(0.72, 1, TimeSpan.FromMilliseconds(240))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.25 }
            };
            ResultatBorder.BeginAnimation(OpacityProperty, animation);
        }

        private static string FormatNombre(double valeur)
        {
            return Math.Round(valeur, 2).ToString("0.##", CultureInfo.CurrentCulture);
        }
    }
}
