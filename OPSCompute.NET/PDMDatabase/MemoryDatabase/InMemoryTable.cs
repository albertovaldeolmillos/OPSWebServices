using PDMHelpers;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public abstract class InMemoryTable<T> : IInMemoryTable
    {
        protected ITraceable trace { get; set; }
        protected IDbConnection Connection { get; set; }
        protected IEnumerable<T> Data { get; set; }
        protected bool IsLoaded { get; set; }
        public long Version { get; set; }

        protected InMemoryTable(IDbConnection connection)
        {
            Connection = connection;
        }

        public abstract void LoadData();

        public int GetNum()
        {
            return (Data != null) ? (Data as IList<T>).Count : 0 ;
        }

        protected virtual void LoadIfIsNeeded()
        {
            if (!IsLoaded)
            {
                LoadData();
            }
        }
                        
        public void SetTracerEnabled(bool enabled)
        {
            if (trace != null)
            {
                trace.Enabled = enabled;
            }
        }

    }
}
