using Jobby.lib.Core.JobTypes;

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
        
        public void RunJobs(Task<T> body)
        {
            var applicableTypes = GetClassesForInterface(typeof(IJobbyJob<T>));
            if (applicableTypes?.Count() > 0)
            {
                foreach (IJobbyJob<T> job in applicableTypes)
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