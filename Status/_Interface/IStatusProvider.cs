using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Status.Models;
using System;

namespace Flyga.AdditionalAchievements.Status
{
    public interface IStatusProvider : IDisposable
    {
        /// <summary>
        /// Fires, when the current status of the <see cref="IStatusProvider"/> changes.
        /// </summary>
        event EventHandler<StatusData> StatusChanged;

        /// <summary>
        /// An id, that is unique across all <see cref="IStatusProvider"/>s.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The title that is used when displaying the status of the <see cref="IStatusProvider"/>.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The category that is used when displaying the status of the <see cref="IStatusProvider"/>.
        /// </summary>
        string Category { get; }
        
        /// <summary>
        /// The current status of the <see cref="IStatusProvider"/>.
        /// </summary>
        StatusData Status {  get; }
        
        /// <summary>
        /// A <see cref="IView"/> to display details of the current <see cref="Status"/>.
        /// </summary>
        /// <remarks>
        /// Might be <see langword="null"/>.
        /// </remarks>
        Func<IView> GetStatusView { get; }
    }
}
