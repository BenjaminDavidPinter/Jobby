using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner{
    public interface IJobbyJobQueue<T> {
        List<Tuple<string, List<Task<T>>>> _JobQueueInternal {get;set;}        
        List<Tuple<string, List<T>>> _JobResultInternal {get;set;}

        void InitializeJobQueues(string queueName);
        void AddJobToQueue(string queueName, Task<T> job);
        void IssueJobQueueCommand(string queueName, JobQueueCommand command);
        List<Task<T>> GetJobQueue(string queueName);
        List<T> GetJobResultQueue(string queueName);
    }
}