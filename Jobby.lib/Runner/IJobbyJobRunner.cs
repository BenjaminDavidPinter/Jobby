namespace Jobby.Lib.Runner {
    public interface IJobbyJobRunner<T> {
        JobbyJobQueue<T> _backingQueue {get;set;}

        //TODO: B.Pinter - Build job runner which only runs the built sql jobs
        void RunJobs(Task<T> body);
        IEnumerable<Type>? GetClassesForInterface(Type t);
    }
}