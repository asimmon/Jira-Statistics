using System;

namespace JiraStatistics.GuiApp
{
    public class PersistedOptionsNotFoundException : OptionsSerializationException
    {
        public PersistedOptionsNotFoundException(Exception innerException)
            : base(innerException)
        {
        }
    }
}