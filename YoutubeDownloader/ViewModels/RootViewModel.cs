using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels;

public class RootViewModel : Screen
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public SnackbarMessageQueue Notifications { get; } = new(TimeSpan.FromSeconds(5));

    public DashboardViewModel Dashboard { get; }

    public RootViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService
        )
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;

        Dashboard = _viewModelFactory.CreateDashboardViewModel();

        DisplayName = $"{App.Name} v{App.VersionString}";
    }

    public async void OnViewFullyLoaded()
    {
    }

    protected override void OnViewLoaded()
    {
        base.OnViewLoaded();

        _settingsService.Load();

        // Sync the theme with settings
        if (_settingsService.IsDarkModeEnabled)
        {
            App.SetDarkTheme();
        }
        else
        {
            App.SetLightTheme();
        }

        // App has just been updated, display the changelog
        if (_settingsService.LastAppVersion is not null && _settingsService.LastAppVersion != App.Version)
        {
            Notifications.Enqueue(
                $"Successfully updated to {App.Name} v{App.VersionString}",
                "CHANGELOG", () => ProcessEx.StartShellExecute(App.ChangelogUrl)
            );

            _settingsService.LastAppVersion = App.Version;
            _settingsService.Save();
        }
    }

    protected override void OnClose()
    {
        base.OnClose();

        Dashboard.CancelAllDownloads();

        _settingsService.Save();
    }
}