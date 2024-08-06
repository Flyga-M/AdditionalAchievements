using System;
using AchievementLib.Pack.V1.Models;
using Blish_HUD;
using Flyga.AdditionalAchievements.Status.Models;
using Flyga.AdditionalAchievements.Status.Provider;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Mumble
{
    public class LookingAtHandler : MumbleHandler<LookingAtAction>
    {
        private static readonly Logger Logger = Logger.GetLogger<LookingAtHandler>();

        public LookingAtHandler(MumbleStatusProvider mumbleStatusProvider) : base (mumbleStatusProvider)
        { /** NOOP **/}

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
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
