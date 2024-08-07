using AchievementLib.Pack;
using AchievementLib.Pack.V1.Models;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Provider;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Mumble
{
    public class IdentityHandler : MumbleHandler<IdentityAction>
    {
        private ConcurrentDictionary<ProfessionType, SafeList<IdentityAction>> _actionsByProfessionType;
        private ConcurrentDictionary<SpecializationType, SafeList<IdentityAction>> _actionsBySpecializationType;
        private ConcurrentDictionary<RaceType, SafeList<IdentityAction>> _actionsByRaceType;
        private SafeList<IdentityAction> _actionsByActiveCommanderTag;

        // Use unused value as an 'unset' value
        private Gw2Sharp.Models.ProfessionType _currentProfessionType = (Gw2Sharp.Models.ProfessionType)255;
        private int _currentSpecializationType = -1;
        // Use unused value as an 'unset' value
        private Gw2Sharp.Models.RaceType _currentRaceType = (Gw2Sharp.Models.RaceType)255;
        private bool _currentActiveCommanderTag;
        // used to determine if the _currentActiveCommanderTag value was set at least once
        private bool _commanderTagInitialized = false;

        public Gw2Sharp.Models.ProfessionType CurrentProfessionType
        {
            get => _currentProfessionType;
            private set
            {
                Gw2Sharp.Models.ProfessionType oldValue = _currentProfessionType;
                _currentProfessionType = value;

                if (oldValue != _currentProfessionType)
                {
                    OnProfessionTypeChanged(oldValue, value);
                }
            }
        }

        public int CurrentSpecializationType
        {
            get => _currentSpecializationType;
            private set
            {
                int oldValue = _currentSpecializationType;
                _currentSpecializationType = value;

                if (oldValue != _currentSpecializationType)
                {
                    OnSpecializationTypeChanged(oldValue, value);
                }
            }
        }

        public Gw2Sharp.Models.RaceType CurrentRaceType
        {
            get => _currentRaceType;
            private set
            {
                Gw2Sharp.Models.RaceType oldValue = _currentRaceType;
                _currentRaceType = value;

                if (oldValue != _currentRaceType)
                {
                    OnRaceTypeChanged(oldValue, value);
                }
            }
        }

        public bool CurrentActiveCommanderTag
        {
            get => _currentActiveCommanderTag;
            set
            {
                bool oldValue = _currentActiveCommanderTag;
                _currentActiveCommanderTag = value;

                if (oldValue != _currentActiveCommanderTag || !_commanderTagInitialized)
                {
                    OnActiveCommanderTagChanged(oldValue, value);
                    _commanderTagInitialized = true;
                }
            }
        }

        public IdentityHandler(MumbleStatusProvider mumbleStatusProvider) : base(mumbleStatusProvider)
        {
            _actionsByProfessionType = new ConcurrentDictionary<ProfessionType, SafeList<IdentityAction>>();
            _actionsBySpecializationType = new ConcurrentDictionary<SpecializationType, SafeList<IdentityAction>>();
            _actionsByRaceType = new ConcurrentDictionary<RaceType, SafeList<IdentityAction>>();
            _actionsByActiveCommanderTag = new SafeList<IdentityAction>();

            _context.PlayerCharacter.SpecializationChanged += OnMumbleSpecializationChanged;
            _context.PlayerCharacter.IsCommanderChanged += OnMumbleIsCommanderChanged;
        }

        public override bool TryRegisterAction(IdentityAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            if (action.Profession.HasValue)
            {
                if (!_actionsByProfessionType.ContainsKey(action.Profession.Value))
                {
                    _actionsByProfessionType[action.Profession.Value] = new SafeList<IdentityAction>();
                }

                _actionsByProfessionType[action.Profession.Value].Add(action);
            }

            if (action.Specialization.HasValue)
            {
                if (!_actionsBySpecializationType.ContainsKey(action.Specialization.Value))
                {
                    _actionsBySpecializationType[action.Specialization.Value] = new SafeList<IdentityAction>();
                }

                _actionsBySpecializationType[action.Specialization.Value].Add(action);
            }

            if (action.Race.HasValue)
            {
                if (!_actionsByRaceType.ContainsKey(action.Race.Value))
                {
                    _actionsByRaceType[action.Race.Value] = new SafeList<IdentityAction>();
                }

                _actionsByRaceType[action.Race.Value].Add(action);
            }

            if (action.ActiveCommanderTag.HasValue)
            {
                _actionsByActiveCommanderTag.Add(action);
            }

            UpdateAction(action);

            return true;
        }

        public override bool TryUnregisterAction(IdentityAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            bool eval = true;

            if (action.Profession.HasValue && _actionsByProfessionType.ContainsKey(action.Profession.Value))
            {
                eval = eval && _actionsByProfessionType[action.Profession.Value].Remove(action);
            }

            if (action.Specialization.HasValue && _actionsBySpecializationType.ContainsKey(action.Specialization.Value))
            {
                eval = eval && _actionsBySpecializationType[action.Specialization.Value].Remove(action);
            }

            if (action.ActiveCommanderTag.HasValue)
            {
                eval = eval && _actionsByActiveCommanderTag.Remove(action);
            }

            return eval;
        }

        public override void Update(GameTime gameTime)
        {
            if (State != HandlerState.Working)
            {
                return;
            }

            // no events for these properties, so we have to check them every frame
            CurrentProfessionType = _context.PlayerCharacter.Profession;
            CurrentRaceType = _context.PlayerCharacter.Race;
        }

        private void OnMumbleSpecializationChanged(object _, ValueEventArgs<int> _1)
        {
            CurrentSpecializationType = _context.PlayerCharacter.Specialization;
        }

        private void OnMumbleIsCommanderChanged(object _, ValueEventArgs<bool> _1)
        {
            CurrentActiveCommanderTag = _context.PlayerCharacter.IsCommander;
        }

        private void OnProfessionTypeChanged(Gw2Sharp.Models.ProfessionType oldValue, Gw2Sharp.Models.ProfessionType newValue)
        {
            if (_actionsByProfessionType.ContainsKey((ProfessionType)oldValue))
            {
                UpdateActions(_actionsByProfessionType[(ProfessionType)oldValue]);
            }

            if (_actionsByProfessionType.ContainsKey((ProfessionType)newValue))
            {
                UpdateActions(_actionsByProfessionType[(ProfessionType)newValue]);
            }
        }
        
        private void OnSpecializationTypeChanged(int oldValue, int newValue)
        {
            if (_actionsBySpecializationType.ContainsKey((SpecializationType)oldValue))
            {
                UpdateActions(_actionsBySpecializationType[(SpecializationType)oldValue]);
            }

            if (_actionsBySpecializationType.ContainsKey((SpecializationType)newValue))
            {
                UpdateActions(_actionsBySpecializationType[(SpecializationType)newValue]);
            }
        }

        private void OnRaceTypeChanged(Gw2Sharp.Models.RaceType oldValue, Gw2Sharp.Models.RaceType newValue)
        {
            if (_actionsByRaceType.ContainsKey((RaceType)oldValue))
            {
                UpdateActions(_actionsByRaceType[(RaceType)oldValue]);
            }

            if (_actionsByRaceType.ContainsKey((RaceType)newValue))
            {
                UpdateActions(_actionsByRaceType[(RaceType)newValue]);
            }
        }

        private void OnActiveCommanderTagChanged(bool _, bool _1)
        {
            UpdateActions(_actionsByActiveCommanderTag);
        }

        private void UpdateActions(IEnumerable<IdentityAction> actions)
        {
            foreach (IdentityAction action in actions)
            {
                UpdateAction(action);
            }
        }

        private void UpdateAction(IdentityAction action)
        {
            if (action.FreezeUpdates) // save calculations, if action is not updating
            {
                return;
            }

            action.IsFulfilled = (!action.Profession.HasValue || action.Profession.Value == (ProfessionType)CurrentProfessionType)
                && (!action.Specialization.HasValue || action.Specialization.Value == (SpecializationType)CurrentSpecializationType)
                && (!action.Race.HasValue || action.Race.Value == (RaceType)CurrentRaceType)
                && (!action.ActiveCommanderTag.HasValue || action.ActiveCommanderTag.Value == CurrentActiveCommanderTag);
        }

        protected override void Cleanup()
        {
            _actionsByProfessionType.Clear();
            _actionsBySpecializationType.Clear();
            _actionsByRaceType.Clear();
            _actionsByActiveCommanderTag.Clear();

            if (_context != null)
            {
                _context.PlayerCharacter.SpecializationChanged -= OnMumbleSpecializationChanged;
                _context.PlayerCharacter.IsCommanderChanged -= OnMumbleIsCommanderChanged;
            }
            base.Cleanup();
        }
    }
}
