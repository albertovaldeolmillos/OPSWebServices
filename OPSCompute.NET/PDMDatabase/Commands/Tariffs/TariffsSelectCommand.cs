//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMDatabase.Core;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.Commands
{
    public class TariffsSelectCommand : BaseCommand
    {
        public TariffsSelectCommand(IDbConnection connection, ITraceable trace, long tableVersion = 1) : base(connection, trace, tableVersion){}

        public override IDbCommand Build() {

            string sql = string.Empty;
            if (tableVersion == 1)
            {
                sql = @"SELECT  TAR_ID, TAR_NUMBER, TAR_TAR_ID, TAR_DISCOUNT, TAR_STAR_ID,
                                TAR_DDAY_ID, TAR_DAY_ID, TAR_TIM_ID, TO_CHAR(TAR_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(TAR_ENDDATE,'HH24MISSDDMMYY'), 
                                TAR_NEXTDAY, TAR_NEXTBLOCK, TAR_NB_CONDITIONAL_VALUE, TAR_MAXTIMEFORNOTAPPLYREENTRY, TAR_RNEXTBLOCKTIME, TAR_RNEXTBLOCKINT, 
                                TAR_RNEXTDAYINT, TAR_RNEXTDAYTIME, TAR_ADDFREETIME 
                        from    TARIFFS 
                        where   TAR_VALID = 1 and TAR_DELETED = 0";
            }
            else if (tableVersion == 2)
            {
                sql =   @"SELECT    TAR_ID, TAR_NUMBER, TAR_TAR_ID, TAR_DISCOUNT, TAR_STAR_ID, 
                                    TAR_DDAY_ID, TAR_DAY_ID, TAR_TIM_ID, TO_CHAR(TAR_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(TAR_ENDDATE,'HH24MISSDDMMYY'), 
                                    TAR_NEXTDAY, TAR_NEXTBLOCK, TAR_NB_CONDITIONAL_VALUE, TAR_MAXTIMEFORNOTAPPLYREENTRY, TAR_RNEXTBLOCKTIME, TAR_RNEXTBLOCKINT, 
                                    TAR_RNEXTDAYINT, TAR_RNEXTDAYTIME, TAR_ADDFREETIME, TAR_ROUNDTOENDOFDAY 
                            FROM    TARIFFS 
                            WHERE   TAR_VALID = 1 and TAR_DELETED = 0";

            }
            else if (tableVersion == 3)
            {
                sql = @"SELECT    TAR_ID, TAR_NUMBER, TAR_TAR_ID, TAR_DISCOUNT, TAR_STAR_ID, 
                                    TAR_DDAY_ID, TAR_DAY_ID, TAR_TIM_ID, TO_CHAR(TAR_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(TAR_ENDDATE,'HH24MISSDDMMYY'), 
                                    TAR_NEXTDAY, TAR_NEXTBLOCK, TAR_NB_CONDITIONAL_VALUE, TAR_MAXTIMEFORNOTAPPLYREENTRY, TAR_RNEXTBLOCKTIME, TAR_RNEXTBLOCKINT, 
                                    TAR_RNEXTDAYINT, TAR_RNEXTDAYTIME, TAR_ADDFREETIME, TAR_ROUNDTOENDOFDAY, TAR_NUMDAYS_PASSED
                            FROM    TARIFFS 
                            WHERE   TAR_VALID = 1 and TAR_DELETED = 0";

            }

            return new OracleCommand
            {
                CommandText = sql,
                CommandType = CommandType.Text,
                Connection = connection as OracleConnection
            };
        }

        public IEnumerable<Tariffs> Execute()
        {
            return connection.Query<Tariffs>(this);
        }
    }


}
