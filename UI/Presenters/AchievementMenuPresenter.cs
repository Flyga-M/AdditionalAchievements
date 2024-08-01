using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Views;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementMenuPresenter : Presenter<AchievementMenuView, AchievementHandler>
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementMenuPresenter>();

        public AchievementMenuPresenter(AchievementMenuView view, AchievementHandler model) : base(view, model)
        { /** NOOP **/ }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            RecalculateCategories(Gw2Sharp.WebApi.Locale.English);
            Model.PackLoadedOrUnloaded += OnPackLoadedOrUnloaded;

            return Task.FromResult(true);
        }

        private void RecalculateCategories(Gw2Sharp.WebApi.Locale fallbackLocale)
        {
            string[] categories = Model.GetCategoryNamesForUserLocale(fallbackLocale);

            Dictionary<string, CollectionMenuData[]> result = new Dictionary<string, CollectionMenuData[]>();

            foreach (string category in categories)
            {
                CollectionMenuData[] collections = Model.GetCollectionMenuDataForUserLocale(category, fallbackLocale);

                result[category] = collections;
            }


            View?.Categories?.Clear();
            View.Categories = result;
            try
            {
                View?.BuildMenuPanel();
            }
            catch (Exception ex) { Logger.Warn($"Ex: {ex}"); }
        }

        private void OnPackLoadedOrUnloaded(object _, bool added)
        {
            // TODO: ignore when being disposed (all packs are being removed)
            RecalculateCategories(Gw2Sharp.WebApi.Locale.English); // TODO: make the fallback locale an option
        }

        protected override void Unload()
        {
            Model.PackLoadedOrUnloaded -= OnPackLoadedOrUnloaded;
        }
    }
}
