using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraIssueChangelogPage : JsonPage<JsonJiraIssueChangelogEvent>
    {
        [JsonProperty("values")]
        public override List<JsonJiraIssueChangelogEvent> Values { get; set; }
    }
}