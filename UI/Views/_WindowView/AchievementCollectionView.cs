using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementCollectionView : View
    {
        private static Logger Logger = Logger.GetLogger<AchievementCollectionView>();

        private Container _parent;
        private FlowPanel _flowPanel;

        private IAchievement[] _achievements;

        private int _achievementWidth;

        public event EventHandler<IAchievement> AchievementSelected;

        private void OnAchievementSelected(IAchievement achievement)
        {
            AchievementSelected?.Invoke(this, achievement);
        }

        public AchievementCollectionView(IEnumerable<IAchievement> achievements)
        {
            _achievements = achievements.ToArray();
        }

        public AchievementCollectionView(IAchievementCollection collection) : this(collection.Achievements)
        { /** NOOP **/ }

        private void RecalculateLayout()
        {
            int spaceWidth = _parent.ContentRegion.Width;
            int spaceHeigt = _parent.ContentRegion.Height;

            if (_flowPanel == null)
            {
                return;
            }

            _flowPanel.Width = spaceWidth;
            _flowPanel.Height = spaceHeigt;


            _achievementWidth = (int)((float)_flowPanel.Width / 2.0f) - 5;

            foreach (Control achievementSelection in _flowPanel.Children.ToArray())
            {
                achievementSelection.Width = _achievementWidth;
                achievementSelection.Height = (int)((float)_achievementWidth / (AchievementSelection.DEFAULT_WIDTH_HEIGHT_RATIO));
            }
        }

        private void OnParentResized(object _, ResizedEventArgs _1)
        {
            RecalculateLayout();
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;
            _parent.Resized += OnParentResized;

            _flowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                Location = new Point(0, 0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                Parent = buildPanel,
                CanScroll = true,
                ControlPadding = new Vector2(7, 7)
            };

            _achievementWidth = (int)((float)_flowPanel.Width / 2.0f) - 5;

            foreach (IAchievement achievement in _achievements)
            {
                AchievementSelection<IAchievement> achievementSelection;

                try
                {
                    achievementSelection = new AchievementSelection<IAchievement>(achievement)
                    {
                        Parent = _flowPanel,
                        Width = _achievementWidth,
                        Height = (int)((float)_achievementWidth / (AchievementSelection.DEFAULT_WIDTH_HEIGHT_RATIO))
                    };
                }
                catch (Exception ex) // TODO: remove at some point? Should not be neccessary
                {
                    Logger.Error($"Exception: {ex}");
                    return;
                }

                achievementSelection.Selected += OnAchievementSelectionSelected;
                achievementSelection.WatchedChanged += OnAchievementWatchedChanged;

                _flowPanel.AddChild(achievementSelection);
            }
        }

        private void OnAchievementSelectionSelected(object _, IAchievement achievement)
        {
            OnAchievementSelected(achievement);
        }

        private void OnAchievementWatchedChanged(object _, (bool IsWatched, IAchievement Achievement) e)
        {
            e.Achievement.IsWatched = e.IsWatched;
        }

        protected override void Unload()
        {
            AchievementSelected = null;
            
            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
            }

            // TODO: implement

            // TODO: unsubscribe from achievementSelection.Selected

            base.Unload();
        }
    }
}
