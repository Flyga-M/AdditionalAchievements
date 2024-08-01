using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the eye icon, that is used to add achievements to the watch list.
    /// </summary>
    /// <remarks>
    /// Does not implement the hover or selected background highlights, because in some places those do not align with the 
    /// placement of the eye icon. These should be implemented by the parent <see cref="Control"/>, also to prevent clipping.
    /// </remarks>
    public class WatchIcon : Control
    {
        private bool _isSelected;

        public event EventHandler<bool> SelectedChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                bool oldValue = _isSelected;
                _isSelected = value;

                if (oldValue != value)
                {
                    OnSelectedChanged();
                }
            }
        }

        /// <summary>
        /// Determines whether the <see cref="WatchIcon"/> is currently being hovered.
        /// </summary>
        public bool IsHovered { get; private set; }

        private void OnSelectedChanged()
        {
            SelectedChanged?.Invoke(this, _isSelected);
        }

        public bool ToggleSelected()
        {
            IsSelected = !IsSelected;
            return IsSelected;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            ToggleSelected();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            IsHovered = true;
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);
            IsHovered = false;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {   
            // icon
            if (IsSelected)
            {
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.WatchIconSelected,
                    bounds);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.WatchIcon,
                    bounds);
            }
        }

        protected override void DisposeControl()
        {
            SelectedChanged = null;

            base.DisposeControl();
        }
    }
}
