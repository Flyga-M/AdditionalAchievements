using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    // resizes its own width based on the height
    public class AchievementPointsDisplay : Control
    {
        #region default constants

        private const int DEFAULT_WIDTH = 90;
        private const int DEFAULT_HEIGHT = 42;

        private const int DEFAULT_TEXT_PADDING = 6;

        private const int DEFAULT_TEXT_HEIGHT = 30;

        #endregion

        private int _points = 0;

        public Color HighlightColor { get; set; } = ColorManager.AchievementPointsHighlightColor;

        public int Points
        {
            get
            {
                return _points;
            }
            set
            {
                if (value == _points) return;

                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Points must be " +
                    "greater than or equal to zero.");

                _points = value;

                RecalculateTextWidth();
                RecalculateCenteredBounds(); // Everything needs to be recentered
            }
        }

        #region calculated fields

        private RelativeInt _targetWidth;

        private RelativeInt _iconSize;
        private Rectangle _iconBounds;

        private RelativeInt _textPadding;

        private RelativeInt _textHeight;
        private RelativeBitmapFont _textFont;
        private int _textWidth;
        private Rectangle _textBounds;

        #endregion

        public AchievementPointsDisplay()
        {
            _targetWidth = new RelativeInt(DEFAULT_WIDTH, DEFAULT_HEIGHT, () => this.Height);

            _iconSize = new RelativeInt(1.0f, () => this.Height);

            _textPadding = new RelativeInt(DEFAULT_TEXT_PADDING, DEFAULT_HEIGHT, () => this.Height);

            _textHeight = new RelativeInt(DEFAULT_TEXT_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _textFont = new RelativeBitmapFont(_textHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            this.Size = new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        }

        private int _lastHeight = -1;
        private int _lastWidth = -1;

        public override void RecalculateLayout()
        {
            _targetWidth.Update();

            if (_lastWidth != Width)
            {
                _lastWidth = Width;
                RecalculateOnWidthChange();
            }

            if (_lastHeight != Height)
            {
                _lastHeight = Height;
                RecalculateOnHeightChange();

                this.Width = _targetWidth;
                RecalculateLayout(); // does not automatically get called (because the layout is already invalidated i think)
            }
        }

        private void RecalculateOnWidthChange()
        {
            RecalculateCenteredBounds();
        }

        private void RecalculateOnHeightChange()
        {
            _iconSize.Update();

            _textPadding.Update();

            _textHeight.Update();
            _textFont.Update();

            RecalculateTextWidth();
        }

        private void RecalculateTextWidth()
        {
            _textWidth = (int)_textFont.GetValue().MeasureString(Points.ToString()).Width;
        }

        private void RecalculateCenteredBounds()
        {
            int totalWidth = _textWidth + _textPadding + _iconSize;
            int paddingLeft = (Width - totalWidth) / 2;

            // no need to center vertically, because the draw call will do that
            // Width is not set to _textWidth but larger, to prevent a harsh cutoff if the calculation is
            //   off by a pixel for some reason
            _textBounds = new Rectangle(paddingLeft, 0, Width - paddingLeft, Height);

            _iconBounds = new Rectangle(paddingLeft + _textWidth + _textPadding, 0, _iconSize, _iconSize);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawStringOnCtrl(this,
                Points.ToString(),
                _textFont,
                _textBounds,
                HighlightColor,
                wrap: false,
                stroke: true,
                1,
                HorizontalAlignment.Left,
                VerticalAlignment.Middle
                );

            spriteBatch.DrawOnCtrl(this,
                TextureManager.Display.AchievementPoint,
                _iconBounds);
        }
    }
}
