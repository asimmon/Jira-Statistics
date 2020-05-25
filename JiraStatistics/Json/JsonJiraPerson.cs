using Newtonsoft.Json;

namespace JiraStatistics.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonJiraPerson
    {
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
    }
}