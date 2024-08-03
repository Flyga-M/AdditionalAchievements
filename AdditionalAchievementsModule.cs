﻿using AchievementLib;
using AchievementLib.Pack;
using AchievementLib.Pack.PersistantData;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.Resources;
using Flyga.AdditionalAchievements.Status;
using Flyga.AdditionalAchievements.Status.Provider;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Fonts;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Views;
using Flyga.AdditionalAchievements.UI.Windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements
{
    [Export(typeof(Module))]
    public class AdditionalAchievementsModule : Module
    {
        // TODO: add option for achievement categories to have a diverging namespace from the pack (so a pack author can add achievements
        // to the collections from other packs)?
        
        private static readonly Logger Logger = Logger.GetLogger<AdditionalAchievementsModule>();

        public const string SUB_FOLDER = "additionalAchievements";

        // TODO: use a ViewContainerInstead?
        AchievementNotificationsView _notificationView;

        private AchievementWindow _achievementWindow;

        private CornerIcon _cornerIcon;

        internal StatusManager StatusManager;

        /// <summary>
        /// The full directory path, where the achievement packs are stored.
        /// </summary>
        public string WatchPath => DirectoriesManager.GetFullDirectoryPath(SUB_FOLDER);
        
        // TODO: there should be another subdirectory depending on the account the player is using.
        //       this is currently only available through the api, if an API key is provided
        // there should be a setting maybe to explicitly use a subdirectory per account
        public string AchievementPackDataPath => Path.Combine(WatchPath, "data", "packState.sqlite");

        internal static AdditionalAchievementsModule Instance { get; private set; }

        AchievementPackInitiator _packInitiator;
        Solve.Handler.AchievementHandler _achievementHandler;
        AchievementPackRepo _achievementPackRepo;

        private readonly Dictionary<IAchievementPackManager, TaskCompletionSource<bool>> _packsLoadedCompletionSources = new Dictionary<IAchievementPackManager, TaskCompletionSource<bool>>(); 

        private readonly HierarchyResolveContext _hierarchyResolveContext = new HierarchyResolveContext();

        public event EventHandler<IAchievementPackManager> AchievementPackRegistered;

        private void OnAchievementPackRegistered(IAchievementPackManager pack)
        {
            AchievementPackRegistered?.Invoke(this, pack);
        }

        /// <summary>
        /// Contains all currently registered achievement packs.
        /// </summary>
        public IAchievementPackManager[] Packs
        {
            get
            {
                List<IAchievementPackManager> packs = new List<IAchievementPackManager>();
                if (_packInitiator != null || _packInitiator.Packs != null)
                {
                    packs.AddRange(_packInitiator.Packs);
                }

                return packs.ToArray();
            }
        }

        // TODO: could manually keep that list. does not need to be generated every time.
        /// <summary>
        /// Contains all currently enabled and loaded achievement packs.
        /// </summary>
        public IAchievementPackManager[] LoadedPacks
        {
            get
            {
                return Packs.Where(pack => pack.State == PackLoadState.Loaded).ToArray();
            }
        }

        private void OnPackLoadStateChanged(object pack, PackLoadState state)
        {
            if (!(pack is IAchievementPackManager manager))
            {
                Logger.Error("OnPackLoadStateChanged called from object that is not " +
                    $"IAchievementPackManager: {pack.GetType()}.");
                return;
            }

            Logger.Debug($"pack state changed for pack {manager.Manifest.Namespace}: {state}.");
            
            if (state == PackLoadState.Loaded)
            {
                if (_packsLoadedCompletionSources.ContainsKey(manager))
                {
                    Logger.Info("Attempting to set completion source to true...");
                    if (_packsLoadedCompletionSources[manager]?.TrySetResult(true) != true)
                    {
                        Logger.Warn("Attempt to set result for completion source to true " +
                            $"for pack {manager.Manifest.Namespace} failed.");
                    }
                }
                else
                {
                    Logger.Warn($"pack state changed to {state}, but " +
                        $"completion source can't be called, since it's set to null.");
                }

                _packsLoadedCompletionSources[manager] = null;
                return;
            }
        }

        public AchievementPackInitiator PackInitiator => _packInitiator;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public AdditionalAchievementsModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {
            Instance = this;
            StatusManager = new StatusManager();
        }

        private void OnPackAddedOrRemoved(object _, bool added)
        {
            string add = "removed";
            if (added)
                add = "added";

            Logger.Info($"Pack {add}.");
        }
        private void OnPackLoadedOrUnloaded(object _, bool added)
        {
            string add = "unloaded";
            if (added)
                add = "loaded";

            Logger.Info($"Pack {add}.");
        }

        protected override async Task LoadAsync()
        {
            ExtractSQLiteStuff();
            AddUnmanagedDllDirectory();

            TextureManager.Initialize(); // TODO: we could wait for every texture to load, if we wanted
            FontManager.Initialize();

            // TODO: statusProvider should probably be private fields
            ApiStatusProvider apiStatusProvider = new ApiStatusProvider(Gw2ApiManager);
            PositionEventsModuleStatusProvider positionEventsModuleStatusProvider = new PositionEventsModuleStatusProvider(GameService.Module);

            StatusManager.AddStatusProvider(apiStatusProvider);
            StatusManager.AddStatusProvider(positionEventsModuleStatusProvider);

            // TODO: this currently does not add custom ActionHandlers
            //       AchievementHandler needs a method for that
            _achievementHandler = new Solve.Handler.AchievementHandler(AchievementPackDataPath, GameService.Gw2Mumble, positionEventsModuleStatusProvider, apiStatusProvider);
            _achievementHandler.PackAddedOrRemoved += OnPackAddedOrRemoved; // TODO: remove before going live
            _achievementHandler.PackLoadedOrUnloaded += OnPackLoadedOrUnloaded; // TODO: remove before going live

            Logger.Info("Initializing achievements from watchpack..");

            await InitializeAchievementsFromWatchPath();

            Logger.Info("  -- done");

            _achievementPackRepo = new AchievementPackRepo();
            await _achievementPackRepo.Load(GetModuleProgressHandler());

            SetupNotificationView();
            BuildAchievementWindow();
            BuildCornerIcon();
        }

        const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDefaultDllDirectories(uint DirectoryFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int AddDllDirectory(string NewDirectory);

        /// <summary>
        ///     https://msdn.microsoft.com/en-us/library/windows/desktop/ms684175(v=vs.85).aspx
        /// </summary>
        /// <param name="lpLibFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpLibFileName);

        public void AddUnmanagedDllDirectory()
        {
            SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

            string subPath = IntPtr.Size == 8 ? "x64" : "x86";
            string dllSubdirectory = "dll";
            string targetPath = Path.Combine(WatchPath, dllSubdirectory, subPath);

            AddDllDirectory(targetPath);
        }

        private void ExtractSQLiteStuff()
        {
            string localBlishHudDirectory = Directory.GetParent(WatchPath).FullName;

            string modulesSubDirectory = "modules";
            string moduleName = "AdditionalAchievements.bhm";

            string localModulesDirectory = Path.Combine(localBlishHudDirectory, modulesSubDirectory);

            string filePath = Path.Combine(localModulesDirectory, moduleName);

            string dllSubdirectory = "dll";

            string dllName = "SQLite.Interop.dll";

            string targetPath64 = Path.Combine(WatchPath, dllSubdirectory, "x64", dllName);
            string targetPath86 = Path.Combine(WatchPath, dllSubdirectory, "x86",  dllName);

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath64));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath86));

            if (File.Exists(targetPath64) && File.Exists(targetPath86))
            {
                return;
            }

            using (ZipArchive archive = ZipFile.OpenRead(filePath))
            {
                ZipArchiveEntry x64 = archive.GetEntry("x64/SQLite.Interop.dll");
                ZipArchiveEntry x86 = archive.GetEntry("x86/SQLite.Interop.dll");

                x64.ExtractToFile(targetPath64);
                x86.ExtractToFile(targetPath86);
            }
        }

        private void FinalizeRegisteredPack(IAchievementPackManager pack)
        {
            if (!_hierarchyResolveContext.TryAdd(pack, out AchievementLibException exception))
            {
                Logger.Warn($"An exception occured while attempting to add the pack {pack.Manifest.GetDetailedName()} to the hierarchy resolve " +
                    $"context: {exception}");
            }

            OnAchievementPackRegistered(pack);
        }

        private void FinalizeDeletedPack(IAchievementPackManager pack)
        {
            if (_packsLoadedCompletionSources.ContainsKey(pack))
            {
                Logger.Debug($"Cancelling previous pack load completion source, because " +
                $"pack was deleted before completing.");
                _packsLoadedCompletionSources[pack]?.TrySetResult(false);
                pack.Disable(true);
            }
            pack.PackLoadStateChanged -= OnPackLoadStateChanged;

            _achievementHandler.TryRemovePack(pack);

            pack.Dispose();
        }

        internal async Task InitializeAchievementsFromWatchPath()
        {
            Directory.CreateDirectory(WatchPath);

            Logger.Debug($"Disposing previous packInitiatior if not null: {_packInitiator != null}");
            _packInitiator?.Dispose();

            _packInitiator = new AchievementPackInitiator(WatchPath);

            PackException[] exceptions = _packInitiator.LoadWatchPath();
            Logger.Debug($"Registered {_packInitiator.Packs.Length} achievement packs from watch path " +
                $"{WatchPath} with {exceptions.Length} exceptions.");

            foreach (PackException exception in exceptions)
            {
                Logger.Warn(exception, "An exception occured while attempting to load an " +
                    $"achievement pack from the watch path ({WatchPath}). Affected achievement " +
                    $"pack will be ignored.");
            }

            Logger.Debug($"Finalizing registered achievement packs ({_packInitiator.Packs.Length}) and attempting to enable " +
                $"packs, that were enabled in the last " +
                $"session ({_packInitiator.Packs.Where(pack => pack.IsEnabled).Count()}).");
            foreach (IAchievementPackManager pack in _packInitiator.Packs)
            {
                FinalizeRegisteredPack(pack);

                Logger.Debug($"Finalizing pack: {pack.GetFullName()}");

                if (pack.IsEnabled)
                {
                    if (!await EnablePackAsync(pack))
                    {
                        Logger.Warn($"Attempt to enable pack {pack.Manifest?.GetDetailedName()}, " +
                            $"that was enabled in the last session failed.");
                    }
                }
            }

        }

        // TODO: maybe change parameter to string namespace. Only packs that are registered should 
        // be able to be enabled. This way we can also make sure, that the PackLoadStateChanged 
        // event subscription will be cleared properly.
        /// <summary>
        /// Waits for the pack to be fully loaded.
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        internal async Task<bool> EnablePackAsync(IAchievementPackManager pack)
        {
            if (pack == null)
            {
                Logger.Error("EnablePackAsync(IAchievementPackManager pack) with " +
                    "pack == null called.");
                return false;
            }

            if (_packsLoadedCompletionSources.ContainsKey(pack))
            {
                Logger.Debug($"Cancelling previous pack load completion source, because " +
                $"pack was registered again before completing.");
                _packsLoadedCompletionSources[pack]?.TrySetResult(false);
                pack.Disable(true);
                pack.PackLoadStateChanged -= OnPackLoadStateChanged; // probably not neccessary
            }

            _packsLoadedCompletionSources[pack] = new TaskCompletionSource<bool>();

            pack.PackLoadStateChanged += OnPackLoadStateChanged;

            GameService.Graphics.QueueMainThreadRender(async (graphicsDevice) =>
            {
                try
                {
                    if (!pack.Enable(graphicsDevice, _hierarchyResolveContext, out Task loading))
                    {
                        Logger.Debug($"Attempt to enable v1Pack {pack.Manifest.Namespace} " +
                            $"failed. v1Pack.Enable returned false.");
                        return;
                    }

                    await loading;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Unable to enable achievement pack {pack.Manifest.Namespace} " +
                        $"from watch path.");
                    return;
                }
            });

            if (_packsLoadedCompletionSources.ContainsKey(pack) && _packsLoadedCompletionSources[pack] != null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logger.Info("Waiting for pack completion source.");
                bool result = await _packsLoadedCompletionSources[pack].Task;
                sw.Stop();
                Logger.Info($"result: {result}. {sw.Elapsed.TotalMilliseconds}ms");

                if (_achievementHandler?.TryAddPack(pack) != true)
                {
                    Logger.Warn($"Unable to add enabled pack {pack.Manifest?.GetDetailedName()} to the achievement handler.");
                }

                return result;
            }
            else
            {
                Logger.Error($"Unable to wait for loading of pack {pack.Manifest.Namespace}, " +
                    $"because completion source is null.");
            }

            return true;
        }

        ///<inheritdoc cref="IAchievementPackManager.Disable(bool)"/>
        internal bool DisablePack(IAchievementPackManager pack, bool forceDisable)
        {
            return pack.Disable(forceDisable);
        }

        /// <summary>
        /// Attempts to manually register a <paramref name="pack"/>.
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>True, if the <paramref name="pack"/> was successfully registered. Otherwise false.</returns>
        internal bool TryRegisterPack(IAchievementPackManager pack)
        {
            if (pack == null )
            {
                Logger.Warn($"Attempt to register pack failed. Pack must be set to an instance.");
                return false;
            }

            if (!_packInitiator.TryRegisterPack(pack, out PackException exception))
            {
                Logger.Warn($"Attempt to register pack {pack.Manifest.GetDetailedName()} at " +
                    $"{pack.Manifest.PackFilePath} failed. An exception occured: {exception}");
                return false;
            }

            FinalizeRegisteredPack(pack);

            return true;
        }

        /// <summary>
        /// Attempts to manually register an achievement pack from the <paramref name="filepath"/>.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="pack"></param>
        /// <returns>True, if the achievement pack was successfully registered. Otherwise false.</returns>
        internal bool TryRegisterPack(string filepath, out IAchievementPackManager pack)
        {
            pack = null;
            
            if (string.IsNullOrWhiteSpace(filepath))
            {
                Logger.Warn("Trying to register pack with empty filepath.");
                return false;
            }

            if (!File.Exists(filepath))
            {
                Logger.Warn($"Trying to register pack from a file that does not exist: {filepath}");
                return false;
            }

            if (!_packInitiator.TryRegisterPack(filepath, out PackException exception, out pack))
            {
                Logger.Warn($"Attempt to register pack at {filepath} failed. An exception occured: {exception}");
                return false;
            }

            FinalizeRegisteredPack(pack);

            return true;
        }

        /// <summary>
        /// Attempts to delete the achievement pack from disk. Disables the pack beforehand. Disposes the manager.
        /// </summary>
        /// <param name="namespace"></param>
        /// <returns><inheritdoc cref="AchievementPackInitiator.TryDeletePack(string, out PackException, out IAchievementPackManager)"/></returns>
        internal bool TryDeletePack(string @namespace)
        {
            if (!_packInitiator.TryDeletePack(@namespace, out PackException ex, out IAchievementPackManager pack))
            {
                Logger.Warn($"Unable to delete pack with namespace {@namespace}. {ex}");
                return false;
            }

            FinalizeDeletedPack(pack);

            return true;
        }

        /// <summary>
        /// Attempts to delete the achievement pack from disk. Disables the pack beforehand. Disposes the manager.
        /// </summary>
        /// <param name="pack"></param>
        /// <returns><inheritdoc cref="AchievementPackInitiator.TryDeletePack(IAchievementPackManager, out PackException)"/></returns>
        internal bool TryDeletePack(IAchievementPackManager pack)
        {
            if (!_packInitiator.TryDeletePack(pack, out PackException ex))
            {
                Logger.Warn($"Unable to delete pack with namespace {pack.Manifest.GetDetailedName()} at " +
                    $"{pack.Manifest.PackFilePath}. {ex}");
                return false;
            }

            FinalizeDeletedPack(pack);

            return true;
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            // TODO: Consider enforcing a source so that multiple items can be shown in the loading tooltip.
            return new Progress<string>(UpdateModuleLoading);
        }

        private void UpdateModuleLoading(string loadingMessage)
        {
            if (this.RunState == ModuleRunState.Loaded && _cornerIcon != null)
            {
                _cornerIcon.LoadingMessage = loadingMessage;
                bool packsLoading = !string.IsNullOrWhiteSpace(loadingMessage);

                if (!packsLoading)
                {
                    _cornerIcon.BasicTooltipText = null;
                }
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        bool _once = false;
        double _elapsed = 0;

        protected override void Update(GameTime gameTime)
        {
            _achievementHandler?.Update(gameTime);

            //_elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

            //if (!_once && _elapsed > 10_000)
            //{
            //    _once = true;
            //    PkgBody pkgBody = new PkgBody(_achievementPackRepo.AchievementPackages.First())
            //    {
            //        Width = 500,
            //        Height = 200,
            //        Top = 60,
            //        Parent = GameService.Graphics.SpriteScreen
            //    };
            //}
        }

        // TODO: only do this when already ingame. Gets wrong values when in loading screen
        // TODO: should also update when the minimap position changes, or is resized
        private void SetupNotificationView()
        {
            int totalWidth = GameService.Graphics.SpriteScreen.Width;
            int totalHeight = GameService.Graphics.SpriteScreen.Height;

            int resultBottom = totalHeight - 16; // 16 = padding bottom

            bool minimapBottomRight = !GameService.Gw2Mumble.UI.IsCompassTopRight;

            if (minimapBottomRight)
            {
                int minimapHeight = GameService.Gw2Mumble.UI.CompassSize.Height;
                int minimapPaddingBotton = 64; // just a guess. I don't know of any way to know for sure

                resultBottom -= (minimapHeight + minimapPaddingBotton);
            }

            Container buildPanel = new Panel()
            {
                Width = 214,
                Height = 600,
                Right = totalWidth,
                Bottom = resultBottom,
                ShowBorder = true,
                CanScroll = false,
                CanCollapse = false,
                Visible = true,
                Parent = GameService.Graphics.SpriteScreen
            };


            _notificationView = new AchievementNotificationsView(_achievementHandler);
            IProgress<string> progress = new Progress<string>();
            _notificationView.DoLoad(progress);
            _notificationView.DoBuild(buildPanel);
            buildPanel.Show();
        }

        private void RemoveNotificationView()
        {
            if (_notificationView == null)
            {
                return;
            }

            _notificationView.NotificationsFlowPanel.Parent.Dispose();

            _notificationView.DoUnload();
        }

        private void BuildCornerIcon()
        {
            if (_cornerIcon != null)
            {
                return;
            }

            _cornerIcon = new CornerIcon()
            {
                IconName = this.Name, // TODO: localization relevant?
                Priority = 3,
                Icon = TextureManager.Display.IconCorner,
                HoverIcon = TextureManager.Display.IconCornerHover
            };

            _cornerIcon.Click += OnCornerIconClick;
        }

        private void OnCornerIconClick(object _, MouseEventArgs e)
        {
            _achievementWindow.ToggleWindow();
        }

        private void BuildAchievementWindow()
        {
            _achievementWindow = new AchievementWindow(_achievementHandler, _achievementPackRepo)
            {
                Parent = GameService.Graphics.SpriteScreen
            };
        }

        private void RemoveCornerIcon()
        {
            if (_cornerIcon != null)
            {
                _cornerIcon.Click -= OnCornerIconClick;
                _cornerIcon?.Dispose();
                _cornerIcon = null;
            }

            if (_achievementWindow != null)
            {
                _achievementWindow.Hide();
                _achievementWindow?.Dispose();
                _achievementWindow = null;
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            RemoveCornerIcon();
            RemoveNotificationView();

            // PackLoadStateChanged event subscriptions will be automatically cleared when
            // disposing the packs.
            _packInitiator?.Dispose();
            _achievementHandler?.Dispose();

            StatusManager?.Dispose();

            Storage.ClearEvents();
            TextureManager.FreeResources();
            FontManager.FreeResources();

            // All static members must be manually unset
            Instance = null;
        }

    }

}
