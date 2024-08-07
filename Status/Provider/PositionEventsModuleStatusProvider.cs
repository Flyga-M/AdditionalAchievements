using Blish_HUD;
using Flyga.AdditionalAchievements.Integration;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class PositionEventsModuleStatusProvider : ModuleDependencyStatusProvider
    {
        public override string Title => "Position Events Module";

        public PositionEventsModuleStatusProvider(ModuleService moduleService) : base(new PositionEventsContextProvider(moduleService))
        { /** NOOP **/ }
    }
}
