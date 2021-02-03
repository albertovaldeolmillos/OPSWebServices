using PDMDatabase.Core;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class Articles : IParseable
    {
        public long ART_ID { get; set; }
        public long ART_DART_ID { get; set; }
        public long ART_CUS_ID { get; set; }
        public string ART_VEHICLEID { get; set; }
        public DateTime ART_INIDATE { get; set; }
        public DateTime ART_ENDDATE { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            ART_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            ART_DART_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            ART_CUS_ID = reader.IsDBNull(2) ? 0 : reader.GetInt64(2); 
            ART_VEHICLEID = reader.IsDBNull(3) ? null : reader.GetString(3);
            ART_INIDATE = reader.IsDBNull(4) ? default(DateTime) : reader.GetDateTime(4);
            ART_ENDDATE = reader.IsDBNull(5) ? default(DateTime) : reader.GetDateTime(5);
        }
    }
}
