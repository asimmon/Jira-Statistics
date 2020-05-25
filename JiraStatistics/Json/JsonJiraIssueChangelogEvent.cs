using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraIssueChangelogEvent
    {
        [JsonProperty("author")]
        public JsonJiraPerson Author { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("items")]
        public List<JsonJiraIssueChangelogEventItem> Items { get; set; }
    }
}