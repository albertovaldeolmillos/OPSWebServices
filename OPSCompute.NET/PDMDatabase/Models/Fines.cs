using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{

    public class Fines : IParseable
    {
        public int FIN_ID { get; set; }
        public string FIN_NUMBER { get; set; }
        public string FIN_VEHICLEID { get; set; }
        public string FIN_MODEL{ get; set; }
        public string FIN_MANUFACTURER{ get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            FIN_ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            FIN_NUMBER = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            FIN_VEHICLEID = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            FIN_MODEL = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            FIN_MANUFACTURER = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
        }
    }
}
