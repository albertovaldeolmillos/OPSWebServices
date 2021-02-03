namespace PDMDatabase.Sql
{
    public static class ArticlesRules
    {
        public static string Select(long dbVersion) {
            string sql = string.Empty;

            if (dbVersion == 1)
            {
                sql = @"SELECT  RUL_ID, RUL_DART_ID, RUL_TAR_ID, RUL_DGRP_ID, RUL_CON_ID, RUL_GRP_ID, 
                                RUL_DDAY_ID, TO_CHAR(RUL_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(RUL_ENDDATE,'HH24MISSDDMMYY') 
                        FROM    ARTICLES_RULES 
                        WHERE   RUL_VALID = 1 and RUL_DELETED = 0";
            }
            if (dbVersion == 2)
            {
                sql = @"SELECT  RUL_ID, RUL_DART_ID, RUL_TAR_ID, RUL_DGRP_ID, RUL_CON_ID, RUL_GRP_ID, 
                                RUL_DDAY_ID, TO_CHAR(RUL_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(RUL_ENDDATE,'HH24MISSDDMMYY'), RUL_TIM_ID 
                        FROM    ARTICLES_RULES 
                        WHERE   RUL_VALID = 1 and RUL_DELETED = 0";
            }

            return sql;
        }

        public static readonly string Count = @"SELECT COUNT(*) 
                                                 FROM ARTICLES_RULES 
                                                 WHERE RUL_VALID = 1 AND RUL_DELETED = 0";
    }
}
