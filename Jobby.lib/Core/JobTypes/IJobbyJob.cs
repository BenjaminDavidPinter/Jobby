using Jobby.lib.Core.Model;
namespace Jobby.lib.Core.JobTypes
{
    public interface IJobbyJob<T> where T: JobbyJobResult
    {
        Guid Id { get; }
        string JobName { get; }
        TimeSpan CycleTime { get; }
        TimeSpan TimeOut => TimeSpan.FromHours(1);
        Func<bool> JobCondition { get; }
        int ConcurrentThreads => 1;
        T Run();
        List<(Action<Task, object?>, TaskContinuationOptions)> Continuations => new();
    }
}

