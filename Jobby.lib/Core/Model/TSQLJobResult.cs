using System.Data;

namespace Jobby.Lib.Core.Model {
    public record class TSQLJobResult {
        public int ImpactedRecords;
        public TimeSpan RunTime;
        public DataSet? Result;
    }
}