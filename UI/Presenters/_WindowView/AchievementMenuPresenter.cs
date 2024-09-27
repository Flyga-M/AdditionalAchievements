using AchievementLib;
using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.Textures;
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
            Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> permanentcollectionItems = new Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)>();
            Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> dailyCollectionItems = new Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)>();
            Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> weeklyCollectionItems = new Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)>();
            Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> monthlyCollectionItems = new Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)>();

            Dictionary<string, MenuItem> pinnedCategoryItems = new Dictionary<string, MenuItem>();

            IEnumerable<ILocalizable> categoryNames = Model.CurrentCategories.Select(category => category.Name);
            IEnumerable<ILocalizable> collectionNames = Model.CurrentCategories.SelectMany(category => category.AchievementCollections).Select(collection => collection.Name);

            foreach (IAchievementCategory category in Model.CurrentCategories)
            {
                string categoryName = category.Name.GetLocalizedForUserLocale(categoryNames, fallbackLocale);

                foreach (IAchievementCollection collection in category.AchievementCollections.ToArray())
                {
                    bool isPermanent = collection.Achievements.Any(achievement => achievement.ResetType == AchievementLib.ResetType.Permanent);
                    bool isDaily = collection.Achievements.Any(achievement => achievement.ResetType == AchievementLib.ResetType.Daily);
                    bool isWeekly = collection.Achievements.Any(achievement => achievement.ResetType == AchievementLib.ResetType.Weekly);
                    bool isMonthly = collection.Achievements.Any(achievement => achievement.ResetType == AchievementLib.ResetType.Monthly);

                    if (isPermanent)
                    {
                        if (!categoryItems.ContainsKey(categoryName))
                        {
                            categoryItems[categoryName] = new MenuItem()
                            {
                                Text = categoryName
                            };
                        }

                        CreateCollectionMenuItem(permanentcollectionItems, collection, collectionNames, fallbackLocale, categoryName, categoryItems);
                    }
                    if (isDaily)
                    {
                        if (!pinnedCategoryItems.ContainsKey("Daily"))
                        {
                            pinnedCategoryItems["Daily"] = new MenuItem()
                            {
                                Text = Resources.Achievements.Categories.Daily
                            };
                        }
                        CreateCollectionMenuItem(dailyCollectionItems, collection, collectionNames, fallbackLocale, "Daily", pinnedCategoryItems);
                    }
                    if (isWeekly)
                    {
                        if (!pinnedCategoryItems.ContainsKey("Weekly"))
                        {
                            pinnedCategoryItems["Weekly"] = new MenuItem()
                            {
                                Text = Resources.Achievements.Categories.Weekly
                            };
                        }
                        CreateCollectionMenuItem(weeklyCollectionItems, collection, collectionNames, fallbackLocale, "Weekly", pinnedCategoryItems);
                    }
                    if (isMonthly)
                    {
                        if (!pinnedCategoryItems.ContainsKey("Monthly"))
                        {
                            pinnedCategoryItems["Monthly"] = new MenuItem()
                            {
                                Text = Resources.Achievements.Categories.Monthly
                            };
                        }
                        CreateCollectionMenuItem(monthlyCollectionItems, collection, collectionNames, fallbackLocale, "Monthly", pinnedCategoryItems);
                    }
                }
            }

            foreach ((MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections) in permanentcollectionItems.Values)
            {
                Item.Data = () => GetCollectionViewWithoutResets(Collections);
            }

            foreach ((MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections) in dailyCollectionItems.Values)
            {
                Item.Data = () => GetDailyView(Collections);
            }

            foreach ((MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections) in weeklyCollectionItems.Values)
            {
                Item.Data = () => GetWeeklyView(Collections);
            }

            foreach ((MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections) in monthlyCollectionItems.Values)
            {
                Item.Data = () => GetMonthlyView(Collections);
            }

            List<MenuItem> sortedCategories = new List<MenuItem>
            {
                CreateWatchListMenuItem()
            };
            sortedCategories.AddRange(pinnedCategoryItems.Values);
            sortedCategories.AddRange(categoryItems.Values);

            View.SetContent(sortedCategories);
        }

        private MenuItemWithData<Func<IView>> CreateWatchListMenuItem()
        {
            return new MenuItemWithData<Func<IView>>()
            {
                Text = Resources.Achievements.Categories.WatchList,
                Icon = TextureManager.Display.WatchIcon,
                Data = GetWatchListView
            };
        }

        private void CreateCollectionMenuItem(Dictionary<string, (MenuItemWithData<Func<IView>> Item, IEnumerable<IAchievementCollection> Collections)> collectionItems,  IAchievementCollection collection, IEnumerable<ILocalizable> collectionNames, Gw2Sharp.WebApi.Locale fallbackLocale, string categoryName, Dictionary<string, MenuItem> categoryItems)
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

        private IView GetWatchListView()
        {
            return new AchievementCollectionView(Model.CurrentCategories.SelectMany(category => category.AchievementCollections), FilterWatchList);
        }

        private IView GetCollectionViewWithoutResets(IEnumerable<IAchievementCollection> collections)
        {
            return new AchievementCollectionView(collections, FilterPermanent);
        }

        private IView GetDailyView(IEnumerable<IAchievementCollection> collections)
        {
            return new AchievementCollectionView(collections, FilterDaily);
        }

        private IView GetWeeklyView(IEnumerable<IAchievementCollection> collections)
        {
            return new AchievementCollectionView(collections, FilterWeekly);
        }

        private IView GetMonthlyView(IEnumerable<IAchievementCollection> collections)
        {
            return new AchievementCollectionView(collections, FilterMonthly);
        }

        private bool FilterWatchList(IAchievement achievement)
        {
            return achievement.IsWatched;
        }

        private bool FilterResetType(IAchievement achievement, ResetType resetType)
        {
            return achievement.ResetType == resetType;
        }

        private bool FilterPermanent(IAchievement achievement)
        {
            return FilterResetType(achievement, ResetType.Permanent);
        }

        private bool FilterDaily(IAchievement achievement)
        {
            return FilterResetType(achievement, ResetType.Daily);
        }

        private bool FilterWeekly(IAchievement achievement)
        {
            return FilterResetType(achievement, ResetType.Weekly);
        }

        private bool FilterMonthly(IAchievement achievement)
        {
            return FilterResetType(achievement, ResetType.Monthly);
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
