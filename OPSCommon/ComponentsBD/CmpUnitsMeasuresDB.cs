using System;
using System.Data;

using OPS.Comm;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_MEASURES.
	/// </summary>
	public class CmpUnitsMeasuresDB : CmpGenericBase
	{
		public CmpUnitsMeasuresDB()
		{
			_standardFields		= new string[] {"MUNI_ID", "MUNI_DATE", "MUNI_UNI_ID", 
												   "MUNI_VALUE1", 
												   "MUNI_VALUE2", 
												   "MUNI_VALUE3",
												   "MUNI_VALUE4", 
												   "MUNI_VALUE5",
												   "MUNI_VALUE6", 
												   "MUNI_VALUE7", 
												   "MUNI_VALUE8",
												   "MUNI_VALUE9", 
												   "MUNI_VALUE10"};
			_standardPks		= new string[] {"MUNI_ID"};
			_standardTableName	= "UNITS_MEASURES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"MUNI_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}

//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//			DataTable dtCmpUnitsDB = new CmpUnitsDB().GetData();
//			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
//
//			ds.Tables.Add (dtCmpUnitsDB);
//			ds.Tables.Add (dtCmpCodesDB);
//
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtCmpUnitsDB.PrimaryKey)[0],parent.Columns["MUNI_UNI_ID"]);
//
//		}

		/// <summary>
		/// Inserts a new register in the UNITS_MEASURES table
		/// </summary>
		/// <param name="dfinid">FIN_DFIN_ID value. Cannot be NULL</param>

		public void InsertMeasures(int uniid, DateTime date,
									float ms1,float ms2, float ms3, float ms4, float ms5)
		{
			IDbTransaction	tran = null;
			IDbConnection	con = null;
			int				res = 0;
			ILogger			logger = null;

			try
			{
				logger =  DatabaseFactory.Logger;
				if(logger != null)
					logger.AddLog("[CmpUnitsMeasuresDB:InsertMeasures ", LoggerSeverities.Debug);
		
				/*
				if (ExistsFine(dfinid, number, vehicleid, model, manufacturer, colour, grpid, strid, strnumber,
					date, comments, usrid, uniid, dpayid))
					return;
				*/
				Database d = DatabaseFactory.GetDatabase();
	 
				con = d.GetNewConnection();

				if(logger != null)
					logger.AddLog("[CmpUnitsMeasuresDB::InsertMeasures]: IOpening...", LoggerSeverities.Debug);

				con.Open();
					
				tran =  con.BeginTransaction(IsolationLevel.Serializable);

				if(logger!=null)
					logger.AddLog("[CmpUnitsMeasuresDB:Deleting previous ", LoggerSeverities.Debug);
				
				// Borro la anterior si existe
				d.ExecuteNonQuery("DELETE FROM UNITS_MEASURES WHERE MUNI_UNI_ID = " + uniid.ToString());
				
				// Obtengo el nuevo índice de inserción
				int id = Convert.ToInt32(d.ExecuteScalar("SELECT NVL(MAX(MUNI_ID),0) FROM UNITS_MEASURES", new object[] {}));
				id++;
				// Sentencia de inserción
				string ssql = "INSERT INTO UNITS_MEASURES (MUNI_ID,MUNI_DATE,MUNI_UNI_ID,MUNI_VALUE1," 
					+ "MUNI_VALUE2,MUNI_VALUE3,MUNI_VALUE4,MUNI_VALUE5,MUNI_VALUE6,MUNI_VALUE7,"
					+ "MUNI_VALUE8,MUNI_VALUE9,MUNI_VALUE10) VALUES ("
					+ "@UNITS_MEASURES.MUNI_ID@,@UNITS_MEASURES.MUNI_DATE@,@UNITS_MEASURES.MUNI_UNI_ID@,@UNITS_MEASURES.MUNI_VALUE1@,"
					+ "@UNITS_MEASURES.MUNI_VALUE2@,@UNITS_MEASURES.MUNI_VALUE3@,@UNITS_MEASURES.MUNI_VALUE4@,@UNITS_MEASURES.MUNI_VALUE5@,@UNITS_MEASURES.MUNI_VALUE6@,"
					+ "@UNITS_MEASURES.MUNI_VALUE7@,@UNITS_MEASURES.MUNI_VALUE8@,@UNITS_MEASURES.MUNI_VALUE9@,@UNITS_MEASURES.MUNI_VALUE10@)";
				
				if(logger != null)
					logger.AddLog(ssql, LoggerSeverities.Debug);
			
				res = d.ExecuteNonQuery(ssql,con,tran, 
					//int res = d.ExecuteNonQuery(ssql, 
					id,date,  (uniid==-1 ? DBNull.Value : (object)uniid),
					(object)ms1, 
					(object)ms2, 
					(object)ms3, 
					(object)ms4, 
					(object)ms5, 
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value);

				if(res ==1)
				{
					if(logger != null)
						logger.AddLog("[CmpUnitsMeasuresDB::InsertMeasures]: Commit", LoggerSeverities.Debug);
					tran.Commit();
				}
				else
				{
					if(logger != null)
						logger.AddLog("[CmpUnitsMeasuresDB::InsertMeasures]: RollBack", LoggerSeverities.Debug);
					tran.Rollback();
				}
			}
			catch (Exception e)
			{
				// OOOOppps.... some error... do a rollback 
				tran.Rollback();
				throw e;			// Propagate the error back!
			}
			finally 
			{
				// Close connection... and release the mutex!
				try 
				{
					if(logger != null)
						logger.AddLog("[CmpUnitsMeasuresDB]: Closing...", LoggerSeverities.Debug);
					con.Close();
				}  
				catch (Exception) { }
				//m.ReleaseMutex();
			}
			//return res;
		}
	}
}

