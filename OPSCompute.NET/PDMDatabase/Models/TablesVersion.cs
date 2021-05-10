using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class TablesVersion : IParseable
    {
        public long TBV_ID { get; set; }
        public string TBV_TABLENAME { get; set; }
        public long TBV_VERSION { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            TBV_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            TBV_TABLENAME = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            TBV_VERSION = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);

    }
    }
}
