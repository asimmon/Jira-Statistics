using System;

namespace JiraStatistics.GuiApp
{
    public class OptionsSerializationException : Exception
    {
        public OptionsSerializationException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}