using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class Tariffs : IParseable
    {
        public long? TAR_ID { get; set; }
        public long? TAR_NUMBER { get; set; }
        public long? TAR_TAR_ID { get; set; }
        public long? TAR_DISCOUNT { get; set; }
        public long? TAR_STAR_ID { get; set; }
        public long? TAR_DDAY_ID { get; set; }
        public long? TAR_DAY_ID { get; set; }
        public long? TAR_TIM_ID { get; set; }
        public COPSDate TAR_INIDATE { get; set; }
        public COPSDate TAR_ENDDATE { get; set; }
        public int? TAR_NEXTDAY { get; set; }
        public int? TAR_NEXTBLOCK { get; set; }
        public int? TAR_NB_CONDITIONAL_VALUE { get; set; }
        public int? TAR_MAXTIMEFORNOTAPPLYREENTRY { get; set; }
        public int? TAR_RNEXTBLOCKTIME { get; set; }
        public int? TAR_RNEXTBLOCKINT { get; set; }
        public int? TAR_RNEXTDAYINT { get; set; }
        public int? TAR_RNEXTDAYTIME { get; set; }
        public int? TAR_ADDFREETIME { get; set; }
        public int? TAR_ROUNDTOENDOFDAY { get; set; }
        public int? TAR_NUMDAYS_PASSED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            TAR_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            TAR_NUMBER = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
            TAR_TAR_ID = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
            TAR_DISCOUNT = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
            TAR_STAR_ID = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4);
            TAR_DDAY_ID = reader.IsDBNull(5) ? null : (long?)reader.GetInt64(5);
            TAR_DAY_ID = reader.IsDBNull(6) ? null : (long?)reader.GetInt64(6);
            TAR_TIM_ID = reader.IsDBNull(7) ? null : (long?)reader.GetInt64(7);
            TAR_INIDATE = reader.IsDBNull(8) ? new COPSDate() : new COPSDate(reader.GetString(8));
            TAR_ENDDATE = reader.IsDBNull(9) ? new COPSDate() : new COPSDate(reader.GetString(9));
            TAR_NEXTDAY = reader.IsDBNull(10) ? null : (int?)reader.GetInt32(10);
            TAR_NEXTBLOCK = reader.IsDBNull(11) ? null : (int?)reader.GetInt32(11);
            TAR_NB_CONDITIONAL_VALUE = reader.IsDBNull(12) ? null : (int?)reader.GetInt32(12);
            TAR_MAXTIMEFORNOTAPPLYREENTRY = reader.IsDBNull(13) ? null : (int?)reader.GetInt32(13);
            TAR_RNEXTBLOCKTIME = reader.IsDBNull(14) ? null : (int?)reader.GetInt32(14);
            TAR_RNEXTBLOCKINT = reader.IsDBNull(15) ? null : (int?)reader.GetInt32(15);
            TAR_RNEXTDAYINT = reader.IsDBNull(16) ? null : (int?)reader.GetInt32(16);
            TAR_RNEXTDAYTIME = reader.IsDBNull(17) ? null : (int?)reader.GetInt32(17);
            TAR_ADDFREETIME = reader.IsDBNull(18) ? null : (int?)reader.GetInt32(18);

            if (tableVersion == 1) {
                TAR_ROUNDTOENDOFDAY = 0;
                TAR_NUMDAYS_PASSED = null;
            }

            if (tableVersion >= 2)
            {
                TAR_ROUNDTOENDOFDAY = reader.IsDBNull(19) ? null : (int?)reader.GetInt32(19);
                TAR_NUMDAYS_PASSED = null;
            }

            if (tableVersion >= 3)
            {
                TAR_NUMDAYS_PASSED = reader.IsDBNull(20) ? null : (int?)reader.GetInt32(20);
            }

        }
    }
}
