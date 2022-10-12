using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraIssueFields
{
    [JsonPropertyName("parent")]
    public JsonBaseJiraIssue Parent { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("resolutiondate")]
    public string ResolutionDate { get; set; }

    [JsonPropertyName("creator")]
    public JsonJiraPerson Creator { get; set; }

    [JsonPropertyName("status")]
    public JsonJiraStatus Status { get; set; }

    [JsonPropertyName("issuetype")]
    public JsonJiraIssueType IssueType { get; set; }

    [JsonPropertyName("priority")]
    public JsonJiraPriority Priority { get; set; }

    [JsonPropertyName("fixVersions")]
    public List<JsonJiraFixVersion> FixVersions { get; set; }

    [JsonPropertyName("components")]
    public List<JsonJiraComponent> Components { get; set; }

    [JsonPropertyName("timetracking")]
    public JsonJiraTimeTracking TimeTracking { get; set; }

    [JsonPropertyName("customfield_10007")]
    public string EpicLink { get; set; }

    [JsonPropertyName("customfield_10006")]
    public JsonJiraSprint[] Sprint { get; set; }

    [JsonPropertyName("customfield_12826")]
    public JsonJiraFieldValue Magnitude { get; set; }

    [JsonPropertyName("customfield_12825")]
    public JsonJiraFieldValue Severity { get; set; }

    [JsonPropertyName("customfield_12827")]
    public JsonJiraFieldValue Complexity { get; set; }
}

public class JsonJiraSprint
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class JsonJiraComponent
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class JsonJiraIssueType
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class JsonJiraPriority
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class JsonJiraFieldValue
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}