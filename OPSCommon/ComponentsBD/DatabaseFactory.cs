using System;
using System.Configuration;
using OPS.Comm;
using OPS.Components.Data;

namespace OPS.Components.Data
{
	public sealed class DatabaseFactory 
	{
 
		private static Database _db = null;
		private static ILogger m_Logger = null; 
		private DatabaseFactory () {}
		
		public static ILogger Logger
		{ 
			get { return m_Logger;}
			set 
			{
				m_Logger = value;
				if(m_Logger != null)
				{
					m_Logger.AddLog("DataBase Factory Init Log", LoggerSeverities.Debug);
				}
				
			}
		}
		public static Database GetDatabase()
		{
			if (_db == null) 
			{
				_db = new Database (Database.DataProvider.ORACLE,
                                                ConfigurationManager.AppSettings["ConnectionString"],
                                                ConfigurationManager.AppSettings["SchemaOwner"]);
				_db.LoadDatabaseSchema();
				_db.LoadUserSchema(ConfigurationManager.AppSettings["UserSchemaPath"]);
			}
			return _db;
		}
	}
}


