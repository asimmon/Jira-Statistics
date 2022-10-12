using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Name} - {ReleaseDate}")]
public class JsonJiraFixVersion
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; set; }

    [JsonPropertyName("released")]
    public bool Released { get; set; }
}