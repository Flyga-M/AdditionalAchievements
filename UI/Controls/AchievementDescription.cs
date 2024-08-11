using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Fonts;
using Flyga.AdditionalAchievements.UI.Controller;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the achievement description when viewing a single achievement.
    /// </summary>
    public class AchievementDescription : Container<AchievementDescriptionController>
    {
        private const int DEFAULT_WIDTH = 565;
        private const int DEFAULT_HEIGHT = 620;

        private Control _progressTextIndicator;
        private Control _details;

        private int _elementPadding = 12;

        private int _titlePaddingLeft = 12;
        private int _titleHeight = 36;
        private BitmapFont _titleFont;
        private Rectangle _titleBounds;
        private Rectangle _titleBackgroundSourceBounds;
        private Texture2D _titleBackground => TextureManager.Display.Back.Background;

        private int _progressOngoingHeight = 62;

        private int _descriptionPaddingRight = 72;
        private BitmapFont _descriptionFont = Content.DefaultFont16;
        private Rectangle _descriptionBounds;

        /// <summary>
        /// The progress text indicator. Usually displays something similar to "Tier: 1/4 | Objectives: 2/25".
        /// </summary>
        /// <remarks>
        /// Disposes the previous <see cref="ProgressTextIndicator"/>, if it's not <see langword="null"/> and overwritten. 
        /// Will also be disposed, when the <see cref="AchievementDescription"/> is disposed.
        /// </remarks>
        public Control ProgressTextIndicator
        {
            get => _progressTextIndicator;
            set
            {
                if (_progressTextIndicator != null)
                {
                    RemoveChild(_progressTextIndicator);
                    _progressTextIndicator.Parent = null;
                    _progressTextIndicator.Dispose();
                }

                _progressTextIndicator = value;
                _progressTextIndicator.Parent = this;
                RecalculateLayout();
            }
        }

        /// <summary>
        /// The details, that will be displayed under the <see cref="Description"/>. May be <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// Disposes the previous <see cref="Details"/>, if it's not <see langword="null"/> and overwritten. 
        /// Will also be disposed, when the <see cref="AchievementDescription"/> is disposed.
        /// </remarks>
        public Control Details
        {
            get => _details;
            set
            {
                if (_details != null)
                {
                    _details.Parent = null;
                    _details.Dispose();
                }

                _details = value;
                _details.Parent = this;
                RecalculateLayout();
            }
        }

        /// <summary>
        /// The title of the achievement.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description of the achievement.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// [Will change] Determines whether the objectives will be displayed as a list.
        /// </summary>
        public bool ShowObjectives { get; set; }

        public AchievementDescription()
        {
            this.Width = DEFAULT_WIDTH;
            this.Height = DEFAULT_HEIGHT;

            _titleFont = FontManager.GetFontFromHeight((int)(_titleHeight * 0.85), ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);
            _titleBounds = new Rectangle(0, 0, this.Width, _titleHeight);

            _descriptionBounds = new Rectangle(0, _titleHeight + _elementPadding, this.Width - _descriptionPaddingRight, this.Height - _titleHeight - _elementPadding);
        }

        public AchievementDescription(IAchievement achievement) : this()
        {
            this.WithController(new AchievementDescriptionController(this, achievement));
        }

        public override void RecalculateLayout()
        {
            _descriptionPaddingRight = (int)(this.Width * 0.1275);

            _titleBounds = new Rectangle(0, 0, this.Width, _titleHeight);
            _titleBackgroundSourceBounds = new Rectangle(0, 0, _titleBackground.Width, Math.Min(this.Height, _titleBackground.Height));

            if (ProgressTextIndicator != null)
            {
                ProgressTextIndicator.Location = new Point(0, _titleBounds.Bottom + _elementPadding);
                ProgressTextIndicator.Size = new Point(this.Width, _progressOngoingHeight);
            }

            _descriptionBounds = new Rectangle(0, ProgressTextIndicator?.Bottom ?? _titleBounds.Bottom + _elementPadding, this.Width - _descriptionPaddingRight, this.Height - _titleHeight - _elementPadding - ProgressTextIndicator?.Height ?? 0 - _elementPadding);

            _descriptionBounds.Height = GetActualDescriptionHeight() + 20;

            if (Details != null)
            {
                Details.Width = this.Width;
                Details.Location = new Point(0, _descriptionBounds.Bottom + _elementPadding);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            PaintTitle(spriteBatch, _titleBounds);
            PaintDescription(spriteBatch, _descriptionBounds);
        }

        private void PaintTitle(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // background image
            spriteBatch.DrawOnCtrl(this,
                                   _titleBackground,
                                   _titleBounds,
                                   _titleBackgroundSourceBounds,
                                   Color.White * 0.5f);

            // title
            spriteBatch.DrawStringOnCtrl(this,
                Title,
                _titleFont,
                new Rectangle(bounds.Left + _titlePaddingLeft, bounds.Top, bounds.Width - _titlePaddingLeft, bounds.Height),
                Color.White,
                false,
                true,
                1,
                HorizontalAlignment.Left,
                VerticalAlignment.Middle);
        }

        private void PaintDescription(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawStringOnCtrl(this,
                Description,
                _descriptionFont,
                bounds,
                Color.White,
                true,
                true,
                1,
                HorizontalAlignment.Left,
                VerticalAlignment.Top);
        }

        private int GetActualDescriptionHeight()
        {
            string descriptionWrap = DrawUtil.WrapText(_descriptionFont, Description, _descriptionBounds.Width);
            return (int)_descriptionFont.MeasureString(descriptionWrap).Height;
        }

        public int GetActualHeight()
        {
            return Details != null ? Details.Bottom + _elementPadding : _descriptionBounds.Bottom + _elementPadding;
        }

        protected override void DisposeControl()
        {
            if (_progressTextIndicator != null)
            {
                RemoveChild(_progressTextIndicator);
                _progressTextIndicator.Dispose();
                _progressTextIndicator = null;
            }

            if (_details != null)
            {
                _details.Parent = null;
                _details.Dispose();
                _details = null;
            }

            base.DisposeControl();
        }
    }
}
