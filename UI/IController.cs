using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Flyga.AdditionalAchievements.UI
{
    /// <inheritdoc cref="IController"/>
    /// <typeparam name="TControl">The type of <see cref="Blish_HUD.Controls.Control"/> that the <see cref="IController{TControl}"/> will be controlling.</typeparam>
    public interface IController<out TControl> : IController where TControl : Control
    {
        /// <summary>
        /// The <see cref="Control"/> this <see cref="IController{TControl}"/> will be controlling.
        /// </summary>
        TControl Control { get; }
    }

    /// <summary>
    /// Counterpart to the <see cref="IPresenter"/> for <see cref="Control"/>s.
    /// </summary>
    /// <remarks>
    /// https://github.com/blish-hud/Blish-HUD/blob/dev/Blish%20HUD/GameServices/Graphics/UI/IPresenter.cs
    /// </remarks>
    public interface IController
    {
        /// <summary>
        /// The time to load anything that will be needed for the <see cref="IController"/>.
        /// This runs just before the <see cref="Control"/> has loaded.
        /// </summary>
        /// <param name="progress">Currently does not do anything.</param>
        /// <returns>A <see cref="bool"/> indicating if the <see cref="IController"/> loaded successfully or not.</returns>
        Task<bool> DoLoad(IProgress<string> progress);

        /// <summary>
        /// Runs after the <see cref="Control"/> is shown.
        /// This is a good time to update the <see cref="Control"/> to match the model state.
        /// </summary>
        void DoUpdateControl();

        /// <summary>
        /// Unload any resources that need to be manually unloaded
        /// as this <see cref="IController"/> will no longer be used.
        /// </summary>
        void DoUnload();
    }
}
