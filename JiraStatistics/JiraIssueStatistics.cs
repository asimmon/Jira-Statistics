namespace JiraStatistics;

public class JiraIssueStatistics
{
    private readonly SortedDictionary<JiraStatusCategory, TimeSpan> _categoryDurations;
    private readonly SortedDictionary<string, TimeSpan> _personDurations;

    public JiraIssueStatistics()
    {
        this._categoryDurations = new SortedDictionary<JiraStatusCategory, TimeSpan>();
        this._personDurations = new SortedDictionary<string, TimeSpan>();
    }

    public IReadOnlyDictionary<JiraStatusCategory, TimeSpan> CategoryDurations => this._categoryDurations;

    public IReadOnlyDictionary<string, TimeSpan> PersonDurations => this._personDurations;

    public void AddCategoryDuration(JiraStatusCategory category, TimeSpan duration)
    {
        var totalDuration = this._categoryDurations.TryGetValue(category, out var tmpDuration) ? tmpDuration : TimeSpan.Zero;
        this._categoryDurations[category] = totalDuration + duration;
    }

    public void AddPersonDuration(string person, TimeSpan duration)
    {
        if (!string.IsNullOrWhiteSpace(person))
        {
            var totalDuration = this._personDurations.TryGetValue(person, out var tmpDuration) ? tmpDuration : TimeSpan.Zero;
            this._personDurations[person] = totalDuration + duration;
        }
    }
}