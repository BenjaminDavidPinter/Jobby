using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner
{
    public interface IJobbyJobQueue<T>
    {
        List<Tuple<string, List<Task>>> JobQueue { get; set; }
        List<Tuple<string, List<T>>> JobResults { get; set; }
        List<Tuple<string, List<Exception>>> JobErrors { get; set; }

        void InitializeJobQueues(string queueName);
        void AddJobToQueue(string queueName, Task job);
        void IssueJobQueueCommand(string queueName, JobQueueCommand command);
        List<Task> GetJobQueue(string queueName);
        List<Exception> GetExceptionQueue(string queueName);
        List<T> GetJobResultQueue(string queueName);
    }
}