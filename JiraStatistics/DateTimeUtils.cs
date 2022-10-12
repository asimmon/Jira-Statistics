namespace JiraStatistics;

public static class DateTimeUtils
{
    public static IEnumerable<T> SplitOverTime<T>(this IEnumerable<T> source, TimeSpan interval)
        where T : ITimestampeable, ICloneable<T>
    {
        if (interval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval));

        var hasAtLeastOneElement = false;
        var lastEncounteredElement = default(T);

        foreach (var (previous, next) in source.Lag())
        {
            if (previous.Timestamp > next.Timestamp)
                throw new ArgumentOutOfRangeException(nameof(source));

            yield return previous;

            var ticksToRoundedDate = interval.Ticks - (previous.Timestamp.Ticks % interval.Ticks);
            var roundedDate = new DateTime(previous.Timestamp.Ticks + ticksToRoundedDate);

            if (roundedDate > previous.Timestamp && roundedDate < next.Timestamp)
                yield return CloneWithDate(previous, roundedDate);

            while ((roundedDate = roundedDate.Add(interval)) < next.Timestamp)
                yield return CloneWithDate(previous, roundedDate);

            hasAtLeastOneElement = true;
            lastEncounteredElement = next;
        }

        if (hasAtLeastOneElement)
            yield return lastEncounteredElement;
    }

    private static T CloneWithDate<T>(T cloneable, DateTime newTimestamp)
        where T : ITimestampeable, ICloneable<T>
    {
        var cloned = cloneable.Clone();
        cloned.Timestamp = newTimestamp;
        return cloned;
    }

    public static IEnumerable<DateTime> GetDateTimesBetween(DateTime startDate, DateTime endDate, TimeSpan interval)
    {
        if (interval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval));

        if (startDate > endDate)
            throw new ArgumentOutOfRangeException(nameof(startDate));

        yield return startDate;

        var ticksToRoundedDate = interval.Ticks - (startDate.Ticks % interval.Ticks);
        var roundedDate = new DateTime(startDate.Ticks + ticksToRoundedDate);

        if (roundedDate > startDate && roundedDate < endDate)
            yield return roundedDate;

        while ((roundedDate = roundedDate.Add(interval)) < endDate)
            yield return roundedDate;

        if (endDate > startDate)
            yield return endDate;
    }

    public static int ComputeLeadOrCycleTime(DateTime startDate, DateTime endDate, Func<DateTime, bool> isWorkDayPredicate)
    {
        var oneDay = TimeSpan.FromDays(1);
        var duration = GetDateTimesBetween(startDate.Date, endDate.Date, oneDay)
            .Lag((prevDate, nextDate) => isWorkDayPredicate(prevDate))
            .Aggregate(TimeSpan.Zero, (acc, pair) => acc + (pair.Next - pair.Previous));

        return 1 + (int)duration.TotalDays;
    }
}