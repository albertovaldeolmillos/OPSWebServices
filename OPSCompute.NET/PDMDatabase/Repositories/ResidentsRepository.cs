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
    public class ResidentsRepository: IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public ResidentsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Residents> GetByVehicleId(string vehicleId, long version = 1)
        {
            ResidentsSelectCommand command = new ResidentsSelectCommand(vehicleId, Connection, Trace, version);
            return command.Execute();
        }
    }
}
