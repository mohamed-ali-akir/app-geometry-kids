using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Geometrie.Core;

namespace Geometrie
{
    public partial class App : Application
    {
        private static readonly string JournalErreurs = Path.Combine(
            Path.GetDirectoryName(DepotProgression.FichierParDefaut)!,
            "erreurs.log");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += SurErreurInterface;
            AppDomain.CurrentDomain.UnhandledException += (_, args) => Journaliser(args.ExceptionObject as Exception);

            var fenetre = new MainWindow(new DepotProgression(DepotProgression.FichierParDefaut));
            MainWindow = fenetre;
            fenetre.Show();
        }

        /// <summary>
        /// Dernier filet de sécurité : une erreur imprévue est notée dans un journal et l'enfant
        /// voit un message simple au lieu d'une fermeture brutale de l'application.
        /// </summary>
        private void SurErreurInterface(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Journaliser(e.Exception);
            e.Handled = true;

            MessageBox.Show(
                "Oups, Géo le robot a trébuché ! Tu peux continuer à jouer.\nSi ça recommence, demande de l'aide à un adulte.",
                "Petit souci",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private static void Journaliser(Exception? erreur)
        {
            if (erreur is null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(JournalErreurs)!);
                string horodatage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                File.AppendAllText(JournalErreurs, $"[{horodatage}] {erreur}{Environment.NewLine}{Environment.NewLine}");
            }
            catch (Exception erreurJournal) when (erreurJournal is IOException or UnauthorizedAccessException)
            {
                // Impossible d'écrire le journal : on n'empêche pas l'application de continuer pour autant.
            }
        }
    }
}
