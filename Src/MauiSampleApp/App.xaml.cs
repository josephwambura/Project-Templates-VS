using Microsoft.Extensions.DependencyInjection;

namespace MauiSampleApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SessionService _sessionService;
        private readonly IEnterpriseSyncEngine _syncEngine;
        private Window? _mainWindow;

        public App(IServiceProvider serviceProvider, SessionService sessionService, IEnterpriseSyncEngine syncEngine)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _sessionService = sessionService;
            _syncEngine = syncEngine;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            bool isUserLoggedIn = Preferences.Default.Get("IsUserLoggedIn", false);

            Page rootPage;

            if (isUserLoggedIn)
            {
                if (!_sessionService.CurrentUserId.HasValue)
                {
                    string savedUserId = Preferences.Default.Get("CurrentUserId", string.Empty);
                    if (Guid.TryParse(savedUserId, out Guid userId))
                    {
                        _sessionService.CurrentUserId = userId;
                    }
                }

                rootPage = _serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                rootPage = _serviceProvider.GetRequiredService<SignInPage>();
            }

            _mainWindow = new Window(rootPage);

            _mainWindow.Activated += OnWindowActivated;
            _mainWindow.Deactivated += OnWindowDeactivated;
            _mainWindow.Destroying += OnWindowDestroying;

            return _mainWindow;
        }

        private void OnWindowActivated(object? sender, EventArgs e)
        {
            _syncEngine.InitializeMonitoring();
        }

        private void OnWindowDeactivated(object? sender, EventArgs e)
        {
            _syncEngine.ShutdownMonitoring();
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            // Clean up event handlers explicitly when the window is closed
            if (_mainWindow != null)
            {
                _mainWindow.Activated -= OnWindowActivated;
                _mainWindow.Deactivated -= OnWindowDeactivated;
                _mainWindow.Destroying -= OnWindowDestroying;
            }
        }
    }
}