using System;
namespace Jobby.lib.Core.JobTypes
{
	public interface IJobbyJob<T>
	{
        string JobName { get; set; }
        TimeSpan CycleTime { get; set; }
        TimeSpan TimeOut { get; set; }
        T Result { get; set; }
    }
}

