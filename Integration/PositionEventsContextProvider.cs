using Blish_HUD;
//using Flyga.PositionEventsModule.Contexts;

namespace Flyga.AdditionalAchievements.Integration
{
    public class PositionEventsContextProvider : ModuleContextProvider2
    {
        public const string POSITION_EVENTS_MODULE_NAMESPACE = "Flyga.PositionEvents";
        public const string POSITION_EVENTS_CONTEXT_NAME = "Flyga.PositionEventsModule.Contexts.PositionEventsContext";

        public PositionEventsContextProvider(ModuleService moduleService) : base(moduleService, POSITION_EVENTS_MODULE_NAMESPACE, POSITION_EVENTS_CONTEXT_NAME)
        {
            /** NOOP **/
        }
    }
}
