/*
using System;
using System.Data;

using System.Text;

namespace OPS.Components.Data
{
	/// <summary>
	/// Executant for literales
	/// </summary>
	public class CmpDayStatusDB : CmpBase
	{
		public CmpDayStatusDB(){}

		// ********************* Implementation of CmpDataSourceAdapter ***************************

		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			if (fields == null) 
			{
				fields = new string[] {"LIT_ID","LIT_LAN_ID","LIT_DESCSHORT","LIT_DESCLONG", "LIT_CATEGORY"};
			}
			string[] pk = new string[] {"LIT_ID","LIT_LAN_ID"};
			StringBuilder sb = base.ProcessFields(fields,pk);
			return base.DoGetData (sb.ToString(),where, orderby, values, "VW_DAY_STATUS","VW_DAY_STATUS",pk);
		}
		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
		{
			Database d = DatabaseFactory.GetDatabase();
			if (fields == null) fields = new string[] {"LIT_ID", "LIT_LAN_ID", "LIT_DESCSHORT", "LIT_DESCLONG", "LIT_CATEGORY"};
			string[] pk = new string[] {"LIT_ID","LIT_LAN_ID"};
			StringBuilder sb = base.ProcessFields(fields,pk);
			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"VW_DAY_STATUS","VW_DAY_STATUS",pk);
		}
		public override void SaveData (DataTable dt)
		{
			Database d = DatabaseFactory.GetDatabase ();
			IDbDataAdapter da = d.GetDataAdapter ();
			
			da.InsertCommand = d.PrepareCommand ("INSERT INTO VW_DAY_STATUS (LIT_ID, LIT_LAN_ID, LIT_DESCSHORT, LIT_DESCLONG, LIT_CATEGORY) VALUES (@VW_DAY_STATUS.LIT_ID@, @VW_DAY_STATUS.LIT_LAN_ID@, @VW_DAY_STATUS.LIT_DESCSHORT@, @VW_DAY_STATUS.LIT_DESCLONG@, @VW_DAY_STATUS.LIT_CATEGORY@)",false);
			da.UpdateCommand = d.PrepareCommand ("UPDATE VW_DAY_STATUS SET LIT_ID = @VW_DAY_STATUS.LIT_ID@, LIT_LAN_ID = @VW_DAY_STATUS.LIT_LAN_ID@, LIT_DESCSHORT = @VW_DAY_STATUS.LIT_DESCSHORT@, LIT_DESCLONG= @VW_DAY_STATUS.LIT_DESCLONG@, LIT_CATEGORY= @VW_DAY_STATUS.LIT_CATEGORY@ WHERE LIT_ID = #VW_DAY_STATUS.LIT_ID# AND LIT_LAN_ID = #VW_DAY_STATUS.LIT_LAN_ID#", false);
			da.DeleteCommand = d.PrepareCommand ("DELETE FROM VW_DAY_STATUS WHERE LIT_ID = @VW_DAY_STATUS.LIT_ID@ AND LIT_LAN_ID = @VW_DAY_STATUS.LIT_LAN_ID@", false);
			IDbConnection con = d.GetNewConnection();
			con.Open();
			da.InsertCommand.Connection = con;
			da.UpdateCommand.Connection = con;
			da.DeleteCommand.Connection = con;
			d.UpdateDataSet(da,dt);
			dt.AcceptChanges();
			con.Close();

		}
		public override void GetForeignData(DataSet ds, string sTable) 
		{
			// Add the table of languages to the DataSet
			DataTable dtLanguages = new CmpLanguagesDB().GetData();
			ds.Tables.Add (dtLanguages);
			DataTable parent = ds.Tables[sTable];
			ds.Relations.Add ((dtLanguages.PrimaryKey)[0],parent.Columns["LIT_LAN_ID"]);

		}
		public override string MainTable  {get { return "VW_DAY_STATUS"; }} 
		public override long GetCount(string where, params object[] values)
		{
			Database d = DatabaseFactory.GetDatabase();
			string sql = "SELECT COUNT(*) FROM VW_DAY_STATUS";
			if (where != null) 
			{
				sql = sql + " WHERE " + where;
			}
			return Convert.ToInt64(d.ExecuteScalar(sql, values));
		}

		public override Int64 LastPKValue
		{
			get {return -2L;}
		}

	}

}
*/