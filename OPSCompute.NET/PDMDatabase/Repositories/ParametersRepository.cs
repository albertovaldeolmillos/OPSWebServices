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
    public class ParametersRepository: IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public ParametersRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public long GetHourDifference()
        {
            GetHourDifferenceCommand command = new GetHourDifferenceCommand(Connection, Trace);
            return command.Execute();
        }
    }
}
