using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Field} - {From} - {To}")]
public class JsonJiraIssueChangelogEventItem
{
    [JsonProperty("field")]
    public string Field { get; set; }

    [JsonProperty("from")]
    public string From { get; set; }

    [JsonProperty("to")]
    public string To { get; set; }
}