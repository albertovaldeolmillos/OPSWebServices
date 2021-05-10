namespace PDMDatabase.Sql
{
    public static class Operation
    {
        public static readonly string Select = @"
            SELECT  OPE_ID, OPE_DOPE_ID, OPE_DART_ID, OPE_GRP_ID, 
                    TO_CHAR(OPE_MOVDATE, 'HH24MISSDDMMYY'), TO_CHAR(OPE_INIDATE, 'HH24MISSDDMMYY'), TO_CHAR(OPE_ENDDATE, 'HH24MISSDDMMYY'), OPE_DURATION, OPE_UNI_ID,  
			        OPE_VALUE, OPE_VEHICLEID, OPE_DPAY_ID, OPE_ART_ID
            FROM    OPERATIONS 
            WHERE   OPE_VALID = 1 and OPE_DELETED = 0
            ORDER BY OPE_MOVDATE ASC";

        public static readonly string Count = @"
            SELECT  Count(OPE_ID) Count
            FROM    OPERATIONS 
            WHERE   OPE_VALID = 1 and OPE_DELETED = 0
            ORDER BY OPE_MOVDATE ASC";
    }
}
