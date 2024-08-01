using Blish_HUD.Controls;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI
{
    /// <summary>
    /// Counterpart to the <see cref="Blish_HUD.Graphics.UI.Presenter{TView, TModel}"/> for <see cref="Control"/>s.
    /// </summary>
    /// <remarks>
    /// https://github.com/blish-hud/Blish-HUD/blob/dev/Blish%20HUD/GameServices/Graphics/UI/Presenter%5BTView%2CTModel%5D.cs
    /// </remarks>
    /// <typeparam name="TControl"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public abstract class Controller<TControl, TModel> : IController<TControl> where TControl : Control
    {
        private readonly TControl _control;
        private readonly TModel _model;

        /// <inheritdoc cref="IController{TControl}.Control"/>
        public TControl Control => _control;

        /// <summary>
        /// The model this <see cref="Controller{TControl, TModel}"/> will use to determine
        /// how to present to the <see cref="Control"/>.
        /// </summary>
        public TModel Model => _model;

        protected Controller(TControl control, TModel model)
        {
            _control = control;
            _model = model;
        }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress)
        {
            return await Load(progress);
        }

        /// <inheritdoc />
        public void DoUpdateControl()
        {
            UpdateControl();
        }

        /// <inheritdoc />
        public void DoUnload()
        {
            Unload();
        }

        /// <inheritdoc cref="IController.DoLoad"/>
        protected virtual async Task<bool> Load(IProgress<string> progress)
        {
            return await Task.FromResult(true);
        }

        /// <inheritdoc cref="IController.DoUpdateControl"/>
        protected virtual void UpdateControl() { /* NOOP */ }


        /// <inheritdoc cref="IController.DoUnload"/>
        protected virtual void Unload() { /* NOOP */ }
    }
}
