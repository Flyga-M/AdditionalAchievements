using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyga.AdditionalAchievements.UI.Controls;
using Blish_HUD;
using System.Diagnostics;
using Flurl;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class PkgViewPresenter : Presenter<PkgView, AchievementPackPkg>
    {
        public PkgViewPresenter(PkgView view, AchievementPackPkg model) : base(view, model)
        {
            View.PkgBody = new PkgBody(model);

            Model.State.InstalledChanged += OnModelInstalledChanged;
            Model.State.InstallError += OnModelInstallError;
            Model.State.DeleteError += OnModelDeleteError;

            View.DownloadClicked += OnViewDownloadClicked;
            View.InfoClicked += OnViewInfoClicked;
            View.DeleteClicked += OnViewDeleteClicked;
        }

        private void OnModelInstalledChanged(object _, bool isInstalled)
        {
            View.CanDownload = !isInstalled;
            View.CanUpdate = Model.State.IsUpdateAvailable;

            View.CanDelete = isInstalled;
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

            //PackHandlingUtil.DeletePack(Model);
            AdditionalAchievementsModule.Instance.TryDeletePack(Model.Namespace);

            View.LockAllButtons = false;
        }

        protected override void UpdateView()
        {
            View.PackInfoUrl = Model.InfoUrl;
            View.Tags = Model.Tags;

            View.CanDownload = !Model.State.IsInstalled;
            View.CanUpdate = Model.State.IsUpdateAvailable;

            View.CanDelete = Model.State.IsInstalled;
        }

        protected override void Unload()
        {
            Model.State.InstalledChanged -= OnModelInstalledChanged;
            Model.State.InstallError -= OnModelInstallError;
            Model.State.DeleteError -= OnModelDeleteError;

            View.DownloadClicked -= OnViewDownloadClicked;
            View.InfoClicked -= OnViewInfoClicked;
            View.DeleteClicked -= OnViewDeleteClicked;
        }
    }
}
