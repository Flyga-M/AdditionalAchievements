using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.Status.Models;
using System;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class ActionHandlerStatusProvider : IStatusProvider
    {
        private IActionHandler _handler;

        private StatusData _statusData;

        public StatusData Status
        {
            get => _statusData;
            private set
            {
                StatusData oldValue = _statusData;

                _statusData = value;
                
                if (value != oldValue)
                {
                    OnStatusChanged();
                }
            }
        }

        public virtual Func<IView> GetStatusView => null;

        public virtual string Id => _handler.GetType().Name;

        public virtual string Title => _handler.GetType().Name;

        public virtual string Category => "ActionHandler";

        public event EventHandler<StatusData> StatusChanged;

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, Status);
        }

        public ActionHandlerStatusProvider(IActionHandler actionHandler)
        {
            _handler = actionHandler;

            UpdateStatus();
            _handler.StateChanged += OnHandlerStateChanged;
        }

        private void OnHandlerStateChanged(object _, HandlerState _1)
        {
            UpdateStatus();
        }

        protected virtual void UpdateStatus()
        {
            if (_handler == null)
            {
                Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.GeneralMissingContext);
                return;
            }

            switch(_handler.State)
            {
                case HandlerState.None:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralUnknown);
                        break;
                    }
                case HandlerState.Working:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.GeneralNormal);
                        break;
                    }
                case HandlerState.Suspended:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Paused, Resources.Status.Provider.ActionHandlerPaused);
                        break;
                    }
                case HandlerState.PartiallySuspended:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Inhibited, Resources.Status.Provider.ActionHandlerInhibited);
                        break;
                    }
                case HandlerState.Fatal:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.GeneralStopped);
                        break;
                    }
                case HandlerState.Disposed:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.GeneralStopped);
                        break;
                    }
                default:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralImplementationMissing);
                        break;
                    }
            }
        }

        public void Dispose()
        {
            StatusChanged = null;
            
            if (_handler != null)
            {
                _handler.StateChanged -= OnHandlerStateChanged;
            }
        }
    }
}
