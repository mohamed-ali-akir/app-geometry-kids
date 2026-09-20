using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Geometrie.Core;

namespace Geometrie
{
    public partial class ModeAventureWindow : Window
    {
        private readonly Progression progression;

        private static readonly AventureMonde[] Mondes =
        [
            new(1, "🟦", "Village du Carré", "Exercices sur le carré"),
            new(2, "▭", "Forêt du Rectangle", "Exercices sur le rectangle"),
            new(3, "⚪", "Lac du Cercle", "Exercices sur le cercle"),
            new(4, "🔺", "Montagne du Triangle", "Exercices sur le triangle"),
            new(5, "🏰", "Château des Formes", "Défi final avec formes mélangées")
        ];

        public int? NiveauChoisi { get; private set; }

        public ModeAventureWindow(Progression progression)
        {
            InitializeComponent();
            this.progression = progression;
            ConstruireCarte();
        }

        private void ConstruireCarte()
        {
            MondesWrapPanel.Children.Clear();

            foreach (AventureMonde monde in Mondes)
            {
                bool debloque = progression.NiveauAventureDebloque(monde.Niveau);
                int reussites = progression.ReussitesNiveauAventure(monde.Niveau);

                var carte = new Border
                {
                    Width = 206,
                    Height = 214,
                    Margin = new Thickness(7),
                    CornerRadius = new CornerRadius(18),
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)new BrushConverter().ConvertFromString(debloque ? "#8BCB9A" : "#C6CED6")!,
                    Background = (Brush)new BrushConverter().ConvertFromString(debloque ? "#FFFFFF" : "#EEF1F4")!,
                    Opacity = debloque ? 1 : 0.68,
                    Padding = new Thickness(14),
                    Effect = (System.Windows.Media.Effects.Effect)Resources["AdventureShadow"]
                };

                var pile = new StackPanel();
                pile.HorizontalAlignment = HorizontalAlignment.Stretch;

                pile.Children.Add(new TextBlock
                {
                    Text = debloque ? monde.Icone : "🔒",
                    FontSize = 34,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });

                pile.Children.Add(new TextBlock
                {
                    Text = monde.Nom,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 2)
                });

                pile.Children.Add(new TextBlock
                {
                    Text = debloque ? Etoiles(reussites) : "☆☆☆",
                    FontSize = 20,
                    Foreground = Brushes.Goldenrod,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });

                pile.Children.Add(new TextBlock
                {
                    Text = debloque
                        ? $"Progression : {reussites} / 3 réussites"
                        : "Débloque le monde précédent.",
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                var jouer = new Button
                {
                    Content = debloque ? "Jouer" : "Verrouillé",
                    IsEnabled = debloque,
                    Tag = monde.Niveau,
                    Width = 118,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 12, 0, 0),
                    Background = (Brush)new BrushConverter().ConvertFromString(debloque ? "#BFE8C7" : "#DDE2E7")!,
                    BorderBrush = (Brush)new BrushConverter().ConvertFromString(debloque ? "#79C98A" : "#B7C0C9")!
                };
                jouer.Click += JouerButton_Click;

                pile.Children.Add(jouer);
                carte.Child = pile;
                MondesWrapPanel.Children.Add(carte);
            }
        }

        private static string Etoiles(int reussites)
        {
            int etoiles = Math.Clamp(reussites, 0, 3);
            return new string('★', etoiles) + new string('☆', 3 - etoiles);
        }

        private void JouerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button bouton || bouton.Tag is not int niveau)
            {
                return;
            }

            NiveauChoisi = niveau;
            DialogResult = true;
            Close();
        }

        private void FermerButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private sealed record AventureMonde(int Niveau, string Icone, string Nom, string Description);
    }
}
