////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class ArticlesRulesSelectCommand : BaseCommand
    {
        public long DbVersion { get; }

        public ArticlesRulesSelectCommand(long dbVersion, IDbConnection connection, ITraceable trace) : base(connection, trace) {
            DbVersion = dbVersion;
        }
        

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.ArticlesRules.Select(DbVersion),
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };

        }

        public IEnumerable<ArticlesRules> Execute()
        {
            return connection.Query<ArticlesRules>(this);
        }
    }
}
