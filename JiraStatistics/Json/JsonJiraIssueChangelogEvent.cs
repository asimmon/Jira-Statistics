using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraIssueChangelogEvent
{
    [JsonPropertyName("author")]
    public JsonJiraPerson Author { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("items")]
    public List<JsonJiraIssueChangelogEventItem> Items { get; set; }
}