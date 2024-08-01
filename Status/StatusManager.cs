using Flyga.AdditionalAchievements.Status.Models;
using System;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Status
{
    public class StatusManager : IDisposable
    {
        private const string NO_CATEGORY_IDENTIFIER = "__NONE";
        
        internal Dictionary<string, List<IStatusProvider>> Statuses;

        public event EventHandler StatusChanged;

        public StatusManager()
        {
            Statuses = new Dictionary<string, List<IStatusProvider>>();
        }

        /// <remarks>
        /// Disposes the added <paramref name="provider"/> when the <see cref="StatusManager"/> is disposed.
        /// </remarks>
        public void AddStatusProvider(IStatusProvider provider, string subCategory = NO_CATEGORY_IDENTIFIER)
        {
            if (!Statuses.ContainsKey(subCategory))
            {
                Statuses[subCategory] = new List<IStatusProvider>();
            }

            if (Statuses[subCategory].Contains(provider))
            {
                return;
            }

            Statuses[subCategory].Add(provider);
            provider.StatusChanged += OnStatusChanged;

            StatusChanged?.Invoke(this, null);
        }

        private void OnStatusChanged(object _, StatusData _1)
        {
            StatusChanged?.Invoke(this, null);
        }

        public void Dispose()
        {
            StatusChanged = null;

            if (Statuses != null)
            {
                foreach(string category in Statuses.Keys)
                {
                    foreach(IStatusProvider provider in Statuses[category])
                    {
                        provider.StatusChanged -= OnStatusChanged;
                        provider?.Dispose();
                    }
                }
            }

            Statuses?.Clear();
            Statuses = null;

        }
    }
}
