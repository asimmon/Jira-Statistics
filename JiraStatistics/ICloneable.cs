namespace JiraStatistics;

public interface ICloneable<out T>
{
    T Clone();
}