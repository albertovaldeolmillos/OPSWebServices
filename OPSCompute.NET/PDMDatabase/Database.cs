//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using PDMHelpers;
using System;

namespace PDMDatabase
{
    public class Database
    {
        private readonly ITraceable trace;
        public OracleConnection Connection { get; set; }

        public Database(ILoggerManager loggerManager) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public bool Open(string connectionString)
        {
            bool bReturn = true;
            try
            {
                Connection = new OracleConnection(connectionString);
                Connection.Open();

                if (!IsOpened())
                {
                    bReturn = false;
                }
                else
                {
                    trace.Write(TraceLevel.Info, $"Database connection is already open");
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, $"Error opening database connection");
                trace.Write(TraceLevel.Error, error.ToLogString());
                bReturn = false;
            }

            return bReturn;
        }

        public bool IsOpened()
        {
            return Connection.State == System.Data.ConnectionState.Open;
        }

        public bool Close()
        {
            bool bReturn = true;
            try
            {
                if (Connection != null)
                {
                    Connection.Close();
                    Connection.Dispose();
                }
            }
            catch (Exception error)
            {
                bReturn = false;
            }

            return bReturn;
        }
    }
}
