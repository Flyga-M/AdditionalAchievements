using AchievementLib.Pack;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementCollectionPresenter : Presenter<AchievementCollectionView, IAchievementCollection>
    {
        public AchievementCollectionPresenter(AchievementCollectionView view, IAchievementCollection model) : base(view, model)
        {
            // TODO: make fallback locale an option
            View.Title = Model.Name.GetLocalizedForUserLocale();
            View.Icon = Model.Icon;

            foreach (IAchievement achievement in Model.Achievements.ToArray())
            {
                achievement.IsUnlockedChanged += OnAchievementUnlockedChanged;
                achievement.CurrentObjectivesChanged += OnAchievementCurrentObjectivesChanged;
            }
        }

        protected override void UpdateView()
        {
            SetContent();
        }

        private void OnAchievementUnlockedChanged(object _, bool isUnlocked)
        {
            // The easiest way to (un)hide the correct AchievementSelections is to just rebuild them all.
            // View.SetContent() will take care to dispose the previous AchievementSelections.
            SetContent();
        }

        private void OnAchievementCurrentObjectivesChanged(object _, int _1)
        {
            SortContent();
        }

        private void SetContent()
        {
            IAchievement[] achievements = Model.Achievements.ToArray();

            List<AchievementSelection> achievementSelections = new List<AchievementSelection>();

            foreach (IAchievement achievement in achievements)
            {
                // will be disposed by the control, so does not need to be disposed here
                AchievementSelection selection = new AchievementSelection(achievement)
                {
                    Visible = !achievement.IsHidden || achievement.IsUnlocked
                };

                achievementSelections.Add(selection);


            }

            View.SetContent(achievementSelections);
            SortContent();
        }

        private void SortContent()
        {
            View.SortContent<AchievementSelection>(SortByCompletionPercent);
        }

        /// <summary>
        /// Sorts the <see cref="AchievementSelection"/>s from highest completion percent to 
        /// lowest completion percent, with the 100% completed (or locked) achievements at the end.
        /// </summary>
        /// <remarks>
        /// Will only work, if the <see cref="AchievementSelection.ProgressIndicator"/> is an 
        /// <see cref="AchievementProgressSquare"/>.
        /// </remarks>
        private int SortByCompletionPercent(AchievementSelection x, AchievementSelection y)
        {
            if (!(x.ProgressIndicator is AchievementProgressSquare progressX)
                || !(y.ProgressIndicator is AchievementProgressSquare progressY))
            {
                return 0;
            }

            float fillPercentX = (float)progressX.CurrentFill / (float)progressX.MaxFill;
            float fillPercentY = (float)progressY.CurrentFill / (float)progressY.MaxFill;

            bool isLockedX = progressX.IsLocked;
            bool isLockedY = progressY.IsLocked;

            // put locked achievements to the right
            if (isLockedX && !isLockedY)
            {
                return 1;
            }
            if (!isLockedX && isLockedY)
            {
                return -1;
            }
            if (isLockedX && isLockedY)
            {
                return 0;
            }

            // put fully completed achievements to the right
            if (fillPercentX == 1 && fillPercentY < 1)
            {
                return 1;
            }
            if (fillPercentY == 1 && fillPercentX < 1)
            {
                return -1;
            }

            // sort not fully completed achievements from highest completion percent to lowest
            if (fillPercentX < fillPercentY)
            {
                return 1;
            }

            if (Math.Abs(fillPercentX - fillPercentY) < 0.0001f)
            {
                return 0;
            }

            return -1;
        }

        protected override void Unload()
        {
            foreach (IAchievement achievement in Model.Achievements.ToArray())
            {
                achievement.IsUnlockedChanged -= OnAchievementUnlockedChanged;
                achievement.CurrentObjectivesChanged -= OnAchievementCurrentObjectivesChanged;
            }
        }
    }
}
