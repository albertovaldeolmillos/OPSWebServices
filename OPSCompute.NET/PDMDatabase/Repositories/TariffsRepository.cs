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
    public class TariffsRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public TariffsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Tariffs> GetAll(long version)
        {
            TariffsSelectCommand command = new TariffsSelectCommand(Connection, Trace, version);
            return command.Execute();
        }
    }
}
