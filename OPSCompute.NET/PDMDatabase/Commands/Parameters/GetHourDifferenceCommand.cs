////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Repositories
{
    public class GetHourDifferenceCommand : BaseCommand
    {
        public GetHourDifferenceCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = "SELECT count(*) cpar FROM parameters WHERE PAR_DESCSHORT='P_HOUR_DIFF'",
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