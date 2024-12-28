using Blish_HUD;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flyga.AdditionalAchievements.Textures.Colors
{
    public static class ColorManager
    {
        /// <summary>
        /// The <see cref="Color"/> that is used for achievements, that don't have a <see cref="Color"/> assigned.
        /// </summary>
        public static readonly Color AchievementFallbackColor = Color.BlueViolet;

        public static readonly ReadOnlyDictionary<Status.Status, Color> Status = new ReadOnlyDictionary<Status.Status, Color>(
            new Dictionary<Status.Status, Color>()
            {
                { Flyga.AdditionalAchievements.Status.Status.Unknown, Color.LightGray },
                { Flyga.AdditionalAchievements.Status.Status.Normal, Color.LightGreen },
                { Flyga.AdditionalAchievements.Status.Status.Inhibited, new Color(255, 255, 155, 255) },
                { Flyga.AdditionalAchievements.Status.Status.Paused, Color.LightPink },
                { Flyga.AdditionalAchievements.Status.Status.Stopped, Color.Red }
            }
        );

        /// <summary>
        /// The <see cref="Color"/> that is used for the achievement points font.
        /// </summary>
        public static readonly Color AchievementPointsHighlightColor = ContentService.Colors.ColonialWhite;
    }
}
