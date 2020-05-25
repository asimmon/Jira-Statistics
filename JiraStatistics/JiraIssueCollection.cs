using System.Collections;
using System.Collections.Generic;

namespace JiraStatistics
{
    internal class JiraIssueCollection : IEnumerable<JiraIssue>
    {
        private readonly Dictionary<string, JiraIssue> _issues;
        private bool _hierarchyMade;

        public JiraIssueCollection()
        {
            this._issues = new Dictionary<string, JiraIssue>();
        }

        public IEnumerator<JiraIssue> GetEnumerator()
        {
            this.MakeHierarchy();
            return this._issues.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(JiraIssue issue)
        {
            this._issues[issue.Key] = issue;
            if (this._hierarchyMade) this.MakeHierarchy(issue);
        }

        private void MakeHierarchy()
        {
            if (!this._hierarchyMade)
            {
                foreach (var issue in this._issues.Values) this.MakeHierarchy(issue);

                this._hierarchyMade = true;
            }
        }

        private void MakeHierarchy(JiraIssue issue)
        {
            if (!string.IsNullOrWhiteSpace(issue.ParentKey) && this._issues.TryGetValue(issue.ParentKey, out var parent))
                issue.Parent = parent;
            else
                issue.ParentKey = null;
        }
    }
}