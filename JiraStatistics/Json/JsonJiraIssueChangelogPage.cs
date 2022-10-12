using Newtonsoft.Json;

namespace JiraStatistics.Json;

public class JsonJiraIssueChangelogPage : JsonPage<JsonJiraIssueChangelogEvent>
{
    [JsonProperty("values")]
    public override List<JsonJiraIssueChangelogEvent> Values { get; set; }
}