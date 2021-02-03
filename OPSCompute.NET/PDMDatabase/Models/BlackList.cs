using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class BlackList : IParseable
    {
        public long BLIS_ID { get; set; }
        public long BLIS_DBLIS_ID { get; set; }
        public string BLIS_CATEGORY { get; set; }
        public string BLIS_DESCSHORT { get; set; }
        public string BLIS_VALUE { get; set; }
        public string BLIS_TYPE { get; set; }
        public long BLIS_VERSION { get; set; }
        public long BLIS_VALID { get; set; }
        public long BLIS_DELETED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            BLIS_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            BLIS_DBLIS_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            BLIS_CATEGORY = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            BLIS_DESCSHORT = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            BLIS_VALUE = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            BLIS_TYPE = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
            BLIS_VERSION = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
            BLIS_VALID = reader.IsDBNull(7) ? 0 : reader.GetInt64(7);
            BLIS_DELETED = reader.IsDBNull(8) ? 0 : reader.GetInt64(8);

        }
    }
}
