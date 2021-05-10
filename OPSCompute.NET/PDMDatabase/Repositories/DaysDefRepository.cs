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
    public class DaysDefRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public DaysDefRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<DaysDef> GetAll()
        {
            DaysDefSelectCommand command = new DaysDefSelectCommand(Connection, Trace);
            return command.Execute();
        }
    }
}
