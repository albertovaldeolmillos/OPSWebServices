////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class GroupsChildsSelectCommand : BaseCommand
    {
        public GroupsChildsSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.GroupsChilds.Select,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };

        }

        public IEnumerable<GroupsChilds> Execute()
        {
            return connection.Query<GroupsChilds>(this);
        }
    }
}
