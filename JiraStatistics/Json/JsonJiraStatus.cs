using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [DebuggerDisplay("{Id} - {Name}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraStatus
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}