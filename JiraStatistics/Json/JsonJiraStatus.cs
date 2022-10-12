using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Name}")]
public class JsonJiraStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}