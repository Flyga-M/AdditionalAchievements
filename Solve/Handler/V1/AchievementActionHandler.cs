using AchievementLib.Pack.V1.Models;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1
{
    public class AchievementActionHandler : ActionHandler<AchievementAction>
    {
        // does not need to do anything, since the AchievementActions will be handled by the
        // AchievementLib library.
        // is still implemented so no errors occur, when attempting to add AchievementActions 
        // to the AchievementHandler

        public override void Update(GameTime gameTime)
        {
            /** NOOP **/
        }
    }
}
