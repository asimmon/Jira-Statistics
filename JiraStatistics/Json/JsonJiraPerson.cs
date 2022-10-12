using Newtonsoft.Json;

namespace JiraStatistics.Json;

public class JsonJiraPerson
{
    [JsonProperty("emailAddress")]
    public string EmailAddress { get; set; }
}