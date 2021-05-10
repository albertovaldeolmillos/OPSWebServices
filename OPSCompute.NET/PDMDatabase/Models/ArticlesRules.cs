using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class ArticlesRules : IParseable
    {
        public long? RUL_ID { get; set; }
        public long? RUL_DART_ID { get; set; }
        public long? RUL_TAR_ID { get; set; }
        public long? RUL_DGRP_ID { get; set; }
        public long? RUL_CON_ID { get; set; }
        public long? RUL_GRP_ID { get; set; }
        public long? RUL_DDAY_ID { get; set; }
        public COPSDate RUL_INIDATE { get; set; }
        public COPSDate RUL_ENDDATE { get; set; }
        public long? RUL_TIM_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            
            RUL_ID      = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            RUL_DART_ID = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
            RUL_TAR_ID  = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
            RUL_DGRP_ID = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
            RUL_CON_ID  = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4);
            RUL_GRP_ID  = reader.IsDBNull(5) ? null : (long?)reader.GetInt64(5);
            RUL_DDAY_ID = reader.IsDBNull(6) ? null : (long?)reader.GetInt64(6);
            RUL_INIDATE = reader.IsDBNull(2) ? null : new COPSDate(reader.GetString(7));
            RUL_ENDDATE = reader.IsDBNull(2) ? null : new COPSDate(reader.GetString(8));

            if (tableVersion == 1)
            {
                RUL_TIM_ID = null;
            }
            if (tableVersion >= 2)
            {
                RUL_TIM_ID = reader.IsDBNull(9) ? null : (long?)reader.GetInt64(9);
            }

            
        }
    }
}
