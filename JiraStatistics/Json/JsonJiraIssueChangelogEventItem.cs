using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Field} - {From} - {To}")]
public class JsonJiraIssueChangelogEventItem
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("to")]
    public string To { get; set; }
}