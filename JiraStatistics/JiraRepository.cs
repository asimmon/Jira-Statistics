using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraStatistics
{
    public static class JiraRepository
    {
        public static async Task<IEnumerable<JiraIssue>> GetIssuesAsync(JiraOptions opts)
        {
            ValidateOptions(opts);

            var awaitableJsonIssues = JiraConnector.GetIssuesAsync(opts);
            opts.ThrowIfCancellationRequested();
            
            var jsonStatuses = await JiraConnector.GetStatusesAsync(opts);
            opts.ThrowIfCancellationRequested();

            var jsonVersions = await JiraConnector.GetVersionsAsync(opts);
            opts.ThrowIfCancellationRequested();

            var statuses = new JiraStatusCollection(jsonStatuses.Select(j => new JiraStatus(j)));
            var versions = new JiraFixVersionCollection(jsonVersions.Select(j => new JiraFixVersion(j)));
            var issues = new JiraIssueCollection();

            if (opts.MaxResults.HasValue && opts.MaxResults > 0)
            {
                awaitableJsonIssues = awaitableJsonIssues.Take(opts.MaxResults.Value);
            }

            await foreach (var issue in awaitableJsonIssues)
            {
                opts.ThrowIfCancellationRequested();
                issues.Add(JiraIssueAssembler.Assemble(opts, issue, statuses, versions));
            }

            return issues;
        }

        private static void ValidateOptions(JiraOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.JqlQuery))
                throw new ArgumentNullException(nameof(options.JqlQuery));

            if (string.IsNullOrWhiteSpace(options.OutputFilePath))
                throw new ArgumentNullException(nameof(options.OutputFilePath));

            if (options.Logger == null)
                options.Logger = SinkLogger;

            if (options.IsWorkDayPredicate == null)
                options.IsWorkDayPredicate = IsAlwaysWorkDay;
        }

        private static void SinkLogger(string str)
        {
        }

        private static bool IsAlwaysWorkDay(DateTime date)
        {
            return true;
        }
    }
}