using System;
using System.Threading.Tasks;
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

        protected override Task<bool> Load(IProgress<string> progress)
        {
            Model.PackAddedOrRemoved += OnPackAddedOrRemoved;
            Model.ResetOccured += OnReset;

            return Task.FromResult(true);
        }

        private void OnPackAddedOrRemoved(object _, bool _1)
        {
           // currently NOOP
        }

        private void OnReset(object _, string _1)
        {
            // currently NOOP
        }
    }
}
