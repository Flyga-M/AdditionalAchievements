using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementNotificationsPresenter : Presenter<AchievementNotificationsView, AchievementHandler>
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementNotificationsPresenter>();

        private const float _ratioHeightToWidth = 70.0f / 214.0f;

        public AchievementNotificationsPresenter(AchievementNotificationsView view, AchievementHandler model) : base(view, model)
        { /** NOOP **/ }

        private void OnAchievementCompleted(object _, IAchievement achievement)
        {
            if (SkipNotification(achievement))
            {
                return;
            }

            AchievementCompletedNotification notification = new AchievementCompletedNotification(achievement.Icon ?? ((IAchievementCollection)achievement.Parent).Icon, achievement.Name.GetLocalizedForUserLocale())
            {
                Parent = View.NotificationsFlowPanel,
                Width = View.NotificationsFlowPanel.Width,
                Height = (int)((float)View.NotificationsFlowPanel.Width * _ratioHeightToWidth)
            };
            View.NotificationsFlowPanel.AddChild(notification);

            notification.LifetimeEnd += OnNotificationLifetimeEnd;
        }

        private bool SkipNotification(IAchievement achievement)
        {
            if (!achievement.UsesApi)
            {
                return false;
            }

            if (!(achievement.GetRoot() is IAchievementPackManager manager))
            {
                Logger.Warn("Unable to determine if notification should be skipped, because " +
                    "achievement root is not IAchievementPackManager.");
                return false;
            }

            if (Model.PreviousApiUpdate == DateTime.MaxValue)
            {
                return true;
            }

            // skip notification, if this is the first API update for the pack (previous api update > pack enabled > current api update)
            return Model.PreviousApiUpdate < manager.LastEnabled;
        }

        private void OnNotificationLifetimeEnd(object sender, EventArgs _)
        {
            // TODO: evaluate if this can still be called, after the view was unloaded.

            if (!(sender is AchievementCompletedNotification notification))
            {
                Logger.Error("Unable to remove notification on lifetime end. OnNotificationLifetimeEnd called with a sender " +
                    $"that is not of type {typeof(AchievementCompletedNotification)}. Given type: {sender?.GetType()}");
                return;
            }

            View.NotificationsFlowPanel.RemoveChild(notification);
            notification.Parent = null;

            notification.LifetimeEnd -= OnNotificationLifetimeEnd;
            notification.Dispose();
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            Model.AchievementCompleted += OnAchievementCompleted;

            return Task.FromResult(true);
        }

        protected override void Unload()
        {
            Model.AchievementCompleted -= OnAchievementCompleted;
        }
    }
}
