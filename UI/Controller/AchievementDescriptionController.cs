using AchievementLib.Pack;
using Flyga.AdditionalAchievements.UI.Controls;

namespace Flyga.AdditionalAchievements.UI.Controller
{
    public class AchievementDescriptionController : Controller<AchievementDescription, IAchievement>
    {
        public AchievementDescriptionController(AchievementDescription control, IAchievement model) : base(control, model)
        {
            Control.ProgressTextIndicator = new AchievementProgressText(Model);
        }

        protected override void UpdateControl()
        {
            UpdateLocaleDependentValues();
        }

        private void UpdateLocaleDependentValues()
        {
            // TODO: make fallback locale an option
            Control.Title = Model.Name.GetLocalizedForUserLocale(fallbackLocale: Gw2Sharp.WebApi.Locale.English);
            Control.Description = Model.Description.GetLocalizedForUserLocale(fallbackLocale: Gw2Sharp.WebApi.Locale.English);
        }

    }
}
