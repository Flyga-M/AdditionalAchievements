using Blish_HUD.Graphics.UI;
using System;

namespace Flyga.AdditionalAchievements.UI.Views
{
    /// <summary>
    /// Represents an <see cref="IView"/>, that can invoke a <see cref="Back"/> event.
    /// </summary>
    public interface IBack : IView
    {
        /// <summary>
        /// Fires, when the back action is selected.
        /// </summary>
        event EventHandler Back;
    }
}
