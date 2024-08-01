using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class BackData
    {
        public readonly Texture2D Icon;
        public readonly string Title;

        public BackData(Texture2D icon, string title)
        {
            Icon = icon;
            Title = title;
        }
    }
}
