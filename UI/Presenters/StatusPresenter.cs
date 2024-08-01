using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Status;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class StatusPresenter : Presenter<StatusView, StatusManager>
    {
        public StatusPresenter(StatusView view, StatusManager model) : base(view, model)
        {
            model.StatusChanged += OnStatusChanged;

            OnStatusChanged(null, null);
        }

        private void OnStatusChanged(object _, EventArgs _1)
        {
            RefreshStatuses();
        }

        private void RefreshStatuses()
        {
            View.SetStatusesByCategory(
                Model.Statuses.ToDictionary(
                    keyValuePair => keyValuePair.Key,
                    keyValuePair => keyValuePair.Value
                        .Select(provider => new StatusDisplay(provider))
                        .ToArray()
                )
            );
        }

        protected override void Unload()
        {
            Model.StatusChanged -= OnStatusChanged;
        }

    }
}
