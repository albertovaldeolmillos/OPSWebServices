using PDMDatabase.Core;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase
{
    public static class DataReaderExtensions
    {
        public static List<T> AsList<T>(this IDataReader reader, long tableVersion) where T : class, IParseable {
            List<T> results = new List<T>();
            T instance;

            while (reader.Read())
            {
                instance = System.Activator.CreateInstance<T>();
                instance.Parse(reader, tableVersion);
                
                results.Add(instance);
            }

            return results;
        }
    }
}
