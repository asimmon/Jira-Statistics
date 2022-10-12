using System.Collections;
using JiraStatistics.Json;

namespace JiraStatistics;

public class JiraIssueTypeCollection : IReadOnlyCollection<JiraIssueType>
{
    private readonly IDictionary<string, JiraIssueType> _issuesTypes;

    public JiraIssueTypeCollection()
    {
        this._issuesTypes = new Dictionary<string, JiraIssueType>();
    }

    public int Count => this._issuesTypes.Count;

    public IEnumerator<JiraIssueType> GetEnumerator()
    {
        return this._issuesTypes.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public JiraIssueType GetOrAdd(JsonJiraIssueType issueType)
    {
        return this._issuesTypes.TryGetValue(issueType.Id, out var existingIssueType)
            ? existingIssueType
            : this._issuesTypes[issueType.Id] = new JiraIssueType(issueType);
    }
}