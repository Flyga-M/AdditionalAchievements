using System;

namespace Flyga.AdditionalAchievements.UI.Views._Interface
{
    public interface IMenuView : IViewSelection
    {
        /// <summary>
        /// Fires, when the text of the search bar changed.
        /// </summary>
        event EventHandler<string> SearchTextChanged;
    }
}
