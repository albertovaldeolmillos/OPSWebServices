////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Commands
{
    public class OperationsCountCommand : BaseCommand
    {
        public OperationsCountCommand(IDbConnection connection, ITraceable trace) : base(connection, trace)
        {
        }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.Operation.Count,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection,
            };
        }

        public Double Execute()
        {
            return connection.ExecuteScalar<Double>(this);
        }

    }

}
