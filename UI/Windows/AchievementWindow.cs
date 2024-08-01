using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Views;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.UI.Windows
{
    public class AchievementWindow : TabbedWindow2
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementWindow>();

        private AchievementHandler _achievementHandler;

        private const int TAB_WIDTH = 84;

        private static readonly Rectangle _fixedWindowRegion = new Rectangle(40, 26, 913, 691);
        private static readonly Rectangle _fixedContentRegion = new Rectangle(45 + TAB_WIDTH, 31, 903 - TAB_WIDTH, 675);

        public AchievementWindow(AchievementHandler achievementHandler) : base(TextureManager.Display.AchievementWindowBackground, _fixedWindowRegion, _fixedContentRegion)
        {
            _achievementHandler = achievementHandler;

            Title = "AdditionalAchievements"; // TODO: localize (maybe)
            Emblem = TextureManager.Display.Emblem;
            CanResize = true; // TODO: overthink. maybe just make it resizable in an "edit mode" or something.
            SavesSize = true;
            SavesPosition = true;
            Id = $"{nameof(AdditionalAchievementsModule)}.AchievementWindow";

            // TODO: localize
            Tabs.Add(new Tab(TextureManager.Display.TabAchievementsIcon, () => new AchievementWindowView(_achievementHandler), "Achievements"));
            Tabs.Add(new Tab(TextureManager.Display.TabRepositoryIcon, () => null, "Pack Repository"));
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
