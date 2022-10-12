using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Name} - {ReleaseDate}")]
public class JsonJiraFixVersion
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("releaseDate")]
    public string ReleaseDate { get; set; }

    [JsonProperty("released")]
    public bool Released { get; set; }
}