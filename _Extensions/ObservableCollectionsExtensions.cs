using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flyga.AdditionalAchievements
{
    public static class ObservableCollectionsExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            foreach (T item in range)
            {
                collection.Add(item);
            }
        }
    }
}
