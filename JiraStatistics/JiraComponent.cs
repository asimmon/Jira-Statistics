using System.Globalization;
using JiraStatistics.Json;

namespace JiraStatistics
{
    public class JiraComponent
    {
        public JiraComponent(JsonJiraComponent json)
        {
            this.Id = int.Parse(json.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
            this.Name = json.Name;
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}