using System;
using System.Collections.Generic;
using System.Globalization;
using JiraStatistics.Json;
using Nager.Date;
using Xunit;

namespace JiraStatistics.UnitTests
{
    public class JiraIssueTests
    {
        private static readonly JsonJiraStatus Open = MakeStatus("1", "open");
        private static readonly JsonJiraStatus Todo = MakeStatus("2", "todo");
        private static readonly JsonJiraStatus Work = MakeStatus("3", "work");
        private static readonly JsonJiraStatus Done = MakeStatus("4", "done");
        private static readonly JsonJiraStatus Rejected = MakeStatus("5", "rejected");

        private static readonly JiraStatusCollection AllStatuses = new JiraStatusCollection(new[]
        {
            Open, Todo, Work, Done, Rejected
        });

        private static readonly JsonJiraPerson John = MakePerson("john");
        private static readonly JsonJiraPerson Jane = MakePerson("jane");
        private static readonly JsonJiraPerson Mike = MakePerson("mike");

        private static readonly JsonJiraFixVersion Release1 = MakeFixVersion("3", "3", "2020-01-20");

        private static readonly JiraFixVersionCollection AllFixVersions = new JiraFixVersionCollection(new[]
        {
            Release1
        });

        private static JsonJiraStatus MakeStatus(string id, string name)
        {
            return new JsonJiraStatus
            {
                Id = id,
                Name = name
            };
        }

        private static JsonJiraPerson MakePerson(string name)
        {
            return new JsonJiraPerson
            {
                EmailAddress = name
            };
        }

        private static JsonJiraFixVersion MakeFixVersion(string id, string name, string releaseDate)
        {
            return new JsonJiraFixVersion
            {
                Id = id,
                Name = name,
                Released = true,
                ReleaseDate = releaseDate
            };
        }

        private static JsonJiraIssueChangelogEvent MakeIssueChange(JsonJiraPerson author, string date, JsonJiraStatus from, JsonJiraStatus to)
        {
            return new JsonJiraIssueChangelogEvent
            {
                Author = author,
                Created = date,
                Items = new List<JsonJiraIssueChangelogEventItem>
                {
                    new JsonJiraIssueChangelogEventItem
                    {
                        Field = "status",
                        From = from.Id,
                        To = to.Id
                    }
                }
            };
        }

        private static JsonJiraIssueChangelogEvent MakeIssueChange(JsonJiraPerson author, string date, bool from, bool to)
        {
            return new JsonJiraIssueChangelogEvent
            {
                Author = author,
                Created = date,
                Items = new List<JsonJiraIssueChangelogEventItem>
                {
                    new JsonJiraIssueChangelogEventItem
                    {
                        Field = "flagged",
                        From = from ? "true" : "",
                        To = to ? "true" : ""
                    }
                }
            };
        }

        private static DateTime ParseDateTime(string date)
        {
            return DateTime.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }

        [Fact]
        public void Statistics()
        {
            var jsonIssue = new JsonJiraIssue
            {
                Id = "1",
                Key = "JIRA-1",
                Fields = new JsonJiraIssueFields
                {
                    Created = "2020-01-01T12:00:00.000-0500",
                    // 1, 2, 3, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17, 20
                    ResolutionDate = "2020-01-15T12:00:01.746-0500",
                    Creator = John,
                    Status = Done,
                    FixVersions = new List<JsonJiraFixVersion>
                    {
                        Release1
                    },
                    Components = new List<JsonJiraComponent>(),
                    Sprint = new string[0],
                    IssueType = new JsonJiraIssueType
                    {
                        Id = "1",
                        Name = "Story"
                    }
                },
                Changelog = new List<JsonJiraIssueChangelogEvent>
                {
                    MakeIssueChange(John, "2020-01-02T12:00:00.000-0500", Open, Todo),
                    MakeIssueChange(John, "2020-01-03T12:00:00.000-0500", Todo, Work),
                    MakeIssueChange(Mike, "2020-01-07T12:00:00.000-0500", false, true), // john +
                    MakeIssueChange(Jane, "2020-01-08T12:00:00.000-0500", true, false),
                    MakeIssueChange(Mike, "2020-01-10T12:00:00.000-0500", Work, Rejected),
                    MakeIssueChange(Mike, "2020-01-13T12:00:00.000-0500", Rejected, Todo),
                    MakeIssueChange(John, "2020-01-14T12:00:00.000-0500", Todo, Work),
                    MakeIssueChange(John, "2020-01-15T12:00:00.000-0500", Work, Done),
                }
            };

            var opts = new JiraOptions
            {
                IsWorkDayPredicate = date => false == DateSystem.IsWeekend(date, CountryCode.CA)
            };

            var issue = JiraIssueAssembler.Assemble(opts, jsonIssue, AllStatuses, AllFixVersions);

            Assert.Equal(ParseDateTime("2020-01-01T12:00:00.000-0500"), issue.Created);
            Assert.Equal(ParseDateTime("2020-01-15T12:00:00.000-0500"), issue.Finished);

            Assert.Equal(14, issue.CycleTime);

            var statistics = issue.Statistics;

            Assert.Equal(TimeSpan.FromDays(3), statistics.CategoryDurations[JiraStatusCategory.New]);
            Assert.Equal(TimeSpan.FromDays(5), statistics.CategoryDurations[JiraStatusCategory.InProgress]);
            Assert.Equal(TimeSpan.FromDays(1), statistics.CategoryDurations[JiraStatusCategory.OnHold]);
            Assert.True(statistics.CategoryDurations[JiraStatusCategory.Done] > TimeSpan.Zero);

            Assert.Equal(TimeSpan.FromDays(3), statistics.PersonDurations[John.EmailAddress]);
            Assert.Equal(TimeSpan.FromDays(2), statistics.PersonDurations[Jane.EmailAddress]);
        }
    }
}