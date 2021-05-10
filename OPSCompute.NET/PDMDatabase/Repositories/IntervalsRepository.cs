using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMDatabase.Repositories
{
    public class IntervalsRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public IntervalsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Intervals> GetAll(long version)
        {
            IntervalsSelectCommand command = new IntervalsSelectCommand(Connection,Trace, version);
            return command.Execute();
        }
    }
}
