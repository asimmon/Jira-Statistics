using System.Text.Json.Serialization;

namespace JiraStatistics.Json;

public class JsonJiraPerson
{
    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; }
}