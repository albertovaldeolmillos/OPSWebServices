namespace PDMDatabase.Sql
{
    public static class TablesVersion
    {
        public static readonly string Select = @"select TBV_ID, TBV_TABLENAME, TBV_VERSION from TABLES_VERSION";

        public static readonly string Count = @"select count(*) from TABLES_VERSION";
    }
}
