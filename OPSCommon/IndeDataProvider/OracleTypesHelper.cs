using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTS.Data
{
    public class OracleTypesHelper
    {
        public static OracleDbType Convert(string  type) {

            OracleDbType tipo; 
            switch (type)
            {
                case "BFILE":
                    tipo = OracleDbType.BFile;
                    break;
                case "BLOB":
                    tipo = OracleDbType.Blob;
                    break;
                case "CHAR":
                    tipo = OracleDbType.Char;
                    break;
                case "CLOB":
                    tipo = OracleDbType.Clob;
                    break;
                case "DATE":
                    tipo = OracleDbType.Date;
                    break;
                case "INTERVAL YEAR TO MONTH":
                    tipo = OracleDbType.IntervalYM;
                    break;
                case "INTERVAL DAY TO SECOND":
                    tipo = OracleDbType.IntervalDS;
                    break;
                case "LONG":
                    tipo = OracleDbType.Long;
                    break;
                case "LONG RAW":
                    tipo = OracleDbType.LongRaw;
                    break;
                case "NCHAR":
                    tipo = OracleDbType.NChar;
                    break;
                case "NCLOB":
                    tipo = OracleDbType.NClob;
                    break;
                case "FLOAT":
                case "INTEGER":
                case "UNSIGNED INTEGER":
                case "NUMBER":
                    tipo = OracleDbType.Decimal;
                    break;
                case "NVARCHAR2":
                    tipo = OracleDbType.NVarchar2;
                    break;
                case "RAW":
                    tipo = OracleDbType.Raw;
                    break;
                case "REF CURSOR":
                    tipo = OracleDbType.RefCursor;
                    break;
                case "ROWID":
                    tipo = OracleDbType.Varchar2;
                    break;
                case "TIMESTAMP":
                    tipo = OracleDbType.TimeStamp;
                    break;
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    tipo = OracleDbType.TimeStampLTZ;
                    break;
                case "TIMESTAMP WITH TIME ZONE":
                    tipo = OracleDbType.TimeStampTZ;
                    break;
                case "VARCHAR2":
                    tipo = OracleDbType.Varchar2;
                    break;
                default:
                    throw new DatabaseLoadException("Error loading schema - Oracle type '" + type + "'");
            }


            return tipo;
        }
    }
}
