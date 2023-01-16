using System.Data;

namespace Jobby.Lib.Core.Model {
    public record class TerminalJobResult {
        //TODO: What will the result of a terminal job be like?
        public object? CommandResults;
        public TimeSpan RunTime;
    }
}