using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JiraStatistics.Json;

namespace JiraStatistics
{
    public class JiraFixVersionCollection : IReadOnlyCollection<JiraFixVersion>
    {
        private readonly IDictionary<int, JiraFixVersion> _versions;

        public JiraFixVersionCollection(IEnumerable<JsonJiraFixVersion> versions)
            : this(versions.Select(v => new JiraFixVersion(v)))
        {
        }

        public JiraFixVersionCollection(IEnumerable<JiraFixVersion> versions)
        {
            this._versions = versions.ToDictionary(v => v.Id);
        }

        public int Count => this._versions.Count;

        public IEnumerator<JiraFixVersion> GetEnumerator()
        {
            return this._versions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetById(int id, out JiraFixVersion fixVersion)
        {
            return this._versions.TryGetValue(id, out fixVersion);
        }
    }
}