using AchievementLib.Pack;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Views;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class AchievementSelectionController : Controller<AchievementSelection, IAchievement>
    {
        public AchievementSelectionController(AchievementSelection control, IAchievement model, bool autoHide) : base(control, model)
        {
            Model.FulfilledChanged += OnAchievementCompleted;
            Model.IsWatchedChanged += OnAchievementIsWatchedChanged;

            Control.WatchedChanged += OnControlWatchedChanged;

            Control.HighlightColor = Model.Color ?? (Model.Parent as IAchievementCollection)?.Color ?? ColorManager.AchievementFallbackColor;
            Control.IsPinned = Model.IsPinned;

            // will be disposed by the control, does not need to be disposed by the controller
            Control.ProgressIndicator = new AchievementProgressSquare(Model, false);

            Control.GetSelectedView = GetAchievementView;

            if (autoHide)
            {
                Control.Visible = !Model.IsHidden || Model.IsUnlocked;
                Model.IsUnlockedChanged += OnAchievementIsUnlockedChanged;
            }
        }

        public AchievementSelectionController(AchievementSelection control, IAchievement model) : this(control, model, false)
        { /** NOOP **/ }

        protected override void UpdateControl()
        {
            UpdateLocaleDependentValues();
            UpdateCompletedStatus();
            UpdateIsWatchedStatus();
            // don't update visibility here, because it is updated in the constructor, only when autoHide is true
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

        private void UpdateVisibility()
        {
            Control.Visible = !Model.IsHidden || Model.IsUnlocked;
        }

        private void OnAchievementCompleted(object _, bool _1)
        {
            UpdateCompletedStatus();
        }

        private void OnAchievementIsWatchedChanged(object _, bool _1)
        {
            UpdateIsWatchedStatus();
        }

        private void OnAchievementIsUnlockedChanged(object _, bool _1)
        {
            UpdateVisibility();
        }

        private void OnControlWatchedChanged(object _, bool controlIsWatched)
        {
            Model.IsWatched = controlIsWatched;
        }

        private IView GetAchievementView(object[] @params)
        {
            if (@params.Length != 1)
            {
                // TODO: could log here, instead of just failing silently
                return null;
            }

            if (!(@params[0] is BackData backData))
            {
                return null;
            }

            return new AchievementView(Model, backData);
        }

        protected override void Unload()
        {
            Model.FulfilledChanged -= OnAchievementCompleted;
            Model.IsWatchedChanged -= OnAchievementIsWatchedChanged;
            Model.IsUnlockedChanged -= OnAchievementIsUnlockedChanged;

            Control.WatchedChanged -= OnControlWatchedChanged;
        }
    }
}
