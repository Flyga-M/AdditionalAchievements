using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Controller;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    // heavily inspired by https://github.com/blish-hud/Pathing/blob/main/UI/Controls/MarkerPackHero.cs
    public class PkgBody : Container<PkgBodyController>
    {
        private const int DEFAULT_WIDTH = 500;
        private const int DEFAULT_HEIGHT = 170;

        private const int EDGE_PADDING = 20;

        private bool _keepUpdated;
        private bool _showKeepUpdated;

        private bool _isDownloaded;
        private bool _isEnabled;
        private Color _indicatorColor = Color.Transparent;

        private string _lastUpdatedMessage;

        private Checkbox _keepUpdatedCheckbox;

        private Rectangle _backgroundBounds;

        #region calculated fields

        private Rectangle _lastUpdatedBounds;

        private Rectangle _titleBounds;
        private Rectangle _descriptionBounds;

        private Rectangle _indicatorBounds;

        #endregion

        public event EventHandler<bool> KeepUpdatedChanged;
        public event EventHandler<bool> IsDownloadedChanged;
        public event EventHandler<bool> IsEnabledChanged;

        /// <summary>
        /// Determines whether the 'Keep Updated' checkbox is checked.
        /// </summary>
        public bool KeepUpdated
        {
            get => _keepUpdated;
            set
            {
                bool oldValue = _keepUpdated;
                _keepUpdated = value;

                if (oldValue != value)
                {
                    if (_keepUpdatedCheckbox != null)
                    {
                        _keepUpdatedCheckbox.Checked = value;
                    }

                    KeepUpdatedChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Determines whether the 'Keep Updated' checkbox is visible.
        /// </summary>
        public bool ShowKeepUpdated
        {
            get => _showKeepUpdated;
            set
            {
                _showKeepUpdated = value;
                if (_keepUpdatedCheckbox != null)
                {
                    _keepUpdatedCheckbox.Visible = value;
                }

                RecalculateLastUpdatedLayout();
            }
        }

        /// <summary>
        /// The title of the package.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The message that describes, when the package was last updated.
        /// </summary>
        /// <remarks>
        /// e.g. 'Last updated 2 days ago'
        /// </remarks>
        public string LastUpdateMessage
        {
            get => _lastUpdatedMessage;
            set
            {
                _lastUpdatedMessage = value;
                RecalculateLastUpdatedLayout();
            }
        }

        /// <summary>
        /// The description of the package.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines whether a gray bar is rendered to signal that the package is downloaded.
        /// </summary>
        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                bool oldValue = _isDownloaded;
                _isDownloaded = value;

                if (oldValue != value)
                {
                    UpdateIndicatorColor();
                    IsDownloadedChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Determines whether a green bar is rendered to signal that the package is enabled.
        /// </summary>
        /// <remarks>
        /// Will always be <see langword="false"/>, if <see cref="IsDownloaded"/> is <see langword="false"/>.
        /// </remarks>
        public bool IsEnabled
        {
            get => IsDownloaded && _isEnabled;
            set
            {
                if (value && !IsDownloaded)
                {
                    return;
                }

                bool oldValue = _isEnabled;
                _isEnabled = value;

                if (oldValue != value)
                {
                    UpdateIndicatorColor();
                    IsEnabledChanged?.Invoke(this, value);
                }
            }
        }

        public PkgBody()
        {
            // TODO: localize
            _keepUpdatedCheckbox = new Checkbox()
            {
                Text = "Keep Updated",
                BasicTooltipText = "If checked, new pack versions will be automatically downloaded on launch.",
                Parent = this,
                Checked = _keepUpdated,
                Visible = _showKeepUpdated
            };

            _keepUpdatedCheckbox.CheckedChanged += OnKeepUpdatedCheckboxChecked;

            _backgroundBounds = new Rectangle(-9, -13, TextureManager.Display.Repo.PkgBodyBackground.Width, TextureManager.Display.Repo.PkgBodyBackground.Height);
        }

        public PkgBody(AchievementPackPkg pkg) : this()
        {
            this.WithController(new PkgBodyController(this, pkg));
        }

        private void OnKeepUpdatedCheckboxChecked(object _, CheckChangedEvent e)
        {
            KeepUpdated = e.Checked;
        }

        public override void RecalculateLayout()
        {
            if (_keepUpdatedCheckbox != null)
            {
                _keepUpdatedCheckbox.Right = this.Width - EDGE_PADDING;
                _keepUpdatedCheckbox.Top = EDGE_PADDING;
            }

            RecalculateLastUpdatedLayout();

            // TODO: calculate height if title is too long and has to be wrapped
            _titleBounds = new Rectangle(EDGE_PADDING, EDGE_PADDING, this.Width - _lastUpdatedBounds.Width - EDGE_PADDING, 40);

            _descriptionBounds = new Rectangle(EDGE_PADDING, _titleBounds.Bottom + EDGE_PADDING/2, this.Width - EDGE_PADDING, this.Height - _titleBounds.Bottom - EDGE_PADDING/2);

            _indicatorBounds = new Rectangle(0, 0, 12, this.Height);
        }

        private void RecalculateLastUpdatedLayout()
        {
            int offsetRight = EDGE_PADDING;
            if (_keepUpdatedCheckbox != null && _keepUpdatedCheckbox.Visible)
            {
                offsetRight += _keepUpdatedCheckbox.Width + EDGE_PADDING;
            }

            int messageWidth = (int)Math.Ceiling(GameService.Content.DefaultFont14.MeasureString(LastUpdateMessage).Width);

            _lastUpdatedBounds = new Rectangle(this.Width - offsetRight - messageWidth, EDGE_PADDING, messageWidth, 40);
        }

        private void UpdateIndicatorColor()
        {
            if (IsEnabled)
            {
                _indicatorColor = Color.Green * 0.8f;
                return;
            }

            if (IsDownloaded)
            {
                _indicatorColor = Color.Gray * 0.8f;
                return;
            }

            _indicatorColor = Color.Transparent;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // background
            spriteBatch.DrawOnCtrl(this,
                TextureManager.Display.Repo.PkgBodyBackground,
                _backgroundBounds);

            // indicator
            spriteBatch.DrawOnCtrl(this,
                ContentService.Textures.Pixel,
                _indicatorBounds,
                _indicatorColor);

            // title
            spriteBatch.DrawStringOnCtrl(this,
                Title,
                GameService.Content.DefaultFont18,
                _titleBounds,
                ContentService.Colors.Chardonnay);

            // description
            spriteBatch.DrawStringOnCtrl(this,
                Description,
                GameService.Content.DefaultFont14,
                _descriptionBounds,
                StandardColors.Default,
                true,
                HorizontalAlignment.Left,
                VerticalAlignment.Top);

            // last updated
            spriteBatch.DrawStringOnCtrl(this,
                LastUpdateMessage,
                GameService.Content.DefaultFont14,
                _lastUpdatedBounds,
                ContentService.Colors.Chardonnay,
                false,
                HorizontalAlignment.Right,
                VerticalAlignment.Top);
        }

        protected override void DisposeControl()
        {
            if (_keepUpdatedCheckbox != null)
            {
                _keepUpdatedCheckbox.CheckedChanged -= OnKeepUpdatedCheckboxChecked;
                _keepUpdatedCheckbox.Parent = null;
                _keepUpdatedCheckbox?.Dispose();
                _keepUpdatedCheckbox = null;
            }
            
            base.DisposeControl();
        }
    }
}
