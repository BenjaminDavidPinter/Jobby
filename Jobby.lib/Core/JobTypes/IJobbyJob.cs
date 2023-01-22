using System;
namespace Jobby.lib.Core.JobTypes
{
    public interface IJobbyJob<T>
    {
        string JobName { get; }
        TimeSpan CycleTime { get; }
        TimeSpan TimeOut { get; }
        TimeOnly StartTime { get; }
        TimeOnly EndTime { get; }
        Guid Id { get; }
        T Run();
    }
}

