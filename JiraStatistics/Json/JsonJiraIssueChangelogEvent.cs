using Newtonsoft.Json;

namespace JiraStatistics.Json;

public class JsonJiraIssueChangelogEvent
{
    [JsonProperty("author")]
    public JsonJiraPerson Author { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; }

    [JsonProperty("items")]
    public List<JsonJiraIssueChangelogEventItem> Items { get; set; }
}