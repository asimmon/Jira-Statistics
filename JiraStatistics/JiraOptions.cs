namespace JiraStatistics;

public class JiraOptions
{
    public string OutputFilePath { get; set; }

    public string JqlQuery { get; set; }
        
    public string JiraUsername { get; set; }
        
    public string JiraAccessToken { get; set; }

    public string JiraProjectKey { get; set; }

    public int? MaxResults { get; set; }

    public Func<DateTime, bool> IsWorkDayPredicate { get; set; }

    public Action<string> Logger { get; set; }

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public void ThrowIfCancellationRequested()
    {
        this.CancellationToken.ThrowIfCancellationRequested();
    }
}