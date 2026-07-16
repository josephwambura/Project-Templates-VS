using CommunityToolkit.Maui;

using Microsoft.Extensions.Logging;

using Plugin.Fingerprint;

using Syncfusion.Maui.Toolkit.Hosting;

namespace MauiSampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});

    				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Controls.CategoryChart), (handler, view) =>
    				{
    					if (view is Pages.Controls.CategoryChart && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
    					{
    						contentPanel.IsTabStop = true;
    					}
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            // Register IFingerprint as a singleton so it can be injected into ViewModels.
            builder.Services.AddSingleton<IFingerprint>(CrossFingerprint.Current);

            builder.Services.AddSingleton<SessionService>();

            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(Constants.DatabasePath);
            });

            builder.Services.AddSingleton<ProjectRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<TagRepository>();
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<UserDeviceRepository>();
            builder.Services.AddSingleton<SeedDataService>();

            builder.Services.AddSingleton<IErrorHandler, ModalErrorHandler>();
            builder.Services.AddSingleton<ISecurityService, SecurityService>();

            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProjectListPageModel>();
            builder.Services.AddSingleton<ManageMetaPageModel>();

            builder.Services.AddSingleton<SignInPageModel>();
            builder.Services.AddSingleton<SignUpPageModel>();
            builder.Services.AddSingleton<UserDeviceListPageModel>();

            builder.Services.AddTransient<AppShell>();

            builder.Services.AddSingleton<SignInPage>();
            builder.Services.AddSingleton<SignUpPage>();

            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            builder.Services.AddHttpClient<IEnterpriseSyncEngine, EnterpriseSyncEngine>(client =>
            {
                client.BaseAddress = new Uri(Constants.BackendBaseAddress);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return builder.Build();
        }
    }
}
