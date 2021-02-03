using PDMHelpers;
using System.Data;

namespace PDMDatabase.Core
{
    public abstract class BaseCommand
    {
        public long tableVersion { get; protected set; }
        protected readonly IDbConnection connection;
        public ITraceable Trace { get; set; }

        protected BaseCommand(IDbConnection connection, ITraceable trace, long tableVersion = 1)
        {
            this.connection = connection;
            this.Trace = trace;
            this.tableVersion = tableVersion;
        } 
        public abstract IDbCommand Build();
    }
}
