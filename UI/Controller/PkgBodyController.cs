using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Controls;
using System;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class PkgBodyController : Controller<PkgBody, AchievementPackPkg>
    {
        public PkgBodyController(PkgBody control, AchievementPackPkg model) : base(control, model)
        {
            Control.KeepUpdatedChanged += OnControlKeepUpdatedChanged;
            Model.KeepUpdatedChanged += OnModelKeepUpdatedChanged;

            Model.State.InstalledChanged += OnModelInstalledChanged;

            if (Model.State.CurrentManager != null)
            {
                Model.State.CurrentManager.PackUnloaded += OnPackUnloaded;
                if (Model.State.CurrentManager.State == AchievementLib.Pack.PackLoadState.Loaded)
                {
                    OnPackLoaded(null, null);
                }
                else
                {
                    Model.State.CurrentManager.PackLoaded += OnPackLoaded;
                }
            }
        }

        private void OnControlKeepUpdatedChanged(object _, bool keepUpdated)
        {
            Model.KeepUpdated = keepUpdated;
        }

        private void OnModelKeepUpdatedChanged(object _, bool keepUpdated)
        {
            Control.KeepUpdated = keepUpdated;
        }

        private void OnModelInstalledChanged(object _, bool isInstalled)
        {
            Control.ShowKeepUpdated = isInstalled && !Model.IsLocalOnly;
            Control.IsDownloaded = isInstalled;

            if (isInstalled && Model.State.CurrentManager != null)
            {
                Model.State.CurrentManager.PackUnloaded += OnPackUnloaded;
                if (Model.State.CurrentManager.State == AchievementLib.Pack.PackLoadState.Loaded)
                {
                    OnPackLoaded(null, null);
                }
                else
                {
                    Model.State.CurrentManager.PackLoaded += OnPackLoaded;
                }
            }
        }

        private void OnPackLoaded(object _, EventArgs _1)
        {
            Control.IsEnabled = true;
        }

        private void OnPackUnloaded(object _, EventArgs _1)
        {
            Control.IsEnabled = false;
        }

        protected override void UpdateControl()
        {
            Control.Title = Model.Name;
            Control.Description = Model.Description.Replace(@"\n", "\n");
            Control.KeepUpdated = Model.KeepUpdated;
            Control.ShowKeepUpdated = Model.State.IsInstalled && !Model.IsLocalOnly;
            Control.IsDownloaded = Model.State.IsInstalled;
            Control.IsEnabled = Model.State.CurrentManager?.State == AchievementLib.Pack.PackLoadState.Loaded;

            // TODO: localize
            Control.LastUpdateMessage = $"Last Update: {Model.LastUpdate.ToShortDateString()}";
        }

        protected override void Unload()
        {
            Control.KeepUpdatedChanged -= OnControlKeepUpdatedChanged;
            Model.KeepUpdatedChanged -= OnModelKeepUpdatedChanged;

            Model.State.InstalledChanged -= OnModelInstalledChanged;

            if (Model.State.CurrentManager != null)
            {
                Model.State.CurrentManager.PackLoaded -= OnPackLoaded;
                Model.State.CurrentManager.PackUnloaded -= OnPackUnloaded;
            }
        }
    }
}
