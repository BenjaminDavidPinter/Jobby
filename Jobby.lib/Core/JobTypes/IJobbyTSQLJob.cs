using Jobby.Lib.Core.Model;
namespace Jobby.Lib.Core.JobTypes {
    public interface IJobbyTSQLJob {
        string JobName {get;set;}
        TimeSpan CycleTime {get;set;}
        TimeSpan TimeOut {get;set;}
        string ConnectionString {get;set;}
        string StoredProcedure {get;set;}
        List<KeyValuePair<string, string>> Parameters {get;set;}
        TSQLJobResult Result {get;set;}
    }
}