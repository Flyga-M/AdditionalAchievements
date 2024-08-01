using AchievementLib.Pack;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class AchievementSelectionController : Controller<AchievementSelection, IAchievement>
    {
        public AchievementSelectionController(AchievementSelection control, IAchievement model) : base(control, model)
        {
            Model.FulfilledChanged += OnAchievementCompleted;
            Model.IsWatchedChanged += OnAchievementIsWatchedChanged;

            Control.HighlightColor = Model.Color ?? ColorManager.AchievementFallbackColor;

            Control.ProgressIndicator = new AchievementProgressSquare(Model, false);
        }

        protected override void UpdateControl()
        {
            UpdateLocaleDependentValues();
            UpdateCompletedStatus();
            UpdateIsWatchedStatus();
        }

        private void UpdateLocaleDependentValues()
        {
            // TODO: make fallback locale an option
            Control.Title = Model.Name.GetLocalizedForUserLocale(fallbackLocale: Gw2Sharp.WebApi.Locale.English);
        }

        private void UpdateCompletedStatus()
        {
            if (Model.IsFulfilled)
            {
                Control.BackgroundOpacity = 0.5f;
                Control.ShowHighlight = true;
                Control.ShowWatchIcon = false;
                Control.ShowBottomSeparator = false;
                Control.ShowVignette = true;
                Control.Subtitle = "Completed"; // TODO: localize
                return;
            }

            Control.BackgroundOpacity = 0.1f;
            Control.ShowHighlight = false;
            Control.ShowWatchIcon = true;
            Control.ShowBottomSeparator = true;
            Control.ShowVignette = false;
            Control.Subtitle = null;
        }

        private void UpdateIsWatchedStatus()
        {
            Control.IsWatched = Model.IsWatched;
        }

        private void OnAchievementCompleted(object _, bool _1)
        {
            UpdateCompletedStatus();
        }

        private void OnAchievementIsWatchedChanged(object _, bool _1)
        {
            UpdateIsWatchedStatus();
        }

        protected override void Unload()
        {
            Model.FulfilledChanged -= OnAchievementCompleted;
            Model.IsWatchedChanged -= OnAchievementIsWatchedChanged;
        }
    }
}
