using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Constraints : IParseable
    {
        public long? CON_ID { get; set; }
        public long? CON_NUMBER { get; set; }
        public long? CON_DGRP_ID { get; set; }
        public long? CON_GRP_ID { get; set; }
        public long? CON_DCON_ID { get; set; }
        public long? CON_VALUE { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            CON_ID = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
            CON_NUMBER = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
            CON_DGRP_ID = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
            CON_GRP_ID = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
            CON_DCON_ID = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4);
            CON_VALUE = reader.IsDBNull(5) ? null : (long?)reader.GetInt64(5);
        }
    }
}
