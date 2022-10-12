using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public abstract class JsonPage<T>
{
    [JsonPropertyName("startAt")]
    public int StartAt { get; set; }

    [JsonPropertyName("maxResults")]
    public int MaxResults { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    public abstract List<T> Values { get; set; }
}