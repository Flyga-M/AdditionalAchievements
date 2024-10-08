﻿using Blish_HUD.Controls;
using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;

namespace Flyga.AdditionalAchievements.UI
{
    /// <summary>
    /// Counterpart to the <see cref="View{TPresenter}"/> for <see cref="Control"/>s.
    /// </summary>
    /// <remarks>
    /// https://github.com/blish-hud/Blish-HUD/blob/dev/Blish%20HUD/GameServices/Graphics/UI/View%5BTPresenter%5D.cs
    /// </remarks>
    public abstract class Control<TController> : Control where TController : IController
    {
        public event EventHandler<EventArgs> Loaded;

        public event EventHandler<EventArgs> Unloaded;

        private bool _loaded = false;

        private TController _controller;
        public TController Controller
        {
            get => _controller;
            protected set
            {
                _controller = value;
                OnControllerAssigned(value);
            }
        }

        public Control<TController> WithController(TController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            this.Controller = controller;
            if (this.Visible)
            {
                // this is necessary, because Control.Visible is defaulted to true, so we need to make sure the
                // controller is properly loaded
                // it's imperfect, because the controller and control should be loaded BEFORE the first time the 
                // control is shown, but i don't see any way around this other than defaulting Visible to false,
                // which i don't wan't to do
                Initialize();
            }
            return this;
        }

        protected virtual void OnControllerAssigned(TController controller) { /* NOOP */ }

        /// <remarks>
        /// Needs to be called, before the <see cref="Control"/> is shown.
        /// </remarks>
        public async Task<bool> DoLoad(IProgress<string> progress)
        {
            if (_loaded)
            {
                return true;
            }

            _loaded = true;

            bool loadResult = await Controller.DoLoad(progress)
                           && await Load(progress);

            if (loadResult)
            {
                this.Loaded?.Invoke(this, EventArgs.Empty);
            }

            return loadResult;
        }

        private void Initialize()
        {
            Controller.DoUpdateControl();

            _ = DoLoad(new Progress<string>());
        }

        public override void Show()
        {
            Controller.DoUpdateControl();

            DoLoad(new Progress<string>()).ContinueWith((loadResult) => { if (loadResult.Result) { base.Show(); } });
        }

        protected virtual async Task<bool> Load(IProgress<string> progress)
        {
            return await Task.FromResult(true);
        }

        protected virtual void Unload() { /* NOOP */ }

        protected override void DisposeControl()
        {
            Controller.DoUnload();
            Unload();

            this.Unloaded?.Invoke(this, EventArgs.Empty);

            base.DisposeControl();
        }
    }
}
