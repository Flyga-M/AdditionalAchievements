using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.UI.Windows
{
    public class TabbedWindow2WithStatus : TabbedWindow2
    {
        // copied from TabbedWindow2
        private const int TAB_VERTICALOFFSET = 40;
        private const int TAB_HEIGHT = 50;
        private const int TAB_WIDTH = 84;

        private const int STATUS_SIZE = 16;
        private const int STATUS_OFFSET = -4;

        private Texture2D _statusTexture;

        private Rectangle _baseStatusRectangle;

        private Dictionary<int, bool> _tabStatuses = new Dictionary<int, bool>();

        public Texture2D StatusTexture
        {
            get => _statusTexture ?? ContentService.Textures.Error;
            set => _statusTexture = value;
        }

        public TabbedWindow2WithStatus(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            // all those magic numbers come from TabbedWindow2.PaintAfterChildren
            int tabX = base.SidebarActiveBounds.Left - (84 - base.SidebarActiveBounds.Width) + 2;
            int tabY = base.SidebarActiveBounds.Top + TAB_VERTICALOFFSET;

            int statusX = tabX + TAB_WIDTH - STATUS_SIZE + STATUS_OFFSET;
            int statusY = tabY + TAB_HEIGHT - STATUS_SIZE + STATUS_OFFSET;

            _baseStatusRectangle = new Rectangle(statusX, statusY, STATUS_SIZE, STATUS_SIZE);
        }

        public void SetTabStatus(int index, bool showStatus)
        {
            _tabStatuses[index] = showStatus;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            foreach (KeyValuePair<int, bool> tabStatus in _tabStatuses)
            {
                if (!tabStatus.Value || tabStatus.Key >= Tabs.Count)
                {
                    continue;
                }

                spriteBatch.DrawOnCtrl(this, StatusTexture, ApplyTabOffset(tabStatus.Key));
            }
        }

        private Rectangle ApplyTabOffset(int index)
        {
            return new Rectangle(_baseStatusRectangle.X,
                _baseStatusRectangle.Y + (TAB_HEIGHT * index),
                _baseStatusRectangle.Width,
                _baseStatusRectangle.Height);
        }
    }
}
