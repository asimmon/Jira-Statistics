using System.Collections;
using JiraStatistics.Json;

namespace JiraStatistics;

public class JiraComponentCollection : IReadOnlyCollection<JiraComponent>
{
    private readonly IDictionary<string, JiraComponent> _components;

    public JiraComponentCollection()
    {
        this._components = new Dictionary<string, JiraComponent>();
    }

    public int Count => this._components.Count;

    public IEnumerator<JiraComponent> GetEnumerator()
    {
        return this._components.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public JiraComponent GetOrAdd(JsonJiraComponent component)
    {
        return this._components.TryGetValue(component.Id, out var exisingComponent)
            ? exisingComponent
            : this._components[component.Id] = new JiraComponent(component);
    }
}