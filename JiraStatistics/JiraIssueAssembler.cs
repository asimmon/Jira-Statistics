using System.Globalization;
using System.Text.RegularExpressions;
using JiraStatistics.Json;

namespace JiraStatistics;

public class JiraIssueAssembler
{
    private static readonly Regex SprintRegex = new Regex(
        @"\[((?<key>[^=\[\],]+)=(?<value>[^=\[\],]*),?)+\]",
        RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    private readonly JiraComponentCollection _components;
    private readonly JiraIssue _issue;
    private readonly JiraIssueTypeCollection _issueTypes;

    private readonly JiraOptions _options;
    private readonly JsonJiraIssue _jsonIssue;
    private readonly JiraStatusCollection _statuses;
    private readonly JiraFixVersionCollection _versions;

    private JiraIssueAssembler(JiraOptions options, JsonJiraIssue jsonIssue, JiraStatusCollection statuses, JiraFixVersionCollection versions)
    {
        this._options = options;
        this._jsonIssue = jsonIssue;
        this._statuses = statuses;
        this._versions = versions;
        this._components = new JiraComponentCollection();
        this._issueTypes = new JiraIssueTypeCollection();
        this._issue = new JiraIssue();
    }

    public static JiraIssue Assemble(JiraOptions options, JsonJiraIssue jsonIssue, JiraStatusCollection statuses, JiraFixVersionCollection versions)
    {
        return new JiraIssueAssembler(options, jsonIssue, statuses, versions).Assemble();
    }

    private JiraIssue Assemble()
    {
        this._issue.Id = int.Parse(this._jsonIssue.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
        this._issue.Key = this._jsonIssue.Key;
        this._issue.Title = this._jsonIssue.Fields.Summary;
        this._issue.Created = DateTime.Parse(this._jsonIssue.Fields.Created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        this._issue.Creator = this._jsonIssue.Fields.Creator.EmailAddress;
        this._issue.EpicLink = this._jsonIssue.Fields.EpicLink;
        this._issue.Priority = string.IsNullOrWhiteSpace(this._jsonIssue.Fields.Priority?.Name) ? string.Empty : this._jsonIssue.Fields.Priority.Name;

        if (this._jsonIssue.Fields.Parent != null)
            this._issue.ParentKey = this._jsonIssue.Fields.Parent.Key;

        if (this._jsonIssue.Fields.ResolutionDate != null)
            this._issue.ResolutionDate = DateTime.Parse(this._jsonIssue.Fields.ResolutionDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

        this._issue.IssueType = this._issueTypes.GetOrAdd(this._jsonIssue.Fields.IssueType);
        this._issue.Magnitude = string.IsNullOrWhiteSpace(this._jsonIssue.Fields.Magnitude?.Value) ? string.Empty : this._jsonIssue.Fields.Magnitude.Value;
        this._issue.Severity = string.IsNullOrWhiteSpace(this._jsonIssue.Fields.Severity?.Value) ? string.Empty : this._jsonIssue.Fields.Severity.Value;
        this._issue.Complexity = string.IsNullOrWhiteSpace(this._jsonIssue.Fields.Complexity?.Value) ? string.Empty : this._jsonIssue.Fields.Complexity.Value;

        // sprints
        var jsonSprints = this._jsonIssue.Fields.Sprint ?? Enumerable.Empty<JsonJiraSprint>();
        foreach (var jsonSprint in jsonSprints)
        {
            var sprintMatch = SprintRegex.Match(jsonSprint.Name);
            if (sprintMatch.Success)
            {
                var keys = sprintMatch.Groups["key"].Captures.Select(c => c.Value);
                var values = sprintMatch.Groups["value"].Captures.Select(c => c.Value);
                var kvps = keys.Zip(values, (key, val) => (key, val)).ToDictionary(tuple => tuple.key, tuple => tuple.val);
                if (kvps.TryGetValue("name", out var sprintName) && sprintName.Length > 0) this._issue.Sprints.Add(sprintName);
            }
        }

        // status
        var currentStatusId = int.Parse(this._jsonIssue.Fields.Status.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (this._statuses.TryGetById(currentStatusId, out var currentStatus)) this._issue.CurrentStatus = currentStatus;

        // fix versions
        var fixVersions = new List<JiraFixVersion>();
        foreach (var jsonVersion in this._jsonIssue.Fields.FixVersions)
        {
            var versionId = int.Parse(jsonVersion.Id, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (this._versions.TryGetById(versionId, out JiraFixVersion version)) fixVersions.Add(version);
        }

        this._issue.FixVersion = fixVersions
            .OrderBy(v => v.ReleaseDate.HasValue ? 0 : 1)
            .ThenBy(v => v.ReleaseDate)
            .ThenBy(v => v.Id)
            .FirstOrDefault();

        // days estimated
        if (this._jsonIssue.Fields.TimeTracking?.OriginalEstimateSeconds != null)
        {
            // stored in seconds, jira consider 8 hours per day
            var hours = this._jsonIssue.Fields.TimeTracking.OriginalEstimateSeconds / 3600;
            this._issue.DaysEstimated = hours / 8d;
        }

        // components
        foreach (var jsonComponent in this._jsonIssue.Fields.Components)
        {
            var component = this._components.GetOrAdd(jsonComponent);
            this._issue.Components.Add(component);
        }

        // changes
        this._issue.Changes.AddRange(this.GetSnapshots());

        this._issue.Started = this.ComputeStarted();
        this._issue.Finished = this.ComputeFinished();
        this._issue.LeadTime = this.ComputeLeadTime();
        this._issue.CycleTime = this.ComputeCycleTime();

        this._issue.Statistics = this.ComputeStatistics();

        return this._issue;
    }

    private DateTime? ComputeStarted()
    {
        var firstInProgressChange = this._issue.Changes.Where(c => c.RealCategory == JiraStatusCategory.InProgress).OrderBy(c => c.Date).FirstOrDefault();
        return firstInProgressChange?.Date ?? this._issue.Created;
    }

    private DateTime? ComputeFinished()
    {
        // not yet finished
        if (this._issue.CurrentStatus == null || this._issue.CurrentStatus.Category != JiraStatusCategory.Done)
            return null;

        JiraIssueChange doneChange = null;

        for (var i = this._issue.Changes.Count - 1; i >= 0; i--)
        {
            if (doneChange == null)
            {
                if (this._issue.Changes[i].RealCategory == JiraStatusCategory.Done)
                    doneChange = this._issue.Changes[i];
            }
            else
            {
                if (this._issue.Changes[i].RealCategory == JiraStatusCategory.Done)
                    doneChange = this._issue.Changes[i];
                else
                    break;
            }
        }

        if (doneChange == null)
            return this._issue.ResolutionDate;

        if (this._issue.ResolutionDate.HasValue == false)
            return doneChange.Date;

        var gap = doneChange.Date - this._issue.ResolutionDate.Value;
        if (gap.Duration() > TimeSpan.FromMinutes(5))
        {
            // can happen only if Jira's resolution date is out of sync
            return this._issue.ResolutionDate;
        }

        return this._issue.ResolutionDate != null
            ? doneChange.Date < this._issue.ResolutionDate.Value ? doneChange.Date : this._issue.ResolutionDate.Value
            : doneChange.Date;
    }

    private int? ComputeLeadTime()
    {
        var finishedDate = this._issue.Finished?.Date;
        var startedDate = this._issue.Started?.Date;

        if (startedDate.HasValue && finishedDate.HasValue)
        {
            if (finishedDate.Value >= startedDate.Value)
                return DateTimeUtils.ComputeLeadOrCycleTime(startedDate.Value, finishedDate.Value, this._options.IsWorkDayPredicate);
        }

        return null;
    }

    private int? ComputeCycleTime()
    {
        var releaseDate = this._issue.ReleaseDate?.Date;
        var createdDate = this._issue.Created.Date;

        if (this._issue.CurrentStatus?.Category == JiraStatusCategory.Done && releaseDate.HasValue)
        {
            if (releaseDate.Value >= createdDate)
                return DateTimeUtils.ComputeLeadOrCycleTime(createdDate, releaseDate.Value, this._options.IsWorkDayPredicate);
        }

        return null;
    }

    private IEnumerable<JiraIssueChange> GetSnapshots()
    {
        var snapshot = this.MakeFirstTransition();
        yield return snapshot;

        foreach (var changelogEvent in this._jsonIssue.Changelog)
        {
            snapshot = snapshot.Clone();
            snapshot.Author = changelogEvent.Author.EmailAddress;
            snapshot.Date = DateTime.Parse(changelogEvent.Created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

            foreach (var eventItem in changelogEvent.Items) snapshot = this.PopulateSnapshot(snapshot, eventItem);

            yield return snapshot;
        }

        snapshot = snapshot.Clone();
        snapshot.Date = DateTime.Now;
        yield return snapshot;
    }

    private JiraIssueChange PopulateSnapshot(JiraIssueChange change, JsonJiraIssueChangelogEventItem eventItem)
    {
        if ("status".Equals(eventItem.Field, StringComparison.OrdinalIgnoreCase))
        {
            var toId = int.Parse(eventItem.To, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (this._statuses.TryGetById(toId, out var toStatus)) change.Status = toStatus;
        }
        else if ("flagged".Equals(eventItem.Field, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(eventItem.From) && !string.IsNullOrWhiteSpace(eventItem.To))
                change.IsFlagged = true;
            else if (!string.IsNullOrWhiteSpace(eventItem.From) && string.IsNullOrWhiteSpace(eventItem.To)) change.IsFlagged = false;
        }

        return change;
    }

    private JiraIssueChange MakeFirstTransition()
    {
        return new JiraIssueChange
        {
            Date = this._issue.Created,
            Author = this._issue.Creator,
            Timestamp = this._issue.Created,
            Status = this.GetFirstStatus(),
            IsFlagged = false
        };
    }

    private JiraStatus GetFirstStatus()
    {
        var firstTransitionStatus = this._jsonIssue.Changelog
            .SelectMany(changelog => changelog.Items)
            .Select(eventItem =>
            {
                return "status".Equals(eventItem.Field, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(eventItem.From, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fromId) && this._statuses.TryGetById(fromId, out var fromStatus)
                        ? fromStatus
                        : null;
            })
            .FirstOrDefault(status => status != null);
        return firstTransitionStatus ?? this._issue.CurrentStatus;
    }



    private JiraIssueStatistics ComputeStatistics()
    {
        var splitInterval = TimeSpan.FromHours(1);
        var accumulator = new JiraIssueStatistics();

        return this._issue.Changes
            .SplitOverTime(splitInterval)
            .Lag(this.IsRelevantChange)
            .Aggregate(accumulator, AccumulateJiraStatistics);
    }

    private bool IsRelevantChange(JiraIssueChange prevChange, JiraIssueChange nextChange)
    {
        return prevChange.HasChanged(nextChange) || this._options.IsWorkDayPredicate(nextChange.Date);
    }

    private static JiraIssueStatistics AccumulateJiraStatistics(JiraIssueStatistics accumulator, ILag<JiraIssueChange> pair)
    {
        var (prevChange, nextChange) = pair;

        var changeCategory = prevChange.RealCategory;
        var actualDuration = nextChange.Timestamp - prevChange.Timestamp;

        accumulator.AddCategoryDuration(changeCategory, actualDuration);

        if (changeCategory == JiraStatusCategory.InProgress) accumulator.AddPersonDuration(prevChange.Author, actualDuration);

        return accumulator;
    }
}