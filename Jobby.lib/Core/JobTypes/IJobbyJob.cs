using System;
namespace Jobby.lib.Core.JobTypes
{
    //TODO: Implement scheduling feature
    //      It's easy enough to get jobs to run every N micro/milli/second, but in real world
    //      scenarios, we need a way to 'schedule' tasks to run. If someone wants a task to run
    //      only between 9am and 11pm, we need to support that.
	public interface IJobbyJob<T>
	{
        string JobName { get; set; }
        TimeSpan CycleTime { get; set; }
        TimeSpan TimeOut { get; set; }
        TimeOnly StartTime {get;set;}
        TimeOnly EndTime {get;set;}
        T Run();
    }
}

