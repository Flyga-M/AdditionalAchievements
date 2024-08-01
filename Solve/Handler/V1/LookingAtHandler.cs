using System;
using AchievementLib.Pack.V1.Models;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1
{
    public class LookingAtHandler : ActionHandler<LookingAtAction>
    {
        private static readonly Logger Logger = Logger.GetLogger<LookingAtHandler>();

        private readonly Gw2MumbleService _context;

        private void UpdateState()
        {
            if (State == HandlerState.Fatal)
            {
                return;
            }
            
            if (_context == null)
            {
                State = HandlerState.Fatal;
                return;
            }
            if (_context.IsAvailable)
            {
                State = HandlerState.Working;
                return;
            }

            State = HandlerState.Suspended;
        }

        public LookingAtHandler(Gw2MumbleService mumbleService)
        {
            _context = mumbleService ?? throw new ArgumentNullException(nameof(mumbleService));
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            UpdateState();

            if (State != HandlerState.Working)
            {
                return;
            }
            
            foreach(LookingAtAction action in _actions)
            {
                UpdateAction(action);
            }
        }

        private void UpdateAction(LookingAtAction action)
        {
            if (action.FreezeUpdates) // save calculations, if action is not updating
            {
                return;
            }

            if (action.MapId != _context.CurrentMap.Id)
            {
                return;
            }

            Vector3 targetDirection = action.Target.Value - _context.PlayerCamera.Position;

            float cosineSimilarity = AchievementLib.CosineSimilarityUtil.CosineSimilarity(_context.PlayerCamera.Forward, targetDirection);

            action.IsFulfilled = cosineSimilarity >= action.CosineSimilarityTolerance;
        }
    }
}
