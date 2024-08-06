using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// <inheritdoc/> 
    /// Also has a border on the right side.
    /// </summary>
    public class PanelWithRightBorder : Panel
    {
        private readonly AsyncTexture2D _textureRightSideAccent = AsyncTexture2D.FromAssetId(605025);

        private Rectangle _layoutRightAccentBounds;
        private Rectangle _layoutRightAccentSrc;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            bool showsHeader = !string.IsNullOrEmpty(_title);

            int topOffset = showsHeader ? HEADER_HEIGHT : 0;

            topOffset = Math.Max(TOP_PADDING, topOffset);
            int rightOffset = RIGHT_PADDING;
            int bottomOffset = BOTTOM_PADDING;

            int height = Math.Min(_size.Y - topOffset - bottomOffset, _textureRightSideAccent.Height);

            // right side accent
            _layoutRightAccentBounds = new Rectangle(_size.X - rightOffset - 7, _size.Y - bottomOffset - height, _textureRightSideAccent.Width, height);
            _layoutRightAccentSrc = new Rectangle(0, 0, _textureRightSideAccent.Width, height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this,
                _textureRightSideAccent,
                _layoutRightAccentBounds,
                _layoutRightAccentSrc,
                Color.Black * AccentOpacity);
        }
    }
}
