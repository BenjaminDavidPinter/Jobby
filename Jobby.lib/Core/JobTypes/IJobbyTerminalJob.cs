namespace Jobby.Lib.Core.JobTypes {
    public interface IJobbyTerminalJob {
        string JobName {get;set;}
        TimeSpan CycleTime {get;set;}
        TimeSpan Timeout {get;set;}
        string CommandText {get;set;}
        string CommandResult {get;set;}
    }
}