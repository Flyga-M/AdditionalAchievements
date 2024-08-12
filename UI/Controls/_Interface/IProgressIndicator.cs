using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public interface IProgressIndicator
    {
        /// <summary>
        /// Determines whether the progress is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// The current progress. Between 0 and 1.
        /// </summary>
        float Progress { get; }
    }
}
