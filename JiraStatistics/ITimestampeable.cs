using System;

namespace JiraStatistics
{
    public interface ITimestampeable
    {
        DateTime Timestamp { get; set; }
    }
}