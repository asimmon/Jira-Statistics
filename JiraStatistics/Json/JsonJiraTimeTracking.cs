using Newtonsoft.Json;

namespace JiraStatistics.Json;

public class JsonJiraTimeTracking
{
    [JsonProperty("originalEstimateSeconds")]
    public int? OriginalEstimateSeconds { get; set; }
}