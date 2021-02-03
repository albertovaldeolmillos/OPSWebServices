using PDMDatabase.Models;
using System;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_AlarmsDef : InMemoryTable<AlarmsDef>
    {
        public IMT_AlarmsDef(IDbConnection connection) : base(connection)
        {
        }

        public override void LoadData()
        {
            throw new NotImplementedException();
        }
    }
}
