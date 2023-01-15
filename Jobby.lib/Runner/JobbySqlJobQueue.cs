using System.Collections.Concurrent;
using Jobby.Lib.Core.Model;
using Jobby.Lib.Model.Enums;

namespace Jobby.Lib.Runner{
    public class JobbySqlJobQueue {
        //Sql Job Queue
        private List<Tuple<string, List<Task<TSQLJobResult>>>> _SqlJobQueueInternal {get;set;}

        //Sql Job Result Queue
        private List<Tuple<string, List<TSQLJobResult>>> _SqlJobResultInternal {get;set;}

        public JobbySqlJobQueue() {
            _SqlJobQueueInternal = new();
            _SqlJobResultInternal = new();
        }

        public void RegisterNewSqlJobQueue(string queueName){
            _SqlJobQueueInternal.Add(Tuple.Create(queueName, new List<Task<TSQLJobResult>>()));
            _SqlJobResultInternal.Add(Tuple.Create(queueName, new List<TSQLJobResult>()));
        }

        /*
        TODO: Ensure that when we search for a queue that it actually exists.
        */
        public void AddJobToQueue(string queueName, Task<TSQLJobResult> job){
            var jobQueue = _SqlJobQueueInternal.First(x => x.Item1 == queueName);
            jobQueue.Item2.Add(job);
        }  

        public void SqlJobQueueCommand(string queueName, JobQueueCommand command){
            foreach (var jobQueue in _SqlJobQueueInternal.Where(x => x.Item1 == queueName))
            {
                foreach (var job in jobQueue.Item2)
                {
                    job.Start();
                }
            }
        }

        public List<Task<TSQLJobResult>> GetSqlJobQueue(string queueName){
            return _SqlJobQueueInternal.First(x => x.Item1 == queueName).Item2;
        }

        public List<TSQLJobResult> GetSqlJobResultQueue(string queueName){
            return _SqlJobResultInternal.First(x => x.Item1 == queueName).Item2;
        }
    }
}