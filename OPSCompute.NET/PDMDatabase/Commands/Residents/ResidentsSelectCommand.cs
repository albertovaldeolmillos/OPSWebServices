////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class ResidentsSelectCommand : BaseCommand
    {
        readonly string _vehicleId;

        public ResidentsSelectCommand(string vehicleId, IDbConnection connection, ITraceable trace, long tableVersion = 1) : base(connection, trace, tableVersion)
        {
            _vehicleId = vehicleId;
        }

        public override IDbCommand Build() {
            string sql = $@"SELECT  RES_DART_ID, RES_GRP_ID 
                            FROM    RESIDENTS
                            WHERE   RES_VEHICLEID='{_vehicleId}'";

            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Residents> Execute()
        {
            return connection.Query<Residents>(this);
        }
    }


}
