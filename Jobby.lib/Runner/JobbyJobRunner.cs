using Jobby.lib.Core.JobTypes;

namespace Jobby.Lib.Runner {
    public class JobbyJobRunner<T> : IJobbyJobRunner<T>
    {
        public IJobbyJobQueue<T> _backingQueue {get;set;}

        public JobbyJobRunner(IJobbyJobQueue<T> backingQueue) {
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
                    var instance = (IJobbyJob<T>)Activator.CreateInstance(job) ?? throw new Exception("Unable to create instance of job. Check configuration.");
                    _backingQueue.InitializeJobQueues(instance.JobName);
                    //TODO: B.Pinter  - Abstract away the code which actually sets up and runs the SQL
                    //                  procedure. Probably need a service layer for this.
                    _backingQueue.AddJobToQueue(instance.JobName, Task.Run(() => { 
                        var results = instance.Run();
                        _backingQueue._JobResultInternal.First(x => x.Item1 == instance.JobName).Item2.Add(results);
                    }));
                }
            }
        }
    }
}