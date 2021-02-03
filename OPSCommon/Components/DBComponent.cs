using System;
using System.Data;
using System.Configuration;
//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Components
{
    public class DBComponent: IDisposable
    {

        OracleConnection cnnConnection = null;
        private bool _disposed = false;

        protected DBComponent()
        {
            this.OpenConnection();
        }


        ~DBComponent()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (cnnConnection != null)
                    {
                        cnnConnection.Close();
                        cnnConnection.Dispose();
                        cnnConnection = null;
                    }
                }
                _disposed = true;
            }
        }


        protected OracleConnection getConnection()
        {
            return cnnConnection;
        }


        /// <summary>
        /// Abre una conexión con la base de datos
        /// </summary>
        protected void OpenConnection()
        {
            try
            {
                string connectionString = ConfigurationSettings.AppSettings["ConnectionString"];

                if (connectionString.Length == 0)
                    throw new Exception("No ConnectionString configuration");
                cnnConnection = new OracleConnection(connectionString);
                cnnConnection.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Cierra la conexión con la base de datos
        /// </summary>
        protected void CloseConnection()
        {
            try
            {
                cnnConnection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene una transacción de la base de datos
        /// </summary>
        /// <returns></returns>
        protected OracleTransaction GetTransaction()
        {
            OracleTransaction transaction = null;
            try
            {
                if (cnnConnection != null)
                    transaction = cnnConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (Exception)
            {
                throw;
            }
            return transaction;
        }

        /// <summary>
        /// Ejecuta una sentencia SQL en la base de datos.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <returns>Número de registros afectados.</returns>
        protected long ExecuteCommand(string sqlCommand)
        {
            OracleCommand oraCommand = null;
            int numRowsAffected = 0;

			try
            {
                if (cnnConnection == null)
                    throw new Exception("No Oracle Connection");

                oraCommand = new OracleCommand();
                oraCommand.Connection = cnnConnection;
                oraCommand.CommandText = sqlCommand;
                oraCommand.CommandType = CommandType.Text;

                numRowsAffected = oraCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oraCommand != null)
                {
                    oraCommand.Dispose();
                    oraCommand = null;
                }
            }

            return numRowsAffected;
        }

        /// <summary>
        /// Ejecuta una sentencia SQL dentro de una transacción
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        protected long ExecuteCommand(string sqlCommand, OracleTransaction transaction)
        {
            OracleCommand oraCommand = null;
            int numRowsAffected = 0;

            try
            {
                if (cnnConnection == null)
                    throw new Exception("No Oracle Connection");

                oraCommand = new OracleCommand();
                oraCommand.Connection = cnnConnection;
                oraCommand.Transaction = transaction;
                oraCommand.CommandText = sqlCommand;
                oraCommand.CommandType = CommandType.Text;

                numRowsAffected = oraCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oraCommand != null)
                {
                    oraCommand.Dispose();
                    oraCommand = null;
                }
            }

            return numRowsAffected;
        }

        /// <summary>
        /// Ejecuta una sentencia SQL con la posibilidad de indicar el valor de la columna que se quiere devolver. Por ejemplo:
        /// INSERT INTO MyTest (OtherCol) VALUES ('Test') RETURNING ID INTO :ID
        /// </summary>
        /// <param name="sqlCommand">Sentencia SQL</param>
        /// <returns>Valor del identificador, -1 si no es posible obtenerlo</returns>
        protected long ExecuteCommandReturningId(string sqlCommand)
        {
            OracleCommand oraCommand = null;
            int numRowsAffected = 0;
            long returningId = -1;

            try
            {
                if (cnnConnection == null)
                    throw new Exception("No Oracle Connection");

                oraCommand = new OracleCommand();
                oraCommand.Connection = cnnConnection;
                oraCommand.CommandText = sqlCommand;
                oraCommand.CommandType = CommandType.Text;

                OracleParameter p = oraCommand.Parameters.Add(":ID", OracleDbType.Decimal);
                p.Direction = ParameterDirection.Output;

                numRowsAffected = oraCommand.ExecuteNonQuery();

                returningId = long.Parse(p.Value.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oraCommand != null)
                {
                    oraCommand.Dispose();
                    oraCommand = null;
                }
            }

            return returningId;
        }

		/// <summary>
		/// Ejecuta una sentencia SQL bajo transacción con la posibilidad de indicar el valor de la columna que se quiere devolver. Por ejemplo:
		/// INSERT INTO MyTest (OtherCol) VALUES ('Test') RETURNING ID INTO :ID
		/// </summary>
		/// <param name="sqlCommand">Sentencia SQL</param>
		/// <param name="transaction">Transacción Oracle</param>
		/// <returns>Valor del identificador, -1 si no es posible obtenerlo</returns>
        protected long ExecuteCommandReturningId(string sqlCommand, OracleTransaction transaction)
        {
            OracleCommand oraCommand = null;
            int numRowsAffected = 0;
            int returningId = -1;

            try
            {
                if (cnnConnection == null)
                    throw new Exception("No Oracle Connection");

                oraCommand = new OracleCommand();
                oraCommand.Connection = cnnConnection;
                oraCommand.Transaction = transaction;
                oraCommand.CommandText = sqlCommand;
                oraCommand.CommandType = CommandType.Text;

                OracleParameter p = oraCommand.Parameters.Add(":ID", OracleDbType.Decimal);
                p.Direction = ParameterDirection.Output;

                numRowsAffected = oraCommand.ExecuteNonQuery();

                returningId = int.Parse(p.Value.ToString());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oraCommand != null)
                {
                    oraCommand.Dispose();
                    oraCommand = null;
                }
            }

            return returningId;
        }

        /// <summary>
        /// Obtiene en un DataSet el resultado de la consulta realizado.
        /// </summary>
        /// <param name="sqlSelect">Sentencia SQL</param>
        /// <returns>DataSet con el resultado</returns>
        protected DataSet GetDataSet(string sqlSelect)
        {
            OracleCommand oraCommand = null;
            OracleDataAdapter dataAdapter = null;
            DataSet dataSet = null;

            try
            {
                oraCommand = new OracleCommand();

                oraCommand.Connection = cnnConnection;
                oraCommand.CommandType = CommandType.Text;
                oraCommand.CommandText = sqlSelect;

                dataAdapter = new OracleDataAdapter(oraCommand);

                dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oraCommand != null)
                {
                    oraCommand.Dispose();
                    oraCommand = null;
                }
                if (dataAdapter != null)
                {
                    dataAdapter.Dispose();
                    dataAdapter = null;
                }
            }

            return dataSet;
        }


    }
}
