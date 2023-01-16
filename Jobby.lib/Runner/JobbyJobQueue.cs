using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner {
    public class JobbyJobQueue<T> : IJobbyJobQueue<T>
    {
        public List<Tuple<string, List<Task<T>>>> _JobQueueInternal { get; set; }
        public List<Tuple<string, List<T>>> _JobResultInternal { get; set; }

        public JobbyJobQueue(){
            _JobQueueInternal = new();
            _JobResultInternal = new();
        }
        
        public void AddJobToQueue(string queueName, Task<T> job)
        {
            var jobQueue = _JobQueueInternal.First(x => x.Item1 == queueName);
            jobQueue.Item2.Add(job);
        }

        public List<Task<T>> GetJobQueue(string queueName)
        {
            return _JobQueueInternal.First(x => x.Item1 == queueName).Item2;
        }

        public List<T> GetJobResultQueue(string queueName)
        {
            return _JobResultInternal.First(x => x.Item1 == queueName).Item2;
        }

        public void InitializeJobQueues(string queueName)
        {
            _JobQueueInternal.Add(Tuple.Create(queueName, new List<Task<T>>()));
            _JobResultInternal.Add(Tuple.Create(queueName, new List<T>()));
        }

        public void IssueJobQueueCommand(string queueName, JobQueueCommand command)
        {
            foreach (var jobQueue in _JobQueueInternal.Where(x => x.Item1 == queueName))
            {
                foreach (var job in jobQueue.Item2)
                {
                    job.Start();
                }
            }
        }
    }
}