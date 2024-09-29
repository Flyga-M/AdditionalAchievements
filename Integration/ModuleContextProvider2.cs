using Blish_HUD;
using Blish_HUD.Contexts;
using Blish_HUD.Modules;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Integration
{
    public class ModuleContextProvider2 : IModuleContextProvider
    {
        private IntegrationState _state;

        private string _namespace;
        private ModuleService _service;

        private ModuleManager _manager;
        private Context _context;
        protected readonly string _contextTypeName;

        /// <inheritdoc/>
        public string Namespace => _namespace;

        /// <inheritdoc/>
        public ModuleManager Manager => _manager;

        /// <inheritdoc/>
        public event EventHandler<IntegrationState> StateChanged;

        private void OnStateChange(IntegrationState state)
        {
            StateChanged?.Invoke(this, state);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        public ModuleContextProvider2(ModuleService moduleService, string moduleNamspace, string contextTypeName)
        {
            _service = moduleService;
            _namespace = moduleNamspace;
            _contextTypeName = contextTypeName;

            bool isInstalled = false;

            foreach (ModuleManager manager in GameService.Module.Modules)
            {
                if (IsCorrectModuleManager(manager))
                {
                    _manager = manager;
                    PrepareManager(manager);

                    isInstalled = true;

                    break;
                }
            }

            if (!isInstalled)
            {
                State = IntegrationState.DependencyMissing;
            }

            _service.ModuleRegistered += OnModuleRegistered;
            _service.ModuleUnregistered += OnModuleUnregistered;
        }

        private bool IsCorrectModuleManager(ModuleManager manager)
        {
            return manager.Manifest.Namespace == _namespace;
        }

        private void PrepareManager(ModuleManager manager)
        {
            manager.ModuleEnabled += OnModuleEnabled;
            manager.ModuleDisabled += OnModuleDisabled;
            manager.ModuleLoaded += OnModuleLoaded;

            if (manager.ModuleInstance?.Loaded == true)
            {
                OnModuleLoaded(manager, EventArgs.Empty);
            }
            else
            {
                State = IntegrationState.DependencyDisabled;
            }
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

        private void OnModuleLoaded(object _, EventArgs _1)
        {
            RetrieveContext();
        }

        private void OnModuleDisabled(object _, EventArgs _1)
        {
            State = IntegrationState.DependencyDisabled;
        }

        private void OnModuleEnabled(object _, EventArgs _1)
        {
            if (_manager.ModuleInstance == null)
            {
                return; // TODO: maybe throw here or at least log
            }

            _manager.ModuleInstance.ModuleException += OnModuleException;
        }

        private void OnModuleException(object _, UnobservedTaskExceptionEventArgs loadError)
        {
            if (!loadError.Observed)
            {
                State = IntegrationState.DependencyDisabled;
            }
        }

        private void OnModuleRegistered(object sender, ValueEventArgs<ModuleManager> manager)
        {
            if (IsCorrectModuleManager(manager.Value))
            {
                _manager = manager.Value;
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

        public void Dispose()
        {
            // just in case anything is still accessing the State, after the context provider
            // was disposed
            State = IntegrationState.None;

            if (_context != null)
            {
                _context.StateChanged -= OnContextStateChange;
            }

            if (_manager != null)
            {
                _manager.ModuleEnabled -= OnModuleEnabled;
                _manager.ModuleDisabled -= OnModuleDisabled;
                _manager.ModuleLoaded -= OnModuleLoaded;

                if (_manager.ModuleInstance != null)
                {
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
