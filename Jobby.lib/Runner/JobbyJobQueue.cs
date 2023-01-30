using Jobby.Lib.Core.Model;

namespace Jobby.Lib.Runner
{
    public class JobbyJobQueue<T> : IJobbyJobQueue<T>
    {
        public List<(string QueueName, List<Task> Jobs)> JobQueue { get; set; }
        public List<(string QueueName, List<T> Results)> JobResults { get; set; }
        public List<(string QueueName, List<Exception> Errors)> JobErrors { get; set; }

        public JobbyJobQueue()
        {
            JobQueue = new();
            JobResults = new();
            JobErrors = new();
        }

        public void AddJobToQueue(string queueName, Task job)
        {
            GetJobQueue(queueName).Add(job);
        }

        public List<Task> GetJobQueue(string queueName)
        {
            return JobQueue.First(x => x.QueueName == queueName).Jobs;
        }

        public List<T> GetJobResultQueue(string queueName)
        {
            return JobResults.First(x => x.QueueName == queueName).Results;
        }

        public List<Exception> GetExceptionQueue(string queueName)
        {
            return JobErrors.First(x => x.QueueName == queueName).Errors;
        }

        public void InitializeJobQueues(string queueName)
        {
            JobQueue.Add((queueName, new List<Task>()));
            JobResults.Add((queueName, new List<T>()));
            JobErrors.Add((queueName, new List<Exception>()));
        }

        public void IssueJobQueueCommand(string queueName, JobQueueCommand command)
        {
            foreach (var jobQueue in JobQueue.Where(x => x.Item1 == queueName))
            {
                foreach (var job in jobQueue.Item2)
                {
                    job.Start();
                }
            }
        }
    }
}