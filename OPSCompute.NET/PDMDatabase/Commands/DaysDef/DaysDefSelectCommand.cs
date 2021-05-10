////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class DaysDefSelectCommand : BaseCommand
    {
        public DaysDefSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = "SELECT  DDAY_ID, DDAY_DESCSHORT, DDAY_CODE FROM DAYS_DEF WHERE DDAY_VALID = 1 AND DDAY_DELETED = 0",
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<DaysDef> Execute()
        {
            return connection.Query<DaysDef>(this);
        }
    }
}