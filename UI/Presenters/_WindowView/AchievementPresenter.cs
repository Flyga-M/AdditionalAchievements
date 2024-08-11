using AchievementLib.Pack;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementPresenter : Presenter<AchievementView, IAchievement>
    {
        public AchievementPresenter(AchievementView view, IAchievement model) : base(view, model)
        {
            View.ParentResized += OnViewParentResized;

            Model.FulfilledChanged += OnModelFulfilledChanged;
        }

        private void OnViewParentResized(object _, EventArgs _1)
        {
            if (View.AchievementContent != null && View.AchievementContent is AchievementDescription description)
            {
                description.Height = description.GetActualHeight();
            }
        }

        private void OnModelFulfilledChanged(object _, bool isFulfilled)
        {
            View.ShowCompletedHighlight = isFulfilled;
        }

        protected override void UpdateView()
        {
            // will be disposed by the view, so they don't need to be disposed here
            View.ProgressIndicator = new AchievementProgressSquare(Model, true);
            
            AchievementDescription description = new AchievementDescription(Model);
            View.AchievementContent = description;
            description.Height = description.GetActualHeight();

            View.ShowCompletedHighlight = Model.IsFulfilled;
            if (Model.Color.HasValue)
            {
                View.CompletedHighlightColor = Model.Color.Value;
            }
        }

        protected override void Unload()
        {
            View.ParentResized -= OnViewParentResized;

            Model.FulfilledChanged -= OnModelFulfilledChanged;
        }
    }
}
