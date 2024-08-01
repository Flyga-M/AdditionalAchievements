using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class CollectionMenuData
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Texture2D Icon;

        public CollectionMenuData(string id, string localizedName, Texture2D icon)
        {
            Id = id;
            Name = localizedName;
            Icon = icon;
        }
    }
}
