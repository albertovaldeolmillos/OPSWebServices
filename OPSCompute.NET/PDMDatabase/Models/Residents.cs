using PDMDatabase.Core;
using PDMHelpers;
using System;
using System.Data;

namespace PDMDatabase.Models
{
    public class Residents : IParseable
    {
        public long RES_DART_ID { get; set; }
        public long RES_GRP_ID { get; set; }

        public void Parse(IDataReader reader, long tableVersion)
        {
            RES_DART_ID = reader.IsDBNull(0) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(0);
            RES_GRP_ID = reader.IsDBNull(1) ? GlobalDefs.DEF_UNDEFINED_VALUE : reader.GetInt64(1);
        }
    }
}
