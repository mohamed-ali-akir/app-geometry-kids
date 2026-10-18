using System.Windows.Media;

namespace Geometrie
{
    /// <summary>Couleurs de l'interface. Les pinceaux sont créés une seule fois puis réutilisés.</summary>
    internal static class Couleurs
    {
        public static readonly (string Fond, string Bordure) Info = ("#FFF7D6", "#F3D36B");
        public static readonly (string Fond, string Bordure) Erreur = ("#FFE7E7", "#E89A9A");
        public static readonly (string Fond, string Bordure) Succes = ("#E8F8ED", "#8BCB9A");
        public static readonly (string Fond, string Bordure) Badge = ("#F7E8FF", "#B984E8");
        public static readonly (string Fond, string Bordure) Partiel = ("#FFF4C2", "#E5BE4E");
        public static readonly (string Fond, string Bordure) Echec = ("#FFE2DD", "#E99B8F");
        public static readonly (string Fond, string Bordure) Indice = ("#FFF7D6", "#E3BD45");

        private static readonly Dictionary<string, SolidColorBrush> Pinceaux = [];

        public static SolidColorBrush Pinceau(string couleur)
        {
            if (!Pinceaux.TryGetValue(couleur, out SolidColorBrush? pinceau))
            {
                pinceau = new SolidColorBrush((Color)ColorConverter.ConvertFromString(couleur));
                pinceau.Freeze();
                Pinceaux[couleur] = pinceau;
            }

            return pinceau;
        }
    }
}
