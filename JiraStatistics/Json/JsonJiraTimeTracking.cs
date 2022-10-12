using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraTimeTracking
{
    [JsonPropertyName("originalEstimateSeconds")]
    public int? OriginalEstimateSeconds { get; set; }
}