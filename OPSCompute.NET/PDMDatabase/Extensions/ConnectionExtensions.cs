using PDMDatabase.Core;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase
{
    public static class ConnectionExtensions
    {
        public static IDataReader Query(this IDbConnection connection, BaseCommand command)
        {
            IDbCommand dbCommand = command.Build();

            command.Trace?.Write(PDMHelpers.TraceLevel.Info, $@"Executing Query ( {ToInlineString(dbCommand.CommandText)} )");

            return dbCommand.ExecuteReader();
        }

        public static IEnumerable<T> Query<T>(this IDbConnection connection, BaseCommand command) where T : class, IParseable
        {
            IDataReader reader = connection.Query(command);
            return reader.AsList<T>(command.tableVersion) as IEnumerable<T>;
        }

        public static object ExecuteScalar(this IDbConnection connection, BaseCommand command)
        {
            IDbCommand dbCommand = command.Build();
            command.Trace?.Write(PDMHelpers.TraceLevel.Info, $@"Executing Query ( {ToInlineString(dbCommand.CommandText)} )");
            return dbCommand.ExecuteScalar();
        }

        public static T ExecuteScalar<T>(this IDbConnection connection, BaseCommand command)
        {
            object result = connection.ExecuteScalar(command);
            return (T)System.Convert.ChangeType(result, typeof(T));
        }

        private static string ToInlineString(string input) {

            if (input.Contains("\r") || input.Contains("\t") || input.Contains("\n"))
            {
                return input.Replace("\r", string.Empty).Replace("\t", string.Empty).Replace("\n", string.Empty);
            }
            else
            {
                return input;
            }
        }
    }
}