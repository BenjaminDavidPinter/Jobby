using Jobby.lib.Core.JobTypes;

namespace Jobby.Lib.Runner
{
    public class JobbyJobRunner<T> : IJobbyJobRunner<T>
    {
        public IJobbyJobQueue<T> _backingQueue { get; set; }

        public JobbyJobRunner(IJobbyJobQueue<T> backingQueue)
        {
            _backingQueue = backingQueue;
        }

        public IEnumerable<Type>? GetClassesForInterface(Type t)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => t.IsAssignableFrom(p));
        }

        public void StartJobs()
        {
            var applicableTypes = GetClassesForInterface(typeof(IJobbyJob<T>));
            if (applicableTypes?.Count() > 0)
            {
                foreach (var job in applicableTypes)
                {
                    var instance = (IJobbyJob<T>)Activator.CreateInstance(job);
                    _backingQueue.InitializeJobQueues(instance.JobName);
                    //TODO: B.Pinter  - Abstract away the code which actually sets up and runs the SQL
                    //                  procedure. Probably need a service layer for this.
                    _backingQueue.AddJobToQueue(instance.JobName, CreateJobbyTask(instance));
                }
            }
        }

        private Task CreateJobbyTask(IJobbyJob<T> job)
        {
            var taskToStart = new Task(() =>
            {
                var nowAsTimeOnly = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
                var isAfterStartTime = nowAsTimeOnly > job.StartTime;
                var isBeforeEndTime = nowAsTimeOnly < job.EndTime;

                if(isAfterStartTime && isBeforeEndTime){
                    var results = job.Run();
                    _backingQueue._JobResultInternal.First(x => x.Item1 == job.JobName).Item2.Add(results);
                    System.Threading.Thread.Sleep((int)job.CycleTime.TotalMilliseconds);
                }
                else {
                    var timeToWait = 0;
                    if(!isAfterStartTime){
                        timeToWait = (int)(job.StartTime - nowAsTimeOnly).TotalMilliseconds;
                    }
                    else if (!isBeforeEndTime) {
                        timeToWait = (int)(new TimeOnly(23,59,59,999) - nowAsTimeOnly).TotalMilliseconds;
                        timeToWait += (int)job.StartTime.ToTimeSpan().TotalMilliseconds;
                    }
                    System.Threading.Thread.Sleep(timeToWait);
                }
                _backingQueue._JobQueueInternal.First(x => x.Item1 == job.JobName).Item2.Add(CreateJobbyTask(job));
            });

            taskToStart.Start();

            return taskToStart;
        }
    }
}