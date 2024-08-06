using AchievementLib.Pack;
using AchievementLib.Pack.V1.Models;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Provider;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Mumble
{
    public class MapHandler : MumbleHandler<MapAction>
    {
        private ConcurrentDictionary<int, SafeList<MapAction>> _actionsByMapId;
        private ConcurrentDictionary<MapType, SafeList<MapAction>> _actionsByMapType;

        private int _currentMapId = -1;
        // Use unused value as an 'unset' value
        private Gw2Sharp.Models.MapType _currentMapType = (Gw2Sharp.Models.MapType)255;

        public int CurrentMapId
        {
            get => _currentMapId;
            private set
            {
                int oldValue = _currentMapId;
                _currentMapId = value;

                if (oldValue != _currentMapId)
                {
                    OnMapIdChanged(oldValue, value);
                }
            }
        }

        public Gw2Sharp.Models.MapType CurrentMapType
        {
            get => _currentMapType;
            private set
            {
                Gw2Sharp.Models.MapType oldValue = _currentMapType;
                _currentMapType = value;

                if (oldValue != _currentMapType)
                {
                    OnMapTypeChanged(oldValue, value);
                }
            }
        }


        public MapHandler(MumbleStatusProvider mumbleStatusProvider) : base(mumbleStatusProvider)
        {
            _actionsByMapId = new ConcurrentDictionary<int, SafeList<MapAction>>();
            _actionsByMapType = new ConcurrentDictionary<MapType, SafeList<MapAction>>();
            
            _context.CurrentMap.MapChanged += OnMumbleMapChanged;
        }

        public override bool TryRegisterAction(MapAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            if (action.MapId.HasValue)
            {
                if (!_actionsByMapId.ContainsKey(action.MapId.Value))
                {
                    _actionsByMapId[action.MapId.Value] = new SafeList<MapAction>();
                }

                _actionsByMapId[action.MapId.Value].Add(action);
                
            }

            if (action.MapType.HasValue)
            {
                if (!_actionsByMapType.ContainsKey(action.MapType.Value))
                {
                    _actionsByMapType[action.MapType.Value] = new SafeList<MapAction>();
                }

                _actionsByMapType[action.MapType.Value].Add(action);
            }

            UpdateAction(action);

            return true;
        }

        public override bool TryUnregisterAction(MapAction action)
        {
            if (!base.TryRegisterAction(action))
            {
                return false;
            }

            bool eval = true;

            if (action.MapId.HasValue && _actionsByMapId.ContainsKey(action.MapId.Value))
            {
                eval = eval && _actionsByMapId[action.MapId.Value].Remove(action);
            }

            if (action.MapType.HasValue && _actionsByMapType.ContainsKey(action.MapType.Value))
            {
                eval = eval && _actionsByMapType[action.MapType.Value].Remove(action);
            }

            return eval;
        }

        public override void Update(GameTime gameTime)
        { /** NOOP **/}

        private void OnMumbleMapChanged(object _, ValueEventArgs<int> _1)
        {
            CurrentMapId = _context.CurrentMap.Id;
            CurrentMapType = _context.CurrentMap.Type;
        }

        private void OnMapIdChanged(int oldValue, int newValue)
        {
            if (_actionsByMapId.ContainsKey(oldValue))
            {
                UpdateActions(_actionsByMapId[oldValue]);
            }

            if (_actionsByMapId.ContainsKey(newValue))
            {
                UpdateActions(_actionsByMapId[newValue]);
            }
        }

        private void OnMapTypeChanged(Gw2Sharp.Models.MapType oldValue, Gw2Sharp.Models.MapType newValue)
        {
            if (_actionsByMapType.ContainsKey((MapType)oldValue))
            {
                UpdateActions(_actionsByMapType[(MapType)oldValue]);
            }

            if (_actionsByMapType.ContainsKey((MapType)newValue))
            {
                UpdateActions(_actionsByMapType[(MapType)newValue]);
            }
        }

        private void UpdateActions(IEnumerable<MapAction> actions)
        {
            foreach (MapAction action in actions)
            {
                UpdateAction(action);
            }
        }

        private void UpdateAction(MapAction action)
        {
            if (action.FreezeUpdates) // save calculations, if action is not updating
            {
                return;
            }

            action.IsFulfilled = (!action.MapId.HasValue || action.MapId.Value == CurrentMapId)
                && (!action.MapType.HasValue || action.MapType.Value == (MapType)CurrentMapType);
        }

        protected override void Cleanup()
        {
            _actionsByMapId.Clear();
            _actionsByMapType.Clear();

            if (_context != null)
            {
                _context.CurrentMap.MapChanged -= OnMumbleMapChanged;
            }
            base.Cleanup();
        }
    }
}
