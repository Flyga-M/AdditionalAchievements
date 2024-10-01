using AchievementLib;
using AchievementLib.Pack;
using AchievementLib.Pack.PersistantData;
using AchievementLib.Reset;
using AchievementLib.Reset.Default;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Provider;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.Solve.Handler
{
    public class AchievementHandler : IDisposable
    {   
        private static readonly Logger Logger = Logger.GetLogger<AchievementHandler>();

        private bool _disposed;

        private ResetManager _resetManager;
        private ActionHandlerCollection _actionHandlers;

        private SafeList<IAchievementPackManager> _packs = new SafeList<IAchievementPackManager>();

        public int CurrentPackCount => _packs.Count;

        /// <inheritdoc cref="ActionHandlerCollection.LastApiUpdate"/>
        public DateTime LastApiUpdate => _actionHandlers.LastApiUpdate;


        /// <inheritdoc cref="ActionHandlerCollection.PreviousApiUpdate"/>
        public DateTime PreviousApiUpdate => _actionHandlers.PreviousApiUpdate;

        /// <summary>
        /// Currently registered categories (from enabled achievement packs).
        /// </summary>
        public IAchievementCategory[] CurrentCategories => _packs.SelectMany(pack => pack.Categories).ToArray();

        // specifically calculated when it's called, just so we don't keep a collection in memory all the time
        private IEnumerable<IAchievement> _achievements => _packs.SelectMany(pack => pack.Categories).SelectMany(category => category.AchievementCollections).SelectMany(collection => collection.Achievements);

        // specifically calculated when it's called, just so we don't keep a collection in memory all the time
        private IAchievement[] _dailyAchievements => _achievements.Where(achievement => achievement.ResetType == ResetType.Daily).ToArray();
        private IAchievement[] _weeklyAchievements => _achievements.Where(achievement => achievement.ResetType == ResetType.Weekly).ToArray();
        private IAchievement[] _monthlyAchievements => _achievements.Where(achievement => achievement.ResetType == ResetType.Monthly).ToArray();

        public event EventHandler<IAchievement> AchievementCompleted;

        public event EventHandler<string> ResetOccured;

        /// <summary>
        /// Fires, when a <see cref="IAchievementPackManager"/> was added or removed.
        /// </summary>
        /// <remarks>
        /// An added <see cref="IAchievementPackManager"/> is not neccessarily also loaded. Use 
        /// <see cref="PackLoadedOrUnloaded"/> instead!
        /// </remarks>
        public event EventHandler<bool> PackAddedOrRemoved;

        /// <summary>
        /// Fires, when an added <see cref="IAchievementPackManager"/> was loaded or unloaded.
        /// </summary>
        public event EventHandler<bool> PackLoadedOrUnloaded;

        public AchievementHandler(string storagePath, ActionHandlerCollection actionHandlers, ResetManager resetManager = null)
        {
            if (storagePath == null)
            {
                throw new ArgumentNullException(nameof(storagePath));
            }
            
            if (actionHandlers == null)
            {
                throw new ArgumentNullException(nameof(actionHandlers));
            }
            
            if (resetManager == null)
            {
                resetManager = new Gw2Resets();
            }

            Storage.DefaultPath = storagePath;

            _resetManager = resetManager;

            _actionHandlers = actionHandlers;

            Storage.ExceptionOccured += OnStorageException;
            _resetManager.Reset += OnReset;
        }

        public AchievementHandler(string storagePath, MumbleStatusProvider mumbleStatusProvider, PositionEventsModuleStatusProvider positionEventsModuleStatusProvider, ApiStatusProvider apiStatusProvider, ResetManager resetManager = null)
            : this(storagePath, new ActionHandlerCollection(mumbleStatusProvider, positionEventsModuleStatusProvider, apiStatusProvider), resetManager) { /** NOOP **/ }

        public string[] GetCategoryNames(Gw2Sharp.WebApi.Locale locale, Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            IAchievementCategory[] categories = _packs.SelectMany(pack => pack.Categories).ToArray();

            List<string> categoryNames = new List<string>();

            IEnumerable<ILocalizable> references = categories.Select(category => category.Name);

            foreach (IAchievementCategory category in categories)
            {
                string categoryName = category.Name.GetLocalized(locale, references, fallbackLocale);
                if (!categoryNames.Contains(categoryName))
                {
                    categoryNames.Add(categoryName);
                }
            }

            return categoryNames.ToArray();
        }

        public string[] GetCategoryNamesForUserLocale(Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            IAchievementCategory[] categories = _packs.SelectMany(pack => pack.Categories).ToArray();

            List<string> categoryNames = new List<string>();

            IEnumerable<ILocalizable> references = categories.Select(category => category.Name);

            foreach (IAchievementCategory category in categories)
            {
                string categoryName = category.Name.GetLocalizedForUserLocale(references, fallbackLocale);
                if (!categoryNames.Contains(categoryName))
                {
                    categoryNames.Add(categoryName);
                }
            }

            return categoryNames.ToArray();
        }

        public CollectionMenuData[] GetCollectionMenuDataForUserLocale(IAchievementCategory category, Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            IAchievementCollection[] collections = category.AchievementCollections.ToArray();

            List<CollectionMenuData> result = new List<CollectionMenuData>();

            IEnumerable<ILocalizable> references = collections.Select(collection => collection.Name);

            foreach (IAchievementCollection collection in collections)
            {
                string collectionId = collection.GetFullName();
                string collectionName = collection.Name.GetLocalizedForUserLocale(references, fallbackLocale);
                Texture2D collectionIcon = collection.Icon ?? TextureManager.Notification.DefaultAchievement;

                // don't remove collection if their name already exists, because different packs can have collections with the same name
                if (!result.Any(collectionData => collectionData.Id == collectionId))
                {
                    result.Add(new CollectionMenuData(collectionId, collectionName, collectionIcon));
                }
            }

            return result.ToArray();
        }

        public CollectionMenuData[] GetCollectionMenuDataForUserLocale(string categoryName, Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            IAchievementCategory[] categories = _packs.SelectMany(pack => pack.Categories).ToArray();

            IEnumerable<ILocalizable> categoryReferences = categories.Select(category => category.Name);

            IAchievementCategory[] relevantCategories = categories.Where(category => category.Name.GetLocalizedForUserLocale(fallbackLocale) == categoryName).ToArray();

            if (relevantCategories.Length == 0)
            {
                Logger.Warn($"Unable to retrieve collection names and icons for user locale for category with name {categoryName}. " +
                    $"No such category found.");

                return Array.Empty<CollectionMenuData>();
            }

            IAchievementCollection[] collections = relevantCategories.SelectMany(category => category.AchievementCollections).ToArray();

            List<CollectionMenuData> result = new List<CollectionMenuData>();

            IEnumerable<ILocalizable> collectionReferences = collections.Select(collection => collection.Name);

            foreach (IAchievementCollection collection in collections)
            {
                string collectionId = collection.GetFullName();
                string collectionName = collection.Name.GetLocalizedForUserLocale(collectionReferences, fallbackLocale);
                Texture2D collectionIcon = collection.Icon ?? TextureManager.Notification.DefaultAchievement;

                // don't remove collection if their name already exists, because different packs can have collections with the same name
                if (!result.Any(collectionData => collectionData.Id == collectionId))
                {
                    result.Add(new CollectionMenuData(collectionId, collectionName, collectionIcon));
                }
            }

            return result.ToArray();
        }

        public IAchievement[] GetAchievementsForCollection(string collectionId)
        {
            // this is the easiest way, because multiple packs might use the same collection id (that is currently only true if the pack
            // uses the same exact namespace. Other options will be implemented in the future)
            // TODO: evaluate if this is too slow for many achievements
            return _achievements.Where(achievement => achievement.Parent.GetFullName() == collectionId).ToArray();
        }

        private void OnStorageException(object _, Exception ex)
        {
            Logger.Error($"An exception occured while attempting to store or retrieve data about an achievement. {ex}");
        }

        private void OnReset(object _, string id)
        {
            switch (id)
            {
                case "daily":
                    {
                        foreach (IAchievement achievement in _dailyAchievements)
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                case "weekly":
                    {
                        foreach (IAchievement achievement in _weeklyAchievements)
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                case "monthly":
                    {
                        foreach (IAchievement achievement in _monthlyAchievements)
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                default:
                    {
                        Logger.Error($"Unable to handle reset with id \"{id}\". An internal exception occured. Please report to the module author. Reset has not been implemented yet.");
                        return;
                    }
            }

            ResetOccured?.Invoke(this, id);
        }

        private void ResetIfNecessary(IAchievement achievement)
        {
            if (achievement.ResetType == ResetType.Permanent)
            {
                return;
            }

            switch(achievement.ResetType)
            {
                case ResetType.Daily:
                    {
                        if (_resetManager.ResetOccured("daily", achievement.LastCompletion))
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                case ResetType.Weekly:
                    {
                        if (_resetManager.ResetOccured("weekly", achievement.LastCompletion))
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                case ResetType.Monthly:
                    {
                        if (_resetManager.ResetOccured("monthly", achievement.LastCompletion))
                        {
                            achievement.ResetProgress();
                        }
                        break;
                    }
                default:
                    {
                        Logger.Error($"ResetType not implemented {new NotImplementedException()}");
                        return;
                    }
            }
        }

        private void OnAchievementFulfilled(object _, IAchievement achievement)
        {
            // TODO: free memory where possible (maybe add a "shallow" version of the Achievement, without all the condition data etc)

            if (achievement.ResetType == ResetType.Permanent && !achievement.IsRepeatable)
            {
                IAction[] actions = achievement.GetActions();

                if (!_actionHandlers.TryUnregisterActions(actions))
                {
                    Logger.Warn($"Some actions for the achievement {achievement.GetFullName()} could not be " +
                        $"removed from the action handler after achievement was completed.");
                }
            }

            AchievementCompleted?.Invoke(this, achievement);
        }

        private void OnAchievementFulfilledChanged(object sender, bool isFulfilled)
        {
            if (!(sender is IAchievement achievement))
            {
                Logger.Error($"Unable to determine if an achievement was fulfilled. OnAchievementFulfilledChanged was " +
                    $"called by a sender of type " +
                    $"{sender?.GetType()}. Expected type: {typeof(IAchievement)}.");
                return;
            }

            if (isFulfilled)
            {
                OnAchievementFulfilled(this, achievement);
            }
        }

        public bool Contains(IAchievement achievement)
        {
            return _achievements.Contains(achievement);
        }
        
        /// <remarks>
        /// Make sure the pack is loaded before adding.
        /// </remarks>
        public bool TryAddPack(IAchievementPackManager pack)
        {
            if (pack == null)
            {
                return false;
            }

            if (_packs.Contains(pack))
            {
                return false;
            }

            if (pack.State == PackLoadState.FatalError)
            {
                Logger.Warn($"Unable to add pack {pack.GetFullName()} to {nameof(AchievementHandler)}. Pack " +
                    $"has experienced a fatal error.");
                return false;
            }

            if (pack.State != PackLoadState.Loaded)
            {
                Logger.Warn($"Unable to add pack {pack.GetFullName()} to {nameof(AchievementHandler)}. Pack " +
                    $"is not loaded (current state: {pack.State}).");
                return false;
            }
            pack.PackUnloaded += OnPackUnloaded;

            OnPackLoaded(pack, null);

            _packs.Add(pack);
            PackAddedOrRemoved?.Invoke(this, true);

            return true;
        }

        public bool TryRemovePack(IAchievementPackManager pack)
        {
            if (pack == null)
            {
                return false;
            }

            if (!_packs.Contains(pack))
            {
                return false;
            }

            pack.PackUnloaded -= OnPackUnloaded;

            if (pack.State != PackLoadState.Unloaded)
            {
                OnPackUnloaded(pack, null);
            }

            _packs.Remove(pack);
            PackAddedOrRemoved?.Invoke(this, false);

            return true;
        }

        private void OnPackLoaded(object sender, EventArgs _)
        {
            if (!(sender is IAchievementPackManager pack))
            {
                Logger.Error($"Unable to register actions for loaded pack. OnPackLoaded was called by a sender of type " +
                    $"{sender?.GetType()}. Expected type: {typeof(IAchievementPackManager)}.");
                return;
            }

            IAchievement[] achievements = pack.Categories.SelectMany(category => category.GetAchievements()).ToArray();

            foreach (IAchievement achievement in achievements)
            {
                if (achievement.IsFulfilled && achievement.ResetType == ResetType.Permanent && !achievement.IsRepeatable)
                {
                    // TODO: this currently excludes objectives that grant partial completion from being able to be 
                    // fully completed, if achievement.MaxObjectives < the sum of potential objective.MaxAmount(s)
                    continue;
                }

                if (achievement.IsFulfilled && achievement.ResetType != ResetType.Permanent)
                {
                    ResetIfNecessary(achievement);
                }

                IAction[] actions = achievement.GetActions();

                if (!_actionHandlers.TryRegisterActions(actions, out IAction[] failedActions))
                {
                    IEnumerable<IAction> successfullyAddedActions = actions.Except(failedActions);

                    _actionHandlers.TryUnregisterActions(successfullyAddedActions);

                    Logger.Warn($"Unable to register actions for achievement {achievement.GetFullName()} in pack {pack.GetFullName()} to achievement handler. Some actions could not be " +
                        $"added to the action handler. Skipping achievement accordingly. " +
                        $"Affected actions: {string.Join<IAction>(", ", failedActions)}.");
                    continue;
                }

                achievement.FulfilledChanged += OnAchievementFulfilledChanged;
            }

            PackLoadedOrUnloaded?.Invoke(this, true);
        }

        private void OnPackUnloaded(object sender, EventArgs _)
        {
            if (!(sender is IAchievementPackManager pack))
            {
                Logger.Error($"Unable to register actions for loaded pack. OnPackLoaded was called by a sender of type " +
                    $"{sender?.GetType()}. Expected type: {typeof(IAchievementPackManager)}.");
                return;
            }

            if (!_actionHandlers.TryUnregisterActions(pack))
            {
                Logger.Warn($"Some actions for the achievement pack could not be " +
                        $"removed from the action handler after pack {pack.GetFullName()} was unloaded.");
            }

            PackLoadedOrUnloaded?.Invoke(this, false);
        }

        public void Update(GameTime gameTime)
        {
            _actionHandlers?.Update(gameTime);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_packs != null)
                    {
                        foreach (IAchievementPackManager pack in _packs.ToArray())
                        {
                            TryRemovePack(pack);
                        }
                    }
                    
                    Storage.ExceptionOccured -= OnStorageException;
                    
                    if (_resetManager != null)
                    {
                        _resetManager.Reset -= OnReset;
                        _resetManager?.Dispose();
                        _resetManager = null;
                    }

                    if (_actionHandlers != null)
                    {
                        _actionHandlers?.Dispose();
                        _actionHandlers = null;
                    }

                    _packs.Clear();
                    _packs = null;

                    AchievementCompleted = null;
                    ResetOccured = null;
                    PackAddedOrRemoved = null;
                    PackLoadedOrUnloaded = null;
                }

                _disposed = true;
            }
        }

        ~AchievementHandler()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
