using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Presenters;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementNotificationsView : View
    {
        private Container _parent;

        public bool IsUnloaded { get; private set; } = false;
        
        public FlowPanel NotificationsFlowPanel { get; private set; }

        public AchievementNotificationsView() { }

        public AchievementNotificationsView(AchievementHandler achievementHandler)
        {
            this.WithPresenter(new AchievementNotificationsPresenter(this, achievementHandler));
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;
            _parent.Resized += OnParentResized;

            this.NotificationsFlowPanel = new FlowPanelWithoutCaptureBlock
            {
                Width = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height,
                Parent = buildPanel,
                FlowDirection = ControlFlowDirection.SingleBottomToTop
            };
        }

        private void OnParentResized(object _, ResizedEventArgs _1)
        {
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            NotificationsFlowPanel.Width = _parent.ContentRegion.Width;
            NotificationsFlowPanel.Height = _parent.ContentRegion.Height;
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            return Task.FromResult(true);
        }

        protected override void Unload()
        {
            IsUnloaded = true;
            
            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
            }

            if (NotificationsFlowPanel != null)
            {
                NotificationsFlowPanel.Parent = null;
                NotificationsFlowPanel.Dispose();
            }
        }
    }
}
