using Blish_HUD;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class PkgBodyController : Controller<PkgBody, AchievementPackPkg>
    {
        public PkgBodyController(PkgBody control, AchievementPackPkg model) : base(control, model)
        {
            Control.KeepUpdatedChanged += OnControlKeepUpdatedChanged;
            Model.KeepUpdatedChanged += OnModelKeepUpdatedChanged;

            Model.State.InstalledChanged += OnModelInstalledChanged;
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
            Control.ShowKeepUpdated = isInstalled;
        }

        protected override void UpdateControl()
        {
            Control.Title = Model.Name;
            Control.Description = Model.Description.Replace(@"\n", "\n");
            Control.KeepUpdated = Model.KeepUpdated;
            Control.ShowKeepUpdated = Model.State.IsInstalled;

            // TODO: localize
            Control.LastUpdateMessage = $"Last Update: {Model.LastUpdate.ToShortDateString()}";

            Control.ShowKeepUpdated = Model.State.IsInstalled;
        }

        protected override void Unload()
        {
            Control.KeepUpdatedChanged -= OnControlKeepUpdatedChanged;
            Model.KeepUpdatedChanged -= OnModelKeepUpdatedChanged;

            Model.State.InstalledChanged -= OnModelInstalledChanged;
        }
    }
}
