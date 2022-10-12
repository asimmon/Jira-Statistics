using Newtonsoft.Json;

namespace JiraStatistics.GuiApp;

[JsonObject(MemberSerialization.OptIn)]
public class PersistedJiraOptions
{
    [JsonProperty("jqlQuery")]
    public string JqlQuery { get; set; }

    [JsonProperty("jiraUsername")]
    public string JiraUsername { get; set; }

    [JsonProperty("jiraAccessToken")]
    public string JiraAccessToken { get; set; }
        
    [JsonProperty("jiraProjectKey")]
    public string JiraProjectKey { get; set; }
        
    [JsonProperty("outputDirectoryPath")]
    public string OutputDirectoryPath { get; set; }
}