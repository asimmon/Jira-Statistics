using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraIssueChangelogPage : JsonPage<JsonJiraIssueChangelogEvent>
{
    [JsonPropertyName("values")]
    public override List<JsonJiraIssueChangelogEvent> Values { get; set; }
}