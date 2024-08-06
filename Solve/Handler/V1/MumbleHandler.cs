using AchievementLib.Pack;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Models;
using Flyga.AdditionalAchievements.Status.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1
{
    public abstract class MumbleHandler<TAction> : ActionHandler<TAction> where TAction : class, IAction
    {
        protected readonly MumbleStatusProvider _statusProvider;
        protected Gw2MumbleService _context => _statusProvider.Mumble;

        public MumbleHandler(MumbleStatusProvider mumbleStatusProvider)
        {
            _statusProvider = mumbleStatusProvider ?? throw new ArgumentNullException(nameof(mumbleStatusProvider));

            _statusProvider.StatusChanged += OnStatusChanged;
            UpdateState();
        }

        private void OnStatusChanged(object _, StatusData _1)
        {
            UpdateState();
        }

        /// <summary>
        /// Updates the <see cref="ActionHandler{TAction}.State"/>, when the 
        /// <see cref="MumbleStatusProvider.Status"/> changes.
        /// </summary>
        protected virtual void UpdateState()
        {
            if (State == HandlerState.Fatal)
            {
                return;
            }

            if (_statusProvider == null || _statusProvider.Status.Status == Status.Status.Stopped)
            {
                State = HandlerState.Fatal;
                return;
            }
            if (_statusProvider.Status.Status == Status.Status.Normal)
            {
                State = HandlerState.Working;
                return;
            }

            State = HandlerState.Suspended;
        }

        protected override void Cleanup()
        {
            if (_statusProvider != null)
            {
                _statusProvider.StatusChanged -= OnStatusChanged;
            }

            base.Cleanup();
        }
    }
}
