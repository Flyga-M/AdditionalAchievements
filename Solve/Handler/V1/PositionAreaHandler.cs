using AchievementLib.Pack.V1.Models;
using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Integration;
using Flyga.AdditionalAchievements.Status.Models;
using Flyga.AdditionalAchievements.Status.Provider;
using Flyga.PositionEventsModule.Contexts;
using Microsoft.Xna.Framework;
using PositionEvents;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1
{
    public class PositionAreaHandler : ActionHandler<PositionAreaAction>
    {
        private static readonly Logger Logger = Logger.GetLogger<PositionAreaHandler>();

        PositionEventsModuleStatusProvider _statusProvider;

        private PositionEventsContext _context => _statusProvider.Context as PositionEventsContext;

        public PositionAreaHandler(PositionEventsModuleStatusProvider positionEventsModuleStatusProvider)
        {
            _statusProvider = positionEventsModuleStatusProvider ?? throw new ArgumentNullException(nameof(positionEventsModuleStatusProvider));

            _statusProvider.StatusChanged += OnStatusProviderStatusChanged;

            if (_statusProvider.Status?.Status == Status.Status.Normal)
            {
                OnModuleAvailable();
            }

            if (_statusProvider.Status.Status != Status.Status.Unknown)
            {
                OnStatusProviderStatusChanged(_statusProvider, _statusProvider.Status);
            }
        }

        private void OnStatusProviderStatusChanged(object _, StatusData status)
        {
            switch(status.Status)
            {
                case Status.Status.Unknown:
                    {
                        State = HandlerState.None;
                        break;
                    }
                case Status.Status.Normal:
                    {
                        State = HandlerState.Working;
                        OnModuleAvailable();
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
                        State = HandlerState.None;
                        Logger.Error($"Status {status.Status} implementation missing on {this.GetType().Name}. Contact module author.");
                        break;
                    }
            }
        }

        private void OnModuleAvailable()
        {
            AddAreasToContext(_actions);
        }

        private void AddAreasToContext(IEnumerable<PositionAreaAction> actions)
        {
            foreach (PositionAreaAction action in actions)
            {
                AddAreaToContext(action);
            }
        }

        private void AddAreaToContext(PositionAreaAction action)
        {
            if (!action.FreezeUpdates)
            {
                _context?.RegisterArea(AdditionalAchievementsModule.Instance, action.MapId, action.Area, (positionData, isInside) => OnAreaLeftOrJoined(action, positionData, isInside));
            }
        }

        private void OnAreaLeftOrJoined(PositionAreaAction action, PositionData positionData, bool isInside)
        {
            action.IsFulfilled = isInside;
        }

        private void OnAreaFreezeUpdatesChanged(object sender, bool doFreeze)
        {
            if (!(sender is PositionAreaAction action))
            {
                Logger.Error("Sender at OnAreaFreezeUpdatesChanged could not be unboxed as PositionAreaAction.");
                return;
            }

            if (State == HandlerState.Working)
            {
                if (doFreeze) // save calculations, if action is not updating
                {
                    _context?.RemoveArea(AdditionalAchievementsModule.Instance, action.MapId, action.Area);
                }
                else
                {
                    AddAreaToContext(action);
                }
            }
        }

        /// <inheritdoc/>
        public override bool TryRegisterAction(PositionAreaAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            if (State == HandlerState.Working)
            {
                AddAreaToContext(action);
            }

            action.FreezeUpdatesChanged += OnAreaFreezeUpdatesChanged;
            return true;
        }

        /// <inheritdoc/>
        public override bool TryUnregisterAction(PositionAreaAction action)
        {
            if (!base.TryUnregisterAction(action))
            {
                return false;
            }

            FinalizeUnregistering(action);

            return true;
        }

        private void FinalizeUnregistering(PositionAreaAction action)
        {
            if (State == HandlerState.Working)
            {
                // can be savely called, even if the area has not been registered before
                _context?.RemoveArea(AdditionalAchievementsModule.Instance, action.MapId, action.Area);
            }

            action.FreezeUpdatesChanged -= OnAreaFreezeUpdatesChanged;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        { /** NOOP **/}

        /// <summary>
        /// <inheritdoc/>
        /// Will NOT dispose of the <see cref="PositionEventsModuleStatusProvider"/>.
        /// Will NOT dispose of the actions itself!
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <inheritdoc/>
        protected override void Cleanup()
        {
            if (_statusProvider != null)
            {
                _statusProvider.StatusChanged -= OnStatusProviderStatusChanged;
                _statusProvider = null;

                // no need to dispose the _statusProvider, since it will be disposed by the main StatusManager.
            }

            foreach (PositionAreaAction action in _actions)
            {
                FinalizeUnregistering(action);
            }

            base.Cleanup();
        }
    }
}
