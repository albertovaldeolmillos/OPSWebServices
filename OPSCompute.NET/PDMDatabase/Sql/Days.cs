namespace PDMDatabase.Sql
{
    public static class Days
    {
        public static readonly string Select = @"SELECT DAY_ID, DAY_DDAY_ID, TO_CHAR(DAY_DATE,'HH24MISSDDMMYY') 
                                                 FROM   DAYS 
                                                 WHERE  DAY_VALID = 1 and DAY_DELETED = 0";
    }
}
