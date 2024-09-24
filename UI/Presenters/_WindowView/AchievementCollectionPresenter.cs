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
                achievement.CurrentObjectivesChanged += OnAchievementCurrentObjectivesChanged;
            }
        }

        protected override void UpdateView()
        {
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
                AchievementSelection selection = new AchievementSelection(achievement, true);

                achievementSelections.Add(selection);
            }

            View.SetContent(achievementSelections);
            SortContent();
        }

        private void SortContent()
        {
            View.SortContent<AchievementSelection>(SortCombined);
        }

        private int SortCombined(AchievementSelection x, AchievementSelection y)
        {
            return SystemComparisonUtil.CombineComparisons(x, y, new Comparison<AchievementSelection>[]
            {
                SortByPinnedStatus,
                SortByLockedStatus,
                SortByCompletionPercent,
                SortByOriginalOrder
            });
        }

        /// <summary>
        /// Sorts the <see cref="AchievementSelection"/>s from highest completion percent to 
        /// lowest completion percent, with the 100% completed achievements at the end.
        /// </summary>
        /// <remarks>
        /// Will only work, if the <see cref="AchievementSelection.ProgressIndicator"/> implements 
        /// <see cref="IProgressIndicator"/>.
        /// </remarks>
        private int SortByCompletionPercent(AchievementSelection x, AchievementSelection y)
        {
            if (!(x.ProgressIndicator is IProgressIndicator progressX)
                || !(y.ProgressIndicator is IProgressIndicator progressY))
            {
                return 0;
            }

            // put fully completed achievements to the right
            if (progressX.IsCompleted && progressY.IsCompleted)
            {
                return 0;
            }
            if (progressX.IsCompleted && !progressY.IsCompleted)
            {
                return 1;
            }
            if (!progressX.IsCompleted && progressY.IsCompleted)
            {
                return -1;
            }

            // sort not fully completed achievements from highest completion percent to lowest
            if (progressX.Progress < progressY.Progress)
            {
                return 1;
            }

            if (Math.Abs(progressX.Progress - progressY.Progress) < 0.0001f)
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Sorts the <see cref="AchievementSelection"/>s from unlocked to locked.
        /// </summary>
        /// <remarks>
        /// Will only work, if the <see cref="AchievementSelection.ProgressIndicator"/> implements 
        /// <see cref="ILockable"/>.
        /// </remarks>
        private int SortByLockedStatus(AchievementSelection x, AchievementSelection y)
        {
            if (!(x.ProgressIndicator is ILockable lockableX)
                || !(y.ProgressIndicator is ILockable lockableY))
            {
                return 0;
            }

            if (lockableX.IsLocked == lockableY.IsLocked)
            {
                return 0;
            }

            if (lockableX.IsLocked && !lockableY.IsLocked)
            {
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Sorts the <see cref="AchievementSelection"/>s from pinned to not pinned.
        /// </summary>
        private int SortByPinnedStatus(AchievementSelection x, AchievementSelection y)
        {
            if (x.IsPinned == y.IsPinned)
            {
                return 0;
            }

            if (x.IsPinned && !y.IsPinned)
            {
                return -1;
            }

            return 1;
        }

        private int SortByOriginalOrder(AchievementSelection x, AchievementSelection y)
        {
            IAchievement achievementX = x.Controller?.Model as IAchievement;
            IAchievement achievementY = y.Controller?.Model as IAchievement;


            IAchievementCollection collectionX = achievementX?.Parent as IAchievementCollection;
            IAchievementCollection collectionY = achievementY?.Parent as IAchievementCollection;

            if (collectionX == null && collectionY == null)
            {
                return 0;
            }

            if (collectionX == null)
            {
                return 1;
            }

            if (collectionY == null)
            {
                return -1;
            }

            if (collectionX != collectionY)
            {
                return string.Compare(collectionX.GetFullName(), collectionY.GetFullName());
            }

            foreach(IHierarchyObject achievement in collectionX.Children)
            {
                if (achievement == achievementX)
                {
                    return -1;
                }

                if (achievement == achievementY)
                {
                    return 1;
                }
            }

            return 0; // TODO: mabye throw here? this shouldn't happen.
        }

        protected override void Unload()
        {
            foreach (IAchievement achievement in Model.Achievements.ToArray())
            {
                achievement.CurrentObjectivesChanged -= OnAchievementCurrentObjectivesChanged;
            }
        }
    }
}
