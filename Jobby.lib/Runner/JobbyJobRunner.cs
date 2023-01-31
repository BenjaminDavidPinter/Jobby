using Jobby.lib.Core.JobTypes;

namespace Jobby.Lib.Runner
{
    public class JobbyJobRunner<T> : IJobbyJobRunner<T>
    {
        private IJobbyJobQueue<T> BackingQueue { get; set; }

        public JobbyJobRunner(IJobbyJobQueue<T> backingQueue)
        {
            BackingQueue = backingQueue;
        }

        private static IEnumerable<Type>? GetClassesForInterface(Type t)
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
                    if (instance == null) throw new Exception("Error while creating job queues");
                    BackingQueue.InitializeJobQueues(instance.JobName);
                    for (int i = 0; i < instance.ConcurrentThreads; i++)
                    {
                        BackingQueue.AddJobToQueue(instance.JobName, CreateJobbyTask(instance));
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


                if (job.JobCondition())
                {
                    try
                    {
                        var results = job.Run();
                        BackingQueue.GetJobResultQueue(job.JobName).Add(results);
                    }
                    catch (Exception e)
                    {
                        BackingQueue.GetExceptionQueue(job.JobName).Add(e);
                    }
                    Thread.Sleep((int)job.CycleTime.TotalMilliseconds);
                }
            });


            taskToStart.ContinueWith((x) =>
            {
                BackingQueue.GetJobQueue(job.JobName).RemoveAll(x => x.Status == TaskStatus.RanToCompletion);
                BackingQueue.GetJobQueue(job.JobName).Add(CreateJobbyTask(job));
            });

            foreach (var cont in job.Continuations)
            {
                taskToStart.ContinueWith(cont.Item1, cont.Item2);
            }

            taskToStart.Start();

            return taskToStart;
        }

        public List<T> GetResults(string queueName)
        {
            return BackingQueue.GetJobResultQueue(queueName);
        }

        public List<Exception> GetErrors(string queueName)
        {
            return BackingQueue.GetExceptionQueue(queueName);
        }

        public List<Task> GetJobQueue(string queueName)
        {
            return BackingQueue.GetJobQueue(queueName);
        }
    }
}