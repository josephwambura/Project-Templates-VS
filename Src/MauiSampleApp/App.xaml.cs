using Microsoft.Extensions.DependencyInjection;

namespace MauiSampleApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SessionService _sessionService;
        private readonly IEnterpriseSyncEngine _syncEngine;

        public App(IServiceProvider serviceProvider, SessionService sessionService, IEnterpriseSyncEngine syncEngine)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _sessionService = sessionService;
            _syncEngine = syncEngine;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Evaluate session states safely on fresh cold-start boot cycles
            bool isUserLoggedIn = _sessionService.CurrentUserId.HasValue ||
                                 Preferences.Default.Get("IsUserLoggedIn", false);

            Page rootPage;

            if (isUserLoggedIn)
            {
                // Instantiate your workspace shell
                rootPage = _serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                // Instantiate your standalone standalone sign-in page
                rootPage = _serviceProvider.GetRequiredService<SignInPage>();
            }

            var window = new Window(rootPage);

            window.Activated += (s, e) => _syncEngine.InitializeMonitoring();
            window.Deactivated += (s, e) => _syncEngine.ShutdownMonitoring();

            return window;
        }
    }
}