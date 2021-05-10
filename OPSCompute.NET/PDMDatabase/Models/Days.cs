using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class Days : IParseable
    {
        public long DAY_ID { get; set; }
        public long DAY_DDAY_ID { get; set; }
        public COPSDate DAY_DATE { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            DAY_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            DAY_DDAY_ID = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
            DAY_DATE = reader.IsDBNull(2) ? new COPSDate() : new COPSDate(reader.GetString(2));
        }
    }
}
