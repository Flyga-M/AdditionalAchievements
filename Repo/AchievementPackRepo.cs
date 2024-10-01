using AchievementLib.Pack;
using Blish_HUD;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Repo
{
    // mostly copied from https://github.com/blish-hud/Pathing/blob/main/MarkerPackRepo/MarkerPackRepo.cs
    public class AchievementPackRepo
    {
        private static Logger Logger = Logger.GetLogger<AchievementPackRepo>();

        private AdditionalAchievementsModule _module => AdditionalAchievementsModule.Instance;
        private AchievementPackInitiator _initiator => AdditionalAchievementsModule.Instance.PackInitiator;

        private const string PUBLIC_REPOURL = "";

        private string _repoUrl => PUBLIC_REPOURL;

        public AchievementPackPkg[] AchievementPackages { get; private set; } = Array.Empty<AchievementPackPkg>();

        public AchievementPackRepo()
        { /** NOOP **/ }

        /// <remarks>
        /// Make sure to call, ONLY after module was already loaded, and initiator was initialized.
        /// </remarks>
        public async Task Load(IProgress<string> progress)
        {
            AchievementPackages = await LoadAchievementPackPkgs(progress);

            Logger.Debug($"Found {AchievementPackages.Length} marker packs from {_repoUrl}.");

            await LoadLocalPackInfo(progress);

            progress.Report(null);
        }

        private async Task<AchievementPackPkg[]> LoadAchievementPackPkgs(IProgress<string> progress)
        {
            progress.Report("Requesting latest list of achievement packs...");

            (AchievementPackPkg[] releases, Exception exception) = await RequestAchievementPacks();

            if (exception != null)
            {
                progress.Report($"Failed to get a list of achievement packs.\r\n{exception.Message}");
            }

            return releases;
        }

        private async Task<(AchievementPackPkg[] Releases, Exception Exception)> RequestAchievementPacks()
        {
            try
            {
                return (await _repoUrl.WithHeader("User-Agent", "Blish-HUD").GetJsonAsync<AchievementPackPkg[]>(), null);
            }
            catch (FlurlHttpException ex)
            {
                Logger.Warn($"Failed to get list of achievement packs. {ex}");
                return (Array.Empty<AchievementPackPkg>(), ex);
            }
        }

        private async Task LoadLocalPackInfo(IProgress<string> progress)
        {
            string packPath = _module.WatchPath;

            IManifest[] existingPacks = _initiator.Packs.Select(pack => pack.Manifest).ToArray();

            foreach (IManifest manifest in existingPacks)
            {
                AchievementPackPkg correspondingPkg = AchievementPackages.Where(pkg => pkg.Namespace == manifest.Namespace).FirstOrDefault();

                if (correspondingPkg == null)
                {
                    correspondingPkg = new AchievementPackPkg(
                        // TODO: make fallback locale an option somewhere?
                        manifest.Name.GetLocalizedForUserLocale(),
                        manifest.Namespace,
                        // TODO: make fallback locale an option somewhere?
                        manifest.Description.GetLocalizedForUserLocale(),
                        string.Empty,
                        string.Empty,
                        new string[] { "Local Only" },
                        manifest.Version,
                        manifest.Author,
                        manifest.TryGetLastFileUpdate(out DateTime lastUpdate) ? lastUpdate : DateTime.Now
                    )
            {
                        IsLocalOnly = true
                    };

                    List<AchievementPackPkg> newPackageList = new List<AchievementPackPkg>(AchievementPackages)
                {
                        correspondingPkg
                    };
                    AchievementPackages = newPackageList.ToArray();
                }

                correspondingPkg.State.IsInstalled = true;
                correspondingPkg.State.CurrentManager = _initiator.Packs.FirstOrDefault(pack => pack.Manifest.Namespace == correspondingPkg.Namespace);

                if (manifest.Version < correspondingPkg.Version)
                {
                    correspondingPkg.State.IsUpdateAvailable = true;
                }
                else
                {
                    correspondingPkg.State.IsUpdateAvailable = false;
                }

                if (correspondingPkg.KeepUpdated && correspondingPkg.State.IsUpdateAvailable)
                {
                    await PackHandlingUtil.DownloadOrUpdatePackAsync(correspondingPkg, progress);
                }
            }
        }
    }
}
