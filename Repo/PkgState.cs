using Blish_HUD;
using System;

namespace Flyga.AdditionalAchievements.Repo
{
    public class PkgState
    {
        private bool _isInstalled;
        private bool _isUpdateAvailable;

        public event EventHandler<bool> InstalledChanged;
        public event EventHandler<string> InstallError;
        public event EventHandler<string> DeleteError;

        public bool InProgress { get; internal set; }

        public int DownloadProgress { get; internal set; }

        public bool IsInstalled
        {
            get => _isInstalled;
            internal set
            {
                bool oldValue = _isInstalled;
                _isInstalled = value;

                if (oldValue != _isInstalled)
                {
                    InstalledChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Determines whether a newer version than the currently installed version is available.
        /// </summary>
        /// <remarks>
        /// Will always be <see langword="false"/>, if <see cref="IsInstalled"/> is <see langword="false"/>.
        /// </remarks>
        public bool IsUpdateAvailable
        {
            get => IsInstalled && _isUpdateAvailable;
            internal set
            {
                _isUpdateAvailable = value;
            }
        }

        internal void ReportInstallError(string message)
        {
            InstallError?.Invoke(this, message);
        }

        internal void ReportDeleteError(string message)
        {
            DeleteError?.Invoke(this, message);
        }
    }
}
