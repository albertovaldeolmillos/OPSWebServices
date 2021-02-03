namespace PDMDatabase.Sql
{
    public static class TimeTables
    {
        public static readonly string Select = @"SELECT TIM_ID, TIM_INI, TIM_END 
			                                     FROM TIMETABLES 
                                                 WHERE TIM_VALID = 1 and TIM_DELETED = 0
			                                     ORDER BY TIM_INI";

        public static readonly string Count = @"SELECT COUNT(*) 
                                                 FROM TIMETABLES 
                                                 WHERE TIM_VALID = 1 and TIM_DELETED = 0";
    }
}
