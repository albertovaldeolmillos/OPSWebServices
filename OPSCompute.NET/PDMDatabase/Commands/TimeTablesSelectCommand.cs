//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class TimeTablesSelectCommand : BaseCommand
    {
        public TimeTablesSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.TimeTables.Select,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };

        }

        public IEnumerable<Timetables> Execute()
        {
            return connection.Query<Timetables>(this);
        }
    }
}
