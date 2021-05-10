using PDMDatabase.Core;
using PDMHelpers;
using System.Data;

namespace PDMDatabase.Models
{
    public class Operations : IParseable
    {
        public long? OPE_ID { get; set; }
        public long? OPE_DOPE_ID { get; set; }
        public long? OPE_DART_ID { get; set; }
        public long? OPE_GRP_ID { get; set; }
        public COPSDate OPE_MOVDATE { get; set; }
        public COPSDate OPE_INIDATE { get; set; }
        public COPSDate OPE_ENDDATE { get; set; }
        public long? OPE_DURATION { get; set; }
        public long? OPE_UNI_ID { get; set; }
        public string OPE_VEHICLEID { get; set; }
        public long? OPE_VALUE { get; set; }
        public long? OPE_VALUE_VIS { get; set; }
        public long? OPE_DPAY_ID { get; set; }
        public long? OPE_ART_ID { get; set; }
        public int OPE_VALID { get; set; } 
        public int OPE_DELETED { get; set; }
        public long? OPE_POST_DAY{ get; set; }
        public long? OPE_OP_ONLINE { get; set; }


        public void Parse(IDataReader reader, long tableVersion)
        {
            OPE_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt32(0);
            OPE_DOPE_ID = reader.IsDBNull(1) ? null : (long?)reader.GetInt32(1);
            OPE_DART_ID = reader.IsDBNull(2) ? null : (long?)reader.GetInt32(2);
            OPE_GRP_ID = reader.IsDBNull(3) ? null : (long?)reader.GetInt32(3);
            OPE_MOVDATE = reader.IsDBNull(4) ? new COPSDate() : new COPSDate(reader.GetString(4));
            OPE_INIDATE = reader.IsDBNull(5) ? new COPSDate() : new COPSDate(reader.GetString(5));
            OPE_ENDDATE = reader.IsDBNull(6) ? new COPSDate() : new COPSDate(reader.GetString(6));
            OPE_DURATION = reader.IsDBNull(7) ? null : (long?)reader.GetInt32(7);
            OPE_UNI_ID = reader.IsDBNull(8) ? null : (long?)reader.GetInt32(8);
            OPE_VEHICLEID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
            OPE_VALUE = reader.IsDBNull(10) ? null : (long?)reader.GetInt32(10);
            OPE_VALUE_VIS = reader.IsDBNull(11) ? null : (long?)reader.GetInt32(11);
            OPE_DPAY_ID = reader.IsDBNull(12) ? null : (long?)reader.GetInt32(12);
            OPE_ART_ID = reader.IsDBNull(13) ? null : (long?)reader.GetInt32(13);
            OPE_POST_DAY = reader.IsDBNull(14) ? null : (long?)reader.GetInt32(14);
            OPE_OP_ONLINE = reader.IsDBNull(15) ? 1 : (long?)reader.GetInt32(15);

            OPE_VALID = 1;
            OPE_DELETED = 0;
        }

    }
}
