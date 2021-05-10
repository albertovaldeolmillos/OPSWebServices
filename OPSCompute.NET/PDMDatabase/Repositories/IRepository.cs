using System.Data;
using PDMHelpers;

namespace PDMDatabase.Repositories
{
    public interface IRepository
    {
        IDbConnection Connection { get; }
        ITraceable Trace { get; set; }
    }
}