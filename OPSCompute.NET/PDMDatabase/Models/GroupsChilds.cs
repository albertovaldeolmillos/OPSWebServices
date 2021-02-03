using PDMDatabase.Core;
using System.Data;

namespace PDMDatabase.Models
{
    public class GroupsChilds : IParseable
    {
        public long CGRP_ID { get; set; }
        public string CGRP_TYPE { get; set; }
        public long CGRP_CHILD { get; set; }
        public long CGRP_ORDER { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            CGRP_ID = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            CGRP_TYPE = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            CGRP_CHILD = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            CGRP_ORDER = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
        }
    }


}
