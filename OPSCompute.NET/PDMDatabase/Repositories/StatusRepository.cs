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
    public class StatusRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public StatusRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Status> GetAll()
        {
            StatusGetAllCommand command = new StatusGetAllCommand(Connection, Trace);
            return command.Execute();
        }
        
    }
}
