////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class ArticlesFindByIdCommand : BaseCommand
    {
        private readonly long articleId;
        private readonly string  date;

        public ArticlesFindByIdCommand(long articleId, string date, IDbConnection connection, ITraceable trace) : base(connection, trace) {
            this.articleId = articleId;
            this.date= date;
        }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = Sql.Articles.SelectById(articleId, date),
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Articles> Execute()
        {
            return connection.Query<Articles>(this);
        }
    }
}
