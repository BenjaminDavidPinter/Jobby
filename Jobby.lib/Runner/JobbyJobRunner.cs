using Jobby.Lib.Core;
using Jobby.Lib.Core.JobTypes;
using Jobby.Lib.Core.Model;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Jobby.Lib.Runner
{
    public class JobbyJobRunner
    {
        private readonly JobbyJobQueue _backingQueue;
        public JobbyJobRunner(JobbyJobQueue backingQueue)
        {
            _backingQueue = backingQueue;
        }

        //TODO: B.Pinter - Build job runner which only runs the built sql jobs
        public void RunTSQLJobs()
        {
            var applicableTypes = GetClassesForInterface(typeof(IJobbyTSQLJob));
            if (applicableTypes?.Count() > 0)
            {
                foreach (IJobbyTSQLJob job in applicableTypes)
                {
                    _backingQueue.RegisterNewSqlJobQueue(job.JobName);
                    _backingQueue.AddJobToQueue(job.JobName, new Task<TSQLJobResult>(() =>
                    {
                        using var con = new SqlConnection(job.ConnectionString);
                        var command = con.CreateCommand();
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = job.StoredProcedure;
                        command.CommandTimeout = (int)job.TimeOut.TotalSeconds;
                        var resultTable = new DataSet();
                        con.Open();
                        var adapter = new SqlDataAdapter(command);

                        Stopwatch s = new();
                        s.Start();
                        adapter.Fill(resultTable);
                        s.Stop();

                        int totalRecords = 0;
                        for (int i = 0; i < resultTable.Tables.Count; i++)
                        {
                            totalRecords += resultTable.Tables[i].Rows.Count;
                        }

                        return new TSQLJobResult()
                        {
                            ImpactedRecords = totalRecords,
                            RunTime = s.Elapsed,
                            Result = resultTable
                        };
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
