using AchievementLib.Pack;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
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
        }

        protected override void Unload()
        {
            foreach (IAchievement achievement in Model.Achievements.ToArray())
            {
                achievement.IsUnlockedChanged -= OnAchievementUnlockedChanged;
            }
        }
    }
}
