////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class TablesVersionSelectCommand : BaseCommand
    {
        public TablesVersionSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.TablesVersion.Select,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };

        }

        public IEnumerable<TablesVersion> Execute()
        {
            return connection.Query<TablesVersion>(this);
        }
    }
}
