using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Presenters;
using Microsoft.Xna.Framework;
using System;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementView : View, IBack
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementView>();

        //private IAchievement _achievement;
        private BackData _backData;

        private Container _parent;

        private BackBar _backBar;

        private Image _achievementCompletedHighlight;

        private Panel _content;
        private Panel _leftColumn;
        private Panel _rightColumn;

        private const int _paddingColumns = 10;

        private Control _progressIndicator;
        private Control _achievementContent;

        private bool _showCompletedHighlight;
        private Color? _completedHighlightColor;


        //private AchievementProgressSquare _progressSquare;
        //private AchievementDescription _description;

        /// <summary>
        /// The progress indicator.
        /// </summary>
        /// <remarks>
        /// Disposes the previous <see cref="ProgressIndicator"/>, if it's not <see langword="null"/> and overwritten. 
        /// Will also be disposed, when the <see cref="AchievementView"/> is unloaded.
        /// </remarks>
        public Control ProgressIndicator
        {
            get => _progressIndicator;
            set
            {
                if (_progressIndicator != null)
                {
                    _progressIndicator.Parent = null;
                    _progressIndicator.Dispose();
                }

                _progressIndicator = value;

                if (_leftColumn != null)
                {
                    _progressIndicator.Parent = _leftColumn;
                    _progressIndicator.Width = _leftColumn.ContentRegion.Width;
                    _progressIndicator.Height = _leftColumn.ContentRegion.Width;
                }
            }
        }

        /// <summary>
        /// The content of the achievement. Usually contains the title, progress (in text form) and a description of 
        /// the achievement.
        /// </summary>
        /// <remarks>
        /// Disposes the previous <see cref="AchievementContent"/>, if it's not <see langword="null"/> and overwritten. 
        /// Will also be disposed, when the <see cref="AchievementView"/> is unloaded.
        /// </remarks>
        public Control AchievementContent
        {
            get => _achievementContent;
            set
            {
                if (_achievementContent != null)
                {
                    _achievementContent.Parent = null;
                    _achievementContent.Dispose();
                }

                _achievementContent = value;

                if (_rightColumn != null)
                {
                    _achievementContent.Parent = _rightColumn;
                    _achievementContent.Width = _rightColumn.Width;
                    _achievementContent.Height = _rightColumn.Height;
                }
            }
        }

        /// <summary>
        /// Determines whether the background highlight that usually marks a completed achievement, will 
        /// be visible.
        /// </summary>
        public bool ShowCompletedHighlight
        {
            get => _showCompletedHighlight;
            set
            {
                _showCompletedHighlight = value;
                if (_achievementCompletedHighlight != null)
                {
                    _achievementCompletedHighlight.Visible = value;
                }
            }
        }

        public Color CompletedHighlightColor
        {
            get => _completedHighlightColor ?? ColorManager.AchievementFallbackColor;
            set
            {
                _completedHighlightColor = value;
                if (_achievementCompletedHighlight != null)
                {
                    _achievementCompletedHighlight.Tint = CompletedHighlightColor * 0.5f;
                }
            }
        }

        /// <summary>
        /// Fires, when the <see cref="BackBar"/> was clicked.
        /// </summary>
        public event EventHandler Back;

        public event EventHandler ParentResized;

        private void OnBack()
        {
            Back?.Invoke(this, EventArgs.Empty);
        }

        public AchievementView(BackData backData)
        {
            _backData = backData;
        }

        public AchievementView(IAchievement achievement, BackData backData) : this (backData)
        {
            this.WithPresenter(new AchievementPresenter(this, achievement));
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

                    if (_progressIndicator != null)
                    {
                        _progressIndicator.Width = _leftColumn.ContentRegion.Width;
                        _progressIndicator.Height = _leftColumn.ContentRegion.Width;
                    }
                }

                if (_rightColumn != null)
                {
                    _rightColumn.Height = _content.Height;
                    _rightColumn.Width = _content.Width - (_leftColumn.Width + _paddingColumns);
                    _rightColumn.Left = _leftColumn.Width + _paddingColumns;

                    if (_achievementContent != null)
                    {
                        _achievementContent.Width = _rightColumn.Width;
                        // TODO: fix
                        _achievementContent.Height = _rightColumn.Height;
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
            ParentResized?.Invoke(this, EventArgs.Empty);
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

            if (_progressIndicator != null)
            {
                _progressIndicator.Parent = leftColumn;
                _progressIndicator.Width = leftColumn.ContentRegion.Width;
                _progressIndicator.Height = leftColumn.ContentRegion.Width;
            }

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

            if (_achievementContent != null)
            {
                _achievementContent.Parent = rightColumn;
                _achievementContent.Width = rightColumn.Width;
                _achievementContent.Height = rightColumn.Height;
            }

            return rightColumn;
        }

        private Panel BuildContent(Container buildPanel)
        {
            _achievementCompletedHighlight = new Image()
            {
                Parent = buildPanel,
                Texture = TextureManager.Display.Description.CompletedBackgroundHighlight,
                ClipsBounds = false,
                Location = new Point(0, _backBar.Height),
                Height = buildPanel.Height - _backBar.Height,
                Width = buildPanel.Width,
                Visible = ShowCompletedHighlight,
                Tint = CompletedHighlightColor * 0.5f
            };

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
            
            if (_progressIndicator != null)
            {
                _progressIndicator.Dispose();
                _progressIndicator = null;
            }

            if (_achievementContent != null)
            {
                _achievementContent.Dispose();
                _achievementContent = null;
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
