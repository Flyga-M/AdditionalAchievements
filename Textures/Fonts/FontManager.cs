using Blish_HUD;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Textures.Fonts
{
    public static class FontManager
    {
        private static Dictionary<Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle>, Tuple<int, BitmapFont>> _fontSizeLookup;

        private static object _lookupLock;

        public static void Initialize()
        {
            FreeResources();

            _fontSizeLookup = new Dictionary<Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle>, Tuple<int, BitmapFont>>();
            _lookupLock = new object();
        }

        public static BitmapFont GetFontFromHeight(int height, ContentService.FontFace fontFace, ContentService.FontStyle fontStyle)
        {
            ContentService.FontSize fontSize = GetClosestAndSmallerFromHeight(height, fontFace, fontStyle);

            return _fontSizeLookup[GetLookupKey(fontFace, fontSize, fontStyle)].Item2;
        }

        private static Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle> GetLookupKey(ContentService.FontFace fontFace, ContentService.FontSize fontSize, ContentService.FontStyle fontStyle)
        {
            return new Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle>(fontFace, fontSize, fontStyle);
        }

        private static Tuple<int, BitmapFont> GetLookupValue(int fontHeight, BitmapFont font)
        {
            return new Tuple<int, BitmapFont>(fontHeight, font);
        }

        private static void PopulateLookup(ContentService.FontFace fontFace, ContentService.FontSize fontSize, ContentService.FontStyle fontStyle)
        {
            Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle> lookupKey = GetLookupKey(fontFace, fontSize, fontStyle);

            lock(_lookupLock)
            {
                if (_fontSizeLookup.ContainsKey(lookupKey))
                {
                    return;
                }

                BitmapFont font = GameService.Content.GetFont(fontFace, fontSize, fontStyle);
                int fontHeight = font.LineHeight;

                _fontSizeLookup.Add(lookupKey, GetLookupValue(fontHeight, font));
            }
        }

        private static void PopulateLookup(ContentService.FontFace fontFace, ContentService.FontStyle fontStyle)
        {
            foreach (int fontSize in Enum.GetValues(typeof(ContentService.FontSize)))
            {
                PopulateLookup(fontFace, (ContentService.FontSize)fontSize, fontStyle);
            }
        }

        private static ContentService.FontSize GetClosestAndSmallerFromHeight(int height, ContentService.FontFace fontFace, ContentService.FontStyle fontStyle)
        {
            //Populate lookup since the font may not have been used before
            PopulateLookup(fontFace, fontStyle);

            ContentService.FontSize currentBest = (ContentService.FontSize)Enum.GetValues(typeof(ContentService.FontSize)).GetValue(0);

            int[] sortedFontSizeEnum = (int[])Enum.GetValues(typeof(ContentService.FontSize));
            Array.Sort(sortedFontSizeEnum);

            foreach (int fontSize in sortedFontSizeEnum)
            {
                Tuple<ContentService.FontFace, ContentService.FontSize, ContentService.FontStyle> lookupKey = GetLookupKey(fontFace, (ContentService.FontSize)fontSize, fontStyle);

                if (height < _fontSizeLookup[lookupKey].Item1)
                {
                    break;
                }

                currentBest = (ContentService.FontSize)fontSize;
            }

            return currentBest;
        }

        /// <remarks>
        /// Any call to the other methods will cause a crash, if it happens after <see cref="FreeResources"/> was called.
        /// </remarks>
        public static void FreeResources()
        {
            _fontSizeLookup?.Clear();
            _fontSizeLookup = null;
            _lookupLock = null;
        }
    }
}
