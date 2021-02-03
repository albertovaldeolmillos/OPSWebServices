////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMDatabase.Commands
{
    public class TariffsConstraintsMaxVersionCommand : BaseCommand
    {
        public TariffsConstraintsMaxVersionCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = "select max(con_version) from constraints",
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public long Execute() {
            return connection.ExecuteScalar<long>(this);
        }
    }
}
