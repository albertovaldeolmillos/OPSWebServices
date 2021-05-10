using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class ElecChargers : IParseable
    {
        public long ECH_ID { get; set; }
        public long ECH_PHYSICAL_ADDRESS { get; set; }
        public long ECH_UNI_ID { get; set; }
        public long ECH_PLUGS_NUMBER { get; set; }
        public string ECH_EXT_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            ECH_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            ECH_PHYSICAL_ADDRESS = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            ECH_UNI_ID = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            ECH_PLUGS_NUMBER = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            ECH_EXT_ID = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
        }
    }
}
