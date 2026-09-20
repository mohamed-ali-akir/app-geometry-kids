using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using RectangleShape = System.Windows.Shapes.Rectangle;

namespace Geometrie
{
    public partial class MiniJeuSnakeWindow : Window
    {
        private const int TailleGrille = 18;
        private const int TailleCase = 20;
        private const int NombrePommes = 3;

        private readonly DispatcherTimer timer;
        private readonly List<(int X, int Y)> serpent = [];
        private readonly List<(int X, int Y)> pommes = [];
        private readonly Random hasard = new();

        private (int X, int Y) direction = (1, 0);
        private (int X, int Y) prochaineDirection = (1, 0);
        private int score;
        private bool partieTerminee;

        public bool MiniJeuQuitte { get; private set; }

        public MiniJeuSnakeWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(120)
            };
            timer.Tick += Timer_Tick;

            Loaded += (_, _) =>
            {
                DemarrerPartie();
                Focus();
                Keyboard.Focus(JeuCanvas);
            };
        }

        private void DemarrerPartie()
        {
            serpent.Clear();
            serpent.Add((8, 9));
            serpent.Add((7, 9));
            serpent.Add((6, 9));
            direction = (1, 0);
            prochaineDirection = (1, 0);
            score = 0;
            partieTerminee = false;
            ScoreTextBlock.Text = "Score : 0";
            MessageTextBlock.Text = "Utilise les flèches pour attraper les pommes.";
            pommes.Clear();
            CompleterPommes();
            Dessiner();
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (partieTerminee)
            {
                return;
            }

            direction = prochaineDirection;
            (int X, int Y) tete = serpent[0];
            var nouvelleTete = (X: tete.X + direction.X, Y: tete.Y + direction.Y);

            if (nouvelleTete.X < 0
                || nouvelleTete.X >= TailleGrille
                || nouvelleTete.Y < 0
                || nouvelleTete.Y >= TailleGrille
                || serpent.Contains(nouvelleTete))
            {
                TerminerPartie();
                return;
            }

            serpent.Insert(0, nouvelleTete);

            int pommeMangee = pommes.FindIndex(pomme => pomme == nouvelleTete);
            if (pommeMangee >= 0)
            {
                score++;
                ScoreTextBlock.Text = $"Score : {score}";
                MessageTextBlock.Text = "Bravo ! Tu as attrapé une pomme.";
                pommes.RemoveAt(pommeMangee);
                CompleterPommes();
            }
            else
            {
                serpent.RemoveAt(serpent.Count - 1);
            }

            Dessiner();
        }

        private void CompleterPommes()
        {
            while (pommes.Count < NombrePommes)
            {
                (int X, int Y) nouvellePomme;

                do
                {
                    nouvellePomme = (hasard.Next(TailleGrille), hasard.Next(TailleGrille));
                }
                while (serpent.Contains(nouvellePomme) || pommes.Contains(nouvellePomme));

                pommes.Add(nouvellePomme);
            }
        }

        private void Dessiner()
        {
            JeuCanvas.Children.Clear();

            for (int index = serpent.Count - 1; index >= 0; index--)
            {
                bool tete = index == 0;
                DessinerMorceauSerpent(
                    serpent[index].X,
                    serpent[index].Y,
                    tete);
            }

            foreach ((int X, int Y) pomme in pommes)
            {
                DessinerPomme(pomme.X, pomme.Y);
            }
        }

        private void DessinerMorceauSerpent(int x, int y, bool tete)
        {
            var forme = new RectangleShape
            {
                RadiusX = tete ? 8 : 6,
                RadiusY = tete ? 8 : 6
            };

            forme.Width = TailleCase - 2;
            forme.Height = TailleCase - 2;
            forme.Fill = (Brush)new BrushConverter().ConvertFromString(tete ? "#1F8F4D" : "#61C86D")!;
            forme.Stroke = (Brush)new BrushConverter().ConvertFromString(tete ? "#0F5F32" : "#2E8B46")!;
            forme.StrokeThickness = 1.5;

            JeuCanvas.Children.Add(forme);
            Canvas.SetLeft(forme, x * TailleCase + 1);
            Canvas.SetTop(forme, y * TailleCase + 1);

            if (!tete)
            {
                return;
            }

            AjouterOeil(x * TailleCase + 6, y * TailleCase + 6);
            AjouterOeil(x * TailleCase + 13, y * TailleCase + 6);
        }

        private void AjouterOeil(double gauche, double haut)
        {
            var oeil = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = Brushes.White
            };

            JeuCanvas.Children.Add(oeil);
            Canvas.SetLeft(oeil, gauche);
            Canvas.SetTop(oeil, haut);
        }

        private void DessinerPomme(int x, int y)
        {
            var pomme = new Ellipse
            {
                Width = TailleCase - 5,
                Height = TailleCase - 5,
                Fill = new SolidColorBrush(Color.FromRgb(225, 64, 64)),
                Stroke = new SolidColorBrush(Color.FromRgb(160, 38, 38)),
                StrokeThickness = 1.4
            };

            JeuCanvas.Children.Add(pomme);
            Canvas.SetLeft(pomme, x * TailleCase + 3);
            Canvas.SetTop(pomme, y * TailleCase + 4);

            var tige = new RectangleShape
            {
                Width = 3,
                Height = 6,
                RadiusX = 1,
                RadiusY = 1,
                Fill = new SolidColorBrush(Color.FromRgb(116, 74, 38))
            };

            JeuCanvas.Children.Add(tige);
            Canvas.SetLeft(tige, x * TailleCase + 10);
            Canvas.SetTop(tige, y * TailleCase + 1);

            var feuille = new Ellipse
            {
                Width = 7,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(74, 156, 69))
            };

            JeuCanvas.Children.Add(feuille);
            Canvas.SetLeft(feuille, x * TailleCase + 12);
            Canvas.SetTop(feuille, y * TailleCase + 3);
        }

        private void TerminerPartie()
        {
            partieTerminee = true;
            timer.Stop();
            MessageTextBlock.Text = "Bien joué ! Clique sur Rejouer ou Quitter.";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            (int X, int Y)? nouvelleDirection = e.Key switch
            {
                Key.Left => (-1, 0),
                Key.Right => (1, 0),
                Key.Up => (0, -1),
                Key.Down => (0, 1),
                _ => null
            };

            if (nouvelleDirection is null)
            {
                return;
            }

            e.Handled = true;

            if (nouvelleDirection.Value.X + direction.X == 0
                && nouvelleDirection.Value.Y + direction.Y == 0)
            {
                return;
            }

            prochaineDirection = nouvelleDirection.Value;
        }

        private void RejouerButton_Click(object sender, RoutedEventArgs e)
        {
            DemarrerPartie();
            Keyboard.Focus(JeuCanvas);
        }

        private void QuitterMiniJeuButton_Click(object sender, RoutedEventArgs e)
        {
            MiniJeuQuitte = true;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            timer.Stop();
            MiniJeuQuitte = true;
            base.OnClosed(e);
        }
    }
}
