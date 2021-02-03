using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class AlarmsDef : IParseable
    {
        public long DALA_ID { get; set; }
        public long DALA_DSTA_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            DALA_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            DALA_DSTA_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        }
    }
}
