using System;
using System.Globalization;
using JiraStatistics.Json;

namespace JiraStatistics
{
    public class JiraFixVersion
    {
        public JiraFixVersion(JsonJiraFixVersion json)
        {
            this.Id = int.Parse(json.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
            this.Name = json.Name;
            if (json.Released && !string.IsNullOrWhiteSpace(json.ReleaseDate))
            {
                this.ReleaseDate = DateTime.ParseExact(json.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Nullable<DateTime> ReleaseDate { get; set; }
    }
}