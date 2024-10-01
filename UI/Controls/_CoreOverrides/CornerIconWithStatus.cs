using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public class CornerIconWithStatus : CornerIcon
    {
        private const int STATUS_SIZE = 16;
        private const int STATUS_OFFSET = 2;

        private Texture2D _statusTexture;

        private bool _showStatus;

        private Rectangle _statusRectangle;

        public bool ShowStatus
        {
            get => _showStatus;
            set => _showStatus = value;
        }

        public Texture2D StatusTexture
        {
            get => _statusTexture ?? ContentService.Textures.Error;
            set => _statusTexture = value;
        }

        public CornerIconWithStatus() : base()
        {
            // so the status icon does not get clipped if the offset is > 0.
            this.ClipsBounds = false;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _statusRectangle = new Rectangle(this.Width - STATUS_SIZE + STATUS_OFFSET, this.Height - STATUS_SIZE + STATUS_OFFSET, STATUS_SIZE, STATUS_SIZE);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (!ShowStatus)
            {
                return;
            }

            spriteBatch.DrawOnCtrl(this, StatusTexture, _statusRectangle);
        }
    }
}
