using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Key} - {Fields.FixVersions.Count}")]
public class JsonJiraIssue : JsonBaseJiraIssue
{
    [JsonPropertyName("fields")]
    public JsonJiraIssueFields Fields { get; set; }

    [JsonIgnore]
    public List<JsonJiraIssueChangelogEvent> Changelog { get; set; }
}