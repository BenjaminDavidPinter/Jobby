using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner
{
    public interface IJobbyJobQueue<T>
    {
        void InitializeJobQueues(string queueName);
        void AddJobToQueue(string queueName, Task job);
        List<Task> GetJobQueue(string queueName);
        List<Exception> GetExceptionQueue(string queueName);
        List<T> GetJobResultQueue(string queueName);
    }
}