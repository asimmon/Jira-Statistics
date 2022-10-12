using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json;

[DebuggerDisplay("{Id} - {Name}")]
public class JsonJiraStatus
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}