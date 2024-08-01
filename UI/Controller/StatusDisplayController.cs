using Flyga.AdditionalAchievements.Status;
using Flyga.AdditionalAchievements.Status.Models;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class StatusDisplayController : Controller<StatusDisplay, IStatusProvider>
    {
        public StatusDisplayController(StatusDisplay control, IStatusProvider model) : base(control, model)
        {
            Model.StatusChanged += OnStatusChanged;
        }

        private void OnStatusChanged(object _, StatusData _1)
        {
            UpdateStatus();
        }

        protected override void UpdateControl()
        {
            Control.GetDetailView = Model.GetStatusView;
            Control.Title = Model.Title;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Control.Subtitle = Model.Status.Status.ToString();
            Control.HighlightColor = ColorManager.Status[Model.Status.Status];
            Control.BasicTooltipText = Model.Status.StatusMessage;
        }

        protected override void Unload()
        {
            Model.StatusChanged -= OnStatusChanged;
            
            base.Unload();
        }

    }
}
