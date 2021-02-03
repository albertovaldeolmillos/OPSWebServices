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
    public class VipsRepository: IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public VipsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Vips> GetByVehicleId(string vehicleId, long version = 1)
        {
            VipsSelectCommand command = new VipsSelectCommand(vehicleId, Connection, Trace, version);
            return command.Execute();
        }
    }
}
