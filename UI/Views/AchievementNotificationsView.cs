using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Presenters;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementNotificationsView : View
    {
        public bool IsUnloaded { get; private set; } = false;
        
        public FlowPanel NotificationsFlowPanel { get; private set; }

        public AchievementNotificationsView() { }

        public AchievementNotificationsView(AchievementHandler achievementHandler)
        {
            this.WithPresenter(new AchievementNotificationsPresenter(this, achievementHandler));
        }

        protected override void Build(Container buildPanel)
        {
            this.NotificationsFlowPanel = new FlowPanel
            {
                Width = buildPanel.Width,
                Height = buildPanel.Height,
                CanScroll = false,
                CanCollapse = false,
                Parent = buildPanel,
                FlowDirection = ControlFlowDirection.SingleBottomToTop
            };
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            
            
            return Task.FromResult(true);
        }

        protected override void Unload()
        {
            IsUnloaded = true;
            
            if (NotificationsFlowPanel != null)
            {
                NotificationsFlowPanel.Parent?.RemoveChild(NotificationsFlowPanel);

                foreach (Control child in NotificationsFlowPanel.Children.ToArray())
                {
                    NotificationsFlowPanel.RemoveChild(child);
                    child.Dispose();
                }

                NotificationsFlowPanel.Dispose();
            }
        }
    }
}
