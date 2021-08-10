using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JiraStatistics.Json;
using Newtonsoft.Json;

namespace JiraStatistics
{
    public static class JiraConnector
    {
        private static readonly Uri BaseApiUrl = new Uri("https://gsoftdev.atlassian.net", UriKind.Absolute);
        private static readonly HttpClient Client = new HttpClient();

        private static readonly Regex FixVersionNameRegex = new Regex(
            @"^((\d+\.)(\d+\.)?(\d+\.)?(\*|\d+)|MASTER|DEVELOP)$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        private static readonly HashSet<string> SearchIssueFields = new HashSet<string>
        {
            "key", "creator", "created", "status", "issuetype", "fixVersions", "resolutiondate", "parent", "summary", "components", "priority",
            "timetracking", "customfield_10006", "customfield_10007", "customfield_12826", "customfield_12825", "customfield_12827"
        };

        private static readonly string SearchFields = string.Join(",", SearchIssueFields);

        static JiraConnector()
        {
            Client.BaseAddress = BaseApiUrl;
        }

        private static async Task<string> GetStringAsync(string urlStr, JiraOptions opts)
        {
            var encodedUsernamePassword = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{opts.JiraUsername}:{opts.JiraAccessToken}"));
            
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = HttpMethod.Get;
            httpRequestMessage.RequestUri = new Uri(urlStr, UriKind.Relative);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedUsernamePassword);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                using (var response = await Client.SendAsync(httpRequestMessage))
                {
                    response.EnsureSuccessStatusCode();

                    using (var responseContent = response.Content)
                    {
                        return await responseContent.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new JiraNetworkException(urlStr, ex);
            }
        }

        public static async Task<List<JsonJiraFixVersion>> GetVersionsAsync(JiraOptions opts)
        {
            var jsonStr = await GetStringAsync($"/rest/api/2/project/{opts.JiraProjectKey}/versions", opts);
            var jsonVersions = JsonConvert.DeserializeObject<List<JsonJiraFixVersion>>(jsonStr);
            var numericJsonVersions = jsonVersions.Where(IsNumericFixVersion).ToList();

            opts.Logger($"Fetched {numericJsonVersions.Count} fix versions for project {opts.JiraProjectKey}");
            
            return numericJsonVersions;
        }

        private static bool IsNumericFixVersion(JsonJiraFixVersion fixVersion)
        {
            return FixVersionNameRegex.IsMatch(fixVersion.Name);
        }

        public static async IAsyncEnumerable<JsonJiraIssue> GetIssuesAsync(JiraOptions opts)
        {
            var urlFormat = $"/rest/api/2/search?jql={Uri.EscapeDataString(opts.JqlQuery)}&fields={SearchFields}&startAt={{0}}";
            var startAt = 0;
            var totalFetched = 0;
            var totalYielded = 0;
            var totalYieldedPaddingLength = 0;
            
            JsonJiraIssuePage jsonIssuePage;
            do
            {
                var jsonStr = await GetStringAsync(string.Format(urlFormat, startAt), opts);
                jsonIssuePage = JsonConvert.DeserializeObject<JsonJiraIssuePage>(jsonStr);
                startAt += jsonIssuePage.MaxResults;
                totalFetched += jsonIssuePage.Values.Count;

                if (totalYielded == 0)
                {
                    totalYieldedPaddingLength = jsonIssuePage.Total.ToString().Length;
                }

                foreach (var issue in jsonIssuePage.Values)
                {
                    issue.Changelog = await GetChangelog(issue.Key, opts);
                    yield return issue;

                    totalYielded++;
                    var totalYieldedPadded = totalYielded.ToString().PadLeft(totalYieldedPaddingLength, '0');
                    opts.Logger($"Fetched issue {totalYieldedPadded}/{jsonIssuePage.Total} named {issue.Key}");
                }
            } while (!jsonIssuePage.IsLast && totalFetched < jsonIssuePage.Total);
        }

        private static async Task<List<JsonJiraIssueChangelogEvent>> GetChangelog(string issueKey, JiraOptions opts)
        {
            var urlFormat = $"/rest/api/2/issue/{issueKey}/changelog?startAt={{0}}";
            var events = new List<JsonJiraIssueChangelogEvent>();
            var startAt = 0;
            var totalFetched = 0;

            JsonJiraIssueChangelogPage changelogPage;
            do
            {
                var jsonStr = await GetStringAsync(string.Format(urlFormat, startAt), opts);
                changelogPage = JsonConvert.DeserializeObject<JsonJiraIssueChangelogPage>(jsonStr);
                events.AddRange(changelogPage.Values);
                startAt += changelogPage.MaxResults;
                totalFetched += changelogPage.Values.Count;
            } while (!changelogPage.IsLast && totalFetched < changelogPage.Total);

            return events.Where(IsStatusOrFlaggedChangelogItem).OrderBy(e => e.Created).ToList();
        }

        private static bool IsStatusOrFlaggedChangelogItem(JsonJiraIssueChangelogEvent changelogEvent)
        {
            return changelogEvent.Items.Any(i =>
            {
                if ("status".Equals(i.Field, StringComparison.OrdinalIgnoreCase)) return true;
                if ("flagged".Equals(i.Field, StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            });
        }

        public static async Task<List<JsonJiraStatus>> GetStatusesAsync(JiraOptions opts)
        {
            var jsonStr = await GetStringAsync("/rest/api/2/status", opts);
            var jsonVersions = JsonConvert.DeserializeObject<List<JsonJiraStatus>>(jsonStr);

            opts.Logger($"Fetched {jsonVersions.Count} statuses");
            
            return jsonVersions;
        }

        [Serializable]
        private class JiraNetworkException : Exception
        {
            public JiraNetworkException(string url, Exception innerException)
                : base($"A network error occurred on {url}", innerException)
            {
            }
        }
    }
}