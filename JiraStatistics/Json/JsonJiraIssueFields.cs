using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraIssueFields
    {
        [JsonProperty("parent")]
        public JsonBaseJiraIssue Parent { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("resolutiondate")]
        public string ResolutionDate { get; set; }

        [JsonProperty("creator")]
        public JsonJiraPerson Creator { get; set; }

        [JsonProperty("status")]
        public JsonJiraStatus Status { get; set; }

        [JsonProperty("issuetype")]
        public JsonJiraIssueType IssueType { get; set; }

        [JsonProperty("priority")]
        public JsonJiraPriority Priority { get; set; }

        [JsonProperty("fixVersions")]
        public List<JsonJiraFixVersion> FixVersions { get; set; }

        [JsonProperty("components")]
        public List<JsonJiraComponent> Components { get; set; }

        [JsonProperty("timetracking")]
        public JsonJiraTimeTracking TimeTracking { get; set; }

        [JsonProperty("customfield_10007")]
        public string EpicLink { get; set; }

        [JsonProperty("customfield_10006")]
        public JsonJiraSprint[] Sprint { get; set; }

        [JsonProperty("customfield_12826")]
        public JsonJiraFieldValue Magnitude { get; set; }

        [JsonProperty("customfield_12825")]
        public JsonJiraFieldValue Severity { get; set; }

        [JsonProperty("customfield_12827")]
        public JsonJiraFieldValue Complexity { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraSprint
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraComponent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraIssueType
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraPriority
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraFieldValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}