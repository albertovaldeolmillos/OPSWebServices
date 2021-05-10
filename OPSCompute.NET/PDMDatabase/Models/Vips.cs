using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class Vips : IParseable
    {
        public long VIP_DART_ID { get; set; }
        public long VIP_GRP_ID { get; set; }
        public string VIP_DAYOFWEEK { get; set; }
        public string VIP_INIDATE { get; set; }
        public string VIP_ENDDATE { get; set; }
        public long VIP_INIHOUR { get; set; }
        public long VIP_INIMINUTE { get; set; }
        public long VIP_ENDHOUR { get; set; }
        public long VIP_ENDMINUTE { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            VIP_DART_ID = reader.IsDBNull(0) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(0);
            VIP_GRP_ID = reader.IsDBNull(1) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(1);
            VIP_DAYOFWEEK = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            VIP_INIDATE = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            VIP_ENDDATE = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            VIP_INIHOUR = reader.IsDBNull(5) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(5);
            VIP_INIMINUTE = reader.IsDBNull(6) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(6);
            VIP_ENDHOUR = reader.IsDBNull(7) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(7);
            VIP_ENDMINUTE = reader.IsDBNull(8) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(8);
        }
    }
}
