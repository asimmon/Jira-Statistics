using System.Reactive;
using System.Text;
using Nager.Date;
using ReactiveUI;

namespace JiraStatistics.GuiApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string DefaultTitle = "Jira Statistics";
    private const string ExportingTitle = "Exporting...";

    private readonly OpenFolderService _openFolderService;
    private readonly PersistedOptionsService _persistedOptionsService;
    private readonly StringBuilder _logsBuilder;

    private string _title;
    private string _jqlQuery;
    private string _outputDirectoryPath;
    private string _jiraUsername;
    private string _jiraAccessToken;
    private string _jiraProjectKey;
    private bool _isExporting;
    private string _logs;

    private CancellationTokenSource _tokenSource;

    public MainWindowViewModel(OpenFolderService openFolderService, PersistedOptionsService persistedOptionsService)
    {
        this._openFolderService = openFolderService;
        this._persistedOptionsService = persistedOptionsService;
        this._logsBuilder = new StringBuilder();
        this._tokenSource = new CancellationTokenSource();

        this.LoadDefaultProperties();
            
        this.ExportCommand = ReactiveCommand.Create(this.Export, this.CreateCanExportObservable());
        this.CancelCommand = ReactiveCommand.Create(this.Cancel, this.CreateCanCancelObservable());
        this.SelectDirectoryCommand = ReactiveCommand.Create(this.SelectDirectory, this.CreateCanSelectDirectoryObservable());

        _ = this.LoadOptionsAsync();
    }

    public string Title
    {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public string JqlQuery
    {
        get => this._jqlQuery;
        set => this.RaiseAndSetIfChanged(ref this._jqlQuery, value);
    }

    public string OutputDirectoryPath
    {
        get => this._outputDirectoryPath;
        set => this.RaiseAndSetIfChanged(ref this._outputDirectoryPath, value);
    }

    public string JiraUsername
    {
        get => this._jiraUsername;
        set => this.RaiseAndSetIfChanged(ref this._jiraUsername, value);
    }

    public string JiraAccessToken
    {
        get => this._jiraAccessToken;
        set => this.RaiseAndSetIfChanged(ref this._jiraAccessToken, value);
    }

    public string JiraProjectKey
    {
        get => this._jiraProjectKey;
        set => this.RaiseAndSetIfChanged(ref this._jiraProjectKey, value);
    }

    public string Logs
    {
        get => this._logs;
        set => this.RaiseAndSetIfChanged(ref this._logs, value);
    }

    public bool IsExporting
    {
        get => this._isExporting;
        set => this.RaiseAndSetIfChanged(ref this._isExporting, value);
    }

    public ReactiveCommand<Unit, Unit> ExportCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        
    public ReactiveCommand<Unit, Unit> SelectDirectoryCommand { get; }

    private void Cancel()
    {
        this._tokenSource.Cancel();
    }

    private IObservable<bool> CreateCanCancelObservable()
    {
        return this.WhenAnyValue(
            self => self.IsExporting,
            isExporting => isExporting == true);
    }

    private void Export()
    {
        _ = this.ExportAsync();
    }

    private async Task ExportAsync()
    {
        if (this.IsExporting)
        {
            return;
        }

        this._tokenSource.Dispose();
        this._tokenSource = new CancellationTokenSource();

        try
        {
            await this.SaveOptionsAsync();
                
            this.IsExporting = true;
            this.Title = ExportingTitle;
            this.WriteLogSeparator();
                
            await JiraIssuesWriter.Write(this.CreateOptions());
        }
        catch (OperationCanceledException)
        {
            this.WriteLogLine("The operation was canceled");
        }
        catch (Exception ex)
        {
            this.WriteLogLine(ex.ToString());
        }
        finally
        {
            this.Title = DefaultTitle;
            this.IsExporting = false;
        }
    }

    private JiraOptions CreateOptions()
    {
        return new JiraOptions
        {
            OutputFilePath = Path.Combine(this.OutputDirectoryPath, $"jira-{DateTime.Now:HHmmss}.xlsx"),
            JqlQuery = this.JqlQuery,
            JiraUsername = this.JiraUsername,
            JiraAccessToken = this.JiraAccessToken,
            JiraProjectKey = this.JiraProjectKey,
            IsWorkDayPredicate = IsWorkDay,
            Logger = this.WriteLogLine,
            CancellationToken = this._tokenSource.Token
        };
    }

    private IObservable<bool> CreateCanExportObservable()
    {
        return this.WhenAnyValue(
            self => self.IsExporting,
            self => self.JqlQuery,
            self => self.JiraUsername,
            self => self.JiraAccessToken,
            self => self.JiraProjectKey,
            (isExporting, jqlQuery, jiraUsername, jiraAccessToken, jiraProjectKey) =>
            {
                return !isExporting &&
                    jqlQuery.Trim().Length > 0 &&
                    jiraUsername.Trim().Length > 0 &&
                    jiraAccessToken.Trim().Length > 0 &&
                    jiraProjectKey.Trim().Length > 0;
            });
    }

    private void SelectDirectory()
    {
        _ = this.SelectDirectoryAsync();
    }

    private async Task SelectDirectoryAsync()
    {
        var newOutputDirectory = await this._openFolderService.SelectDirectory(this.OutputDirectoryPath);
            
        if (!string.IsNullOrWhiteSpace(newOutputDirectory))
        {
            this.OutputDirectoryPath = newOutputDirectory;
        }
    }

    private IObservable<bool> CreateCanSelectDirectoryObservable()
    {
        return this.WhenAnyValue(
            self => self.IsExporting,
            isExporting => isExporting == false);
    }

    private static bool IsWorkDay(DateTime date) => date switch
    {
        var d when DateSystem.IsWeekend(d, CountryCode.CA) => false,
        var d when DateSystem.IsPublicHoliday(d, CountryCode.CA) => false,
        _ => true
    };

    private void WriteLogLine(string text)
    {
        this._logsBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss:ff}] {text}");
        this.Logs = this._logsBuilder.ToString();
    }

    private void WriteLogSeparator()
    {
        if (this.Logs.Length > 0)
        {
            this.WriteLogLine(string.Empty);
        }
    }

    private void LoadDefaultProperties()
    {
        this.Title = DefaultTitle;
        this.IsExporting = false;
        this.Logs = string.Empty;
            
        this.JqlQuery = string.Empty;
        this.JiraUsername = string.Empty;
        this.JiraAccessToken = string.Empty;
        this.JiraProjectKey = string.Empty;
        this.OutputDirectoryPath = string.Empty;
    }

    private async Task LoadOptionsAsync()
    {
        try
        {
            var persistedOptions = await this._persistedOptionsService.LoadAsync();

            this.JqlQuery = persistedOptions.JqlQuery;
            this.JiraUsername = persistedOptions.JiraUsername;
            this.JiraAccessToken = persistedOptions.JiraAccessToken;
            this.JiraProjectKey = persistedOptions.JiraProjectKey;
            this.OutputDirectoryPath = persistedOptions.OutputDirectoryPath;
        }
        catch (PersistedOptionsNotFoundException)
        {
            this.LoadDefaultProperties();
        }
        catch (Exception ex)
        {
            this.LoadDefaultProperties();
            this.WriteLogLine(ex.ToString());
        }
    }

    private Task SaveOptionsAsync()
    {
        try
        {
            var persistedOptions = new PersistedJiraOptions
            {
                JqlQuery = this.JqlQuery,
                JiraUsername = this.JiraUsername,
                JiraAccessToken = this.JiraAccessToken,
                JiraProjectKey = this.JiraProjectKey,
                OutputDirectoryPath = this.OutputDirectoryPath
            };
                
            return this._persistedOptionsService.SaveAsync(persistedOptions);
        }
        catch (Exception ex)
        {
            this.WriteLogLine(ex.ToString());
            return Task.CompletedTask;
        }
    }
}