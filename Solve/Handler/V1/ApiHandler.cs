using AchievementLib.Pack;
using AchievementLib.Pack.V1.Models;
using ApiParser;
using ApiParser.Endpoint;
using ApiParser.Settings;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Flyga.AdditionalAchievements.Solve.Handler.V1.Api;
using Flyga.AdditionalAchievements.Status.Models;
using Flyga.AdditionalAchievements.Status.Provider;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1
{
    public class ApiHandler : ActionHandler<ApiAction>
    {   
        private static readonly Logger Logger = Logger.GetLogger<ApiHandler>();

        private ApiStatusProvider _apiStatusProvider;

        private readonly SafeList<ApiAction> _recoverableActionsPermission = new SafeList<ApiAction>();

        private double _elapsed;

        public int ApiRefreshRate => _apiStatusProvider.Context.Settings.Cooldown;

        public ApiState ApiState => _apiStatusProvider.Context.State;

        /// <remarks>
        /// Will not dispose of the <paramref name="apiStatusProvider"/> when disposed.
        /// </remarks>
        public ApiHandler(ApiStatusProvider apiStatusProvider)
        {
            _apiStatusProvider = apiStatusProvider ?? throw new ArgumentNullException(nameof(apiStatusProvider));
            _apiStatusProvider.SubtokenUpdated += OnSubtokenUpdated;
            _apiStatusProvider.StatusChanged += OnApiStatusChanged;

            // so the first api update occurs 20 seconds after initialization
            _elapsed = ApiRefreshRate - 20_000;
        }

        // TODO: should the cache be cleared, when the subtoken updates?
        private async void OnSubtokenUpdated(object _, ValueEventArgs<IEnumerable<TokenPermission>> permissions)
        {
            foreach (ApiAction action in _recoverableActionsPermission)
            {
                EndpointQuery query = ConstructQuery(action);
                if (query == null)
                {
                    Logger.Warn($"Unable to re-add ApiAction {action} after permission changes. " +
                        $"Removing action from the recoverable action queue (permission). Success? {_recoverableActionsPermission.Remove(action)}");
                    continue;
                }

                bool? hasRequiredPermissions = await HasRequiredPermissionsAsync(query);

                if (!hasRequiredPermissions.HasValue)
                {
                    Logger.Warn($"Unable to re-add ApiAction {action} after permission changes. " +
                        $"Removing action from the recoverable action queue (permission). Success? {_recoverableActionsPermission.Remove(action)}");
                    continue;
                }

                if (!hasRequiredPermissions.Value)
                {
                    continue;
                }

                Logger.Info($"Re-adding ApiAction {action} after permission changes. ApiAction is no longer unable to be processed.");
                _recoverableActionsPermission.Remove(action);

                if (!TryRegisterAction(action))
                {
                    Logger.Warn($" -> Unable to re-add ApiAction {action} after permission changes. Registering failed.");
                }
            }
        }

        private void OnApiStatusChanged(object _, StatusData _1)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            if (State == HandlerState.Fatal)
            {
                return;
            }

            switch (_apiStatusProvider.Status.Status)
            {
                case Status.Status.Unknown:
                    {
                        State = HandlerState.None;
                        break;
                    }
                case Status.Status.Normal:
                    {
                        State = HandlerState.Working;
                        break;
                    }
                case Status.Status.Inhibited:
                    {
                        State = HandlerState.PartiallySuspended;
                        break;
                    }
                case Status.Status.Paused:
                    {
                        State = HandlerState.Suspended;
                        break;
                    }
                case Status.Status.Stopped:
                    {
                        State = HandlerState.Fatal;
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }


        public override bool TryRegisterAction(ApiAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            EndpointQuery query = ConstructQuery(action);
            if (query == null)
            {
                base.TryUnregisterAction(action);
                return false;
            }

            GetRequiredPermissionsAsync(query)
                .ContinueWith(requiredPermissions => _apiStatusProvider.AddPermissionRequest(action, requiredPermissions.Result));

            return true;
        }

        public override bool TryUnregisterAction(ApiAction action)
        {
            if (!base.TryUnregisterAction(action))
            {
                return false;
            }

            _apiStatusProvider.RemovePermissionRequest(action);

            return true;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            _elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (State != HandlerState.Working && State != HandlerState.PartiallySuspended)
            {
                return;
            }

            // no need to update actions faster than the Api Cache will be refreshed
            if (_elapsed < ApiRefreshRate)
            {
                return;
            }

            _elapsed = 0;

            foreach (ApiAction action in _actions)
            {
                _ = UpdateActionAsync(action);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <inheritdoc cref="FilterUtil.ApplyAlternativeFiltersAsync(IEnumerable{object}, IEnumerable{FilterQuery})"/>
        private async Task<object[]> ApplyFilterAsync(object[] data, Restraint filter)
        {
            FilterQuery[] alternativeFilters = FilterUtil.QueryFromFilter(filter);

            return await FilterUtil.ApplyAlternativeFiltersAsync(data, alternativeFilters);
        }

        private object[] PrepareApiData(object data)
        {
            if (!(data is IEnumerable enumerable))
            {
                return new object[] { data };
            }

            IEnumerable<object> result = enumerable.Cast<object>();

            return result.ToArray();
        }

        private async Task UpdateActionAsync(ApiAction action)
        {
            
            if (action.FreezeUpdates) // save calculations, if action is not updating
            {
                return;
            }

            object data = await RequestApiDataAsync(action);

            if (data == null)
            {
                Logger.Warn($"Unable to update ApiAction {action}.");
                return;
            }

            // if an enumerable is retrieved, split the responses
            object[] preparedData = PrepareApiData(data);

            object[] filteredData = preparedData;

            if (action.Filter != null)
            {
                try
                {
                    filteredData = await ApplyFilterAsync(preparedData, action.Filter);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Warn($"Unable to update ApiAction {action}. Filter malformed. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (NotImplementedException ex)
                {
                    Logger.Error($"Unable to update ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to update ApiAction {action}. An uncaught exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
            }

            if (action.ResultLayer != null)
            {
                try
                {
                    filteredData = await FilterUtil.ApplyResultLayerAsync(filteredData, action.ResultLayer);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Warn($"Unable to update ApiAction {action}. ResultLayer malformed. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to update ApiAction {action}. An uncaught exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
            }

            if (action is ApiActionComparison actionComparison)
            {
                await UpdateComparisonAsync(actionComparison, filteredData);
                return;
            }
            if (action is ApiActionContains actionContains)
            {
                UpdateContains(actionContains, filteredData);
                return;
            }
            if (action is ApiActionCopy actionCopy)
            {
                await UpdateCopyAsync(actionCopy, filteredData);
                return;
            }
            if (action is ApiActionCount actionCount)
            {
                UpdateCount(actionCount, filteredData);
                return;
            }
            if (action is ApiActionCountComparison actionCountComparison)
            {
                UpdateCountComparison(actionCountComparison, filteredData);
                return;
            }

            Logger.Error($"Unable to update {action}. An internal exception occured. Please report to the module author. Not implemented.");
            HandleNonRecoverableAction(action);
        }

        /// <summary>
        /// Updates the given <see cref="ApiActionComparison"/> <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        private async Task UpdateComparisonAsync(ApiActionComparison action, object[] data)
        {
            bool eval = true;
            
            foreach(object date in data)
            {
                object value;

                try
                {
                    value = await FilterUtil.GetValueAsync(date, action.Key);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Warn($"Unable to update ApiActionComparison {action}. Key malformed. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to update ApiActionComparison {action}. An uncaught exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }

                bool comparisonResult;

                try
                {
                    comparisonResult = ComparisonUtil.Compare(value, action.Value, action.Comparison);
                }
                catch (NotImplementedException ex)
                {
                    Logger.Error($"Unable to update ApiActionComparison {action}. An internal exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Warn($"Unable to update ApiActionComparison {action}. Incompatible comparison, or expected value " +
                        $"can't be parsed to same type as retrieved value ({value.GetType()}). {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to update ApiActionComparison {action}. An uncaught exception occured. Please report to the module author. {ex}");
                    HandleNonRecoverableAction(action);
                    return;
                }

                if (action.ComparisonTarget == ComparisonTarget.All)
                {
                    if (!comparisonResult)
                    {
                        eval = false;
                        break;
                    }
                }
                else if (action.ComparisonTarget == ComparisonTarget.Any)
                {
                    eval = false; // if we don't do this, this would still be evaluated to true, even if comparisonResult == false for every data object
                    if (comparisonResult)
                    {
                        eval = true;
                        break;
                    }
                }
                else
                {
                    Logger.Error($"Unable to update ApiActionComparison {action}. An internal exception occured. Please report to the module author. ComparisonTarget {action.ComparisonTarget} not implemeted.");
                    HandleNonRecoverableAction(action);
                    return;
                }
            }

            action.IsFulfilled = eval;
        }

        /// <summary>
        /// Updates the given <see cref="ApiActionContains"/> <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private void UpdateContains(ApiActionContains action, object[] data)
        {
            object[] relevantData;

            try
            {
                relevantData = SelectRelevantData(data, action.ChooseOption);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn($"Unable to update ApiActionContains {action}. ChooseOption invalid for retrieved data. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionContains {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionContains {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            bool eval = false;

            try
            {
                eval = relevantData.Any(date => {
                    return ComparisonUtil.Compare(date, action.Value, Comparison.Equal);
                });
                
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionContains {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionContains {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            try
            {
                action.IsFulfilled = eval;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to set action fulfillment stats. Exception occured: {ex}");
            }
        }

        /// <summary>
        /// Updates the given <see cref="ApiActionCopy"/> <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        private async Task UpdateCopyAsync(ApiActionCopy action, object[] data)
        {
            object[] relevantData;

            try
            {
                relevantData = SelectRelevantData(data, action.ChooseOption);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn($"Unable to update ApiActionCopy {action}. ChooseOption invalid for retrieved data. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionCopy {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionCopy {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (relevantData.Length != 1)
            {
                Logger.Warn($"Unable to update ApiActionCopy {action}. Choosen data must be a single item.");
                HandleNonRecoverableAction(action);
                return;
            }

            object date = relevantData.First();

            object value;

            try
            {
                value = await FilterUtil.GetValueAsync(date, action.Key);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn($"Unable to update ApiActionCopy {action}. Key malformed. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionCopy {action}. An uncaught exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (!(value is int copyValue))
            {
                Logger.Warn($"Unable to update ApiActionCopy {action}. Retrieved value is not an int. Type given: {value.GetType()}.");
                HandleNonRecoverableAction(action);
                return;
            }

            if (copyValue < 0)
            {
                Logger.Warn($"Unable to update ApiActionCopy {action}. Retrieved value is < 0. Value given: {copyValue}.");
                HandleNonRecoverableAction(action);
                return;
            }

            action.Parent.Parent.CurrentAmount = copyValue;
        }

        /// <summary>
        /// Updates the given <see cref="ApiActionCount"/> <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private void UpdateCount(ApiActionCount action, object[] data)
        {
            object[] relevantData;

            try
            {
                relevantData = SelectRelevantData(data, action.ChooseOption);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn($"Unable to update ApiActionCount {action}. ChooseOption invalid for retrieved data. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionCount {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionCount {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            int count = relevantData.Length;

            action.Parent.Parent.CurrentAmount = count;
        }

        /// <summary>
        /// Updates the given <see cref="ApiActionCountComparison"/> <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private void UpdateCountComparison(ApiActionCountComparison action, object[] data)
        {
            object[] relevantData;

            try
            {
                relevantData = SelectRelevantData(data, action.ChooseOption);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Warn($"Unable to update ApiActionCountComparison {action}. ChooseOption invalid for retrieved data. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionCountComparison {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionCountComparison {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            int count = relevantData.Length;

            bool eval = false;

            try
            {
                eval = ComparisonUtil.CompareInt(count, action.Value.Value, action.Comparison);
            }
            catch (NotImplementedException ex)
            {
                Logger.Error($"Unable to update ApiActionCountComparison {action}. An internal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to update ApiActionCountComparison {action}. An unhandled exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            action.IsFulfilled = eval;
        }

        /// <summary>
        /// Selects the relevant data from <paramref name="data"/> according to <paramref name="choice"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="choice"></param>
        /// <returns>The selected data.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="choice"/> is something other than 
        /// <see cref="ChooseOption.Self"/> and the first/last or at least one element of <paramref name="data"/> 
        /// is not <see cref="IEnumerable"/>.</exception>
        /// <exception cref="NotImplementedException">If a <see cref="ChooseOption"/> was not implemented.</exception>
        private object[] SelectRelevantData(IEnumerable<object> data, ChooseOption choice)
        {
            switch (choice)
            {
                case ChooseOption.Self:
                    {
                        return data.ToArray();
                    }
                case ChooseOption.First:
                    {
                        object first = data.First();
                        return EnumerableUtil.GetEnumerable<object>(first).ToArray(); // might throw InvalidOperationException
                    }
                case ChooseOption.Last:
                    {
                        object last = data.Last();
                        return EnumerableUtil.GetEnumerable<object>(last).ToArray(); // might throw InvalidOperationException
                    }
                case ChooseOption.Max:
                    {
                        object[] max = Array.Empty<object>();
                        int length = 0;

                        foreach(object date in data)
                        {
                            IEnumerable<object> element = EnumerableUtil.GetEnumerable<object>(date); // might throw InvalidOperationException

                            if (element.Count() > length)
                            {
                                max = element.ToArray();
                            }
                        }

                        return max;
                    }
                case ChooseOption.Min:
                    {
                        object[] min = Array.Empty<object>();
                        int length = int.MaxValue;

                        foreach (object date in data)
                        {
                            IEnumerable<object> element = EnumerableUtil.GetEnumerable<object>(date); // might throw InvalidOperationException

                            if (element.Count() < length)
                            {
                                min = element.ToArray();
                            }
                        }

                        return min;
                    }
                case ChooseOption.AppendAll:
                    {
                        List<object> all = new List<object>();

                        foreach (object date in data)
                        {
                            IEnumerable<object> element = EnumerableUtil.GetEnumerable<object>(date); // might throw InvalidOperationException

                            all.AddRange(element);
                        }

                        return all.ToArray();
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }

            }
        }

        /// <summary>
        /// Retrieves the endpoint data, for the <paramref name="action"/> from the api.
        /// </summary>
        /// <remarks>
        /// Removes the given <paramref name="action"/> from the ApiHandler, if a non recoverable error occured. If a 
        /// recoverable error occured, it will be temporarily removed and re-added later, if the situation changes.
        /// </remarks>
        /// <param name="action"></param>
        /// <returns>The retrieved data, or <see langword="null"/>, if the api responded with an error code.</returns>
        private async Task<object> RequestApiDataAsync(ApiAction action)
        {
            EndpointQuery query = ConstructQuery(action);

            if (query == null)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}.");
                HandleNonRecoverableAction(action);
                return null;
            }

            bool? hasPermissions = await HasRequiredPermissionsAsync(query);

            if (!hasPermissions.HasValue)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}.");
                HandleNonRecoverableAction(action);
                return null;
            }

            if (!hasPermissions.Value)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}. Insufficient permissions.");
                HandleRecoverableActionPermission(action);
                return null;
            }

            object data;
            try
            {
                data = await _apiStatusProvider.Context.ResolveQueryAsync(query);
            }
            catch (Exception ex)
            {
                HandleQueryExceptions(action, query, ex);
                return null;
            }

            return data;
        }

        private void HandleQueryExceptions(ApiAction action, EndpointQuery query, Exception ex)
        {
            if (ex is QueryResolveException)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}. Query {query} can't be resolved. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is QueryParsingException)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}. A variable can't be parsed correctly. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is QueryNotSupportedException)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}. Targeted endpoint currently not supported. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is EndpointRequestException)
            {
                Logger.Warn($"Unable to request api data for ApiAction {action}. Api returned with an error code. {ex}");

                Exception requestException = ex.InnerException;

                if (requestException == null)
                {
                    HandleNonRecoverableAction(action);
                    return;
                }

                if (RequestExceptionUtil.IsRecoverable(requestException))
                {
                    return;
                }

                if (requestException is AuthorizationRequiredException authorizationException)
                {
                    if (authorizationException is InvalidAccessTokenException || authorizationException is MissingScopesException)
                    {
                        HandleRecoverableActionPermission(action);
                        return;
                    }

                    HandleNonRecoverableAction(action);
                    return;
                }

                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is ArgumentNullException)
            {
                Logger.Error($"Unable to request api data for ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is ObjectDisposedException)
            {
                Logger.Error($"Unable to request api data for ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is ApiParserInternalException)
            {
                Logger.Error($"Unable to request api data for ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            if (ex is SettingsException)
            {
                Logger.Error($"Unable to request api data for ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                HandleNonRecoverableAction(action);
                return;
            }

            Logger.Error($"Unable to request api data for ApiAction {action}. An uncaught exception occured. Please report to the module author. {ex}");
            HandleNonRecoverableAction(action);
        }

        private void HandleNonRecoverableAction(ApiAction action)
        {
            Logger.Warn($"ApiAction {action} failed in a non recoverable way. " +
                $"Removing action from the ApiHandler. Success? {TryUnregisterAction(action)}");
        }

        private void HandleRecoverableActionPermission(ApiAction action)
        {
            Logger.Warn($"ApiAction {action} failed in a way, that may be recoverable when permission changes. " +
                $"Removing action from the ApiHandler for now. Success? {TryUnregisterAction(action)}");

            _recoverableActionsPermission.Add(action);
        }

        /// <summary>
        /// Determines whether the module currently has the required permissions to execute the <paramref name="query"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns><see langword="true"/>, if the module currently has the required permissions, or the required 
        /// permissions for the <paramref name="query"/> are unknown. Otherwise false. 
        /// Will return <see langword="null"/>, if an exception occured while attempting to determine the required permissions.</returns>
        private async Task<bool?> HasRequiredPermissionsAsync(EndpointQuery query)
        {
            TokenPermission[] requiredPermissions = await GetRequiredPermissionsAsync(query);

            if (requiredPermissions == null)
            {
                Logger.Warn($"Unable to check permissions for query {query}. Permissions could not be retrieved.");
                return null;
            }

            if (requiredPermissions.Length == 0)
            {
                Logger.Error($"Unable to check permissions for query {query}. Permissions could not be retrieved. " +
                    $"Endpoint is valid, but not implemented. Please report to the module author.");
                return true;
            }

            return _apiStatusProvider.Gw2ApiManager.HasPermissions(requiredPermissions);
        }

        /// <summary>
        /// Returns the permissions that are required by the <paramref name="query"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>The permissions that are required by the <paramref name="query"/>, or an empty array, if the required 
        /// permissions for the <paramref name="query"/> are unknown.
        /// Will return <see langword="null"/>, if an exception occured while attempting to determine the required permissions.</returns>
        private async Task<TokenPermission[]> GetRequiredPermissionsAsync(EndpointQuery query)
        {
            TokenPermission[] requiredPermissions;

            try
            {
                requiredPermissions = await _apiStatusProvider.Context.RequiredPermissions(query);
            }
            catch (QueryResolveException ex)
            {
                Logger.Warn($"Unable to get permissions for query {query}. Query can't be resolved. {ex}");
                return null;
            }
            catch (QueryParsingException ex)
            {
                Logger.Warn($"Unable to get permissions for query {query}. A variable in the query could not be resolved. {ex}");
                return null;
            }
            catch (ArgumentNullException ex)
            {
                Logger.Error($"Unable to get permissions for query {query}. An interal exception occured. Please report to the module author. {ex}");
                return null;
            }
            catch (SettingsException ex)
            {
                Logger.Error($"Unable to get permissions for query {query}. An interal exception occured. Please report to the module author. {ex}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to get permissions for query {query}. An uncaught exception occured. Please report to the module author. {ex}");
                return null;
            }

            if (requiredPermissions == null)
            {
                Logger.Error($"Unable to get permissions for query {query}. Endpoint is valid, but not implemented. Please report to the module author.");
                // return empty array, so endpoints that have not been implemented can still be attempted
                return Array.Empty<TokenPermission>();
            }

            return requiredPermissions;
        }

        /// <summary>
        /// Constructs an <see cref="EndpointQuery"/> from the given <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>The constructed <see cref="EndpointQuery"/>, or <see langword="null"/>, if an exception occured.</returns>
        private EndpointQuery ConstructQuery(ApiAction action)
        {
            EndpointQuery result;

            try
            {
                result = EndpointQuery.FromString(action.Endpoint);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Warn($"Unable to resolve ApiAction {action}. Endpoint is null. {ex}");
                return null;
            }
            catch (ArgumentException ex)
            {
                Logger.Warn($"Unable to resolve ApiAction {action}. Endpoint is empty or whitespace. {ex}");
                return null;
            }
            catch (QueryParsingException ex)
            {
                Logger.Warn($"Unable to resolve ApiAction {action}. Endpoint is invalid. {ex}");
                return null;
            }
            catch (SettingsException ex)
            {
                Logger.Error($"Unable to resolve ApiAction {action}. An interal exception occured. Please report to the module author. {ex}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to resolve ApiAction {action}. An uncaught exception occured. Please report to the module author. {ex}");
                return null;
            }

            return result;
        }

        /// <inheritdoc/>
        internal override bool IsValid(ApiAction action)
        {
            // if the construction of the query fails, there's no need to add it to the ApiHandler
            return ConstructQuery(action) != null;
        }

        // TODO: maybe it should dispose of the actions?
        /// <summary>
        /// <inheritdoc/>
        /// Will NOT dispose of the <see cref="Gw2ApiManager"/>.
        /// Will NOT dispose of the actions itself!
        /// Will NOT dispose of the <see cref="ApiStatusProvider"/>.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <inheritdoc/>
        protected override void Cleanup()
        {
            if (_apiStatusProvider != null)
            {
                _apiStatusProvider.SubtokenUpdated -= OnSubtokenUpdated;
                _apiStatusProvider.StatusChanged -= OnApiStatusChanged;

                // no need to dispose the _apiStatusProvider, since it will be disposed by the main StatusManager.
            }

            _recoverableActionsPermission.Clear();

            base.Cleanup();
        }
    }
}
