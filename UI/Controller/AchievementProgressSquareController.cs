using AchievementLib.Pack;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class AchievementProgressSquareController : Controller<AchievementProgressSquare, IAchievement>
    {
        private bool _showFillForCurrentTier;
        private bool _alwaysHideFillFraction;

        /// <summary>
        /// Determines whether the progress for the current tier, instead of for the whole achievement, is displayed.
        /// </summary>
        public bool ShowFillForCurrentTier
        {
            get => _showFillForCurrentTier;
            set
            {
                _showFillForCurrentTier = value;
                UpdateFill();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="AchievementProgressSquare.ShowFillFraction"/> will be set to 
        /// <see langword="false"/> regardless of the completion status of the achievement.
        /// </summary>
        public bool AlwaysHideFillFraction
        {
            get => _alwaysHideFillFraction;
            set
            {
                _alwaysHideFillFraction = value;
                UpdateCompletedDisplay();
            }
        }

        public AchievementProgressSquareController(AchievementProgressSquare control, IAchievement model) : base(control, model)
        {
            Model.CurrentObjectivesChanged += OnCurrentObjectivesChanged;
            Model.FulfilledChanged += OnFulfilledChanged;
            Model.IsUnlockedChanged += OnIsUnlockedChanged;

            Control.IsLocked = !Model.IsUnlocked;

            Control.FillColor = Model.Color ?? (Model.Parent as IAchievementCollection)?.Color ?? ColorManager.AchievementFallbackColor;

            if (Model.Icon != null)
            {
                Control.Icon = Model.Icon;
            }
            else if (Model.Parent != null && Model.Parent is IAchievementCollection collection && collection.Icon != null)
            {
                Control.Icon = collection.Icon;
            }
        }

        protected override void UpdateControl()
        {
            UpdateFill();
            UpdateTier();
            UpdateCompletedDisplay();
        }

        private void OnCurrentObjectivesChanged(object _, int i)
        {
            UpdateFill();
            UpdateTier();
            UpdateCompletedDisplay();
        }

        private void OnFulfilledChanged(object _, bool s)
        {
            UpdateCompletedDisplay();
        }

        private void OnIsUnlockedChanged(object _, bool isUnlocked)
        {
            Control.IsLocked = !isUnlocked;
        }

        private void UpdateFill()
        {
            Control.CurrentFill = Model.CurrentObjectives;

            if (ShowFillForCurrentTier)
            {
                Control.MaxFill = Model.Tiers.ElementAt(Model.CurrentTier - 1).Count;
            }
            else
            {
                Control.MaxFill = Model.Tiers.ElementAt(Model.GetMaxTier() - 1).Count;
            }
        }

        private void UpdateTier()
        {
            Control.CurrentTier = Model.CurrentTier;
        }

        private void UpdateCompletedDisplay()
        {
            if (Model.IsFulfilled)
            {
                Control.ShowVignette = false;
                Control.ShowTier = false;
                Control.ShowFill = false;
                Control.ShowFillFraction = false;
                Control.AnimateFill = false;

                Control.ShowBackgroundTint = false;

                return;
            }

            Control.ShowVignette = true;
            Control.ShowTier = Model.Tiers.Count() > 1;
            Control.ShowFill = true;
            if (AlwaysHideFillFraction)
            {
                Control.ShowFillFraction = false;
            }
            else
            {
                Control.ShowFillFraction = true;
            }
            Control.AnimateFill = true;

            Control.ShowBackgroundTint = true;
        }

        protected override void Unload()
        {
            Model.CurrentObjectivesChanged -= OnCurrentObjectivesChanged;
            Model.FulfilledChanged -= OnFulfilledChanged;
            Model.IsUnlockedChanged -= OnIsUnlockedChanged;

            base.Unload();
        }
    }
}
