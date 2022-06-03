using System.Drawing;

namespace AccountManager.Core.Static
{
    public static class ColorExtensions
    {
        public static Color LightenColor(this Color color, int factor)
        {
            var lightenedColor = Color.FromArgb(5, (int)(color.R + (255 - color.R) / factor), (int)(color.G + (255 - color.G) / factor), (int)(color.B + (255 - color.B) / factor));
            return lightenedColor;
        }
    }
}
