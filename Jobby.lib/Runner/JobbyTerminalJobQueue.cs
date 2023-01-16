using System.Collections.Concurrent;
using Jobby.Lib.Core.Model;
using Jobby.Lib.Model.Enums;

namespace Jobby.Lib.Runner{
    public class JobbyTerminalJobQueue {
        //Sql Job Queue
        //TODO: Dictionary best here? Benchmark it.
        private List<Tuple<string, List<Task<TSQLJobResult>>>> _TerminalJobQueueInternal {get;set;}

        //Sql Job Result Queue
        //TODO: Dictionary best here? Benchmark it.
        private List<Tuple<string, List<TSQLJobResult>>> _TerminalJobResultInternal {get;set;}

        public JobbyTerminalJobQueue() {
            _TerminalJobQueueInternal = new();
            _TerminalJobResultInternal = new();
        }

        public void RegisterNewTerminalJobQueue(string queueName){
            _TerminalJobQueueInternal.Add(Tuple.Create(queueName, new List<Task<TSQLJobResult>>()));
            _TerminalJobResultInternal.Add(Tuple.Create(queueName, new List<TSQLJobResult>()));
        }

        /*
        TODO: Ensure that when we search for a queue that it actually exists.
        */
        public void AddJobToQueue(string queueName, Task<TSQLJobResult> job){
            var jobQueue = _TerminalJobQueueInternal.First(x => x.Item1 == queueName);
            jobQueue.Item2.Add(job);
        }  

        public void TerminalJobQueueCommand(string queueName, JobQueueCommand command){
            foreach (var jobQueue in _TerminalJobQueueInternal.Where(x => x.Item1 == queueName))
            {
                foreach (var job in jobQueue.Item2)
                {
                    job.Start();
                }
            }
        }

        public List<Task<TSQLJobResult>> GetTerminalJobQueue(string queueName){
            return _TerminalJobQueueInternal.First(x => x.Item1 == queueName).Item2;
        }

        public List<TSQLJobResult> GetTerminalJobResultQueue(string queueName){
            return _TerminalJobResultInternal.First(x => x.Item1 == queueName).Item2;
        }
    }
}