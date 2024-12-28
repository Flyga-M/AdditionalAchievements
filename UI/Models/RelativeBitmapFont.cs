using Blish_HUD;
using Flyga.AdditionalAchievements.Textures.Fonts;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class RelativeBitmapFont
    {
        private bool _cacheValue;

        private readonly RelativeInt _relativeHeight;

        private readonly ContentService.FontFace _fontFace;
        private readonly ContentService.FontStyle _fontStyle;

        private BitmapFont _cachedValue;

        // <summary>
        /// Determines whether the resulting relative value will be cached until <see cref="Update"/> is called. 
        /// [Default: <see langword="true"/>].
        /// </summary>
        public bool CacheValue
        {
            get => _cacheValue;
            set
            {
                _cacheValue = value;

                if (value)
                {
                    Update();
                    return;
                }

                _cachedValue = null;
            }
        }

        public RelativeBitmapFont(RelativeInt relativeHeight, ContentService.FontFace fontFace, ContentService.FontStyle fontStyle, bool cacheValue = true)
        {
            _relativeHeight = relativeHeight;
            _fontFace = fontFace;
            _fontStyle = fontStyle;

            _cacheValue = cacheValue;
        }

        public RelativeBitmapFont(float factor, Func<int> getReferenceValue, ContentService.FontFace fontFace, ContentService.FontStyle fontStyle, bool cacheValue = true)
            : this (new RelativeInt(factor, getReferenceValue), fontFace, fontStyle, cacheValue)
        { /** NOOP **/ }

        public RelativeBitmapFont(int defaultValue, int defaultReferenceValue, Func<int> getReferenceValue, ContentService.FontFace fontFace, ContentService.FontStyle fontStyle, bool cacheValue = true)
            : this(new RelativeInt(defaultValue, defaultReferenceValue, getReferenceValue), fontFace, fontStyle, cacheValue)
        { /** NOOP **/ }

        /// <summary>
        /// Call every time the reference value updates, if <see cref="CacheValue"/> is set to <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// Will also update the given <see cref="RelativeInt">relativeHeight</see>.  
        /// Does not need to be called, if <see cref="CacheValue"/> is set to <see langword="false"/>.
        /// </remarks>
        public void Update()
        {
            if (!CacheValue)
            {
                return;
            }

            _cachedValue = GetValue(true);
        }

        private BitmapFont GetValue(bool ignoreCache)
        {
            if (ignoreCache || !CacheValue || _cachedValue == null)
            {
                _relativeHeight.Update();
                return FontManager.GetFontFromHeight(_relativeHeight, _fontFace, _fontStyle);
            }

            return _cachedValue;
        }

        /// <summary>
        /// Returns the value of the <see cref="RelativeBitmapFont"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="CacheValue"/> is set to <see langword="true"/>, the value is only as recent as the last call 
        /// to <see cref="Update"/>.
        /// </remarks>
        /// <returns>The value of the <see cref="RelativeBitmapFont"/>.</returns>
        public BitmapFont GetValue()
        {
            return GetValue(false);
        }

        public static implicit operator BitmapFont(RelativeBitmapFont relativeBitmapFont)
        {
            return relativeBitmapFont.GetValue();
        }
    }
}
