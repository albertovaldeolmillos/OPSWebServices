using System.Data;

namespace PDMDatabase.Core
{
    public interface IParseable
    {
        void Parse(IDataReader reader, long tableVersion);
    }
}