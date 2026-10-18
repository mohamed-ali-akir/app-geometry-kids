using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Geometrie.Core;
using RectangleShape = System.Windows.Shapes.Rectangle;

namespace Geometrie
{
    /// <summary>
    /// Fenêtre principale. Elle ne fait qu'afficher et lire les saisies : les règles du jeu sont dans
    /// <see cref="SessionExercice"/> et <see cref="Progression"/>, la sauvegarde dans <see cref="DepotProgression"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string[] MessagesReussite =
        [
            "Bravo champion ! Tu progresses super bien !",
            "Excellent travail, continue comme ça !",
            "Tu deviens un expert en géométrie !"
        ];

        private static readonly string[] MessagesPartiels =
        [
            "Bien joué, tu as trouvé une partie !",
            "Presque ! Tu es sur la bonne voie !",
            "Continue, tu vas y arriver !"
        ];

        private static readonly string[] MessagesErreur =
        [
            "Ce n'est pas grave, l'erreur aide à apprendre !",
            "Essaie encore, tu peux y arriver !",
            "Regarde la correction puis retente ta chance !"
        ];

        private readonly DepotProgression depot;
        private readonly SessionExercice session;
        private ClasseCalcul? formeDessinee;

        public MainWindow(DepotProgression depot)
        {
            InitializeComponent();
            this.depot = depot;

            ResultatChargement chargement = depot.Charger();
            session = new SessionExercice(chargement.Progression);

            AfficherChampsSelonForme(null);
            MettreAJourBoutonsFormes(null);
            MettreAJourProgression();
            MettreAJourBoutonGenerer();
            SignalerChargement(chargement);
        }

        private Progression Progression => session.Progression;

