using Blish_HUD.Graphics.UI;
using Flurl;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Diagnostics;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class PkgPresenter : Presenter<PkgView, AchievementPackPkg>
    {
        public PkgPresenter(PkgView view, AchievementPackPkg model) : base(view, model)
        {
            View.PkgBody = new PkgBody(model);

            Model.State.InstalledChanged += OnModelInstalledChanged;
            Model.State.InstallError += OnModelInstallError;
            Model.State.DeleteError += OnModelDeleteError;

            View.DownloadClicked += OnViewDownloadClicked;
            View.InfoClicked += OnViewInfoClicked;
            View.DeleteClicked += OnViewDeleteClicked;
            View.EnableClicked += OnViewEnableClicked;

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

        private void OnModelInstalledChanged(object _, bool isInstalled)
        {
            View.CanDownload = !isInstalled;
            View.CanUpdate = Model.State.IsUpdateAvailable;

            View.CanDelete = isInstalled;

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

        private void OnModelInstallError(object _, string message)
        {
            View.DownloadTooltip = message;
            View.CanDownload = false;
        }

        private void OnModelDeleteError(object _, string message)
        {
            // TODO: currently invisible when CanDelete is false
            View.DeleteTooltip = message;
            View.CanDelete = false;
        }

        private async void OnViewDownloadClicked(object _, EventArgs _1)
        {   
            View.LockAllButtons = true;

            await PackHandlingUtil.DownloadOrUpdatePackAsync(
                Model,
                AdditionalAchievementsModule.Instance.GetModuleProgressHandler());

            View.LockAllButtons = false;
        }

        private void OnViewInfoClicked(object _, EventArgs _1)
        {
            if (Url.IsValid(Model.InfoUrl))
            {
                Process.Start(Model.InfoUrl);
            }
        }

        private void OnViewDeleteClicked(object _, EventArgs _1)
        {
            View.LockAllButtons = true;

            PackHandlingUtil.DeletePack(Model);

            View.LockAllButtons = false;
        }

        private async void OnViewEnableClicked(object _, EventArgs _1)
        {
            View.LockAllButtons = true;

            if (Model.State.CurrentManager != null)
            {
                if (Model.State.CurrentManager.State == AchievementLib.Pack.PackLoadState.Loaded)
                {
                    AdditionalAchievementsModule.Instance.DisablePack(Model.State.CurrentManager, true);
                }
                else if (Model.State.CurrentManager.State == AchievementLib.Pack.PackLoadState.Unloaded)
                {
                    // TODO: inform user if this returns false
                    await AdditionalAchievementsModule.Instance.EnablePackAsync(Model.Namespace);
                }

                // TODO: give user some feedback why nothing is happening
            }

            View.LockAllButtons = false;
        }

        private void OnPackLoaded(object _, EventArgs _1)
        {
            View.IsEnabled = true;
        }

        private void OnPackUnloaded(object _, EventArgs _1)
        {
            View.IsEnabled = false;
        }

        protected override void UpdateView()
        {
            View.PackInfoUrl = Model.InfoUrl;
            View.Tags = Model.Tags;

            View.CanDownload = !Model.State.IsInstalled;
            View.CanUpdate = Model.State.IsUpdateAvailable;

            View.CanDelete = Model.State.IsInstalled;

            View.IsEnabled = Model.State.CurrentManager?.State == AchievementLib.Pack.PackLoadState.Loaded;
        }

        protected override void Unload()
        {
            Model.State.InstalledChanged -= OnModelInstalledChanged;
            Model.State.InstallError -= OnModelInstallError;
            Model.State.DeleteError -= OnModelDeleteError;

            View.DownloadClicked -= OnViewDownloadClicked;
            View.InfoClicked -= OnViewInfoClicked;
            View.DeleteClicked -= OnViewDeleteClicked;

            if (Model.State.CurrentManager != null)
            {
                Model.State.CurrentManager.PackLoaded -= OnPackLoaded;
                Model.State.CurrentManager.PackUnloaded -= OnPackUnloaded;
            }
        }
    }
}
