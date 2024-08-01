using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using Flyga.AdditionalAchievements.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Status.Provider
{
    public class PositionEventsModuleStatusProvider : ModuleDependencyStatusProvider
    {
        public override string Title => "Position Events Module";

        public PositionEventsModuleStatusProvider(ModuleService moduleService) : base(new PositionEventsContextProvider(moduleService))
        { /** NOOP **/ }
    }
}
