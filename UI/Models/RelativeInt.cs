using System;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class RelativeInt
    {
        private bool _cacheValue;

        private readonly float _factor;

        private readonly Func<int> _getReferenceValue;

        private int? _cachedValue;

        /// <summary>
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

        public RelativeInt(float factor, Func<int> getReferenceValue, bool cacheValue = true)
        {
            _factor = factor;
            _getReferenceValue = getReferenceValue;
            _cacheValue = cacheValue;
        }

        public RelativeInt(int defaultValue, int defaultReferenceValue, Func<int> getReferenceValue, bool cacheValue = true)
            : this ((float)defaultValue / (float)defaultReferenceValue, getReferenceValue, cacheValue)
        { /** NOOP **/ }

        /// <summary>
        /// Call every time the reference value updates, if <see cref="CacheValue"/> is set to <see langword="true"/>.
        /// </summary>
        /// <remarks>
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

        private int GetValue(bool ignoreCache)
        {
            if (ignoreCache || !CacheValue || !_cachedValue.HasValue)
            {
                return (int)(_getReferenceValue() * _factor);
            }

            return _cachedValue.Value;
        }

        /// <summary>
        /// Returns the value of the <see cref="RelativeInt"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="CacheValue"/> is set to <see langword="true"/>, the value is only as recent as the last call 
        /// to <see cref="Update"/>.
        /// </remarks>
        /// <returns>The value of the <see cref="RelativeInt"/>.</returns>
        public int GetValue()
        {
            return GetValue(false);
        }

        public static implicit operator int(RelativeInt relativeInt)
        {
            return relativeInt.GetValue();
        }
    }
}
