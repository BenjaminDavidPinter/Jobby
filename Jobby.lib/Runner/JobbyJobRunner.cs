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
                    var instance = Activator.CreateInstance(job) as IJobbyJob<T>;
                    _backingQueue.InitializeJobQueues(instance.JobName);
                    for (int i = 0; i < instance.ConcurrentThreads; i++)
                    {
                        _backingQueue.AddJobToQueue(instance.JobName, CreateJobbyTask(instance));
                    }
                }
            }
        }


        /*
        Recursive method which generates jobs for the _backingQueue. 

        TODO:
            - Better error handling
            - Right now, the backing queue which holds jobs (_backingQueue.JobQueueInternal), is cleaned up after each child task of the same type
                is completed by running _backingQueue._JobQueueInternal.First(x => x.Item1 == job.JobName).Item2.RemoveAll(x => x.Status == TaskStatus.RanToCompletion);.
                Because there may be multiple instances of the same task running within a queue, it might be dangerous to allow them all to try and clean this queue, 
                at the same time.
            
        */
        private Task CreateJobbyTask(IJobbyJob<T> job)
        {
            var taskToStart = new Task(() =>
            {
                var nowAsTimeOnly = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);
                var isAfterStartTime = nowAsTimeOnly > job.StartTime;
                var isBeforeEndTime = nowAsTimeOnly < job.EndTime;

                if (isAfterStartTime && isBeforeEndTime)
                {
                    try
                    {
                        var results = job.Run();
                        _backingQueue.GetJobResultQueue(job.JobName).Add(results);
                    }
                    catch (Exception e)
                    {
                        _backingQueue.GetExceptionQueue(job.JobName).Add(e);
                    }
                    System.Threading.Thread.Sleep((int)job.CycleTime.TotalMilliseconds);
                }
                else
                {
                    var timeToWait = 0;
                    if (!isAfterStartTime)
                    {
                        timeToWait = (int)(job.StartTime - nowAsTimeOnly).TotalMilliseconds;
                    }
                    else if (!isBeforeEndTime)
                    {
                        timeToWait = (int)(new TimeOnly(23, 59, 59, 999) - nowAsTimeOnly).TotalMilliseconds;
                        timeToWait += (int)job.StartTime.ToTimeSpan().TotalMilliseconds;
                    }
                    System.Threading.Thread.Sleep(timeToWait);
                }
            });


            taskToStart.ContinueWith((x) =>
            {
                _backingQueue.GetJobQueue(job.JobName).RemoveAll(x => x.Status == TaskStatus.RanToCompletion);
                _backingQueue.GetJobQueue(job.JobName).Add(CreateJobbyTask(job));
            });

            taskToStart.Start();

            return taskToStart;
        }
    }
}