        private void FormeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string tag } && Enum.TryParse(tag, out TypeForme forme))
            {
                ChoisirForme(forme);
            }
        }

        private void ChoisirForme(TypeForme? forme)
        {
            if (!session.ChoisirForme(forme))
            {
                return;
            }

            EffacerExercice();
            AfficherFormeChoisie();
            MettreAJourMascotte(forme);
            AfficherResultat("Entre tes dimensions ou clique sur Générer un exercice.", Couleurs.Info);
        }

        private void GenererButton_Click(object sender, RoutedEventArgs e)
        {
            if (session.NiveauAventure is int niveau)
            {
                LancerExerciceAventure(niveau);
                return;
            }

            if (session.Generer() is not ClasseCalcul exercice)
            {
                AfficherResultat("Choisis d'abord une forme.", Couleurs.Erreur);
                return;
            }

            AfficherExercice(exercice, "À toi de jouer ! Calcule le périmètre et la surface.");
        }

        private void DefiRapideButton_Click(object sender, RoutedEventArgs e)
        {
            ClasseCalcul exercice = session.LancerDefiRapide();
            AfficherFormeChoisie();
            MettreAJourMascotte(exercice.Type);
            AfficherExercice(exercice, "Défi rapide lancé ! Si tout est juste, tu gagnes 5 points bonus.");
        }

        private void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConstruireFormeDepuisSaisie(out ClasseCalcul? forme)
                || !LireReponse(PerimetreTextBox, "le périmètre", out double perimetre)
                || !LireReponse(SurfaceTextBox, "la surface", out double surface))
            {
                return;
            }

            DessinerForme(forme);
            AfficherDimensions(forme);

            ResultatValidation resultat = session.Valider(forme, perimetre, surface);
            string message = MessageAleatoire(resultat);
            string progressionMessage;

            if (resultat.Progression is ProgressionResult progression)
            {
                progressionMessage = CreerMessageProgression(progression);

                if (resultat.Aventure is AventureResult aventure)
                {
                    progressionMessage += CreerMessageAventure(aventure);
                }

                progressionMessage += Sauvegarder();
                MettreAJourProgression();
            }
            else
            {
                progressionMessage = "\nCet exercice a déjà été compté dans ta progression.";
            }

            if (resultat.DimensionsModifiees)
            {
                progressionMessage += session.Mode == ModeExercice.Aventure
                    ? "\nPour gagner une étoile, garde les dimensions données par l'exercice."
                    : "\nLe bonus du défi ne compte que si tu gardes les dimensions données.";
            }

            MettreAJourBoutonGenerer();

            if (resultat.ToutCorrect)
            {
                string serieMessage = Progression.SerieActuelle switch
                {
                    3 => "\nSuper série !",
                    5 => "\nIncroyable, tu es lancé !",
                    _ => string.Empty
                };

                bool nouveauBadge = resultat.Progression?.NouveauxBadges.Count > 0 || resultat.Aventure?.BadgeDebloque == true;
                AfficherResultat($"{message}{progressionMessage}{serieMessage}", nouveauBadge ? Couleurs.Badge : Couleurs.Succes);
                return;
            }

            AfficherResultat(
                $"{message}{progressionMessage}\n" +
                $"Périmètre attendu : {FormatNombre(resultat.PerimetreAttendu)}\n" +
                $"Surface attendue : {FormatNombre(resultat.SurfaceAttendue)}\n" +
                $"Formule : {CatalogueFormes.Infos(forme.Type).FormuleCourte}",
                resultat.UneReponseCorrecte ? Couleurs.Partiel : Couleurs.Echec);
        }

        private void ReinitialiserButton_Click(object sender, RoutedEventArgs e)
        {
            session.Reinitialiser();
            EffacerExercice();
            AfficherFormeChoisie();
            MettreAJourMascotte(null);
            AfficherResultat("Choisis une forme, puis génère un exercice ou entre tes dimensions.", Couleurs.Info);
        }

        private void ModeAventureButton_Click(object sender, RoutedEventArgs e)
        {
            var aventure = new ModeAventureWindow(Progression)
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
            if (session.FormeChoisie is not TypeForme forme)
            {
                AfficherResultat("Choisis d'abord une forme pour voir la formule.", Couleurs.Erreur);
                return;
            }

            session.UtiliserFormule();
            string formule = CatalogueFormes.Infos(forme).FormuleAide;

            MascotteTextBlock.Text =
                $"Géo le robot te donne un indice !\n{formule}\nAvec l'indice, tu gagnes moitié moins d'XP.";

            AfficherResultat($"💡 Indice utilisé ! Tu gagnes 50% d'XP en moins pour cet exercice.\n{formule}", Couleurs.Indice);
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

            if (!Progression.DebloquerPetitFilou())
            {
                MascotteTextBlock.Text = "Le mini-jeu secret est toujours là si tu veux faire une pause.";
                AfficherResultat("Tu as retrouvé le mini-jeu secret ! Le badge Petit filou est déjà débloqué.", Couleurs.Info);
                return;
            }

            string sauvegarde = Sauvegarder();
            MettreAJourProgression();
            MascotteTextBlock.Text = "Géo le robot a vu ton clin d'oeil secret ! Badge Petit filou débloqué.";
            AfficherResultat(
                $"{CatalogueBadges.Icone(CatalogueBadges.PetitFilou)} Badge secret débloqué : Petit filou ! Tu gagnes aussi +1 niveau, une seule fois.{sauvegarde}",
                Couleurs.Badge);
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

            Progression.Reinitialiser();
            string sauvegarde = Sauvegarder();
            MettreAJourProgression();
            AfficherResultat($"La progression est remise à zéro. Nouveau départ !{sauvegarde}", Couleurs.Info);
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

        private void FormeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (formeDessinee is not null)
            {
                DessinerForme(formeDessinee);
            }
        }

        private void LancerExerciceAventure(int niveau)
        {
            if (session.LancerAventure(niveau) is not ClasseCalcul exercice)
            {
                AfficherResultat(
                    $"Ce monde est encore verrouillé. Gagne {Aventure.ReussitesParMonde} étoiles dans le monde précédent.",
                    Couleurs.Erreur);
                return;
            }

            string monde = Aventure.NomDansUnePhrase(niveau);
            AfficherFormeChoisie();
            MettreAJourBoutonGenerer();
            AfficherExercice(
                exercice,
                $"Mode Aventure : {monde} ! Réussis l'exercice pour gagner une étoile. Ensuite, clique sur Suivant.");
            MascotteTextBlock.Text =
                $"Bienvenue dans {monde}. Objectif : {Aventure.ReussitesParMonde} réussites pour ouvrir le monde suivant !";
        }

        private void AfficherExercice(ClasseCalcul exercice, string message)
        {
            MettreAJourBoutonGenerer();
            RemplirDimensions(exercice);
            DessinerForme(exercice);
            AfficherDimensions(exercice);
            PerimetreTextBox.Clear();
            SurfaceTextBox.Clear();
            AfficherResultat(message, Couleurs.Succes);
        }

        /// <summary>Synchronise les boutons et les champs avec la forme choisie dans la séance.</summary>
        private void AfficherFormeChoisie()
        {
            AfficherChampsSelonForme(session.FormeChoisie);
            MettreAJourBoutonsFormes(session.FormeChoisie);
            MettreAJourBoutonGenerer();
        }

        private void EffacerExercice()
        {
            formeDessinee = null;
            FormeCanvas.Children.Clear();
            DimensionsTextBlock.Text = string.Empty;

            foreach (TextBox zone in ToutesLesZonesDeSaisie())
            {
                zone.Clear();
            }
        }

        private TextBox[] ToutesLesZonesDeSaisie() =>
        [
            CoteTextBox, LongueurTextBox, LargeurTextBox, RayonTextBox, BaseTextBox, C1TextBox, C2TextBox,
            HauteurTextBox, GrandRayonTextBox, PetitRayonTextBox, PerimetreTextBox, SurfaceTextBox
        ];

        private void MettreAJourBoutonsFormes(TypeForme? forme)
        {
            Button[] boutons = [CarreButton, RectangleButton, CercleButton, TriangleButton, EllipseButton];

            foreach (Button bouton in boutons)
            {
                bool selectionne = forme is not null && bouton.Tag as string == forme.ToString();
                bouton.Background = Couleurs.Pinceau(selectionne ? "#FFD76B" : "#F7FBFF");
                bouton.BorderBrush = Couleurs.Pinceau(selectionne ? "#EEA83B" : "#B9DCEB");
                bouton.FontWeight = selectionne ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }

        private void MettreAJourBoutonGenerer()
        {
            bool aventure = session.NiveauAventure is not null;
            GenererButton.Content = aventure ? "Suivant" : "Générer";
            GenererButton.Background = Couleurs.Pinceau(aventure ? "#CFF0D8" : "#BDE7F6");
            GenererButton.BorderBrush = Couleurs.Pinceau(aventure ? "#79C98A" : "#75BBD4");
        }

        private void AfficherChampsSelonForme(TypeForme? forme)
        {
            CarrePanel.Visibility = Visible(forme == TypeForme.Carre);
            RectanglePanel.Visibility = Visible(forme == TypeForme.Rectangle);
            CerclePanel.Visibility = Visible(forme == TypeForme.Cercle);
            TrianglePanel.Visibility = Visible(forme == TypeForme.Triangle);
            EllipsePanel.Visibility = Visible(forme == TypeForme.Ellipse);

            static Visibility Visible(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;
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

        private bool ConstruireFormeDepuisSaisie([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ClasseCalcul? forme)
        {
            forme = null;

            switch (session.FormeChoisie)
            {
                case TypeForme.Carre:
                    if (LireDimension(CoteTextBox, "le côté", out double cote))
                    {
                        forme = new ClasseCarre { UneValeur = cote };
                    }

                    break;

                case TypeForme.Rectangle:
                    if (LireDimension(LongueurTextBox, "la longueur", out double longueur)
                        && LireDimension(LargeurTextBox, "la largeur", out double largeur))
                    {
                        forme = new ClasseRectangle { UneValeur = longueur, Largeur = largeur };
                    }

                    break;

                case TypeForme.Cercle:
                    if (LireDimension(RayonTextBox, "le rayon", out double rayon))
                    {
                        forme = new ClasseCercle { UneValeur = rayon };
                    }

                    break;

                case TypeForme.Triangle:
                    if (LireDimension(BaseTextBox, "la base", out double baseTriangle)
                        && LireDimension(C1TextBox, "le côté 1", out double c1)
                        && LireDimension(C2TextBox, "le côté 2", out double c2)
                        && LireDimension(HauteurTextBox, "la hauteur", out double hauteur))
                    {
                        forme = new ClasseTriangle { UneValeur = baseTriangle, C1 = c1, C2 = c2, Haut = hauteur };
                    }

                    break;

                case TypeForme.Ellipse:
                    if (LireDimension(GrandRayonTextBox, "le grand rayon", out double grandRayon)
                        && LireDimension(PetitRayonTextBox, "le petit rayon", out double petitRayon))
                    {
                        forme = new ClasseEllipse { UneValeur = grandRayon, PR = petitRayon };
                    }

                    break;

                default:
                    AfficherResultat("Choisis d'abord une forme.", Couleurs.Erreur);
                    return false;
            }

            if (forme is null)
            {
                return false;
            }

            if (forme.Valider() is string erreur)
            {
                AfficherResultat(erreur, Couleurs.Erreur);
                forme = null;
                return false;
            }

            return true;
        }

        private bool LireDimension(TextBox zone, string nom, out double valeur)
        {
            string maximum = FormatNombre(ClasseCalcul.DimensionMax);
            return LireNombre(zone, ClasseCalcul.DimensionMax, $"Entre un nombre positif pour {nom} (au plus {maximum}).", out valeur);
        }

        private bool LireReponse(TextBox zone, string nom, out double valeur)
        {
            return LireNombre(zone, double.MaxValue, $"Entre uniquement un nombre positif pour {nom}.", out valeur);
        }

        private bool LireNombre(TextBox zone, double maximum, string messageErreur, out double valeur)
        {
            if (LecteurNombre.EssayerLirePositif(zone.Text, maximum, out valeur))
            {
                return true;
            }

            AfficherResultat(messageErreur, Couleurs.Erreur);
            zone.Focus();
            return false;
        }

        private void DessinerForme(ClasseCalcul forme)
        {
            formeDessinee = forme;
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
                Fill = Couleurs.Pinceau(couleur),
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
                Fill = Couleurs.Pinceau(couleur),
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 4
            };

            FormeCanvas.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, centreX - dessinLargeur / 2);
            Canvas.SetTop(ellipse, centreY - dessinHauteur / 2);
        }

        /// <summary>Dessine le vrai triangle : le sommet est placé d'après les longueurs des côtés.</summary>
        private void DessinerTriangleVisuel(ClasseTriangle triangle, double centreX, double centreY)
        {
            double decalage = triangle.DecalageSommet();
            double gauche = Math.Min(0, decalage);
            double droite = Math.Max(triangle.UneValeur, decalage);
            double echelle = CalculerEchelle(droite - gauche, triangle.Haut);

            double origineX = centreX - (gauche + droite) / 2 * echelle;
            double baseY = centreY + triangle.Haut * echelle / 2;

            var polygon = new Polygon
            {
                Fill = Couleurs.Pinceau("#F8A78F"),
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 4,
                Points =
                {
                    new Point(origineX + decalage * echelle, baseY - triangle.Haut * echelle),
                    new Point(origineX, baseY),
                    new Point(origineX + triangle.UneValeur * echelle, baseY)
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

        private void MettreAJourProgression()
        {
            ScoreTextBlock.Text = $"⭐ Score : {Progression.Score}";
            NiveauTextBlock.Text = $"🧠 Niveau : {Progression.Niveau}";
            XpTextBlock.Text = $"XP : {Progression.XP} / {Progression.XpParNiveau}";
            XpProgressBar.Maximum = Progression.XpParNiveau;
            XpProgressBar.Value = Progression.XP;
            SerieTextBlock.Text = $"🔥 Série : {Progression.SerieActuelle}";
            RecordTextBlock.Text = $"🏆 Record : {Progression.MeilleureSerie}";
            MettreAJourBadgesVisuels();
        }

        private void MettreAJourBadgesVisuels()
        {
            BadgesWrapPanel.Children.Clear();

            foreach (InfosBadge badge in CatalogueBadges.Tous)
            {
                bool estDebloque = Progression.Badges.Contains(badge.Nom);

                var carte = new Border
                {
                    Width = 86,
                    Height = 88,
                    Margin = new Thickness(0, 0, 8, 8),
                    CornerRadius = new CornerRadius(14),
                    BorderThickness = new Thickness(2),
                    BorderBrush = Couleurs.Pinceau(estDebloque ? "#E4A72A" : "#C6CED6"),
                    Background = Couleurs.Pinceau(estDebloque ? "#FFF2B8" : "#EEF1F4"),
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
                    Text = badge.Nom == CatalogueBadges.PremierExercice ? "Premier exo" : badge.Nom,
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

        private void MettreAJourMascotte(TypeForme? forme)
        {
            MascotteTextBlock.Text = forme is TypeForme type
                ? CatalogueFormes.Infos(type).Conseil
                : "Le périmètre, c'est le tour de la figure !";
        }

        private static string MessageAleatoire(ResultatValidation resultat)
        {
            string[] source = resultat.ToutCorrect ? MessagesReussite
                : resultat.UneReponseCorrecte ? MessagesPartiels
                : MessagesErreur;
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
                string badges = string.Join(", ", resultat.NouveauxBadges.Select(badge => $"{CatalogueBadges.Icone(badge)} {badge}"));
                message += $"\nNouveau badge débloqué ! {badges}";
            }

            return message;
        }

        private static string CreerMessageAventure(AventureResult resultat)
        {
            string monde = Aventure.NomDansUnePhrase(resultat.Niveau);

            if (!resultat.EtoileGagnee)
            {
                return $"\n{char.ToUpper(monde[0], CultureInfo.CurrentCulture)}{monde[1..]} a déjà ses {Aventure.ReussitesParMonde} étoiles.";
            }

            string message = $"\nBravo ! Tu as gagné une étoile dans {monde} {Aventure.Etoiles(resultat.Reussites)}";

            if (resultat.NiveauSuivantDebloque)
            {
                message += $"\nSuper ! Nouveau monde débloqué : {Aventure.NomDansUnePhrase(resultat.Niveau + 1)} !";
            }

            if (resultat.AventureTerminee)
            {
                message += "\nFélicitations ! Tu as terminé l'aventure des formes ! 🏆";
            }

            if (resultat.BadgeDebloque)
            {
                message += $"\nNouveau badge débloqué ! {CatalogueBadges.Icone(CatalogueBadges.AventurierDesFormes)} {CatalogueBadges.AventurierDesFormes}";
            }

            return message;
        }

        /// <summary>Enregistre la progression ; renvoie un avertissement à afficher si le disque a refusé.</summary>
        private string Sauvegarder()
        {
            return depot.Sauvegarder(Progression)
                ? string.Empty
                : "\n⚠️ Ta progression n'a pas pu être enregistrée sur cet ordinateur.";
        }

        private void SignalerChargement(ResultatChargement chargement)
        {
            switch (chargement.Etat)
            {
                case EtatChargement.Recuperee:
                    AfficherResultat(
                        "Ta sauvegarde était abîmée : une copie a été gardée et tu repars de zéro.",
                        Couleurs.Partiel);
                    break;

                case EtatChargement.Illisible:
                    AfficherResultat(
                        "Ta sauvegarde n'a pas pu être lue. Elle ne sera pas modifiée : ta progression d'aujourd'hui ne sera pas enregistrée.",
                        Couleurs.Partiel);
                    break;
            }
        }

        private void AfficherResultat(string message, (string Fond, string Bordure) couleurs)
        {
            ResultatTextBlock.Text = message;
            ResultatBorder.Background = Couleurs.Pinceau(couleurs.Fond);
            ResultatBorder.BorderBrush = Couleurs.Pinceau(couleurs.Bordure);

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
