////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class GroupsSelectCommand : BaseCommand
    {
        public GroupsSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.Groups.Select,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };

        }

        public IEnumerable<Groups> Execute()
        {
            return connection.Query<Groups>(this);
        }
    }
}
