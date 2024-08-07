using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Status.Models;
using System;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class MumbleStatusProvider : IStatusProvider
    {
        private StatusData _statusData;
        private Gw2MumbleService _context;

        public string Id => "Mumble";

        public string Title => "GW2 Mumble";

        public string Category => "Dependencies"; // TODO: localize

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

        public Func<IView> GetStatusView => null;

        public Gw2MumbleService Mumble => _context;

        public event EventHandler<StatusData> StatusChanged;

        /// <summary>
        /// Fires, when the UI size changes, the compass (minimap) position changes, or the compass 
        /// size changes.
        /// </summary>
        public event EventHandler UiChanged;

        public MumbleStatusProvider(Gw2MumbleService mumbleService)
        {
            _context = mumbleService;

            _context.UI.UISizeChanged += OnUiChanged;
            _context.UI.IsCompassTopRightChanged += OnUiChanged;
            _context.UI.CompassSizeChanged += OnUiChanged;

            UpdateStatus();
        }

        private void OnUiChanged(object _, EventArgs _1)
        {
            UiChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, Status);
        }

        private void UpdateStatus()
        {
            if (_context == null)
            {
                if (Status?.Status == AdditionalAchievements.Status.Status.Stopped)
                {
                    return;
                }

                Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.GeneralMissingContext);
                return;
            }

            if (_context.IsAvailable)
            {
                if (Status?.Status == AdditionalAchievements.Status.Status.Normal)
                {
                    return;
                }

                Status = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.GeneralNormal);
                return;
            }

            if (Status?.Status == AdditionalAchievements.Status.Status.Paused)
            {
                return;
            }

            Status = new StatusData(AdditionalAchievements.Status.Status.Paused, Resources.Status.Provider.MumbleNotReady);
        }

        /// <summary>
        /// Updates the mumble status.
        /// </summary>
        /// <remarks>
        /// Necessary, because there is currently no event when the the Gw2MumbleService.IsActive property changes.
        /// </remarks>
        public void Update()
        {
            UpdateStatus();
        }

        public void Dispose()
        {
            StatusChanged = null;
            UiChanged = null;

            if (_context != null)
            {
                _context.UI.UISizeChanged -= OnUiChanged;
                _context.UI.IsCompassTopRightChanged -= OnUiChanged;
                _context.UI.CompassSizeChanged -= OnUiChanged;
            }
        }
    }
}
