using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class DaysDef : IParseable
    {
        public long? DDAY_ID { get; set; }
        public string DDAY_DESC { get; set; }
        public string DDAY_CODE { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            DDAY_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            DDAY_DESC = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            DDAY_CODE = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            
        }
    }
}
