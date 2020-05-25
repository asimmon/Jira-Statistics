using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JiraStatistics.Json;

namespace JiraStatistics
{
    public class JiraStatusCollection : IReadOnlyCollection<JiraStatus>
    {
        private readonly IDictionary<int, JiraStatus> _statuses;

        public JiraStatusCollection(IEnumerable<JsonJiraStatus> statuses)
            : this(statuses.Select(s => new JiraStatus(s)))
        {
        }

        public JiraStatusCollection(IEnumerable<JiraStatus> statuses)
        {
            this._statuses = statuses.ToDictionary(s => s.Id);
        }

        public int Count => this._statuses.Count;

        public IEnumerator<JiraStatus> GetEnumerator()
        {
            return this._statuses.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetById(int id, out JiraStatus status)
        {
            return this._statuses.TryGetValue(id, out status);
        }
    }
}