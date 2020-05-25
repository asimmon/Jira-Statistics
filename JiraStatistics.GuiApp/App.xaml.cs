using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JiraStatistics.GuiApp.ViewModels;
using JiraStatistics.GuiApp.Views;

namespace JiraStatistics.GuiApp
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                var openFolderService = new OpenFolderService(mainWindow);
                var persistedOptionsService = new PersistedOptionsService();
                mainWindow.DataContext = new MainWindowViewModel(openFolderService, persistedOptionsService);
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}