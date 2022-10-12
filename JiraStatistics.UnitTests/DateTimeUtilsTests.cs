using System.Globalization;
using Nager.Date;
using Xunit;

namespace JiraStatistics.UnitTests;

public class DateTimeUtilsTests
{
    private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

    [Fact]
    public void Case01() => AssertStuff("2020-01-01 06:00:00", "2020-01-01 07:00:00", new[]
    {
        "2020-01-01 06:00:00",
        "2020-01-01 07:00:00"
    });

    [Fact]
    public void Case02() => AssertStuff("2020-01-01 06:12:23", "2020-01-01 07:00:00", new[]
    {
        "2020-01-01 06:12:23",
        "2020-01-01 07:00:00"
    });

    [Fact]
    public void Case03() => AssertStuff("2020-01-01 06:00:00", "2020-01-01 06:12:23", new[]
    {
        "2020-01-01 06:00:00",
        "2020-01-01 06:12:23"
    });

    [Fact]
    public void Case04() => AssertStuff("2020-01-01 06:00:00", "2020-01-01 09:00:00", new[]
    {
        "2020-01-01 06:00:00",
        "2020-01-01 07:00:00",
        "2020-01-01 08:00:00",
        "2020-01-01 09:00:00",
    });

    [Fact]
    public void Case05() => AssertStuff("2020-01-01 06:12:23", "2020-01-01 07:12:23", new[]
    {
        "2020-01-01 06:12:23",
        "2020-01-01 07:00:00",
        "2020-01-01 07:12:23"
    });

    [Fact]
    public void Case06() => AssertStuff("2020-01-01 06:12:23", "2020-01-01 08:12:23", new[]
    {
        "2020-01-01 06:12:23",
        "2020-01-01 07:00:00",
        "2020-01-01 08:00:00",
        "2020-01-01 08:12:23"
    });

    [Fact]
    public void Case07() => AssertStuff("2020-01-01 06:12:23", "2020-01-01 09:12:23", new[]
    {
        "2020-01-01 06:12:23",
        "2020-01-01 07:00:00",
        "2020-01-01 08:00:00",
        "2020-01-01 09:00:00",
        "2020-01-01 09:12:23"
    });

    [Fact]
    public void Case08() => AssertStuff("2020-01-01 06:00:00", "2020-01-01 06:00:00", new[]
    {
        "2020-01-01 06:00:00",
    });

    private static DateTime MakeDate(string timestampStr)
    {
        return DateTime.ParseExact(timestampStr, TimeFormat, CultureInfo.InvariantCulture);
    }

    private static void AssertStuff(string startDate, string endDate, IEnumerable<string> expected)
    {
        var oneHour = TimeSpan.FromHours(1);
        var actual = DateTimeUtils.GetDateTimesBetween(MakeDate(startDate), MakeDate(endDate), oneHour);
        Assert.Equal(expected, actual.Select(d => d.ToString(TimeFormat)));
    }

    [Fact]
    public void Case09() => Assert.Equal(1, DateTimeUtils.ComputeLeadOrCycleTime(MakeDate("2019-12-23 12:00:00"), MakeDate("2019-12-23 12:00:00"), _ => true));

    [Fact]
    public void Case10() => Assert.Equal(2, DateTimeUtils.ComputeLeadOrCycleTime(MakeDate("2019-12-23 12:00:00"), MakeDate("2019-12-24 12:00:00"), _ => true));

    [Fact]
    public void Case11() => Assert.Equal(3, DateTimeUtils.ComputeLeadOrCycleTime(MakeDate("2019-12-23 12:00:00"), MakeDate("2019-12-25 12:00:00"), _ => true));

    [Fact]
    public void Case12() => Assert.Equal(4, DateTimeUtils.ComputeLeadOrCycleTime(MakeDate("2019-12-23 12:00:00"), MakeDate("2019-12-26 12:00:00"), _ => true));

    [Fact]
    public void Case13()
    {
        Assert.Equal(8, DateTimeUtils.ComputeLeadOrCycleTime(MakeDate("2019-12-23 12:00:00"), MakeDate("2020-01-03 12:00:00"), date =>
        {
            return date switch
            {
                var d when DateSystem.IsWeekend(d, CountryCode.CA) => false,
                var d when DateSystem.IsPublicHoliday(d, CountryCode.CA) => false,
                _ => true
            };
        }));
    }
}