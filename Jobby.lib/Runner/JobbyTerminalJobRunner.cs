using Jobby.Lib.Core;
using Jobby.Lib.Core.JobTypes;
using Jobby.Lib.Core.Model;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Jobby.Lib.Runner
{
    public class JobbyTerminalJobRunner
    {
        private readonly JobbyJobQueue<TerminalJobResult> _backingQueue;
        public JobbyTerminalJobRunner(JobbyJobQueue<TerminalJobResult> backingQueue)
        {
            _backingQueue = backingQueue;
        }

        //TODO: B.Pinter - Build job runner which only runs the built sql jobs
        public void RunTerminalJobs()
        {
            var applicableTypes = GetClassesForInterface(typeof(IJobbyTerminalJob));
            if (applicableTypes?.Count() > 0)
            {
                foreach (IJobbyTerminalJob job in applicableTypes)
                {
                    _backingQueue.InitializeJobQueues(job.JobName);
                    //TODO: B.Pinter  - Abstract away the code which actually sets up and runs the SQL
                    //                  procedure. Probably need a service layer for this.
                    _backingQueue.AddJobToQueue(job.JobName, new Task<TerminalJobResult>(() =>
                    {
                        //TODO: Run Terminal Job here
                        if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                            throw new NotImplementedException();
                        }
                        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                            throw new NotImplementedException();
                        }
                        else {
                            throw new NotImplementedException();
                        }
                    }));
                }
            }
        }

        private static IEnumerable<Type>? GetClassesForInterface(Type t)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => t.IsAssignableFrom(p));
        }
    }
}
