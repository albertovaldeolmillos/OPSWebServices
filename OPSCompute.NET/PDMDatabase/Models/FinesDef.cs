using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class FinesDef :IParseable
    {
        public long DFIN_ID { get; set; }
        public string DFIN_CATEGORY { get; set; }
        public string DFIN_DESCSHORT { get; set; }
        public float DFIN_VALUE { get; set; }
        public long DFIN_PAYINPDM { get; set; }
        public long DFIN_NUMTICKETS { get; set; }
        public int DFIN_STATUS { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            DFIN_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            DFIN_CATEGORY = reader.IsDBNull(1) ? string.Empty  : reader.GetString(1);
            DFIN_DESCSHORT = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            DFIN_VALUE = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            DFIN_PAYINPDM = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
            DFIN_NUMTICKETS = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
            DFIN_STATUS = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
        }
    }
}
