﻿using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Views;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.UI.Windows
{
    public class AchievementWindow : TabbedWindow2WithStatus
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementWindow>();

        private AchievementHandler _achievementHandler;
        private AchievementPackRepo _achievementPackRepo;

        private const int TAB_WIDTH = 64;

        private static readonly Rectangle _fixedWindowRegion = new Rectangle(40, 26, 913, 691);
        private static readonly Rectangle _fixedContentRegion = new Rectangle(40 + TAB_WIDTH, 31, 903 - TAB_WIDTH, 675);

        public AchievementWindow(AchievementHandler achievementHandler, AchievementPackRepo achievementPackRepo) : base(TextureManager.Display.AchievementWindowBackground, _fixedWindowRegion, _fixedContentRegion)
        {
            _achievementHandler = achievementHandler;
            _achievementPackRepo = achievementPackRepo;

            Title = "AdditionalAchievements"; // TODO: localize (maybe)
            Emblem = TextureManager.Display.Emblem;
            CanResize = true; // TODO: overthink. maybe just make it resizable in an "edit mode" or something.
            SavesSize = true;
            SavesPosition = true;
            Id = $"{nameof(AdditionalAchievementsModule)}.AchievementWindow";

            // TODO: localize
            Tabs.Add(new Tab(TextureManager.Display.TabAchievementsIcon, () => new AchievementWindowView(_achievementHandler), "Achievements"));
            Tabs.Add(new Tab(TextureManager.Display.TabRepositoryIcon, () => new RepoView(_achievementPackRepo), "Pack Repository"));
            Tabs.Add(new Tab(TextureManager.Display.TabStatusIcon, () =>
                        {
                            StatusView statusView = new StatusView(AdditionalAchievementsModule.Instance.StatusManager);
                            statusView.StatusSelected += (_, view) => this.ShowView(view);
                            return statusView;
                        },
                        "Status Overview")
                    );
        }

        //public override void Show()
        //{
        //    this.Show(new AchievementWindowView(_achievementHandler));
        //}
    }
}
