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
            Dictionary<string, MenuItem> categoryItems = new Dictionary<string, MenuItem>();
            Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> collectionItems = new Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)>();

            IEnumerable<ILocalizable> categoryNames = Model.CurrentCategories.Select(category => category.Name);
            IEnumerable<ILocalizable> collectionNames = Model.CurrentCategories.SelectMany(category => category.AchievementCollections).Select(collection => collection.Name);

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
                    string collectionName = collection.Name.GetLocalizedForUserLocale(collectionNames, fallbackLocale);

                    string key = $"{categoryName}.{collectionName}";

                    if (!collectionItems.ContainsKey(key))
                    {
                        collectionItems[key] = (new MenuItemWithData<Func<IView>>()
                        {
                            Text = collectionName,
                            Icon = collection.Icon,
                            Parent = categoryItems[categoryName],
                            Data = () => null
                        }, Array.Empty<IAchievementCollection>());
                    }

                    List<IAchievementCollection> collections = new List<IAchievementCollection>(collectionItems[key].Collections)
                    {
                        collection
                    };

                    collectionItems[key] = (collectionItems[key].Item, collections);
                }
            }

            foreach ((MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections) in collectionItems.Values)
            {
                Item.Data = () => GetCollectionView(Collections);
            }

            View.SetContent(categoryItems.Values);
        }

        private IView GetCollectionView(IEnumerable<IAchievementCollection> collections)
        {
            return new AchievementCollectionView(collections);
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
