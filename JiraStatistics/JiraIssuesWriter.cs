using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace JiraStatistics
{
    public class JiraIssuesWriter : IDisposable
    {
        private static readonly JiraStatusCategory[] RelevantCategories =
        {
            JiraStatusCategory.New,
            JiraStatusCategory.InProgress,
            JiraStatusCategory.OnHold,
            JiraStatusCategory.Done
        };

        private readonly JiraOptions _opts;
        private readonly ExcelPackage _pkg;
        private IEnumerable<JiraIssue> _issues;

        private JiraIssuesWriter(JiraOptions opts, Stream outputStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            this._opts = opts;
            this._pkg = new ExcelPackage(outputStream);
        }

        public void Dispose()
        {
            this._pkg.Dispose();
        }

        public static async Task Write(JiraOptions opts)
        {
            opts.Logger($"Results will be written to {opts.OutputFilePath}");

            try
            {
                await using var fileStream = File.OpenWrite(opts.OutputFilePath);
                using var serializer = new JiraIssuesWriter(opts, fileStream);

                var issues = await JiraRepository.GetIssuesAsync(opts);
                serializer.Write(issues);
            }
            catch
            {
                QuietlyDeleteCreatedExcelPackage(opts);
                throw;
            }

            opts.Logger($"Done, exported file can be found at {opts.OutputFilePath}");
        }

        private void Write(IEnumerable<JiraIssue> issues)
        {
            this._issues = issues;

            var issuesColumnWriters = new[]
            {
                new SingleCellJiraIssueWriter("Issue", (w, i) => w.WriteCell(w.Row, w.Col, i.Key)),
                new SingleCellJiraIssueWriter("Parent", (w, i) => w.WriteCell(w.Row, w.Col, i.ParentKey)),
                new SingleCellJiraIssueWriter("Type", (w, i) => w.WriteCell(w.Row, w.Col, i.IssueType.Name)),
                new SingleCellJiraIssueWriter("Creator", (w, i) => w.WriteCell(w.Row, w.Col, i.Creator)),
                new SingleCellJiraIssueWriter("Created", (w, i) => w.WriteCell(w.Row, w.Col, i.Created)),
                new SingleCellJiraIssueWriter("Started", (w, i) => w.WriteCell(w.Row, w.Col, i.Started)),
                new SingleCellJiraIssueWriter("Finished", (w, i) => w.WriteCell(w.Row, w.Col, i.Finished)),
                new SingleCellJiraIssueWriter("Status", (w, i) => w.WriteCell(w.Row, w.Col, i.CurrentStatus.Name)),
                new SingleCellJiraIssueWriter("Epic", (w, i) =>
                {
                    var epicLink = string.IsNullOrWhiteSpace(i.EpicLink) && i.Parent != null ? i.Parent.EpicLink : i.EpicLink;
                    w.WriteCell(w.Row, w.Col, epicLink);
                }),
                new SingleCellJiraIssueWriter("Sprints", (w, i) =>
                {
                    var sprints = i.Sprints.Count == 0 && i.Parent != null ? i.Parent.Sprints : i.Sprints;
                    w.WriteCell(w.Row, w.Col, string.Join(", ", sprints));
                }),
                new SingleCellJiraIssueWriter("Components", (w, i) =>
                {
                    var components = i.Components.Count == 0 && i.Parent != null ? i.Parent.Components : i.Components;
                    w.WriteCell(w.Row, w.Col, string.Join(", ", components.Select(c => c.Name)));
                }),
                new SingleCellJiraIssueWriter("Versions", (w, i) =>
                {
                    var fixVersion = i.FixVersion == null && i.Parent != null ? i.Parent.FixVersion : i.FixVersion;
                    w.WriteCell(w.Row, w.Col, fixVersion == null ? string.Empty : fixVersion.Name);
                }),
                new SingleCellJiraIssueWriter("ReleaseDate", (w, i) =>
                {
                    var releaseDate = i.ReleaseDate == null && i.Parent != null ? i.Parent.ReleaseDate : i.ReleaseDate;
                    w.WriteCell(w.Row, w.Col, releaseDate);
                }),
                new SingleCellJiraIssueWriter("Priority", (w, i) =>
                {
                    var priority = string.IsNullOrWhiteSpace(i.Priority) && i.Parent != null ? i.Parent.Priority : i.Priority;
                    w.WriteCell(w.Row, w.Col, priority);
                }),
                new SingleCellJiraIssueWriter("Severity", (w, i) =>
                {
                    var severity = string.IsNullOrWhiteSpace(i.Severity) && i.Parent != null ? i.Parent.Severity : i.Severity;
                    w.WriteCell(w.Row, w.Col, severity);
                }),
                new SingleCellJiraIssueWriter("Magnitude", (w, i) =>
                {
                    var magnitude = string.IsNullOrWhiteSpace(i.Magnitude) && i.Parent != null ? i.Parent.Magnitude : i.Magnitude;
                    w.WriteCell(w.Row, w.Col, magnitude);
                }),
                new SingleCellJiraIssueWriter("Complexity", (w, i) =>
                {
                    var complexity = string.IsNullOrWhiteSpace(i.Complexity) && i.Parent != null ? i.Parent.Complexity : i.Complexity;
                    w.WriteCell(w.Row, w.Col, complexity);
                }),
                new SingleCellJiraIssueWriter("DaysCycleTime", (w, i) => w.WriteCell(w.Row, w.Col, i.CycleTime)),
                new SingleCellJiraIssueWriter("DaysLeadTime", (w, i) => w.WriteCell(w.Row, w.Col, i.LeadTime)),
                new SingleCellJiraIssueWriter("DaysEstimated", (w, i) => w.WriteCell(w.Row, w.Col, i.DaysEstimated)),
                new ColumnWriter<JiraIssue>(
                    () => RelevantCategories.Length, () => 1, i => RelevantCategories.Length, i => 1,
                    w =>
                    {
                        for (var i = 0; i < RelevantCategories.Length; i++)
                        {
                            var categoryDisplayName = GetCategoryDisplayName(RelevantCategories[i]);
                            w.WriteCell(w.Row, w.Col + i, w.Row, w.Col + i, categoryDisplayName);
                        }
                    },
                    (w, i) =>
                    {
                        for (var j = 0; j < RelevantCategories.Length; j++)
                        {
                            var duration = i.Statistics.CategoryDurations.TryGetValue(RelevantCategories[j], out var tmpDuration) ? tmpDuration : TimeSpan.Zero;
                            w.WriteCell(w.Row, w.Col + j, w.Row, w.Col + j, duration.TotalDays);
                        }
                    }),
                new SingleCellJiraIssueWriter("Title", (w, i) => w.WriteCell(w.Row, w.Col, i.Title))
            };

            var jqlWriters = new[]
            {
                new ColumnWriter<JiraIssue>(() => 1, () => 1, _ => 1, _ => 1, w => w.WriteCell(1, 1, "JQL"), (w, _) => w.WriteCell(2, 1, this._opts.JqlQuery))
            };

            this.WriteWorksheets(this._issues, new[]
            {
                new WorksheetWriter<JiraIssue>(this._pkg, "Issues", issuesColumnWriters),
                new WorksheetWriter<JiraIssue>(this._pkg, "JQL", jqlWriters),
            });

            this._pkg.Save();
        }

        private static string GetCategoryDisplayName(JiraStatusCategory category) => category switch
        {
            JiraStatusCategory.New => "DaysAtNew",
            JiraStatusCategory.InProgress => "DaysInProgress",
            JiraStatusCategory.Done => "DaysSinceDone",
            JiraStatusCategory.OnHold => "DaysOnHold",
            _ => throw new NotSupportedException($"Category {category} is not supported")
        };

        private void WriteWorksheets<T>(IEnumerable<T> rowItems, IReadOnlyCollection<WorksheetWriter<T>> sheetWriters)
        {
            foreach (var sheetWriter in sheetWriters)
            {
                sheetWriter.Row = 1;
                sheetWriter.Col = 1;
                var maxHeaderHeight = 0;

                foreach (var colWriter in sheetWriter.ColumnWriters)
                {
                    colWriter.HeaderFunc(sheetWriter);
                    sheetWriter.Col += colWriter.HeaderWidth();
                    maxHeaderHeight = Math.Max(maxHeaderHeight, colWriter.HeaderHeight());
                }

                sheetWriter.Row += maxHeaderHeight;
                sheetWriter.Col = 1;
            }

            var itemWritten = 0;

            foreach (var item in rowItems)
            {
                foreach (var sheetWriter in sheetWriters)
                {
                    sheetWriter.Col = 1;
                    var maxValueHeight = 0;

                    foreach (var colWriter in sheetWriter.ColumnWriters)
                    {
                        colWriter.ValueFunc(sheetWriter, item);
                        sheetWriter.Col += colWriter.ValueWidth(item);
                        maxValueHeight = Math.Max(maxValueHeight, colWriter.ValueHeight(item));
                    }

                    sheetWriter.Row += maxValueHeight;
                    sheetWriter.Col = 1;
                }

                this._opts.Logger($"Items written: {++itemWritten}");
            }

            foreach (var sheetWriter in sheetWriters) sheetWriter.Autofit();
        }

        private static void QuietlyDeleteCreatedExcelPackage(JiraOptions opts)
        {
            try
            {
                File.Delete(opts.OutputFilePath);
            }
            catch
            {
                // ignored
            }
        }

        private sealed class WorksheetWriter<T>
        {
            private readonly ExcelWorksheet _ws;

            public WorksheetWriter(ExcelPackage pkg, string name, IReadOnlyCollection<ColumnWriter<T>> columnWriters)
            {
                this._ws = pkg.Workbook.Worksheets.Add(name);
                this.ColumnWriters = columnWriters;
            }

            public IReadOnlyCollection<ColumnWriter<T>> ColumnWriters { get; }

            public int Row { get; set; } = 1;

            public int Col { get; set; } = 1;

            public void WriteCell(int fromRow, int fromCol, int toRow, int toCol, string value)
            {
                this.Write(fromRow, fromCol, toRow, toCol, value);
            }

            public void WriteCell(int row, int col, string value)
            {
                this.WriteCell(row, col, row, col, value);
            }

            public void WriteCell(int fromRow, int fromCol, int toRow, int toCol, Nullable<DateTime> value)
            {
                this.Write(fromRow, fromCol, toRow, toCol, value, "yyyy-MM-dd hh:mm:ss");
            }

            public void WriteCell(int row, int col, Nullable<DateTime> value)
            {
                this.WriteCell(row, col, row, col, value);
            }

            public void WriteCell(int fromRow, int fromCol, int toRow, int toCol, Nullable<double> value)
            {
                this.Write(fromRow, fromCol, toRow, toCol, value, "0.00");
            }

            public void WriteCell(int row, int col, Nullable<double> value)
            {
                this.WriteCell(row, col, row, col, value);
            }

            public void WriteCell(int fromRow, int fromCol, int toRow, int toCol, Nullable<int> value)
            {
                this.Write(fromRow, fromCol, toRow, toCol, value, "0");
            }

            public void WriteCell(int row, int col, Nullable<int> value)
            {
                this.WriteCell(row, col, row, col, value);
            }

            private void Write(int fromRow, int fromCol, int toRow, int toCol, object value, string format = null)
            {
                // ex: "1, 1, 1, 5" => "A1:A5"
                using (var range = this._ws.Cells[fromRow, fromCol, toRow, toCol])
                {
                    range.Merge = true;
                    range.Value = value;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    range.Style.Numberformat.Format = format ?? range.Style.Numberformat.Format;
                }
            }

            public void Autofit()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    this._ws.Cells.AutoFitColumns();
            }
        }

        private class SingleCellJiraIssueWriter : ColumnWriter<JiraIssue>
        {
            public SingleCellJiraIssueWriter(string headerName, Action<WorksheetWriter<JiraIssue>, JiraIssue> valueFunc)
                : base(One, One, One, One, CreateHeaderNameFunc(headerName), valueFunc)
            {
            }

            private static Action<WorksheetWriter<JiraIssue>> CreateHeaderNameFunc(string headerName)
            {
                void HeaderNameFunc(WorksheetWriter<JiraIssue> w)
                {
                    w.WriteCell(w.Row, w.Col, headerName);
                }

                return HeaderNameFunc;
            }
        }

        private class ColumnWriter<T>
        {
            public ColumnWriter(
                Func<int> hw,
                Func<int> hh,
                Func<T, int> vw,
                Func<T, int> vh,
                Action<WorksheetWriter<T>> headerFunc,
                Action<WorksheetWriter<T>, T> valueFunc)
            {
                this.HeaderWidth = hw;
                this.HeaderHeight = hh;
                this.ValueWidth = vw;
                this.ValueHeight = vh;
                this.HeaderFunc = headerFunc;
                this.ValueFunc = valueFunc;
            }

            public Func<int> HeaderWidth { get; }

            public Func<int> HeaderHeight { get; }

            public Func<T, int> ValueWidth { get; }

            public Func<T, int> ValueHeight { get; }

            public Action<WorksheetWriter<T>> HeaderFunc { get; }

            public Action<WorksheetWriter<T>, T> ValueFunc { get; }

            protected static int One() => 1;

            protected static int One(T issue) => 1;
        }
    }
}