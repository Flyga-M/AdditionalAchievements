using Blish_HUD.Contexts;
using Blish_HUD.Modules;
using System;

namespace Flyga.AdditionalAchievements.Integration
{
    public interface IModuleContextProvider : IDisposable
    {
        /// <summary>
        /// The namespace of the module that provides the context. Must match 
        /// the namespace in the module manifest.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// The <see cref="ModuleManager"/> of the module that provides the context. 
        /// Might be <see langword="null"/>, if the module has not been registered (yet).
        /// </summary>
        ModuleManager Manager { get; }

        /// <summary>
        /// Fires, when the <see cref="State"/> changes.
        /// </summary>
        event EventHandler<IntegrationState> StateChanged;

        /// <summary>
        /// The <see cref="IntegrationState"/> of the <see cref="IModuleContextProvider"/>.
        /// </summary>
        IntegrationState State { get; }

        /// <summary>
        /// The context, that the <see cref="IModuleContextProvider"/> provides. Might be
        /// <see langword="null"/>, if the 
        /// <see cref="State"/> != <see cref="IntegrationState.Working"/>.
        /// </summary>
        Context Context { get; }
    }
}
