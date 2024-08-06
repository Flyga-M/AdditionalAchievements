using AchievementLib.Pack;
using AchievementLib.Pack.V1.Models;
using Flyga.AdditionalAchievements.Status.Provider;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Mumble
{
    public class MountHandler : MumbleHandler<MountAction>
    {
        private ConcurrentDictionary<MountType, SafeList<MountAction>> _actionsByMountType;

        // Use unused value as an 'unset' value
        private Gw2Sharp.Models.MountType _currentMount = (Gw2Sharp.Models.MountType) 255;

        public Gw2Sharp.Models.MountType CurrentMount
        {
            get => _currentMount;
            private set
            {
                Gw2Sharp.Models.MountType oldValue = _currentMount;
                _currentMount = value;

                if (oldValue != _currentMount)
                {
                    OnMountChanged(oldValue, value);
                }
            }
        }

        public MountHandler(MumbleStatusProvider mumbleStatusProvider) : base(mumbleStatusProvider)
        {
            _actionsByMountType = new ConcurrentDictionary<MountType, SafeList<MountAction>>();
        }

        public override bool TryRegisterAction(MountAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            if (!_actionsByMountType.ContainsKey(action.MountType))
            {
                _actionsByMountType[action.MountType] = new SafeList<MountAction>();
            }

            _actionsByMountType[action.MountType].Add(action);
            UpdateAction(action);

            return true;
        }

        public override bool TryUnregisterAction(MountAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            if (!_actionsByMountType.ContainsKey(action.MountType))
            {
                return false;
            }

            return _actionsByMountType[action.MountType].Remove(action);
        }

        public override void Update(GameTime gameTime)
        {
            if (State != HandlerState.Working)
            {
                return;
            }

            CurrentMount = _context.PlayerCharacter.CurrentMount;
        }

        private void OnMountChanged(Gw2Sharp.Models.MountType oldValue, Gw2Sharp.Models.MountType newValue)
        {
            if (_actionsByMountType.ContainsKey((MountType)oldValue))
            {
                UpdateActions(_actionsByMountType[(MountType)oldValue]);
            }

            if (_actionsByMountType.ContainsKey((MountType)newValue))
            {
                UpdateActions(_actionsByMountType[(MountType)newValue]);
            }
        }

        private void UpdateActions(IEnumerable<MountAction> actions)
        {
            foreach (MountAction action in actions)
            {
                UpdateAction(action);
            }
        }

        private void UpdateAction(MountAction action)
        {
            if (action.FreezeUpdates) // save calculations, if action is not updating
            {
                return;
            }

            action.IsFulfilled = action.MountType == (MountType)CurrentMount;
        }

        protected override void Cleanup()
        {
            _actionsByMountType.Clear();
            base.Cleanup();
        }
    }
}
