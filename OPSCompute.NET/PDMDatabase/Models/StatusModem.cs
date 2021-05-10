using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class StatusModem : IParseable
    {
        public long MSTA_ID { get; set; }
        public long MSTA_PSTA_ID { get; set; }
        public long MSTA_DGRP_ID { get; set; }
        public long MSTA_GRP_ID { get; set; }
        public long MSTA_DDAY_ID { get; set; }
        public long MSTA_DAY_ID { get; set; }
        public long MSTA_TIM_ID { get; set; }
        public long MSTA_UNI_ID { get; set; }
        public long MSTA_DSTA_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            MSTA_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            MSTA_PSTA_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            MSTA_DGRP_ID = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            MSTA_GRP_ID = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            MSTA_DDAY_ID = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
            MSTA_DAY_ID = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
            MSTA_TIM_ID = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
            MSTA_UNI_ID = reader.IsDBNull(7) ? 0 : reader.GetInt64(7);
            MSTA_DSTA_ID = reader.IsDBNull(8) ? 0 : reader.GetInt64(8);
        }
    }
}
