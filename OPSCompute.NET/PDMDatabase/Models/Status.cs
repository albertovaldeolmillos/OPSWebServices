using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Status : IParseable
    {
        public long? STA_ID { get; set; }
        public long? STA_PSTA_ID { get; set; }
        public long? STA_DGRP_ID { get; set; }
        public long? STA_GRP_ID { get; set; }
        public long? STA_DDAY_ID { get; set; }
        public long? STA_DAY_ID { get; set; }
        public long? STA_TIM_ID { get; set; }
        public long? STA_UNI_ID { get; set; }
        public long? STA_DSTA_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            STA_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            STA_PSTA_ID = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
            STA_DGRP_ID = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
            STA_GRP_ID = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
            STA_DDAY_ID = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4);
            STA_DAY_ID = reader.IsDBNull(5) ? null : (long?)reader.GetInt64(5);
            STA_TIM_ID = reader.IsDBNull(6) ? null : (long?)reader.GetInt64(6);
            STA_UNI_ID = reader.IsDBNull(7) ? null : (long?)reader.GetInt64(7);
            STA_DSTA_ID = reader.IsDBNull(8) ? null : (long?)reader.GetInt64(8);
        }
    }
}
