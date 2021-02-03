////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Repositories
{
    public class ConstraintsSelectCommand :BaseCommand
    {
        public ConstraintsSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace    ) { }

        public override IDbCommand Build()
        {
            return new OracleCommand
            {
                CommandText = "SELECT CON_ID, CON_NUMBER, CON_DGRP_ID, CON_GRP_ID, CON_DCON_ID, CON_VALUE FROM CONSTRAINTS WHERE CON_VALID = 1 and CON_DELETED = 0",
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Constraints> Execute()
        {
            return connection.Query<Constraints>(this);
        }
    }
}