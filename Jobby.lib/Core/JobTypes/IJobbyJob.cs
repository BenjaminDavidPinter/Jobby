using System;
namespace Jobby.lib.Core.JobTypes
{
    public interface IJobbyJob<T>
    {
        Guid Id { get; }
        string JobName { get; }
        TimeSpan CycleTime { get; }
        TimeSpan TimeOut { get; }
        TimeOnly StartTime { get; }
        TimeOnly EndTime { get; }
        int ConcurrentThreads { get; }
        T Run();
    }
}

