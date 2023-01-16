namespace Jobby.Lib.Runner {
    public class JobbyJobRunner<T> : IJobbyJobRunner<T>
    {
        public JobbyJobQueue<T> _backingQueue {get;set;}

        public JobbyJobRunner(JobbyJobQueue<T> backingQueue) {
            _backingQueue = backingQueue;
        }

        public IEnumerable<Type>? GetClassesForInterface(Type t)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => t.IsAssignableFrom(p));
        }

        //TODO: In order for this method to work, we need to abstract away IJobbyJob types into a single interface
        //      just like we did with the rest of the code.
        public void RunJobs<G>(Func<Task<T>> body)
        {
            var applicableTypes = GetClassesForInterface(typeof(G));
            if (applicableTypes?.Count() > 0)
            {
                foreach (var job in applicableTypes)
                {
                    _backingQueue.InitializeJobQueues(job.JobName);
                    //TODO: B.Pinter  - Abstract away the code which actually sets up and runs the SQL
                    //                  procedure. Probably need a service layer for this.
                    _backingQueue.AddJobToQueue(job.JobName, body);
                }
            }
        }
    }
}