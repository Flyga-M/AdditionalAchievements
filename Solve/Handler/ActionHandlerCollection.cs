﻿using AchievementLib.Pack;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Provider;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.Solve.Handler
{
    public class ActionHandlerCollection : ActionHandler<IAction>
    {
        private static readonly Logger Logger = Logger.GetLogger<ActionHandlerCollection>();

        private readonly SafeList<IActionHandler> _handlers = new SafeList<IActionHandler>();

        private V1.ApiHandler _apiHandler;

        /// <inheritdoc cref="V1.ApiHandler.LastApiUpdate"/>
        public DateTime LastApiUpdate => _apiHandler.LastApiUpdate;

        /// <inheritdoc cref="V1.ApiHandler.PreviousApiUpdate"/>
        public DateTime PreviousApiUpdate => _apiHandler.PreviousApiUpdate;

        public override IEnumerable<IAction> Actions => _handlers.SelectMany(handler => handler.Actions);

        //{ "achievement", typeof(AchievementAction) }, // solves itself
        //{ "apiContains", typeof(ApiActionContains) }, // generally done
        //{ "apiCopy", typeof(ApiActionCopy) }, // generally done
        //{ "apiCount", typeof(ApiActionCount) }, // generally done
        //{ "apiCountComparison", typeof(ApiActionCountComparison) }, // generally done
        //{ "apiComparison", typeof(ApiActionComparison) }, // generally done
        //{ "position", typeof(PositionAction) }, // ignoring for now, maybe removing later
        //{ "positionArea", typeof(PositionAreaAction)}, // generally done
        //{ "lookingAt", typeof(LookingAtAction) } // generally done

        private void UpdateState()
        {
            if (State == HandlerState.Fatal)
            {
                return;
            }

            // Not fatal if all handlers are fatal, because a new handler might be added at a later time.

            // will set the state to supended, if _handlers has no entry. This is appropriate, because it
            // indicates, that the ActionHandlerCollection is currently not doing anything.
            if (_handlers.All(handler => handler.State == HandlerState.Suspended || handler.State == HandlerState.Fatal))
            {
                State = HandlerState.Suspended;
                return;
            }

            if (_handlers.Any(handler => handler.State == HandlerState.Suspended || handler.State == HandlerState.PartiallySuspended || handler.State == HandlerState.Fatal))
            {
                State = HandlerState.PartiallySuspended;
                return;
            }

            State = HandlerState.Working;
        }

        private void OnSubhandlerStateChanged(object sender, HandlerState _)
        {
            UpdateState();
        }

        public ActionHandlerCollection(MumbleStatusProvider mumbleStatusProvider, PositionEventsModuleStatusProvider positionEventsModuleStatusProvider, ApiStatusProvider apiStatusProvider)
        {
            V1.Mumble.LookingAtHandler lookingAtHandler = new V1.Mumble.LookingAtHandler(mumbleStatusProvider);
            V1.Mumble.MountHandler mountHandler = new V1.Mumble.MountHandler(mumbleStatusProvider);
            V1.Mumble.MapHandler mapHandler = new V1.Mumble.MapHandler(mumbleStatusProvider);
            V1.Mumble.IdentityHandler identityHandler = new V1.Mumble.IdentityHandler(mumbleStatusProvider);
            V1.PositionAreaHandler positionAreaHandler = new V1.PositionAreaHandler(positionEventsModuleStatusProvider);
            _apiHandler = new V1.ApiHandler(apiStatusProvider);
            V1.AchievementActionHandler achievementActionHandler = new V1.AchievementActionHandler();

            _handlers.Add(lookingAtHandler);
            _handlers.Add(mountHandler);
            _handlers.Add(mapHandler);
            _handlers.Add(identityHandler);
            _handlers.Add(positionAreaHandler);
            _handlers.Add(_apiHandler);
            _handlers.Add(achievementActionHandler);
        }

        /// <summary>
        /// Attempts to add an <see cref="IActionHandler"/> to the <see cref="ActionHandlerCollection"/>.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns>True, if the <see cref="IActionHandler"/> was successfully added. Otherwise false.</returns>
        public bool TryAddHandler(IActionHandler handler)
        {
            if (handler == null)
            {
                return false;
            }

            if (_handlers.Contains(handler))
            {
                return false;
            }

            _handlers.Add(handler);

            handler.StateChanged += OnSubhandlerStateChanged;

            return true;
        }

        /// <summary>
        /// Attempts to remove an <see cref="IActionHandler"/> from the <see cref="ActionHandlerCollection"/>.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns>True, if the <see cref="IActionHandler"/> was successfully added. Otherwise false.</returns>
        public bool TryRemoveHandler(IActionHandler handler)
        {
            if (handler == null)
            {
                return false;
            }

            if (_handlers.Contains(handler))
            {
                return false;
            }

            bool removeSuccess = _handlers.Remove(handler);

            handler.StateChanged -= OnSubhandlerStateChanged;

            return removeSuccess;
        }

        /// <summary>
        /// Attempts to return the <see cref="IHandler"/> that can handle the given <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns><see langword="true"/>, if the <see cref="ActionHandlerCollection"/> contains an <see cref="IActionHandler"/> 
        /// that can handle the <paramref name="action"/>. Otherwise <see langword="false"/>.</returns>
        private bool TryGetHandler(IAction action, out IActionHandler handler)
        {
            handler = null;
            if (action == null)
            {
                return false;
            }
            
            handler = _handlers.Where(actionHandler => (actionHandler.State != HandlerState.Fatal && actionHandler.CanHandle(action))).FirstOrDefault();
            return handler != null;
        }
        
        /// <inheritdoc/>
        public override bool TryRegisterAction(IAction action)
        {
            if (State == HandlerState.Fatal)
            {
                return false;
            }
            
            if (!TryGetHandler(action, out IActionHandler handler))
            {
                Logger.Warn($"Unable to register action of type {action.GetType()}, because there is no " +
                    $"action handler registered that can handle this type of action.");
                return false;
            }

            return handler.TryRegisterAction(action);
        }

        /// <inheritdoc/>
        public override bool TryUnregisterAction(IAction action)
        {
            if (State == HandlerState.Fatal)
            {
                return false;
            }

            if (!TryGetHandler(action, out IActionHandler handler))
            {
                Logger.Warn($"Unable to unregister action of type {action.GetType()}, because there is no " +
                    $"action handler registered that can handle this type of action.");
                return false;
            }

            return handler.TryUnregisterAction(action);
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (State == HandlerState.Fatal)
            {
                return;
            }

            // no need to manually call UpdateState() here, because it will triggered by the state changes of the 
            // handlers.
            
            foreach (IActionHandler handler in _handlers)
            {
                handler.Update(gameTime);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Will dispose the <see cref="IActionHandler">IActionHandlers</see> that were added!
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void Cleanup()
        {
            foreach (IActionHandler handler in _handlers)
            {
                handler.StateChanged -= OnSubhandlerStateChanged;
                handler.Dispose();
            }

            _handlers.Clear();
            base.Cleanup();
        }

        /// <inheritdoc/>
        public override bool CanHandle(IAction action)
        {
            return TryGetHandler(action, out IActionHandler _);
        }
    }
}
