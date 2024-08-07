using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementMenuPresenter : Presenter<AchievementMenuView, AchievementHandler>
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementMenuPresenter>();

        public AchievementMenuPresenter(AchievementMenuView view, AchievementHandler model) : base(view, model)
        {
            Model.PackLoadedOrUnloaded += OnPackLoadedOrUnloaded;
        }

        protected override void UpdateView()
        {
            // TODO: make fallback locale an option
            RecalculateCategories(Gw2Sharp.WebApi.Locale.English);
        }

        private void RecalculateCategories(Gw2Sharp.WebApi.Locale fallbackLocale)
        {
            string[] categories = Model.GetCategoryNamesForUserLocale(fallbackLocale);

            //Dictionary<string, CollectionMenuData[]> result = new Dictionary<string, CollectionMenuData[]>();

            Dictionary<string, MenuItem> categoryItems = new Dictionary<string, MenuItem>();

            IEnumerable<ILocalizable> categoryNames = Model.CurrentCategories.Select(category => category.Name);

            foreach (IAchievementCategory category in Model.CurrentCategories)
            {
                string categoryName = category.Name.GetLocalizedForUserLocale(categoryNames, fallbackLocale);

                if (!categoryItems.ContainsKey(categoryName))
                {
                    categoryItems[categoryName] = new MenuItem()
                    {
                        Text = categoryName
                    };
                }

                foreach (IAchievementCollection collection in category.AchievementCollections.ToArray())
                {
                    MenuItemWithData<Func<IView>> collectionItem = new MenuItemWithData<Func<IView>>()
                    {
                        // TODO: make fallback locale an option
                        Text = collection.Name.GetLocalizedForUserLocale(),
                        Icon = collection.Icon,
                        Parent = categoryItems[categoryName],
                        Data = () => GetCollectionView(collection)
                    };
                }
            }

            View.SetContent(categoryItems.Values);
        }

        private IView GetCollectionView(IAchievementCollection collection)
        {
            return new AchievementCollectionView(collection);
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
