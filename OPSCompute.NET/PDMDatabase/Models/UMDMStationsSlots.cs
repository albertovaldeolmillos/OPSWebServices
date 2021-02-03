using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class UMDMStationsSlots : IParseable
    {
        public long USS_ID { get; set; }
        public long USS_US_ID { get; set; }
        public long USS_ORDER_STATION { get; set; }
        public long USS_ORDER_WEB { get; set; }
        public string USS_ENABLED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            USS_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            USS_US_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            USS_ORDER_STATION = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            USS_ORDER_WEB = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            USS_ENABLED = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
        }
    }
}
