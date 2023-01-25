using System;
namespace Jobby.lib.Core.JobTypes
{
    public interface IJobbyJob<T>
    {
        Guid Id { get; }
        string JobName { get; }
        TimeSpan CycleTime => TimeSpan.FromSeconds(1);
        TimeSpan TimeOut => TimeSpan.FromHours(1);
        TimeOnly StartTime => new(00, 00);
        TimeOnly EndTime => new(23, 59);
        int ConcurrentThreads => 1;
        T Run();
        List<(Func<T>, TaskContinuationOptions)> Continuations => new();
    }
}

