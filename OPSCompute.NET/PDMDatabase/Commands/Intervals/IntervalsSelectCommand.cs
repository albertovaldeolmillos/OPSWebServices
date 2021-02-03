////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class IntervalsSelectCommand : BaseCommand
    {
        public IntervalsSelectCommand(IDbConnection connection, ITraceable trace, long tableVersion = 1) : base(connection, trace, tableVersion){}

        public override IDbCommand Build() {

            string sql = string.Empty;
            if (tableVersion == 1)
            {
                sql = @"SELECT      INT_ID, INT_STAR_ID, INT_MINUTES, INT_VALUE
                        FROM        INTERVALS 
                        WHERE       INT_VALID = 1 and INT_DELETED = 0
                        ORDER BY    INT_MINUTES";
            }
            else if (tableVersion == 2)
            {
                sql = @"SELECT      INT_ID, INT_STAR_ID, INT_MINUTES, INT_VALUE, INT_VALID_INTERMEDIATE_POINTS
                        FROM        INTERVALS 
                        WHERE       INT_VALID = 1 and INT_DELETED = 0
                        ORDER BY    INT_MINUTES";
            }


            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Intervals> Execute()
        {
            return connection.Query<Intervals>(this);
        }
    }


}
