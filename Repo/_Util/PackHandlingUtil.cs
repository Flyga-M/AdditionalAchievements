using Blish_HUD;
using Flyga.AdditionalAchievements.Textures.Cases;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AchievementLib.Pack;
using AchievementLib;
using System.Runtime.InteropServices;

namespace Flyga.AdditionalAchievements.Repo
{
    // mostly copied from https://github.com/blish-hud/Pathing/blob/main/Utility/PackHandlingUtil.cs
    public static class PackHandlingUtil
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(PackHandlingUtil));

        // At least one pack will deny us from downloading it without a more typical UA set.
        private const string DOWNLOAD_USERAGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

        private static AdditionalAchievementsModule _module => AdditionalAchievementsModule.Instance;
        private static AchievementPackInitiator _packInitiator => AdditionalAchievementsModule.Instance.PackInitiator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="achievementPackPkg"></param>
        /// <param name="progress"></param>
        /// <param name="funcOnComplete"></param>
        /// <param name="skipReload">Will reload all achievement packs, if <see langword="true"/>.</param>
        /// <returns></returns>
        public static async Task DownloadOrUpdatePackAsync(AchievementPackPkg achievementPackPkg, IProgress<string> progress, bool skipReload = false)
        {
            if (achievementPackPkg.State.InProgress)
            {
                Logger.Info($"Skipping download or update of achievement pack {achievementPackPkg.Namespace}, " +
                    "because a download or " +
                    "deletion process is still in progress.");

                achievementPackPkg.State.ReportInstallError("Download or deletion already in progress.");
                return;
            }

            // TODO: Localize 'Downloading pack '{0}'...'
            Logger.Info($"Downloading pack '{achievementPackPkg.Name}'...");
            progress.Report($"Downloading pack '{achievementPackPkg.Name}'...");
            achievementPackPkg.State.InProgress = true;
            achievementPackPkg.State.DownloadProgress = 0;

            string finalPath = Path.Combine(_module.WatchPath, achievementPackPkg.FileName);
            string tempPackDownloadDestination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", DOWNLOAD_USERAGENT);
                    webClient.DownloadProgressChanged += (s, e) => { achievementPackPkg.State.DownloadProgress = e.ProgressPercentage; };
                    await webClient.DownloadFileTaskAsync(achievementPackPkg.DownloadUrl, tempPackDownloadDestination);
                }
            }
            catch (Exception ex)
            {
                achievementPackPkg.State.ReportInstallError("Achievement pack download failed.");
                if (ex is WebException we)
                {
                    Logger.Warn(ex, $"Failed to download achievement pack {achievementPackPkg.Name} from {achievementPackPkg.DownloadUrl} to {tempPackDownloadDestination}.");
                }
                else
                {
                    Logger.Error(ex, $"Failed to download achievement pack {achievementPackPkg.Name} from {achievementPackPkg.DownloadUrl} to {tempPackDownloadDestination}.");
                }
                progress.Report(null);
                achievementPackPkg.State.InProgress = false;
                return;
            }

            // TODO: Localize 'Finalizing new pack download...'
            progress.Report("Finalizing new pack download...");

            // ignore exception, because it only occurs if the pack does not exist.
            if (_packInitiator.TryGetPack(achievementPackPkg.Namespace, out PackException _, out IAchievementPackManager existingPack))
            {
                Logger.Debug($"A pack with the namespace {achievementPackPkg.Namespace} already exists. Comparing versions.");

                if (existingPack.Manifest.Version < achievementPackPkg.Version)
                {
                    if (!_module.TryDeletePack(existingPack))
                    {
                        Logger.Warn($"Failed to delete older version {existingPack.Manifest.Version} of achievement pack {achievementPackPkg.Name} at {existingPack.Manifest.PackFilePath}.");
                        achievementPackPkg.State.ReportInstallError("Achievement pack deletion of older version failed.");

                        progress.Report(null);
                        CleanTempFile(tempPackDownloadDestination);
                        achievementPackPkg.State.InProgress = false;
                        return;
                    }
                }
            }

            // move tmp file to target destination
            try
            {
                File.Move(tempPackDownloadDestination, finalPath);
            }
            catch (IOException moveException)
            {
                Logger.Warn($"Failed to move temp achievement pack from {tempPackDownloadDestination} to {finalPath} " +
                    $"so instead we'll attempt to copy it. {moveException}");
                try
                {
                    File.Copy(tempPackDownloadDestination, finalPath);
                }
                catch (Exception copyException)
                {
                    Logger.Warn($"Failed to copy temp achievement pack from {tempPackDownloadDestination} to {finalPath}. {copyException}");
                    achievementPackPkg.State.ReportInstallError("Achievement pack moving failed.");

                    progress.Report(null);
                    achievementPackPkg.State.InProgress = false;
                    return;
                }
                finally
                {
                    CleanTempFile(tempPackDownloadDestination);
                }
            }

            // add new pack to initiatior
            if (!_module.TryRegisterPack(finalPath, out IAchievementPackManager newPack))
            {
                Logger.Error($"Failed to register new pack at {finalPath}.");
                achievementPackPkg.State.ReportInstallError("Achievement pack registering failed.");
                progress.Report(null);
                achievementPackPkg.State.InProgress = false;
                return;
            }

            if (!skipReload)
            {
                // TODO: localize
                progress.Report("Reloading all packs...");

                await _module.InitializeAchievementsFromWatchPath();
            }

            progress.Report(null);
            achievementPackPkg.State.IsInstalled = true;
            achievementPackPkg.State.IsUpdateAvailable = false;
            achievementPackPkg.State.InProgress = false;
        }

        private static void CleanTempFile(string tmpFilePath)
        {
            try
            {
                File.Delete(tmpFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to clean up tmp file at {tmpFilePath}. {ex}");
            }
        }

        public static void DeletePack(AchievementPackPkg achievementPackPkg)
        {
            if (achievementPackPkg.State.InProgress)
            {
                Logger.Info($"Skipping deletion of achievement pack {achievementPackPkg.Namespace}, " +
                    $"because a download or " +
                    "deletion process is still in progress.");

                achievementPackPkg.State.ReportDeleteError("Download or deletion already in progress.");
                return;
            }

            achievementPackPkg.State.InProgress = true;

            string packPath = Path.Combine(_module.WatchPath, achievementPackPkg.FileName);

            if (!_module.TryDeletePack(achievementPackPkg.Namespace))
            {
                Logger.Warn($"Failed to delete achievement pack {achievementPackPkg.Name}.");
                achievementPackPkg.State.ReportDeleteError("Achievement pack deletion failed.");
            }

            achievementPackPkg.State.InProgress = false;

            achievementPackPkg.State.IsInstalled = false;
        }
    }
}
