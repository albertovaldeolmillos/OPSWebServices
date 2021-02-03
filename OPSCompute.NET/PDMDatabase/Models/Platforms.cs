using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Platforms : IParseable
    {
        public long PLAT_ID { get; set; }
        public long PLAT_PHYSICAL_ADDRESS { get; set; }
        public long PLAT_UNI_ID { get; set; }
        public long PLAT_NUMBER { get; set; }
        public long PLAT_SIDE { get; set; }
        public long PLAT_ORDER { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            PLAT_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            PLAT_PHYSICAL_ADDRESS = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            PLAT_UNI_ID = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            PLAT_NUMBER = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            PLAT_SIDE = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
            PLAT_ORDER = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
        }
    }
}
