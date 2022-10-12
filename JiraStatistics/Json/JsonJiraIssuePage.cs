using Newtonsoft.Json;

namespace JiraStatistics.Json;

public class JsonJiraIssuePage : JsonPage<JsonJiraIssue>
{
    [JsonProperty("issues")]
    public override List<JsonJiraIssue> Values { get; set; }
}