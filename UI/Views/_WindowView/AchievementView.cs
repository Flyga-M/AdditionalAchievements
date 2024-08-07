using System;
using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementView : View, IBack
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementView>();

        private IAchievement _achievement;
        private BackData _backData;

        private Container _parent;

        private BackBar _backBar;

        private Image _achievementCompletedHighlight;

        private Panel _content;
        private Panel _leftColumn;
        private Panel _rightColumn;

        private const int _paddingColumns = 10;

        private AchievementProgressSquare _progressSquare;
        private AchievementDescription _description;

        /// <summary>
        /// Fires, when the <see cref="BackBar"/> was clicked.
        /// </summary>
        public event EventHandler Back;

        private void OnBack()
        {
            Back?.Invoke(this, EventArgs.Empty);
        }

        public AchievementView(IAchievement achievement, BackData backData)
        {
            _achievement = achievement;
            _backData = backData;
        }

        private void OnBackBarClick(object _, MouseEventArgs _1)
        {
            OnBack();
        }

        private void RecalculateLayout()
        {
            int spaceWidth = _parent.ContentRegion.Width;
            int spaceHeigt = _parent.ContentRegion.Height;

            if (_backBar == null)
            {
                return;
            }

            _backBar.Width = spaceWidth;
            _backBar.Height = 48;

            if (_content != null)
            {
                _content.Height = spaceHeigt - 10 - _backBar.Height;
                _content.Width = spaceWidth - 20;

                if (_leftColumn != null)
                {
                    _leftColumn.Height = _content.Height;
                    _leftColumn.Width = _content.Width / 5;

                    if (_progressSquare != null)
                    {
                        _progressSquare.Width = _leftColumn.ContentRegion.Width;
                        _progressSquare.Height = _leftColumn.ContentRegion.Width;
                    }
                }

                if (_rightColumn != null)
                {
                    _rightColumn.Height = _content.Height;
                    _rightColumn.Width = _content.Width - (_leftColumn.Width + _paddingColumns);
                    _rightColumn.Left = _leftColumn.Width + _paddingColumns;

                    if (_description != null)
                    {
                        _description.Width = _rightColumn.Width;
                        _description.Height = _description.GetActualHeight() + 20;
                    }
                }
            }

            if (_achievementCompletedHighlight != null)
            {
                _achievementCompletedHighlight.Location = new Point(0, _backBar.Height);
                _achievementCompletedHighlight.Height = spaceHeigt - _backBar.Height;
                _achievementCompletedHighlight.Width = spaceWidth;
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
            
            // BackBar
            _backBar = new BackBar(_backData.Icon, _backData.Title)
            {
                Parent = buildPanel,
                Width = buildPanel.Width,
                Height = 48
            };
            _backBar.Click += OnBackBarClick;

            // Content
            _content = BuildContent(buildPanel);
        }

        private Panel BuildLeftColumn(Container parentContainer)
        {
            PanelWithRightBorder leftColumn = new PanelWithRightBorder()
            {
                Parent = parentContainer,
                Height = parentContainer.Height,
                Width = parentContainer.Width / 5,
                ShowBorder = true,
                ClipsBounds = false
            };

            _progressSquare = new AchievementProgressSquare(_achievement, true);

            _progressSquare.Parent = leftColumn;
            _progressSquare.Width = leftColumn.ContentRegion.Width;
            _progressSquare.Height = leftColumn.ContentRegion.Width;

            return leftColumn;
        }

        private Panel BuildRightColumn(Container parentContainer)
        {
            Panel rightColumn = new Panel()
            {
                Parent = parentContainer,
                Height = parentContainer.Height,
                Width = parentContainer.Width - (_leftColumn.Width + _paddingColumns),
                Left = _leftColumn.Width + _paddingColumns,
                CanScroll = true
            };

            try
            {
                _description = new AchievementDescription(_achievement)
                {
                    Parent = rightColumn,
                    Width = rightColumn.Width,
                    Height = rightColumn.Height
                };

                _description.Height = _description.GetActualHeight() + 20;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Ayy: {ex}");
            }
            

            return rightColumn;
        }

        private Panel BuildContent(Container buildPanel)
        {
            if (_achievement.IsFulfilled)
            {
                _achievementCompletedHighlight = new Image()
                {
                    Parent = buildPanel,
                    Texture = TextureManager.Display.Description.CompletedBackgroundHighlight,
                    ClipsBounds = false,
                    Location = new Point(0, _backBar.Height),
                    Height = buildPanel.Height - _backBar.Height,
                    Width = buildPanel.Width,
                    Tint = (_achievement.Color ?? ColorManager.AchievementFallbackColor) * 0.5f
                };
            }

            Panel contentContainer = new Panel()
            {
                Parent = buildPanel,
                Left = 20,
                Top = 10 + _backBar.Height,
                Height = buildPanel.Height - 10 - _backBar.Height,
                Width = buildPanel.Width - 20
            };

            _leftColumn = BuildLeftColumn(contentContainer);
            _rightColumn = BuildRightColumn(contentContainer);

            return contentContainer;
        }

        protected override void Unload()
        {
            Back = null;
            
            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
            }
            
            if (_progressSquare != null)
            {
                _progressSquare.Dispose();
                _progressSquare = null;
            }

            if (_description != null)
            {
                _description.Dispose();
                _description = null;
            }

            if (_achievementCompletedHighlight != null)
            {
                _achievementCompletedHighlight.Dispose();
                _achievementCompletedHighlight = null;
            }

            if (_leftColumn != null)
            {
                _leftColumn.Dispose();
                _leftColumn = null;
            }

            if (_rightColumn != null)
            {
                _rightColumn.Dispose();
                _rightColumn = null;
            }

            if (_content != null)
            {
                _content.Dispose();
                _content = null;
            }

            if (_backBar != null)
            {
                _backBar.Click -= OnBackBarClick;
                _backBar.Dispose();
                _backBar = null;
            }
        }
    }
}
