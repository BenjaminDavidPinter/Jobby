using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner
{
    public interface IJobbyJobQueue<T>
    {
        List<(string QueueName, List<Task> Jobs)> JobQueue { get; set; }
        List<(string QueueName, List<T> Results)> JobResults { get; set; }
        List<(string QueueName, List<Exception> Errors)> JobErrors { get; set; }

        void InitializeJobQueues(string queueName);
        void AddJobToQueue(string queueName, Task job);
        List<Task> GetJobQueue(string queueName);
        List<Exception> GetExceptionQueue(string queueName);
        List<T> GetJobResultQueue(string queueName);
    }
}