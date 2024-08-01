using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Api
{
    public static class EnumerableUtil
    {
        /// <summary>
        /// Unboxes <paramref name="data"/> to <see cref="IEnumerable"/> and casts it's elements 
        /// to <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="data"></param>
        /// <returns><paramref name="data"/> as <see cref="IEnumerable{TResult}"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="data"/> is not <see cref="IEnumerable"/>.</exception>
        /// <exception cref="InvalidCastException">If the elements of <paramref name="data"/> can't be cast to 
        /// <typeparamref name="TResult"/>.</exception>
        public static IEnumerable<TResult> GetEnumerable<TResult>(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            
            if (!(data is IEnumerable enumerable))
            {
                throw new InvalidOperationException($"Unable to get enumerable from {nameof(data)} of type {data.GetType()}. " +
                    $"{nameof(data)} is not enumerable.");
            }

            return enumerable.Cast<TResult>();
        }
    }
}
