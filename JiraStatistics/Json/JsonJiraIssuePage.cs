using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraIssuePage : JsonPage<JsonJiraIssue>
    {
        [JsonProperty("issues")]
        public override List<JsonJiraIssue> Values { get; set; }
    }
}