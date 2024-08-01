using AchievementLib.Pack;
using Flyga.AdditionalAchievements.Resources;
using Flyga.AdditionalAchievements.UI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class AchievementProgressTextController : Controller<AchievementProgressText, IAchievement>
    {
        public AchievementProgressTextController(AchievementProgressText control, IAchievement model) : base(control, model)
        {
            Model.CurrentObjectivesChanged += OnObjectivesChanged;
        }

        protected override void UpdateControl()
        {
            UpdateValues();
        }

        private void UpdateValues()
        {
            List<(string Title, string Content)> result = new List<(string Title, string Content)> ();
            
            if (!Model.IsFulfilled)
            {
                result.Add(($"{Strings.AchievementTier}:", $"{Model.CurrentTier}/{Model.GetMaxTier()}"));
            }

            result.Add(($"{Strings.AchievementObjectives}:", $"{Model.CurrentObjectives}/{Model.Tiers.ElementAt(Model.CurrentTier - 1)}"));

            Control.SetValues(result);
        }

        private void OnObjectivesChanged(object _, int _1)
        {
            UpdateControl();
        }

        protected override void Unload()
        {
            Model.CurrentObjectivesChanged -= OnObjectivesChanged;
        }
    }
}
