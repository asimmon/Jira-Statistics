using System.Diagnostics;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [DebuggerDisplay("{Id} - {Key}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonBaseJiraIssue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}