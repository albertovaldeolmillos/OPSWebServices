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
    public class OperationsRepository : IRepository
    {
        public IDbConnection Connection { get; }
        public ITraceable Trace { get; set; }

        public OperationsRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public virtual IEnumerable<Operations> GetAll()
        {
            OperationsSelectCommand command = new OperationsSelectCommand(Connection, Trace);
            return command.Execute();
        }

        public virtual IEnumerable<Operations> GetByVehicleIdAndArticleDef(string vehicleId, long articleDef, bool sorted)
        {
            OperationsSelectByVehicleIdAndArticleDefCommand command = new OperationsSelectByVehicleIdAndArticleDefCommand(vehicleId, articleDef, sorted, Connection, Trace);
            return command.Execute();
        }

        public long GetActualPayedQuantity(long operationId)
        {
            GetActualPayedQuantityForOperationCommand command = new GetActualPayedQuantityForOperationCommand(operationId, Connection, Trace);
            return command.Execute();
        }

        public virtual Operations GetLastOperationInfo(string vehicleId, string date)
        {
            LastOperationInfoCommand command = new LastOperationInfoCommand(vehicleId, date, Connection, Trace);
            return command.Execute().First();
        }
    }
}
