//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class VipsSelectCommand : BaseCommand
    {
        readonly string _vehicleId;

        public VipsSelectCommand(string vehicleId, IDbConnection connection, ITraceable trace, long tableVersion = 1) : base(connection, trace, tableVersion)
        {
            _vehicleId = vehicleId;
        }

        public override IDbCommand Build() {
            string sql = $@"SELECT   VIP_DART_ID,
                                    NVL(VIP_GRP_ID, {GlobalDefs.DEF_UNDEFINED_VALUE}) VIP_GRP_ID, 
                                    NVL(VIP_DAYOFWEEK, '') VIP_DAYOFWEEK,
                                    NVL(TO_CHAR(VIP_INIDATE,'HH24MISSDDMMYY'), '') VIP_INIDATE, 
                                    NVL(TO_CHAR(VIP_ENDDATE,'HH24MISSDDMMYY'), '') VIP_ENDDATE, 
                                    NVL(VIP_INIHOUR, {GlobalDefs.DEF_UNDEFINED_VALUE}) VIP_INIHOUR, 
                                    NVL(VIP_INIMINUTE, {GlobalDefs.DEF_UNDEFINED_VALUE}) VIP_INIMINUTE, 
                                    NVL(VIP_ENDHOUR, {GlobalDefs.DEF_UNDEFINED_VALUE}) VIP_ENDHOUR, 
                                    NVL(VIP_ENDMINUTE, {GlobalDefs.DEF_UNDEFINED_VALUE}) VIP_ENDMINUTE 
                            FROM  VIPS 
                            WHERE VIP_VEHICLEID='{_vehicleId}'";

            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Vips> Execute()
        {
            return connection.Query<Vips>(this);
        }
    }


}
