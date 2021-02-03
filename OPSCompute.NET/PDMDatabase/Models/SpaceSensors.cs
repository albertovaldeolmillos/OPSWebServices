using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class SpaceSensors : IParseable
    {
        public long SSE_ID { get; set; }
        public long SSE_PHYSICAL_ADDRESS { get; set; }
        public long SSE_UNI_ID { get; set; }
        public string SSE_EXT_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            SSE_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            SSE_PHYSICAL_ADDRESS = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            SSE_UNI_ID = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            SSE_EXT_ID = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        }
    }
}
