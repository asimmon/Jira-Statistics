using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JiraStatistics
{
    [DebuggerDisplay("{Id} - {Key} - {Changes.Count}")]
    public class JiraIssue
    {
        public JiraIssue()
        {
            this.Changes = new List<JiraIssueChange>();
            this.Components = new List<JiraComponent>();
            this.Sprints = new List<string>();
            this.CurrentStatus = null;
        }

        public int Id { get; set; }

        public string Key { get; set; }

        public string ParentKey { get; set; }

        public JiraIssue Parent { get; set; }

        public string Title { get; set; }

        public string Creator { get; set; }

        public string Priority { get; set; }

        public string EpicLink { get; set; }

        public string Magnitude { get; set; }

        public string Severity { get; set; }

        public string Complexity { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> ResolutionDate { get; set; }

        public Nullable<DateTime> Started { get; set; }

        public Nullable<DateTime> Finished { get; set; }

        public Nullable<DateTime> ReleaseDate
        {
            get => this.FixVersion?.ReleaseDate;
        }

        public Nullable<double> DaysEstimated { get; set; }

        public Nullable<int> CycleTime { get; set; }

        public Nullable<int> LeadTime { get; set; }

        public JiraIssueType IssueType { get; set; }

        public List<string> Sprints { get; }

        public JiraFixVersion FixVersion { get; set; }

        public List<JiraIssueChange> Changes { get; }

        public List<JiraComponent> Components { get; }

        public JiraStatus CurrentStatus { get; set; }

        public JiraIssueStatistics Statistics { get; set; }

        public override string ToString()
        {
            return $"{this.Id} - {this.Key} - {this.Changes.Count}";
        }
    }
}