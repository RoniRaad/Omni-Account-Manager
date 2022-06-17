using System.Drawing;

namespace AccountManager.Core.Static
{
    public static class ColorExtensions
    {
        public static Color LightenColor(this Color color, int factor)
        {
            var lightenedColor = Color.FromArgb(5, (color.R + (255 - color.R) / factor), (color.G + (255 - color.G) / factor), (color.B + (255 - color.B) / factor));
            return lightenedColor;
        }
    }
}
