using System;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraTimeTracking
    {
        [JsonProperty("originalEstimateSeconds")]
        public Nullable<int> OriginalEstimateSeconds { get; set; }
    }
}