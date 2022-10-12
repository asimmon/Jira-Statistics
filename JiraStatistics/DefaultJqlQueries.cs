namespace JiraStatistics;

public static class DefaultJqlQueries
{
    private static readonly HashSet<string> SearchProject = new HashSet<string>
    {
        "SGD"
    };

    private static readonly HashSet<string> SearchIssueTypes = new HashSet<string>
    {
        "Bug", "Dev Bug", "Customer bug", "FastTrack Bug", "Story", "Improvement", "Analysis"
    };

    public static readonly string ShareGateDesktopJql = string.Format(
        "project IN ({0})",
        string.Join(",", SearchProject.Select(WrapWithQuotes)),
        string.Join(",", SearchIssueTypes.Select(WrapWithQuotes)));

    private static readonly HashSet<string> IssueKeys = new HashSet<string>
    {
        "SGD-525"
    };

    public static readonly string SpecificIssuesJql = $"issuekey IN ({string.Join(",", IssueKeys.Select(WrapWithQuotes))})";

    private static string WrapWithQuotes(string text)
    {
        return $"\"{text}\"";
    }
}