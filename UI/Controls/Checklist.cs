using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public class Checklist : Container
    {
        private Menu _innerMenu;

        public Checklist()
        {
            _innerMenu = new Menu();

            _innerMenu.Resized += OnMenuResized;

            _innerMenu.Parent = this;
        }

        private void OnMenuResized(object _, ResizedEventArgs _1)
        {
            this.Height = _innerMenu.Height;
        }

        public override void RecalculateLayout()
        {
            _innerMenu.Width = this.Width;
        }

        public MenuItem AddChecklistItem(bool @checked, string title)
        {
            MenuItem menuItem = _innerMenu.AddMenuItem(title, @checked ? TextureManager.Display.Description.CheckmarkGreen : TextureManager.Display.Description.Dash);
            RecalculateLayout();
            return menuItem;
        }

        protected override void DisposeControl()
        {
            _innerMenu.Resized -= OnMenuResized;
            _innerMenu?.Dispose();
            base.DisposeControl();
        }
    }
}
