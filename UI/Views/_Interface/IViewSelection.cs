using Blish_HUD.Graphics.UI;
using System;

namespace Flyga.AdditionalAchievements.UI.Views
{
    /// <summary>
    /// Represents an <see cref="IView"/> that can raise an event to signal a different <see cref="IView"/> 
    /// should be shown.
    /// </summary>
    public interface IViewSelection : IView
    {
        /// <summary>
        /// Fires, when a different <see cref="IView"/> should be shown.
        /// </summary>
        event EventHandler<Func<IView>> Selected;
    }
}
