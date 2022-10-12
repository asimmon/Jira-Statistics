using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Key} - {Fields.FixVersions.Count}")]
public class JsonJiraIssue : JsonBaseJiraIssue
{
    [JsonProperty("fields")]
    public JsonJiraIssueFields Fields { get; set; }

    [JsonIgnore]
    public List<JsonJiraIssueChangelogEvent> Changelog { get; set; }
}