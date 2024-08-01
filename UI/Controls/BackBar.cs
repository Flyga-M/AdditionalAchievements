using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the back bar, that is used to navigate to the parent achievement or collection when 
    /// viewing a single achievement.
    /// </summary>
    public class BackBar : Control
    {
        private const int DEFAULT_WIDTH = 556;
        private const int DEFAULT_HEIGHT = 48;

        private const int DEFAULT_BUTTON_PADDING_LEFT = 9;
        private const int DEFAULT_BUTTON_SIZE = 40;
        private const int DEFAULT_BUTTON_PADDING_TOP = (int) ((DEFAULT_HEIGHT - DEFAULT_BUTTON_SIZE) / 2.0f);

        private const int DEFAULT_ICON_PADDING_LEFT = DEFAULT_BUTTON_PADDING_LEFT + DEFAULT_BUTTON_SIZE + DEFAULT_BUTTON_PADDING_LEFT;
        private const int DEFAULT_ICON_SIZE = 32;
        private const int DEFAULT_ICON_PADDING_TOP = (int)((DEFAULT_HEIGHT - DEFAULT_ICON_SIZE) / 2.0f);

        private const int DEFAULT_TITLE_PADDING_LEFT = DEFAULT_ICON_PADDING_LEFT + DEFAULT_ICON_SIZE + 12;
        private const int DEFAULT_TITLE_HEIGHT = 24;

        private Blish_HUD.Controls.Effects.ScrollingHighlightEffect _scrollEffect;

        #region calculated fields

        private RelativeInt _buttonPaddingLeft;
        private RelativeInt _buttonPaddingTop;
        private RelativeInt _buttonSize;

        private RelativeInt _iconPaddingLeft;
        private RelativeInt _iconPaddingTop;
        private RelativeInt _iconSize;

        private RelativeInt _titlePaddingLeft;
        private RelativeInt _titleHeight;
        private RelativeBitmapFont _titleFont;

        private Rectangle _backgroundBounds;
        private Rectangle _backgroundSourceBounds;

        #endregion
        private Texture2D _background => TextureManager.Display.Back.Background;

        /// <summary>
        /// The icon to display next to the back button.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// The title to display next to the <see cref="Icon"/>.
        /// </summary>
        public string Title { get; set; }

        public BackBar(Texture2D icon, string title)
        {
            Icon = icon;
            Title = title;

            _buttonPaddingLeft = new RelativeInt(DEFAULT_BUTTON_PADDING_LEFT, DEFAULT_HEIGHT, () => this.Height);
            _buttonSize = new RelativeInt(DEFAULT_BUTTON_SIZE, DEFAULT_HEIGHT, () => this.Height);
            _buttonPaddingTop = new RelativeInt(DEFAULT_BUTTON_PADDING_TOP, DEFAULT_HEIGHT, () => this.Height);

            _iconPaddingLeft = new RelativeInt(DEFAULT_ICON_PADDING_LEFT, DEFAULT_HEIGHT, () => this.Height);
            _iconSize = new RelativeInt(DEFAULT_ICON_SIZE, DEFAULT_HEIGHT, () => this.Height);
            _iconPaddingTop = new RelativeInt(DEFAULT_ICON_PADDING_TOP, DEFAULT_HEIGHT, () => this.Height);

            _titlePaddingLeft = new RelativeInt(DEFAULT_TITLE_PADDING_LEFT, DEFAULT_HEIGHT, () => this.Height);
            _titleHeight = new RelativeInt(DEFAULT_TITLE_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _titleFont = new RelativeBitmapFont(_titleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            Initialize();
        }

        private void Initialize()
        {
            _scrollEffect = new Blish_HUD.Controls.Effects.ScrollingHighlightEffect(this);

            // currently not implmented. Instead Draw() overwritten
            //this.EffectInFront = _scrollEffect;

            this.Height = DEFAULT_HEIGHT;
            this.Width = DEFAULT_WIDTH;
        }

        public override void RecalculateLayout()
        {
            _buttonPaddingLeft.Update();
            _buttonSize.Update();
            _buttonPaddingTop.Update();

            _iconPaddingLeft.Update();
            _iconSize.Update();
            _iconPaddingTop.Update();

            _titlePaddingLeft.Update();
            _titleHeight.Update();
            _titleFont.Update();

            //_buttonPaddingLeft = (int)(this.Height * 0.1875f);
            //_buttonSize = (int)(this.Height * 0.83);
            //_buttonPaddingTop = (this.Height - _buttonSize) / 2;

            //_iconPaddingLeft = (int)(this.Height * 0.1875f) + _buttonPaddingLeft + _buttonSize;
            //_iconSize = (int)(this.Height * 0.83);
            //_iconPaddingTop = (this.Height - _iconSize) / 2;

            //_titlePaddingLeft = (int)(this.Height * 0.25f) + _iconPaddingLeft + _iconSize;
            //_titleHeight = (int)(this.Height * 0.6);
            //_titleFont = FontManager.GetFontFromHeight(_titleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _backgroundBounds = new Rectangle(0, 0, this.Width, this.Height);
            _backgroundSourceBounds = new Rectangle(0, 0, _background.Width, Math.Min(this.Height, _background.Height));
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            _scrollEffect.Enable();
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            _scrollEffect.Disable();
            base.OnMouseLeft(e);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            base.Draw(spriteBatch, drawBounds, scissor);

            // scroll effect should be in front
            // this.EffectInFront is currently not implemented
            _scrollEffect?.Draw(spriteBatch, drawBounds);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // background color
            //spriteBatch.DrawOnCtrl(this,
            //        ContentService.Textures.Pixel,
            //        _backgroundBounds,
            //        Color.White * 0.03f
            //    );

            // background image
            spriteBatch.DrawOnCtrl(this,
                                   _background,
                                   _backgroundBounds,
                                   _backgroundSourceBounds,
                                   Color.White);

            // button
            spriteBatch.DrawOnCtrl(this,
                TextureManager.Display.Back.Arrow,
                new Rectangle(_buttonPaddingLeft, _buttonPaddingTop, _buttonSize, _buttonSize)
                );

            // icon
            spriteBatch.DrawOnCtrl(this,
                Icon,
                new Rectangle(_iconPaddingLeft, _iconPaddingTop, _iconSize, _iconSize)
                );

            // title
            spriteBatch.DrawStringOnCtrl(this,
                Title,
                _titleFont,
                new Rectangle(_titlePaddingLeft, 0, this.Width - _titlePaddingLeft, this.Height),
                Color.White,
                false,
                true,
                1,
                HorizontalAlignment.Left,
                VerticalAlignment.Middle);
        }
    }
}
