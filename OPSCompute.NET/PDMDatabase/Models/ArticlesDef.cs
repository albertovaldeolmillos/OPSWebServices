using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class ArticlesDef : IParseable
    {
        public long DART_ID { get; set; }
        public string DART_DESCSHORT { get; set; }
        public int DART_REQUIRED { get; set; }
        public int DART_VALID { get; set; }
        public int DART_DELETED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            DART_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            DART_DESCSHORT = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            DART_REQUIRED = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            DART_VALID = reader.IsDBNull(3) ? 1 : reader.GetInt32(3);
            DART_DELETED = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
        }
    }
}
