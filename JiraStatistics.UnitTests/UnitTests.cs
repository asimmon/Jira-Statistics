using System.Globalization;
using Xunit;

namespace JiraStatistics.UnitTests;

public class UnitTests
{
    private static DateTime MakeDate(string text)
    {
        return DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static void AssertSplitOverTime(IEnumerable<Thing> things, IEnumerable<string> expected)
    {
        var actual = things.SplitOverTime(TimeSpan.FromHours(1)).Select(t => t.ToString());
        Assert.Equal(string.Join(", ", expected), string.Join(", ", actual));
    }

    private static void AssertSplitOverTimeThrows(IEnumerable<Thing> things, TimeSpan interval)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => things.SplitOverTime(interval).ToList());
    }

    private class Thing : ITimestampeable, ICloneable<Thing>
    {
        private const string TimestampFormat = "yyyyMMdd HH:mm:ss";

        public Thing(int value, string timestampStr)
        {
            this.Value = value;
            this.Timestamp = DateTime.ParseExact(timestampStr, TimestampFormat, CultureInfo.InvariantCulture);
        }

        private int Value { get; }

        public Thing Clone()
        {
            return (Thing) this.MemberwiseClone();
        }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{this.Value} - {this.Timestamp.ToString(TimestampFormat)}";
        }
    }

    [Fact]
    public void Lag_WithDates()
    {
        var source = new[]
        {
            MakeDate("20200101"),
            MakeDate("20200102"),
            MakeDate("20200104"),
            MakeDate("20200107")
        };

        var actual = string.Join(";", source.Lag().Select(e => e.Next - e.Previous).Select(e => e.TotalDays.ToString("F2")));
        const string expected = "1.00;2.00;3.00";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Lag_WithIntegers()
    {
        var source = new[] {1, 2, 3, 4, 5};
        var actual = string.Join(",", source.Lag().Select(e => $"{e.Previous}:{e.Next}"));
        const string expected = "1:2,2:3,3:4,4:5";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitOverTime_Complex()
    {
        var things = new[]
        {
            new Thing(1, "20200101 01:23:45"),
            new Thing(2, "20200101 04:32:10"),
            new Thing(3, "20200101 06:32:10"),
            new Thing(4, "20200101 06:32:10")
        };

        var expected = new[]
        {
            "1 - 20200101 01:23:45", //
            "1 - 20200101 02:00:00",
            "1 - 20200101 03:00:00",
            "1 - 20200101 04:00:00",
            "2 - 20200101 04:32:10", //
            "2 - 20200101 05:00:00",
            "2 - 20200101 06:00:00",
            "3 - 20200101 06:32:10", //
            "4 - 20200101 06:32:10" //
        };

        AssertSplitOverTime(things, expected);
    }

    [Fact]
    public void SplitOverTime_EmptyResults()
    {
        AssertSplitOverTime(new Thing[0], new string[0]);
        AssertSplitOverTime(new[] {new Thing(1, "20200101 01:23:45")}, new string[0]);
    }

    [Fact]
    public void SplitOverTime_FancyStuff()
    {
        var things = new[]
        {
            new Thing(1, "20200101 01:00:00"),
            new Thing(2, "20200103 04:00:00"), // + 2j 3h
            new Thing(3, "20200103 06:00:00"), // + 2h
            new Thing(3, "20200103 06:00:00"), // + 0
            new Thing(4, "20200108 06:00:00") // + 5j
        };

        var actual = things
            .SplitOverTime(TimeSpan.FromHours(1))
            .Lag()
            .Select(pair => pair.Next.Timestamp - pair.Previous.Timestamp)
            .Aggregate(TimeSpan.Zero, (acc, curr) => acc + curr);

        var expected = TimeSpan.FromDays(7).Add(TimeSpan.FromHours(5));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitOverTime_Throws()
    {
        AssertSplitOverTimeThrows(new Thing[0], TimeSpan.FromHours(-1));

        AssertSplitOverTimeThrows(new[]
        {
            new Thing(1, "20000101 01:23:45"),
            new Thing(2, "19990101 04:32:10")
        }, TimeSpan.FromHours(1));
    }
}