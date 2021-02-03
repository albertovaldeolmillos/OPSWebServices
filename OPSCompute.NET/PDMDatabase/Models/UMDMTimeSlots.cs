using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class UMDMTimeSlots: IParseable
    {
        public long UTS_ID { get; set; }
        public long UTS_TIME_INI { get; set; }
        public long UTS_TIME_END { get; set; }
        public string UTS_ENABLED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            UTS_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            UTS_TIME_INI = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            UTS_TIME_END = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            UTS_ENABLED = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        }
    }
}
