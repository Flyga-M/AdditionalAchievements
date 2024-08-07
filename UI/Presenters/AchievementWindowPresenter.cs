using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Views;


namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementWindowPresenter : Presenter<AchievementWindowView, AchievementHandler>
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementWindowPresenter>();

        public AchievementWindowPresenter(AchievementWindowView view, AchievementHandler model) : base(view, model)
        { /** NOOP **/ }

        protected override void UpdateView()
        {
            View.MenuView = new AchievementMenuView(Model);
        }
    }
}
