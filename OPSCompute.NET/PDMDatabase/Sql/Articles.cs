namespace PDMDatabase.Sql
{
    public static class Articles
    {
        public static readonly string Select = @"SELECT ART_ID, ART_DART_ID, ART_CUS_ID, ART_VEHICLEID, TO_CHAR(ART_INIDATE,'HH24MISSDDMMYY'), TO_CHAR(ART_ENDDATE,'HH24MISSDDMMYY') 
                                                 FROM ARTICLES 
                                                 WHERE ART_VALID = 1 and ART_DELETED = 0";
        public static readonly string Count = @"SELECT COUNT(ART_ID) 
                                                FROM ARTICLES 
                                                WHERE ART_VALID = 1 and ART_DELETED = 0";

        public static string SelectById(long articleId, string date) {
            return  $@"select  ART_DART_ID,ART_CUS_ID,ART_VEHICLEID,TO_CHAR(ART_INIDATE,'HH24MISSDDMMYY'),TO_CHAR(ART_ENDDATE,'HH24MISSDDMMYY') 
                       from    articles 
                       where   ART_ID ={articleId} AND 
		                       (
                                   (ART_INIDATE IS NULL OR ART_ENDDATE IS NULL) OR 
                                   (ART_INIDATE<=TO_DATE('{date}','HH24MISSDDMMYY') AND ART_ENDDATE>=TO_DATE('{date}','HH24MISSDDMMYY'))
                               ) AND 
                               ART_STATUS = 1 AND ART_VALID = 1 AND ART_DELETED =0 ";
        }

    }
}
