using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Key}")]
public class JsonBaseJiraIssue
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }
}