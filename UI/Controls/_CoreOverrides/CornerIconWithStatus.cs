using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public enum CornerIconStatus
    {
        None,
        Green,
        Yellow,
        Red
    }

    public class CornerIconWithStatus : CornerIcon
    {
        private const int STATUS_SIZE = 16;

        private AsyncTexture2D _statusGreen = Content.DatAssetCache.GetTextureFromAssetId(157336);
        private AsyncTexture2D _statusYellow = Content.DatAssetCache.GetTextureFromAssetId(157337);
        private AsyncTexture2D _statusRed = Content.DatAssetCache.GetTextureFromAssetId(157335);

        private bool _showStatus;

        private CornerIconStatus _status;
        private Rectangle _statusRectangle;

        public bool ShowStatus
        {
            get => _showStatus;
            set => _showStatus = value;
        }

        public CornerIconStatus Status
        {
            get => _status;
            set => _status = value;
        }

        private Texture2D GetStatusTexture()
        {
            if (!ShowStatus || Status == CornerIconStatus.None)
            {
                return ContentService.Textures.TransparentPixel;
            }

            switch (Status)
            {
                case (CornerIconStatus.Green):
                    {
                        return _statusGreen;
                    }
                case (CornerIconStatus.Yellow):
                    {
                        return _statusYellow;
                    }
                case (CornerIconStatus.Red):
                    {
                        return _statusRed;
                    }
                default:
                    {
                        return ContentService.Textures.Error;
                    }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _statusRectangle = new Rectangle(this.Width - STATUS_SIZE, this.Height - STATUS_SIZE, STATUS_SIZE, STATUS_SIZE);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, GetStatusTexture(), _statusRectangle);
        }
    }
}
