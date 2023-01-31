namespace Jobby.Lib.Runner
{
    public interface IJobbyJobRunner<T>
    {
        //TODO: B.Pinter - Build job runner which only runs the built sql jobs
        void StartJobs();
        List<T> GetResults(string queueName);
        List<Exception> GetErrors(string queueName);
        List<Task> GetJobQueue(string queueName);
    }
}