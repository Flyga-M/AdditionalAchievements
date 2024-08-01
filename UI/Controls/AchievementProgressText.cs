using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Controller;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the progress area that is shown when a single achievement is viewed. 
    /// e.g. "Tier: 1/3 | Objectives: 2/5"
    /// </summary>
    /// <remarks>
    /// Is a separate <see cref="Control"/>, so it's easier to have a separate <see cref="Tooltip"/>, like in the game.
    /// </remarks>
    public class AchievementProgressText : Control<AchievementProgressTextController>
    {
        private const int DEFAULT_WIDTH = 565;
        private const int DEFAULT_HEIGHT = 62;
        private const int DEFAULT_TITLE_HEIGHT = 20;
        private const int DEFAULT_CONTENT_HEIGHT = 40;
        private const int DEFAULT_TITLE_CONTENT_PADDING = 16;
        private const int DEFAULT_DIVIDER_HEIGHT = 62;
        private const int DEFAULT_DIVIDER_WIDTH = 4;
        private const int DEFAULT_DIVIDER_PADDING = 22;

        private readonly List<ProgressTextData> _values;

        private readonly object _valueLock = new object();

        #region calculated fields

        private readonly BitmapFont _progressTitleFont = Content.DefaultFont16;
        private readonly BitmapFont _progressContentFont = Content.DefaultFont32;

        private readonly RelativeInt _titleHeight;
        private readonly RelativeInt _contentHeight;
        private readonly RelativeInt _titleContentPadding;

        private readonly RelativeBitmapFont _titleFont;
        private readonly RelativeBitmapFont _contentFont;

        private readonly RelativeInt _dividerWidth;
        private readonly RelativeInt _dividerHeight;
        private readonly RelativeInt _dividerPadding;

        private int _dividerTop;

        #endregion

        public AchievementProgressText()
        {
            _values = new List<ProgressTextData>();

            _titleHeight = new RelativeInt(DEFAULT_TITLE_HEIGHT, DEFAULT_HEIGHT, () => Height);
            _contentHeight = new RelativeInt(DEFAULT_CONTENT_HEIGHT, DEFAULT_HEIGHT, () => Height);
            _titleContentPadding = new RelativeInt(DEFAULT_TITLE_CONTENT_PADDING, DEFAULT_HEIGHT, () => Height);

            _titleFont = new RelativeBitmapFont(_titleHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);
            _contentFont = new RelativeBitmapFont(_contentHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _dividerWidth = new RelativeInt(DEFAULT_DIVIDER_WIDTH, DEFAULT_HEIGHT, () => Height);
            _dividerHeight = new RelativeInt(DEFAULT_DIVIDER_HEIGHT, DEFAULT_HEIGHT, () => Height);
            _dividerPadding = new RelativeInt(DEFAULT_DIVIDER_PADDING, DEFAULT_HEIGHT, () => Height);

            Height = DEFAULT_HEIGHT;
            Width = DEFAULT_WIDTH;
        }

        public AchievementProgressText(IAchievement achievement) : this()
        {
            WithController(new AchievementProgressTextController(this, achievement));
        }

        public void SetValues(IEnumerable<(string Title, string Content)> values)
        {
            lock (_valueLock)
            {
                _values.Clear();
                foreach((string Title, string Content) value in values)
                {
                    _values.Add(new ProgressTextData(value.Title, value.Content, () => _titleFont, () => _contentFont, _titleHeight, _contentHeight));
                }
            }
        }

        private void RecalculateValues()
        {
            lock (_valueLock)
            {
                foreach (ProgressTextData entry in _values)
                {
                    entry.Recalculate();
                }
            }
        }

        public override void RecalculateLayout()
        {
            _titleHeight.Update();
            _contentHeight.Update();

            _titleFont.Update();
            _contentFont.Update();

            RecalculateValues();

            _dividerWidth.Update();
            _dividerHeight.Update();
            _dividerPadding.Update();

            _dividerTop = (int)((Height - _dividerHeight) / 2.0f);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            int offsetLeft = 0;
            ProgressTextData[] values = _values.ToArray();

            for(int i = 0; i < values.Length; i++)
            {
                ProgressTextData value = values[i];

                int width = value.TitleWidth + _titleContentPadding + value.ContentWidth;
                Rectangle valueBounds = new Rectangle(offsetLeft, 0, width, Height);

                PaintProgressElement(value.Title, value.Content, spriteBatch, valueBounds);
                
                if (i == values.Length - 1)
                {
                    break;
                }

                offsetLeft += width + _dividerPadding;

                Rectangle dividerBounds = new Rectangle(offsetLeft,
                                                        _dividerTop,
                                                        _dividerWidth,
                                                        _dividerHeight);
                
                // Divider
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Description.VerticalDivider,
                    dividerBounds);

                offsetLeft += dividerBounds.Width + _dividerPadding;

            }
        }

        private void PaintProgressElement(string title, string content, SpriteBatch spriteBatch, Rectangle bounds)
        {
            // title (e.g. "Tier:" or "Objectives:")
            spriteBatch.DrawStringOnCtrl(this,
                title,
                _progressTitleFont,
                bounds,
                Color.LightGray,
                false,
                true,
                1,
                HorizontalAlignment.Left,
                VerticalAlignment.Middle);

            // content (e.g. "200/300")
            spriteBatch.DrawStringOnCtrl(this,
                content,
                _progressContentFont,
                bounds,
                Color.White,
                false,
                true,
                1,
                HorizontalAlignment.Right,
                VerticalAlignment.Middle);
        }

        protected override void DisposeControl()
        {
            _values?.Clear();
            
            base.DisposeControl();
        }
    }
}
