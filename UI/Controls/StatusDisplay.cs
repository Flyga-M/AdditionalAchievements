using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.Status;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Controller;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Displays the status of something.
    /// </summary>
    public class StatusDisplay : Container<StatusDisplayController>
    {
        public const float DEFAULT_WIDTH_HEIGHT_RATIO = (float)DEFAULT_WIDTH / (float)DEFAULT_HEIGHT;

        private const int DEFAULT_WIDTH = 398;
        private const int DEFAULT_HEIGHT = 113;

        private const int DEFAULT_TITLE_HEIGHT = 24;
        private const int DEFAULT_TITLE_FRAME_HEIGHT = 113 - DEFAULT_SUBTITLE_FRAME_HEIGHT;
        private const int DEFAULT_SUBTITLE_FRAME_HEIGHT = 42;

        private const int DEFAULT_PADDING_LEFT = 60;

        public Func<IView> _getDetailView;

        private float _backgroundOpacity = 0.1f;

        #region calculated fields

        private RelativeInt _paddingLeft;

        private RelativeInt _titleHeight;
        private RelativeInt _titleFrameHeight;
        private RelativeBitmapFont _titleFont;
        private Rectangle _titleBounds;

        private RelativeInt _subTitleHeight;
        private RelativeInt _subTitleFrameHeight;
        private RelativeBitmapFont _subtitleFont;
        private Rectangle _subtitleBounds;
        private int _subTitleFrameY => _subTitleFrameHeight;

        private Texture2D _backgroundHighlightTexture => TextureManager.Display.Selection.CompletedBackgroundHighlight;
        private RelativeInt _backgroundHighlightHeight;
        private Rectangle _backgroundHighlightBounds;

        #endregion

        private readonly ScrollingHighlightEffect _scrollEffect;

        public event EventHandler<IView> Selected;

        public string Title { get; set; }
        public string Subtitle { get; set; }

        public Color HighlightColor { get; set; }

        public Func<IView> GetDetailView
        {
            get => _getDetailView;
            set
            {
                _getDetailView = value;

                if (value == null)
                {
                    _scrollEffect.Disable();
                    return;
                }

                _scrollEffect.Enable();
            }
        }

        public bool IsSelectable => GetDetailView != null;

        private void OnSelected()
        {
            if (!IsSelectable)
            {
                return;
            }
            
            Selected?.Invoke(this, GetDetailView());
        }

        public StatusDisplay()
        {
            _paddingLeft = new RelativeInt(DEFAULT_PADDING_LEFT, DEFAULT_WIDTH, () => this.Width);

            _titleHeight = new RelativeInt(DEFAULT_TITLE_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _titleFrameHeight = new RelativeInt(DEFAULT_TITLE_FRAME_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _titleFont = new RelativeBitmapFont(_titleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _subTitleHeight = new RelativeInt(0.8f, () => this._titleHeight);
            _subTitleFrameHeight = new RelativeInt(DEFAULT_SUBTITLE_FRAME_HEIGHT, DEFAULT_HEIGHT, () => this.Height);
            _subtitleFont = new RelativeBitmapFont(_subTitleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _backgroundHighlightHeight = new RelativeInt(1.3f, () => this.Height);

            _scrollEffect = new ScrollingHighlightEffect(this);
            _scrollEffect.Disable();
            this.EffectBehind = _scrollEffect;
        }

        public StatusDisplay(IStatusProvider statusProvider) : this()
        {
            this.WithController(new StatusDisplayController(this, statusProvider));
        }

        public override void RecalculateLayout()
        {
            _paddingLeft.Update();

            _titleHeight.Update();
            _titleFrameHeight.Update();
            _titleFont.Update();
            _titleBounds = new Rectangle(_paddingLeft, 0, Width - _paddingLeft, _titleFrameHeight);

            _subTitleHeight.Update();
            _subTitleFrameHeight.Update();
            _subtitleFont.Update();
            _subtitleBounds = new Rectangle(_paddingLeft, _subTitleFrameY, Width - _paddingLeft, _subTitleFrameHeight);
            
            _backgroundHighlightHeight.Update();
            _backgroundHighlightBounds = new Rectangle(0, 0, Width, _backgroundHighlightHeight);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            OnSelected();
            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // background
            spriteBatch.DrawOnCtrl(this,
                ContentService.Textures.Pixel,
                bounds,
                Color.Black * _backgroundOpacity
            );

            // background highlight
            spriteBatch.DrawOnCtrl(this,
                _backgroundHighlightTexture,
                _backgroundHighlightBounds,
                HighlightColor * 0.5f
            );

            // title
            spriteBatch.DrawStringOnCtrl(this,
                Title,
                _titleFont,
                _titleBounds,
                Color.White,
                false,
                true,
                verticalAlignment: VerticalAlignment.Middle
            );

            // subtitle
            spriteBatch.DrawStringOnCtrl(this,
                Subtitle,
                _subtitleFont,
                _subtitleBounds,
                HighlightColor,
                false,
                true,
                verticalAlignment: VerticalAlignment.Middle
            );

            // border
            spriteBatch.DrawFrame(this, Color.Black, 1);
        }

        protected override void DisposeControl()
        {
            Selected = null;

            base.DisposeControl();
        }
    }
}