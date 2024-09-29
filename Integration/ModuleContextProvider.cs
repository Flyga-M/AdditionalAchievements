using Blish_HUD;
using Blish_HUD.Contexts;
using Blish_HUD.Modules;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Integration
{
    [Obsolete("This class is obsolete. Use ModuleContextProvider2 instead.", true)]
    public abstract class ModuleContextProvider : IModuleContextProvider
    {
        private IntegrationState _state;

        private string _namespace;
        private ModuleService _service;

        private ModuleManager _manager;
        private Context _context;
        protected readonly string _contextTypeName;

        public string Namespace => _namespace;

        public ModuleManager Manager => _manager;

        public event EventHandler<IntegrationState> StateChanged;

        private void OnStateChange(IntegrationState state)
        {
            StateChanged?.Invoke(this, state);
        }

        /// <summary>
        /// The <see cref="IntegrationState"/> of the <see cref="ModuleContextProvider"/>.
        /// </summary>
        public IntegrationState State
        {
            get => _state;
            set
            {
                IntegrationState previousState = _state;

                _state = value;

                if (previousState != value)
                {
                    OnStateChange(value);
                }
            }
        }

        /// <summary>
        /// The context, that the <see cref="ModuleContextProvider"/> provides. Will be null, if 
        /// the <see cref="State"/> != <see cref="IntegrationState.Working"/>.
        /// </summary>
        public Context Context
        {
            get
            {
                // only return context, if it's valid and useable
                if (State == IntegrationState.Working)
                {
                    return _context;
                }

                return null;
            }
        }

        private void OnModuleLoaded(object _, EventArgs _1)
        {
            OnAssemblyLoaded(_manager);
        }

        private void OnModuleException(object _, UnobservedTaskExceptionEventArgs loadError)
        {
            if (!loadError.Observed)
            {
                State = IntegrationState.DependencyDisabled;
            }
        }

        private void OnAssemblyLoaded(ModuleManager manager)
        {
            // save a reference to the ModuleManager for later use
            _manager = manager;

            if (!manager.ModuleInstance.Loaded)
            {
                // if the module is not loaded yet, come back when it is
                manager.ModuleInstance.ModuleLoaded += OnModuleLoaded;
                manager.ModuleInstance.ModuleException += OnModuleException;
                return;
            }
            else
            {
                manager.ModuleInstance.ModuleLoaded -= OnModuleLoaded;
            }

            _manager.ModuleDisabled += OnModuleDisabled;

            // Retrieve the context, once you're sure the Module has been loaded
            RetrieveContext();
        }

        private void OnModuleEnabled(object sender, EventArgs e)
        {
            if (!(sender is ModuleManager moduleManager))
            {
                throw new ArgumentException("OnModuleEnabled must be called " +
                    "by a ModuleManager.", nameof(sender));
            }

            if (moduleManager.Manifest.Namespace != _namespace)
            {
                throw new ArgumentException("OnModuleEnabled must be called " +
                    $"by the ModuleManager of the {_namespace} " +
                    "module.", nameof(sender));
            }

            if (!moduleManager.AssemblyLoaded)
            {
                throw new InvalidOperationException("OnModuleEnabled must be called " +
                    $"after the modules {_namespace} assembly was loaded.");
            }

            OnAssemblyLoaded(moduleManager);
        }

        private void OnModuleDisabled(object sender, EventArgs e)
        {
            if (!(sender is ModuleManager moduleManager))
            {
                throw new ArgumentException("OnModuleEnabled must be called " +
                   "by a ModuleManager.", nameof(sender));
            }

            if (moduleManager.Manifest.Namespace != _namespace)
            {
                throw new ArgumentException("OnModuleEnabled must be called " +
                    $"by the ModuleManager of the {_namespace} " +
                    "module.", nameof(sender));
            }

            State = IntegrationState.DependencyDisabled;
        }

        protected virtual void RetrieveContext()
        {
            _context = GameService.Contexts.GetContext(_contextTypeName);

            if (_context == null)
            {
                State = IntegrationState.DependencyMissing;
                return;
            }

            if (_context.State == ContextState.Ready)
            {
                State = IntegrationState.Working;
            }

            _context.StateChanged += OnContextStateChange;
        }

        private void OnContextStateChange(object _, EventArgs _1)
        {
            if (_context.State == ContextState.Ready)
            {
                State = IntegrationState.Working;
                return;
            }

            if (_context.State == ContextState.Expired)
            {
                State = IntegrationState.DependencyDisabled;
            }
        }

        private void OnModuleRegistered(object sender, ValueEventArgs<ModuleManager> manager)
        {
            if (IsCorrectModuleManager(manager.Value))
            {
                PrepareManager(manager.Value);
            }
        }

        private void OnModuleUnregistered(object sender, ValueEventArgs<ModuleManager> manager)
        {
            if (IsCorrectModuleManager(manager.Value))
            {
                State = IntegrationState.DependencyDisabled;
            }
        }

        private bool IsCorrectModuleManager(ModuleManager manager)
        {
            return manager.Manifest.Namespace == _namespace;
        }

        private void PrepareManager(ModuleManager manager)
        {
            // if the assembly is already loaded, call OnAssemblyLoaded manually
            if (manager.AssemblyLoaded)
            {
                OnAssemblyLoaded(manager);
            }

            // make sure to retrieve the context only after the
            // Module was enabled (and therefor the
            // assembly was loaded)
            manager.ModuleEnabled += OnModuleEnabled;
        }

        public ModuleContextProvider(ModuleService moduleService, string moduleNamspace, string contextTypeName)
        {
            _service = moduleService;
            _namespace = moduleNamspace;
            _contextTypeName = contextTypeName;

            bool isInstalled = false;

            foreach (ModuleManager manager in GameService.Module.Modules)
            {
                if (IsCorrectModuleManager(manager))
                {
                    PrepareManager(manager);

                    isInstalled = true;

                    break;
                }
            }

            if (!isInstalled)
            {
                State = IntegrationState.DependencyMissing;
            }

            moduleService.ModuleRegistered += OnModuleRegistered;
            moduleService.ModuleUnregistered += OnModuleUnregistered;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_context != null)
            {
                _context.StateChanged -= OnContextStateChange;
            }

            if (_manager != null)
            {
                _manager.ModuleDisabled -= OnModuleDisabled;
                _manager.ModuleEnabled -= OnModuleEnabled;

                if (_manager.ModuleInstance != null)
                {
                    _manager.ModuleInstance.ModuleLoaded -= OnModuleLoaded;
                    _manager.ModuleInstance.ModuleException -= OnModuleException;
                }
            }

            if (_service != null)
            {
                _service.ModuleRegistered -= OnModuleRegistered;
                _service.ModuleUnregistered -= OnModuleUnregistered;
            }
        }
    }
}
