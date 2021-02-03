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
    public class ConstraintsRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public ConstraintsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Constraints> GetAll()
        {
            ConstraintsSelectCommand command = new ConstraintsSelectCommand(Connection, Trace   );
            return command.Execute();
        }
    }
}
