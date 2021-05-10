using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class Groups : IParseable
    {
        public long GRP_ID { get; set; }
        public long GRP_DGRP_ID { get; set; }
        public string GRP_DESCSHORT { get; set; }
        public string GRP_DESCLONG  { get; set; }
        public long GRP_RELATED { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            GRP_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            GRP_DESCSHORT = reader.IsDBNull(1) ? null : reader.GetString(1);
            GRP_DESCLONG = reader.IsDBNull(2) ? null : reader.GetString(2);
            GRP_DGRP_ID = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
            GRP_RELATED = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
        }
    }
}
