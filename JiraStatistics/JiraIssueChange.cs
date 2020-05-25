using System;
using System.Diagnostics;

namespace JiraStatistics
{
    [DebuggerDisplay("{Date} - {Author} - {Status.Category} - {IsFlagged}")]
    public class JiraIssueChange : ITimestampeable, ICloneable<JiraIssueChange>
    {
        public DateTime Date { get; set; }

        public JiraStatus Status { get; set; }

        public bool IsFlagged { get; set; }

        public string Author { get; set; }

        public JiraStatusCategory RealCategory =>
            this.Status.Category == JiraStatusCategory.Done
                ? JiraStatusCategory.Done
                : this.IsFlagged ? JiraStatusCategory.OnHold : this.Status.Category;

        public JiraIssueChange Clone()
        {
            return new JiraIssueChange
            {
                Date = this.Date,
                Status = this.Status,
                IsFlagged = this.IsFlagged,
                Author = this.Author
            };
        }

        public DateTime Timestamp
        {
            get => this.Date;
            set => this.Date = value;
        }

        public bool HasChanged(JiraIssueChange other)
        {
            var thisCategory = this.RealCategory;
            var otherCategory = other.RealCategory;

            // if still done, no matter if anything else changed, done is done
            if (thisCategory == JiraStatusCategory.Done && otherCategory == JiraStatusCategory.Done) return false;

            // otherwise, category or author change are relevant
            if (!Equals(thisCategory, otherCategory)) return true;
            if (!Equals(this.Author, other.Author)) return true;

            return false;
        }
    }
}