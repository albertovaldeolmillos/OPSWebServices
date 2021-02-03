using PDMDatabase.Core;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class AlarmsDevHis : IParseable
    {
        public long HDALA_ID { get; set; }
        public string HDALA_MASK { get; set; }
        public long HDALA_DSTA_ID { get; set; }
        public DateTime HDALA_DATETIME { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            HDALA_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            HDALA_MASK = reader.IsDBNull(1) ? null : reader.GetString(1);
            HDALA_DSTA_ID = reader.IsDBNull(2) ? 1 : reader.GetInt64(2);
            HDALA_DATETIME = reader.IsDBNull(3) ? default(DateTime) : reader.GetDateTime(3);
        }
    }


}
