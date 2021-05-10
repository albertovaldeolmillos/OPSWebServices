using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Intervals : IParseable
    {
        public long? INT_ID { get; set; }
        public long? INT_STAR_ID { get; set; }
        public long? INT_MINUTES { get; set; }
        public long? INT_VALUE { get; set; }
        public long? INT_VALID_INTERMEDIATE_POINTS { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            INT_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            INT_STAR_ID = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
            INT_MINUTES = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
            INT_VALUE = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);

            if (tableVersion == 1) {
                INT_VALID_INTERMEDIATE_POINTS = 1;
            }

            if (tableVersion >= 2)
            {
                INT_VALID_INTERMEDIATE_POINTS = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4);
            }

        }
    }


}
