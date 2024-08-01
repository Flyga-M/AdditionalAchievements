using AchievementLib.Pack;
using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.Solve.Handler
{
    public abstract class ActionHandler<TAction> : IActionHandler<TAction> where TAction : class, IAction
    {
        private static readonly Logger Logger = Logger.GetLogger<ActionHandler<TAction>>();

        private HandlerState _state;

        protected readonly SafeList<TAction> _actions = new SafeList<TAction>();

        /// <inheritdoc/>
        public virtual IEnumerable<IAction> Actions => _actions.ToArray();

        /// <inheritdoc/>
        public event EventHandler<HandlerState> StateChanged;
        /// <inheritdoc/>
        public event EventHandler<IEnumerable<IAction>> Fatal;

        /// <inheritdoc/>
        public HandlerState State
        {
            get => _state;
            protected set
            {
                HandlerState oldValue = _state;
                
                _state = value;

                if (value != oldValue)
                {
                    StateChanged?.Invoke(this, value);

                    if (value == HandlerState.Fatal)
                    {
                        Fatal?.Invoke(this, Actions);
                        Cleanup();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool CanHandle(IAction action)
        {
            if (action == null)
            {
                return false;
            }

            return action.GetType() == typeof(TAction)
                || action.GetType().IsSubclassOf(typeof(TAction))
                || typeof(TAction).IsAssignableFrom(action.GetType());
        }

        /// <inheritdoc/>
        bool IActionHandler.TryRegisterAction(IAction action)
        {
            if (!CanHandle(action))
            {
                return false;
            }

            return TryRegisterAction(action as TAction);
        }

        /// <inheritdoc/>
        bool IActionHandler.TryUnregisterAction(IAction action)
        {
            if (!CanHandle(action))
            {
                return false;
            }

            return TryUnregisterAction(action as TAction);
        }

        /// <inheritdoc/>
        bool IActionHandler.TryRegisterActions(IEnumerable<IAction> actions, out IAction[] failedActions)
        {
            failedActions = Array.Empty<IAction>();

            if (actions == null || !actions.Any())
            {
                return false;
            }

            List<TAction> candidates = new List<TAction>();
            List<IAction> failed = new List<IAction>();

            foreach (IAction action in actions)
            {
                if (!CanHandle(action))
                {
                    failed.Add(action);
                    continue;
                }

                candidates.Add(action as TAction);
            }

            if (!candidates.Any())
            {
                failedActions = failed.ToArray();
                return false;
            }

            TryRegisterActions(candidates, out TAction[] failedCandidates);
            failed.AddRange(failedCandidates);

            failedActions = failed.ToArray();
            return failedActions.Length == 0;
        }

        /// <inheritdoc/>
        bool IActionHandler.TryUnregisterActions(IEnumerable<IAction> actions)
        {
            if (actions == null || !actions.Any())
            {
                return false;
            }

            List<TAction> candidates = new List<TAction>();

            foreach (IAction action in actions)
            {
                if (!CanHandle(action))
                {
                    return false;
                }

                candidates.Add(action as TAction);
            }

            return TryUnregisterActions(candidates);
        }

        /// <inheritdoc/>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Cleans up data, when the <see cref="State"/> changes to <see cref="HandlerState.Fatal"/>, or 
        /// the <see cref="ActionHandler{TAction}"/> is disposed.
        /// </summary>
        protected virtual void Cleanup()
        {
            _actions.Clear();
        }

        /// <inheritdoc/>
        public virtual bool TryRegisterAction(TAction action)
        {
            if (State == HandlerState.Fatal)
            {
                return false;
            }

            if (action == null)
            {
                return false;
            }

            if (_actions.Contains(action))
            {
                return false;
            }

            if (!IsValid(action))
            {
                return false;
            }

            _actions.Add(action);

            return true;
        }

        /// <inheritdoc/>
        public virtual bool TryUnregisterAction(TAction action)
        {
            if (State == HandlerState.Fatal)
            {
                return false;
            }

            if (action == null)
            {
                return false;
            }

            if (!_actions.Contains(action))
            {
                return false;
            }

            return _actions.Remove(action);
        }

        /// <inheritdoc/>
        public virtual bool TryRegisterActions(IEnumerable<TAction> actions, out TAction[] failedActions)
        {
            failedActions = Array.Empty<TAction>();
            
            if (actions == null || !actions.Any())
            {
                return false;
            }
            
            List<TAction> failed = new List<TAction>();

            foreach (TAction action in actions)
            {
                if (!TryRegisterAction(action))
                {
                    failed.Add(action);
                    continue;
                }
            }

            failedActions = failed.ToArray();
            return failedActions.Length == 0;
        }

        /// <inheritdoc/>
        public virtual bool TryUnregisterActions(IEnumerable<TAction> actions)
        {
            if (actions == null || !actions.Any())
            {
                return false;
            }

            return actions.All(action => TryUnregisterAction(action));
        }

        /// <summary>
        /// Determines whether an <paramref name="action"/> is valid.
        /// </summary>
        /// <remarks>
        /// Called inside <see cref="TryRegisterAction(TAction)"/> to determine, if an action can be registered.
        /// </remarks>
        /// <param name="action"></param>
        /// <returns><see langword="true"/>, if the <paramref name="action"/> is eligible to be registered. 
        /// Otherwise <see langword="false"/>.</returns>
        internal virtual bool IsValid(TAction action)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            State = HandlerState.Disposed;

            Cleanup();
        }
    }
}
