using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PDMDatabase.MemoryDatabase
{
    public class MemoryDatabaseTableNotFoundException : Exception
    {
        public MemoryDatabaseTableNotFoundException(MemoryDatabaseTables table) : 
            base($"Pointer of {Enum.GetName(typeof(MemoryDatabaseTables), table)} is NULL")
        {
        }
    }
}
