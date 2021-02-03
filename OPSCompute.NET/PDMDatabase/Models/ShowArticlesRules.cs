using PDMDatabase.Core;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class ShowArticlesRules : IParseable
    {
        public long SAR_ID { get; set; }
        public long SAR_DART_ID { get; set; }
        public long SAR_ORDER { get; set; }
        public long SAR_DGRP_ID { get; set; }
        public long SAR_GRP_ID { get; set; }
        public long SAR_UNI_ID { get; set; }
        public long SAR_TIM_ID { get; set; }
        public long SAR_DDAY_ID { get; set; }
        public DateTime SAR_INIDATE { get; set; }
        public DateTime SAR_ENDDATE { get; set; }
        public string SAR_INYEAR_INIDATE { get; set; }
        public string SAR_INYEAR_ENDDATE { get; set; }
        public string SAR_INDAY_INIHOUR { get; set; }
        public string SAR_INDAY_ENDHOUR { get; set; }
        public int SAR_SHOW { get; set; }
        public int SAR_INSERT_PLATE { get; set; }
        public long SAR_SLOT_MANAGING_TYPE { get; set; }
        public long SAR_ELEC_RECH_MANAGING_TYPE { get; set; }
        public long SAR_LIST_BEHAVIOR { get; set; }
        public long SAR_BASE_ARTICLE { get; set; }
        public long SAR_TARIFF_BEHAVIOR { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            SAR_ID                          = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            SAR_DART_ID                     = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            SAR_ORDER                       = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            SAR_DGRP_ID                     = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            SAR_GRP_ID                      = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
            SAR_UNI_ID                      = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
            SAR_TIM_ID                      = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
            SAR_DDAY_ID                     = reader.IsDBNull(7) ? 0 : reader.GetInt64(7);
            SAR_INIDATE                     = reader.IsDBNull(8) ? default(DateTime) : reader.GetDateTime(8);
            SAR_ENDDATE                     = reader.IsDBNull(9) ? default(DateTime) : reader.GetDateTime(9);
            SAR_INYEAR_INIDATE              = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
            SAR_INYEAR_ENDDATE              = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
            SAR_INDAY_INIHOUR               = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);
            SAR_INDAY_ENDHOUR               = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
            SAR_SHOW                        = reader.IsDBNull(14) ? 0 : reader.GetInt32(14);
            SAR_INSERT_PLATE                = reader.IsDBNull(15) ? 0 : reader.GetInt32(15);
            SAR_SLOT_MANAGING_TYPE          = reader.IsDBNull(16) ? 0 : reader.GetInt64(16);
            SAR_ELEC_RECH_MANAGING_TYPE     = reader.IsDBNull(17) ? 0 : reader.GetInt64(17);
            SAR_LIST_BEHAVIOR               = reader.IsDBNull(18) ? 0 : reader.GetInt64(18);
            SAR_BASE_ARTICLE                = reader.IsDBNull(19) ? 0 : reader.GetInt64(19);
            SAR_TARIFF_BEHAVIOR             = reader.IsDBNull(20) ? 0 : reader.GetInt64(20);
        }
    }
}
