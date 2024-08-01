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
        /// The title of the achievement.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description of the achievement.
        /// </summary>
        public string Description { get; set; }

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
                ProgressTextIndicator.Location = new Point(0, _titleBounds.Y + _titleBounds.Height + _elementPadding);
                ProgressTextIndicator.Size = new Point(this.Width, _progressOngoingHeight);
            }

            _descriptionBounds = new Rectangle(0, ProgressTextIndicator?.Bottom ?? 0 + _elementPadding, this.Width - _descriptionPaddingRight, this.Height - _titleHeight - _elementPadding - ProgressTextIndicator?.Height ?? 0 - _elementPadding);
            
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

        public int GetActualHeight()
        {
            int actualDescriptionHeight;

            string descriptionWrap = DrawUtil.WrapText(_descriptionFont, Description, _descriptionBounds.Width);
            actualDescriptionHeight = (int)_descriptionFont.MeasureString(descriptionWrap).Height;

            return _descriptionBounds.Top + actualDescriptionHeight;
        }

        protected override void DisposeControl()
        {
            if (_progressTextIndicator != null)
            {
                RemoveChild(_progressTextIndicator);
                _progressTextIndicator.Dispose();
                _progressTextIndicator = null;
            }

            base.DisposeControl();
        }
    }
}
