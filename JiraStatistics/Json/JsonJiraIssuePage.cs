using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraIssuePage : JsonPage<JsonJiraIssue>
{
    [JsonPropertyName("issues")]
    public override List<JsonJiraIssue> Values { get; set; }
}