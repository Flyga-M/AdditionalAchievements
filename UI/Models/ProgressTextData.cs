using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class ProgressTextData
    {
        public string Title;
        public string Content;
        public int TitleWidth;
        public int ContentWidth;

        public Rectangle TitleBounds;
        public Rectangle ContentBounds;

        public Func<BitmapFont> GetTitleFont;
        public Func<BitmapFont> GetContentFont;

        public Func<int> GetTitleHeight;
        public Func<int> GetContentHeight;

        public ProgressTextData(string title, string content, int titleWidth, int contentWidth, Func<BitmapFont> getTitleFont, Func<BitmapFont> getContentFont, Func<int> getTitleHeight, Func<int> getContentHeight)
        {
            Title = title;
            Content = content;
            TitleWidth = titleWidth;
            ContentWidth = contentWidth;

            GetTitleFont = getTitleFont;
            GetContentFont = getContentFont;

            GetTitleHeight = getTitleHeight;
            GetContentHeight = getContentHeight;
        }

        public ProgressTextData(string title, string content, Func<BitmapFont> getTitleFont, Func<BitmapFont> getContentFont, Func<int> getTitleHeight, Func<int> getContentHeight)
            : this(title, content, 0, 0, getTitleFont, getContentFont, getTitleHeight, getContentHeight)
        {
            Recalculate();
        }

        public ProgressTextData(string title, string content, Func<BitmapFont> getTitleFont, Func<BitmapFont> getContentFont, RelativeInt titleHeight, RelativeInt contentHeight)
            : this(title, content, 0, 0, getTitleFont, getContentFont, () => { titleHeight.Update(); return titleHeight; }, () => { contentHeight.Update(); return contentHeight; })
        {
            Recalculate();
        }

        private void RecalculateWidth(BitmapFont titleFont, BitmapFont contentFont)
        {
            TitleWidth = (int)titleFont.MeasureString($"{Title}: ").Width;
            ContentWidth = (int)contentFont.MeasureString($"{Content}").Width;
        }

        public void Recalculate()
        {
            RecalculateWidth();
            RecalculateBounds();
        }

        private void RecalculateWidth()
        {
            RecalculateWidth(GetTitleFont(), GetContentFont());
        }

        private void RecalculateBounds()
        {
            TitleBounds = new Rectangle(0, 0, TitleWidth, GetTitleHeight());
            ContentBounds = new Rectangle(0, 0, ContentWidth, GetContentHeight());
        }
    }
}
