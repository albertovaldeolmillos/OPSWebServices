using System;
using System.Data;

using System.Text;
using OPS.Components.Globalization;
using System.Configuration;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VIEWS.
	/// </summary>
	public class CmpViewsDB : CmpGenericBase
	{
		public CmpViewsDB()
		{
			_standardFields		= new string[] {"VIE_ID", "VIE_DLUNI_ID", "VIE_URL", "VIE_INUSE", "VIE_USR_ID", "VIE_INUSEDATE", "VIE_LIT_ID", "VIE_LIT_ID", "VIE_IMAGE", "VIE_TITLE_LIT_ID", "VIE_COMPONENT"};
			_standardPks		= new string[] {"VIE_ID"};
			_standardTableName	= "VIEWS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"VIE_VALID", "VIE_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpViewsDB() {}

		// ********************* Custom functions ************************************************

		/// <summary>
		/// Blocks or releases a view by a user
		/// </summary>
		/// <param name="vieId">View to block or release</param>
		/// <param name="userId">User who blocks or releases the view</param>
		/// <param name="block">If true the views is blocked. Otherwise is released.</param>
		public void BlockView (int vieId, int userId, bool block)
		{
			Database d = DatabaseFactory.GetDatabase();
			
			AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			double nDifHour=0;
			try
			{
				nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
			}
			catch
			{
				nDifHour=0;
			}			
			
			
			object p1, p2,p3;
			if (block) 
			{
				p1 = true;
				p2 = userId;
				p3 = DateTime.Now.AddHours(nDifHour);
			}
			else
			{	
				// DXA The VIE_INUSE is now a 0-default value, not null
				//p1 = DBNull.Value;
				p1 = 0;
				p2 = DBNull.Value;
				p3 = DBNull.Value;
			}
			d.ExecuteNonQuery ("UPDATE VIEWS SET VIE_INUSE = @VIEWS.VIE_INUSE@, VIE_USR_ID = @VIEWS.VIE_USR_ID@, VIE_INUSEDATE = @VIEWS.VIE_INUSEDATE@ WHERE VIE_ID = @VIEWS.VIE_ID@",
				p1, p2, p3, vieId);
		}

		/// <summary>
		/// Releases ALL views of a user, except vieId
		/// </summary>
		/// <param name="vieId">Unique view to be NOT released</param>
		/// <param name="userId">Id of the user who blocked the views. Only views of that user will be released</param>
		public void ReleaseRestOfViews (int vieId, int userId)
		{
			Database d= DatabaseFactory.GetDatabase();
			// DXA The VIE_INUSE is now a 0-default value, not null
			//d.ExecuteNonQuery ("UPDATE VIEWS SET VIE_INUSE = @VIEWS.VIE_INUSE@, VIE_USR_ID = @VIEWS.VIE_USR_ID@, VIE_INUSEDATE = @VIEWS.VIE_INUSEDATE@ WHERE VIE_ID <> @VIEWS.VIE_ID@ AND VIE_USR_ID = @VIEWS.VIE_USR_ID@", DBNull.Value, DBNull.Value, DBNull.Value, vieId, userId);
			d.ExecuteNonQuery ("UPDATE VIEWS SET VIE_INUSE = @VIEWS.VIE_INUSE@, VIE_USR_ID = @VIEWS.VIE_USR_ID@, VIE_INUSEDATE = @VIEWS.VIE_INUSEDATE@ WHERE VIE_ID <> @VIEWS.VIE_ID@ AND VIE_USR_ID = @VIEWS.VIE_USR_ID@", 0, DBNull.Value, DBNull.Value, vieId, userId);
		}

//		// ********************* Implementation of CmpDataSourceAdapter ***************************
//
//		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"VIE_ID", "VIE_URL", "VIE_INUSE", "VIE_USR_ID","VIE_INUSEDATE","VIE_LIT_ID","VIE_COMPONENT", "VIE_TITLE_LIT_ID"};
//			}
//			string[] pk = new string[] {"VIE_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"VIEWS","VIEWS",pk);
//
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"VIE_ID", "VIE_URL", "VIE_INUSE", "VIE_USR_ID","VIE_INUSEDATE","VIE_LIT_ID","VIE_COMPONENT", "VIE_TITLE_LIT_ID"};
//			}
//			string[] pk = new string[] {"VIE_ID"};			
//			StringBuilder sb = base.ProcessFields(fields,pk);			
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"VIEWS","VIEWS",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO VIEWS (VIE_ID, VIE_URL, VIE_INUSE, VIE_USR_ID, VIE_INUSEDATE,VIE_LIT_ID, VIE_COMPONENT, VIE_TITLE_LIT_ID) VALUES (@VIEWS.VIE_ID@, @VIEWS.VIE_URL@, @VIEWS.VIE_INUSE@, @VIEWS.VIE_USR_ID@, @VIEWS.VIE_INUSEDATE@,@VIEWS.VIE_LIT_ID@, @VIEWS.VIE_COMPONENT@, @VIEWS.VIE_TITLE_LIT_ID@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE VIEWS SET VIE_URL = @VIEWS.VIE_URL@, VIE_LIT_ID = @VIEWS.VIE_LIT_ID@, VIE_COMPONENT = @VIEWS.VIE_COMPONENT@, VIE_TITLE_LIT_ID = @VIEWS.VIE_TITLE_LIT_ID@ WHERE VIE_ID = @VIEWS.VIE_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM VIEWS WHERE VIE_ID = @VIEWS.VIE_ID@", false);
//			IDbConnection con = d.GetNewConnection();
//			con.Open();
//			da.InsertCommand.Connection = con;
//			da.UpdateCommand.Connection = con;
//			da.DeleteCommand.Connection = con;
//			d.UpdateDataSet(da,dt);
//			dt.AcceptChanges();
//			con.Close();
//		}
//		public override void GetForeignData(DataSet ds, string sTable) 
//		{
//			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
//			ds.Tables.Add (dtCmpCodesDB);
//
//			DataTable parent = ds.Tables[sTable];
//
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["VIE_INUSE"]);
//			
//		}
//		public override string MainTable  {get { return "VIEWS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM VIEWS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
//
//		public override Int64 LastPKValue
//		{
//			get 
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(VIE_ID),0) FROM VIEWS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}

