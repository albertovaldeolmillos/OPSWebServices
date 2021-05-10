////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class GetActualPayedQuantityForOperationCommand : BaseCommand
    {
        public long OperationId { get; }

        public GetActualPayedQuantityForOperationCommand(long operationId, IDbConnection connection, ITraceable trace) : base(connection, trace) {
            OperationId = operationId;
        }

        public override IDbCommand Build() {

            string sql = $@" SELECT  OPE_VALUE_VIS
                            FROM    OPERATIONS
                            WHERE   OPE_VALID = 1 AND OPE_DELETED = 0 AND OPE_ID = {OperationId}
                            ORDER BY OPE_MOVDATE ASC";

            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public long Execute()
        {
            return connection.ExecuteScalar<long>(this);
        }
    }


}
