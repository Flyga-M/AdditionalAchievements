using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controller;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the selectable achievement overview when viewing a collection of achievements.
    /// </summary>
    public class AchievementSelection : Container<AchievementSelectionController>, IPinnable
    {
        #region default constants

        public const float DEFAULT_WIDTH_HEIGHT_RATIO = (float)DEFAULT_WIDTH / (float)DEFAULT_HEIGHT;

        private const int DEFAULT_WIDTH = 398;
        private const int DEFAULT_HEIGHT = 113;

        private const int DEFAULT_BOTTOM_HEIGHT = 42;
        private const int DEFAULT_WATCH_WIDTH = 52;
        private const int DEFAULT_WATCH_ICON_PADDING_RIGHT = (int)((DEFAULT_WATCH_WIDTH - DEFAULT_BOTTOM_HEIGHT) / 2.0f);
        private const int DEFAULT_WATCH_ICON_HOVER_HIGHLIGHT_SIZE = 350;
        private const int DEFAULT_WATCH_ICON_WATCHED_HIGHLIGHT_SIZE = 400;

        private const int DEFAULT_PADDING_PROGRESS = 22;

        private const int DEFAULT_TITLE_HEIGHT = 24;

        private const int DEFAULT_SEPARATOR_HEIGHT = 8;

        #endregion

        private bool _showWatchIcon;

        private Control _progressIndicator;
        protected WatchIcon _watchIcon;

        private bool _isWatched;

        private FlowPanel _additionalIcons;
        private readonly object _additionalIconsLock = new object();

        public event EventHandler<bool> WatchedChanged;

        protected virtual void OnWatchedChanged()
        {
            WatchedChanged?.Invoke(this, _isWatched);

            if (_watchIcon != null)
            {
                _watchIcon.IsSelected = _isWatched;
            }
        }

        /// <summary>
        /// The opacity of the background.
        /// </summary>
        /// <remarks>
        /// Usually 0.1f (uncompleted) or 0.5f (completed).
        /// </remarks>
        public float BackgroundOpacity { get; set; } = 0.1f;

        /// <summary>
        /// Determines whether the achievement is currently being watched.
        /// </summary>
        /// <remarks>
        /// Only makes sense, if the achievement has not been completed yet.
        /// </remarks>
        public bool IsWatched
        {
            get => _isWatched;
            set
            {
                bool oldValue = _isWatched;
                _isWatched = value;

                if (oldValue != value)
                {
                    OnWatchedChanged();
                }
            }
        }

        /// <summary>
        /// Determines whether the background highlight should be visible.
        /// </summary>
        /// <remarks>
        /// Usually visible on completed achievements.
        /// </remarks>
        public bool ShowHighlight { get; set; }

        /// <summary>
        /// Determines whether the watch icon (eye) should be visible.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements.
        /// </remarks>
        public bool ShowWatchIcon
        {
            get => _showWatchIcon;
            set
            {
                _showWatchIcon = value;
                if (_watchIcon != null)
                {
                    if (value)
                    {
                        _watchIcon.Show();
                        return;
                    }
                    _watchIcon.Hide();
                }
            }
        }

        /// <summary>
        /// Determines whether the horizontal separator should be visible.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements.
        /// </remarks>
        public bool ShowBottomSeparator { get; set; }

        /// <summary>
        /// Determines whether a vignette should be visible.
        /// </summary>
        /// <remarks>
        /// Usually visible on completed achievements.
        /// </remarks>
        public bool ShowVignette { get; set; }

        /// <summary>
        /// The color of the background highlight.
        /// </summary>
        /// <remarks>
        /// Usually matches the color that is used by the <see cref="ProgressIndicator"/>.
        /// </remarks>
        public Color HighlightColor { get; set; } = ColorManager.AchievementFallbackColor;

        /// <summary>
        /// The title of the achievement selection.
        /// </summary>
        /// <remarks>
        /// Usually matches the achievement name.
        /// </remarks>
        public string Title {  get; set; }

        /// <summary>
        /// The subtitle of the achievement selection.
        /// </summary>
        /// <remarks>
        /// Usually says "Completed" for a completed achievement.
        /// </remarks>
        public string Subtitle { get; set; }

        /// <summary>
        /// The progress indicator.
        /// </summary>
        /// <remarks>
        /// Disposes the previous <see cref="ProgressIndicator"/>, if it's not <see langword="null"/> and overwritten. 
        /// Will also be disposed, when the <see cref="AchievementSelection"/> is disposed.
        /// </remarks>
        public Control ProgressIndicator
        {
            get => _progressIndicator;
            set
            {
                if (_progressIndicator != null)
                {
                    RemoveChild(_progressIndicator);
                    _progressIndicator.Parent = null;
                    _progressIndicator.Dispose();
                }

                _progressIndicator = value;
                _progressIndicator.Parent = this;

                // some calculations depend on the indicator
                // TODO: maybe save calculated values instead of relying on the control being set.
                RecalculateLayout();
            }
        }

        /// <summary>
        /// A method, that returns an <see cref="IView"/> that should be rendered, when the 
        /// <see cref="AchievementSelection"/> is selected.
        /// </summary>
        /// <remarks>
        /// Might be <see langword="null"/> and might return <see langword="null"/>. Make sure to set a proper 
        /// method before using. 
        /// Make sure the method returns <see langword="null"/> and does not throw if the 
        /// provided parameters are invalid.
        /// </remarks>
        public Func<object[], IView> GetSelectedView { get; set; } = null;

        public bool IsPinned { get; set; }

        #region calculated fields

        private RelativeInt _bottomHeight;
        private RelativeInt _watchWidth;
        private RelativeInt _watchIconPaddingRight;
        private Rectangle _watchBounds;
        private RelativeInt _watchIconHoverHighlightSize;
        private Rectangle _watchIconHoverHighlightBounds;
        private RelativeInt _watchIconWatchedHighlightSize;
        private Rectangle _watchIconWatchedHighlightBounds;

        private RelativeInt _paddingProgress;

        private RelativeInt _titleHeight;
        private RelativeBitmapFont _titleFont;

        private Rectangle _titleBounds;
        private Rectangle _completedBounds;

        private RelativeInt _separatorHeight;
        private Rectangle _bottomSeparatorBounds;

        private RelativeInt _progressSize;

        private Rectangle _completedBackgroundHighlightBounds;

        #endregion

        private readonly ScrollingHighlightEffect _scrollEffect;

        public AchievementSelection()
        {
            _bottomHeight = new RelativeInt(DEFAULT_BOTTOM_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _watchWidth = new RelativeInt(DEFAULT_WATCH_WIDTH, DEFAULT_BOTTOM_HEIGHT, () => _bottomHeight);
            _watchIconPaddingRight = new RelativeInt(DEFAULT_WATCH_ICON_PADDING_RIGHT, DEFAULT_BOTTOM_HEIGHT, () => _bottomHeight);

            _watchIconHoverHighlightSize = new RelativeInt(DEFAULT_WATCH_ICON_HOVER_HIGHLIGHT_SIZE, DEFAULT_BOTTOM_HEIGHT, () => _bottomHeight);
            _watchIconWatchedHighlightSize = new RelativeInt(DEFAULT_WATCH_ICON_WATCHED_HIGHLIGHT_SIZE, DEFAULT_BOTTOM_HEIGHT, () => _bottomHeight);

            _paddingProgress = new RelativeInt(DEFAULT_PADDING_PROGRESS, DEFAULT_WIDTH, () => this.Width);

            _titleHeight = new RelativeInt(DEFAULT_TITLE_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _titleFont = new RelativeBitmapFont(_titleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _separatorHeight = new RelativeInt(DEFAULT_SEPARATOR_HEIGHT, DEFAULT_HEIGHT, () => this.Height);

            _progressSize = new RelativeInt(1.0f, () => this.Height);

            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;

            _watchIcon = new WatchIcon();

            _watchIcon.SelectedChanged += OnWatchIconSelectedChanged;
            _watchIcon.Parent = this;

            _additionalIcons = new FlowPanel
            {
                Parent = this
            };

            _scrollEffect = new ScrollingHighlightEffect(this);

            this.EffectBehind = _scrollEffect;
        }

        public AchievementSelection(IAchievement achievement, bool autoHide) : this()
        {
            WithController(new AchievementSelectionController(this, achievement, autoHide));
        }

        public AchievementSelection(IAchievement achievement) : this(achievement, false)
        { /** NOOP **/ }

        private void OnWatchIconSelectedChanged(object _, bool isSelected)
        {
            IsWatched = isSelected;
        }

        public override void RecalculateLayout()
        {
            _bottomHeight.Update();
            _watchWidth.Update();
            _watchIconPaddingRight.Update();

            _watchIconHoverHighlightSize.Update();
            _watchIconWatchedHighlightSize.Update();

            _watchBounds = new Rectangle(Width - _watchWidth, Height - _bottomHeight, _watchWidth, _bottomHeight);
            _watchIconHoverHighlightBounds = new Rectangle(Width - _watchIconHoverHighlightSize, Height - _watchIconHoverHighlightSize, _watchIconHoverHighlightSize, _watchIconHoverHighlightSize);
            _watchIconWatchedHighlightBounds = new Rectangle(Width - _watchIconWatchedHighlightSize, Height - _watchIconWatchedHighlightSize, _watchIconWatchedHighlightSize, _watchIconWatchedHighlightSize);

            _paddingProgress.Update();

            _titleHeight.Update();
            _titleFont.Update();

            _progressSize.Update();

            if (ProgressIndicator != null)
            {
                ProgressIndicator.Size = new Point(_progressSize);
            }

            _titleBounds = new Rectangle(_progressSize + _paddingProgress, 0, Width - (_progressSize + _paddingProgress), Height - _bottomHeight);
            int titleCenterHeight = _titleBounds.Y + (_titleBounds.Height / 2);
            _completedBounds = new Rectangle(_titleBounds.X, titleCenterHeight, _titleBounds.Width, Height - titleCenterHeight);

            _separatorHeight.Update();

            _bottomSeparatorBounds = new Rectangle(_progressSize, _size.Y - _bottomHeight - (int)((float)_separatorHeight / 2.0f), Width - _progressSize, _separatorHeight);

            _completedBackgroundHighlightBounds = new Rectangle(0, 0, Width, (int)(Height * 1.3f));

            RecalculateWatchIconLayout();
            RecalculateAdditionalIconsLayout();
        }

        private void RecalculateWatchIconLayout()
        {
            if (_watchIcon == null)
            {
                return;
            }

            _watchIcon.Size = new Point(_bottomHeight);
            _watchIcon.Bottom = Height;
            _watchIcon.Right = Width - _watchIconPaddingRight;
        }

        private void RecalculateAdditionalIconsLayout()
        {
            if (_additionalIcons == null || _watchIcon == null || _progressIndicator == null)
            {
                return;
            }

            _additionalIcons.Size = new Point(_watchIcon.Left - _progressIndicator.Right, _bottomHeight);

            _additionalIcons.Left = _progressIndicator.Right;
            _additionalIcons.Bottom = Height;

            if (_additionalIcons.Children.Any())
            {
                lock (_additionalIconsLock)
                {
                    foreach (Control icon in _additionalIcons.Children)
                    {
                        icon.Height = _bottomHeight;
                    }
                }
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            if (_watchIcon == null || !_watchIcon.Visible || !_watchIcon.IsHovered)
            {
                _scrollEffect.Enable();
            }

            if (_watchIcon != null && _watchIcon.Visible && _watchIcon.IsHovered)
            {
                _scrollEffect.Disable();
            }

            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (_watchIcon != null)
            {
                if (_watchIcon.AbsoluteBounds.Contains(e.MousePosition) && _watchIcon.Visible)
                {
                    return;
                }
            }

            base.OnClick(e);
        }

        public void SetAdditionalIcons(IEnumerable<Control> icons)
        {
            lock (_additionalIconsLock)
            {
                foreach (Control child in _additionalIcons.Children)
                {
                    child.Parent = null;
                    child.Dispose();
                }

                foreach (Control icon in icons)
                {
                    icon.Parent = _additionalIcons;
                }

                RecalculateAdditionalIconsLayout();
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // background
            spriteBatch.DrawOnCtrl(this,
                ContentService.Textures.Pixel,
                bounds,
                Color.Black * BackgroundOpacity);

            // highlight
            if (ShowHighlight)
            {
                // background highlight
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Selection.CompletedBackgroundHighlight,
                    _completedBackgroundHighlightBounds,
                    HighlightColor);

                // shine highlight
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Selection.CompletedShineHighlight,
                    new Rectangle(-_progressSize / 2, -_progressSize / 2, _progressSize * 2, _progressSize * 2),
                    Color.White * 0.6f);

                // shine2 highlight
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Selection.CompletedShine2Highlight,
                    new Rectangle(-_progressSize / 2, -_progressSize / 2, _progressSize * 2, _progressSize * 2),
                    Color.White * 0.6f);
            }

            // watch background
            if (ShowWatchIcon)
            {
                // tinted background
                spriteBatch.DrawOnCtrl(this,
                    ContentService.Textures.Pixel,
                    _watchBounds,
                    Color.Black * 0.3f);

                // is watched highlight
                if (_watchIcon != null && _watchIcon.Visible && _watchIcon.IsSelected)
                {
                    spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.WatchBackgroundHighlight,
                    _watchIconWatchedHighlightBounds,
                    null,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically
                    );
                }

                // hover highlight
                if (_watchIcon != null && _watchIcon.Visible && _watchIcon.IsHovered)
                {
                    spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.WatchBackgroundHighlight,
                    _watchIconHoverHighlightBounds,
                    null,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically
                    );
                }
            }

            // bottom separator
            if (ShowBottomSeparator)
            {
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Selection.BottomSeparator,
                    _bottomSeparatorBounds);
            }

            // title
            if (!string.IsNullOrWhiteSpace(Title))
            {
                spriteBatch.DrawStringOnCtrl(this,
                    Title,
                    _titleFont,
                    _titleBounds,
                    Color.White,
                    false,
                    true,
                    verticalAlignment: VerticalAlignment.Middle
                );
            }
            
            // subtitle
            if (!string.IsNullOrWhiteSpace(Subtitle))
            {
                spriteBatch.DrawStringOnCtrl(this,
                    Subtitle,
                    _titleFont,
                    _completedBounds,
                    HighlightColor,
                    verticalAlignment: VerticalAlignment.Middle
                );
            }

            // vignette
            if (ShowVignette)
            {
                // TODO: use propertly sized vignette texture
                // vignette
                //spriteBatch.DrawOnCtrl(this,
                //    TextureManager.Display.Progress.Vignette,
                //    new Rectangle(0, 0, Width, Height)
                //);

                // border
                spriteBatch.DrawFrame(this, Color.Black, 1);
            }
        }

        protected override void DisposeControl()
        {
            WatchedChanged = null;

            if (_progressIndicator != null)
            {
                RemoveChild(_progressIndicator);
                _progressIndicator.Dispose();
                _progressIndicator = null;
            }

            if (_watchIcon != null)
            {
                RemoveChild(_watchIcon);
                _watchIcon.Dispose();
                _watchIcon = null;
            }

            if (_additionalIcons != null)
            {
                RemoveChild(_additionalIcons);
                _additionalIcons.Dispose();
                _additionalIcons = null;
            }

            base.DisposeControl();
        }
    }
}
