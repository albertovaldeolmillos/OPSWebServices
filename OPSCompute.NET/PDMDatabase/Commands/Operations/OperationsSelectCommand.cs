////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class OperationsSelectCommand : BaseCommand
    {
        public OperationsSelectCommand(IDbConnection connection, ITraceable trace) : base(connection, trace) { }

        public override IDbCommand Build() {

            string sql = @" SELECT  OPE_ID, OPE_DOPE_ID, OPE_DART_ID, OPE_GRP_ID, 
                                    TO_CHAR(OPE_MOVDATE, 'HH24MISSDDMMYY'), TO_CHAR(OPE_INIDATE, 'HH24MISSDDMMYY'), TO_CHAR(OPE_ENDDATE, 'HH24MISSDDMMYY'), OPE_DURATION, OPE_UNI_ID,  
				                    OPE_VEHICLEID, OPE_VALUE, OPE_VALUE_VIS, OPE_DPAY_ID, OPE_ART_ID,  OPE_POST_PAY, OPE_OP_ONLINE
                            FROM    OPERATIONS
                            WHERE   OPE_VALID = 1 and OPE_DELETED = 0
                            ORDER BY OPE_MOVDATE ASC";

            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Operations> Execute()
        {
            return connection.Query<Operations>(this);
        }
    }


}
