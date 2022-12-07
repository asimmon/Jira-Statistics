using System.Diagnostics;
using System.Globalization;
using JiraStatistics.Json;

namespace JiraStatistics;

[DebuggerDisplay("{Id} - {Name} - {Category}")]
public class JiraStatus
{
    public JiraStatus(JsonJiraStatus json)
    {
        this.Id = int.Parse(json.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
        this.Name = json.Name;
        this.Category = GuessCategoryFromName(json.Name);
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public JiraStatusCategory Category { get; set; }

    private static JiraStatusCategory GuessCategoryFromName(string name)
    {
        name = name.ToLowerInvariant();

        return name switch
        {
            "open" => JiraStatusCategory.New,
            "todo" => JiraStatusCategory.New,
            "to do" => JiraStatusCategory.New,
            "backlog" => JiraStatusCategory.New,
            "approved" => JiraStatusCategory.New,
            "ready to transfer" => JiraStatusCategory.New,

            "done" => JiraStatusCategory.Done,
            "rejected" => JiraStatusCategory.Done,
            "beta" => JiraStatusCategory.Done,
            "closed" => JiraStatusCategory.Done,

            "blocked" => JiraStatusCategory.OnHold,
            "dev done" => JiraStatusCategory.OnHold,
            "merge back" => JiraStatusCategory.OnHold,
            _ when name.StartsWith("ready to ", StringComparison.Ordinal) => JiraStatusCategory.OnHold,

            _ => JiraStatusCategory.InProgress
        };
    }
}