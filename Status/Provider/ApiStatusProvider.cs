using AchievementLib.Pack;
using AchievementLib.Pack.V1.Models;
using ApiParser;
using ApiParser.Settings;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Flyga.AdditionalAchievements.Status.Models;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class ApiStatusProvider : IStatusProvider
    {
        private Gw2ApiManager _gw2ApiManager;
        private ApiManager _context;
        private StatusData _statusData;

        private StatusData _permissionStatus;
        private StatusData _apiRequestStatus;

        private Dictionary<TokenPermission, IAction[]> _requestedPermissionsAndActions;
        private object _permissionLock = new object();

        public event EventHandler<StatusData> StatusChanged;
        /// <inheritdoc cref="Gw2ApiManager.SubtokenUpdated"/>
        public event EventHandler<ValueEventArgs<IEnumerable<TokenPermission>>> SubtokenUpdated;

        /// <summary>
        /// An array of all <see cref="TokenPermission"/>s that are currently requested by <see cref="IAction"/>s.
        /// </summary>
        public TokenPermission[] RequestedPermissions => _requestedPermissionsAndActions
            .Where(permission => permission.Value.Any())
            .Select(permission => permission.Key)
            .ToArray();

        internal ApiManager Context => _context;

        internal Gw2ApiManager Gw2ApiManager => _gw2ApiManager;

        public string Id => "Api";

        public string Title => "GW2 API"; // TODO: localization relevant?

        public string Category => "Dependencies"; // TODO: localize

        public StatusData Status
        {
            get => _statusData;
            private set
            {
                StatusData oldValue = _statusData;
                _statusData = value;

                if (value != oldValue)
                {
                    OnStatusChanged();
                }

                if (value.Status == AdditionalAchievements.Status.Status.Stopped)
                {
                    OnStopped();
                }
            }
        }

        public StatusData PermissionStatus
        {
            get => _permissionStatus;
            private set
            {
                StatusData oldValue = _permissionStatus;
                _permissionStatus = value;

                if (value != oldValue)
                {
                    CombineStatus();
                }
            }
        }

        public StatusData ApiRequestStatus
        {
            get => _apiRequestStatus;
            private set
            {
                StatusData oldValue = _apiRequestStatus;
                _apiRequestStatus = value;

                if (value != oldValue)
                {
                    CombineStatus();
                }
            }
        }

        public Func<IView> GetStatusView => null;

        public ApiStatusProvider(Gw2ApiManager gw2ApiManager)
        {
            _requestedPermissionsAndActions = new Dictionary<TokenPermission, IAction[]>();

            _context = new ApiManager(gw2ApiManager.Gw2ApiClient, ApiManagerSettings.Default);
            _gw2ApiManager = gw2ApiManager;

            UpdateStatus();

            _context.StateChanged += OnIssueTrackerStateChanged;
            _gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;

        }

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, Status);
        }

        private void OnSubtokenUpdated(object _, ValueEventArgs<IEnumerable<TokenPermission>> activePermissions)
        {
            ScreenNotification.ShowNotification("Subtoken has been updated.");
            UpdatePermissionStatus();
            SubtokenUpdated?.Invoke(this, activePermissions);
        }

        private void OnIssueTrackerStateChanged(object _, ApiState _1)
        {
            UpdateApiRequestStatus();
        }

        public void AddPermissionRequest(IAction action, IEnumerable<TokenPermission> permissions)
        {
            if (Status.Status == AdditionalAchievements.Status.Status.Stopped)
            {
                return;
            }
            
            lock (_permissionLock)
            {
                foreach (TokenPermission permission in permissions)
                {
                    if (!_requestedPermissionsAndActions.ContainsKey(permission))
                    {
                        _requestedPermissionsAndActions[permission] = Array.Empty<IAction>();
                    }

                    List<IAction> actions = _requestedPermissionsAndActions[permission].ToList();
                    if (actions.Contains(action))
                    {
                        // permissions that are requested by actions are assumed to not change during their lifetime,
                        // so we can just break here and assume, if it has already been added to one permission, it has been 
                        // added to all permissions
                        break;
                    }

                    actions.Add(action);
                    _requestedPermissionsAndActions[permission] = actions.ToArray();
                }
            }

            UpdateStatus();
        }

        public void RemovePermissionRequest(IAction action)
        {
            if (Status.Status == AdditionalAchievements.Status.Status.Stopped)
            {
                return;
            }

            lock (_permissionLock)
            {
                foreach (TokenPermission permission in _requestedPermissionsAndActions.Keys)
                {
                    if (!_requestedPermissionsAndActions[permission].Contains(action))
                    {
                        continue;
                    }

                    List<IAction> actions = _requestedPermissionsAndActions[permission].ToList();
                    actions.Remove(action);

                    _requestedPermissionsAndActions[permission] = actions.ToArray();
                }
            }

            UpdateStatus();
        }

        private void CombineStatus()
        {
            if (PermissionStatus == null)
            {
                Status = ApiRequestStatus;
                return;
            }

            if (ApiRequestStatus == null)
            {
                Status = PermissionStatus;
                return;
            }
            
            string combinedStatusMessage = $"{PermissionStatus?.StatusMessage} {ApiRequestStatus?.StatusMessage}";

            Status combinedStatus = (Status)Math.Max((int)PermissionStatus.Status, (int)ApiRequestStatus.Status);

            Status = new StatusData(combinedStatus, combinedStatusMessage);
        }

        private void UpdateStatus()
        {
            if (_context == null || _gw2ApiManager == null)
            {
                Status = new StatusData(AdditionalAchievements.Status.Status.Stopped, Resources.Status.Provider.GeneralMissingContext);
                return;
            }

            // TODO: use Blish 1.1.1 and then check _gw2ApiManager.HasSubtoken

            UpdatePermissionStatus();
            UpdateApiRequestStatus();
        }

        private void UpdatePermissionStatus()
        {
            TokenPermission[] requestedPermissions = RequestedPermissions;

            if (!requestedPermissions.Any())
            {
                PermissionStatus = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.ApiNoPermissionsRequested);
                return;
            }

            // TODO: permissions is allowed permissions. not neccessarily active permissions.
            // use .HasPermission(s) instead

            if (requestedPermissions.All(permission => _gw2ApiManager.Permissions.Contains(permission)))
            {
                PermissionStatus = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.ApiPermissionsOk);
                return;
            }

            if (requestedPermissions.Any(permission => _gw2ApiManager.Permissions.Contains(permission)))
            {
                PermissionStatus = new StatusData(AdditionalAchievements.Status.Status.Inhibited, Resources.Status.Provider.ApiPermissionsMissingSome);
                return;
            }

            PermissionStatus = new StatusData(AdditionalAchievements.Status.Status.Paused, Resources.Status.Provider.ApiPermissionsMissingAll);
        }

        private void UpdateApiRequestStatus()
        {
            switch (_context.State)
            {
                case ApiState.Unknown:
                    {
                        ApiRequestStatus = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.ApiRequestsNotEnoughData);
                        break;
                    }
                case ApiState.Reliable:
                    {
                        ApiRequestStatus = new StatusData(AdditionalAchievements.Status.Status.Normal, Resources.Status.Provider.ApiRequestsOk);
                        break;
                    }
                case ApiState.Unreliable:
                    {
                        ApiRequestStatus = new StatusData(AdditionalAchievements.Status.Status.Inhibited, Resources.Status.Provider.ApiRequestsUnreliable);
                        break;
                    }
                case ApiState.RateLimited:
                    {
                        ApiRequestStatus = new StatusData(AdditionalAchievements.Status.Status.Paused, Resources.Status.Provider.ApiRequestsRateLimited);
                        break;
                    }
                default:
                    {
                        ApiRequestStatus = new StatusData(AdditionalAchievements.Status.Status.Unknown, Resources.Status.Provider.GeneralImplementationMissing);
                        break;
                    }
            }
        }

        public void OnStopped()
        {
            if (_context != null)
            {
                _context.StateChanged -= OnIssueTrackerStateChanged;
                _context?.Dispose();
                _context = null;
            }

            if (_gw2ApiManager != null)
            {
                _gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
                _gw2ApiManager = null;
            }
        }

        public void Dispose()
        {
            StatusChanged = null;
            
            OnStopped();
        }
    }
}
