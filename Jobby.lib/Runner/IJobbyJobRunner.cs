namespace Jobby.Lib.Runner
{
    public interface IJobbyJobRunner<T>
    {
        IJobbyJobQueue<T> _backingQueue { get; set; }

        //TODO: B.Pinter - Build job runner which only runs the built sql jobs
        void StartJobs();
        IEnumerable<Type>? GetClassesForInterface(Type t);
    }
}