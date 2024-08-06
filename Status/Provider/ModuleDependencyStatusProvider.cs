using Blish_HUD.Contexts;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Integration;
using Flyga.AdditionalAchievements.Status.Models;
using System;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class ModuleDependencyStatusProvider : IStatusProvider
    {
        protected ModuleContextProvider _contextProvider;

        private StatusData _statusData;

        public string Id => _contextProvider.GetType().Name;

        public virtual string Title => _contextProvider?.Namespace ?? string.Empty;

        public string Category => "Dependencies"; // TODO: localize. see: ApiStatusProvider.Category

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

        public Context Context => _contextProvider.Context;

        public event EventHandler<StatusData> StatusChanged;

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, Status);
        }

        /// <remarks>
        /// The <paramref name="contextProvider"/> will be disposed when <see cref="Dispose"/> is called.
        /// </remarks>
        public ModuleDependencyStatusProvider(ModuleContextProvider contextProvider)
        {
            _statusData = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralUnknown);
            
            _contextProvider = contextProvider;

            _contextProvider.StateChanged += OnContextStateChanged;

            UpdateStatus();
        }

        private void OnContextStateChanged(object _, IntegrationState _1)
        {
            UpdateStatus();
        }

        protected virtual void UpdateStatus()
        {
            switch (_contextProvider.State)
            {
                case IntegrationState.None:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralUnknown);
                        break;
                    }
                case IntegrationState.DependencyMissing:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.ModuleDependencyMissing);
                        break;
                    }
                case IntegrationState.DependencyDisabled:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Paused, Resources.Status.Provider.ModuleDependencyDisabled);
                        break;
                    }
                case IntegrationState.Working:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.GeneralNormal);
                        break;
                    }
                default:
                    {
                        Status = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralImplementationMissing);
                        break;
                    }
            }
        }

        /// <remarks>
        /// Will dispose the provided <see cref="ModuleContextProvider"/>.
        /// </remarks>
        public virtual void Dispose()
        {
            StatusChanged = null;

            if (_contextProvider != null)
            {
                _contextProvider.StateChanged -= OnContextStateChanged;
                _contextProvider?.Dispose();
                _contextProvider = null;
            }
        }
    }
}
