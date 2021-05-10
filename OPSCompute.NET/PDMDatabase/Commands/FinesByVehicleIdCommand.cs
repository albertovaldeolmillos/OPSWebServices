////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class FinesByVehicleIdCommand : BaseCommand
    {
        private readonly string vehicleId;

        public FinesByVehicleIdCommand(string vehicleId, IDbConnection connection, ITraceable trace) : base(connection, trace) {

            if (string.IsNullOrEmpty(vehicleId))
            {
                throw new System.ArgumentException("Error creating finesVehicleById the parameter is invalid", nameof(vehicleId));
            }

            this.vehicleId = vehicleId;
        }

        public override IDbCommand Build()
        {
            string sqlText = $"{Sql.Fines.Select} WHERE FIN_VEHICLEID='{vehicleId}' ";

            return new OracleCommand
            {
                CommandText = sqlText,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Fines> Execute()
        {
            return connection.Query<Fines>(this);
        }
    }

    //'5525CFH'
}
