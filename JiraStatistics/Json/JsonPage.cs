using Newtonsoft.Json;

namespace JiraStatistics.Json;

public abstract class JsonPage<T>
{
    [JsonProperty("startAt")]
    public int StartAt { get; set; }

    [JsonProperty("maxResults")]
    public int MaxResults { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("isLast")]
    public bool IsLast { get; set; }

    public abstract List<T> Values { get; set; }
}