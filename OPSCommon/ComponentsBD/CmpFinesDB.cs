using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpFinesHisDB.
	/// </summary>
	public class CmpFinesDB: CmpGenericBase
	{
		public CmpFinesDB()
		{
			_standardFields			= new string[]
				{ "FIN_ID", "FIN_DFIN_ID", "FIN_VEHICLEID", "FIN_MODEL",
				"FIN_MANUFACTURER", "FIN_COLOUR", "FIN_STR_ID", "FIN_STRNUMBER", 
				"FIN_DATE", "FIN_COMMENTS", "FIN_USR_ID", "FIN_UNI_ID", "FIN_DPAY_ID", "FIN_COD_ID",
				"FIN_FIN_ID", "FIN_GRP_ID_ZONE", "FIN_GRP_ID_ROUTE", "FIN_STATUS","FIN_STATUSADMON","FIN_STATUSADMONAUTO","FIN_POLICENUMBER","FIN_CONFIRM_DATE"};
			_standardPks			= new string[] { "FIN_ID"};
			_standardTableName		= "FINES";
			_standardOrderByField	= "FIN_DATE";		
			_standardOrderByAsc		= "DESC";		
		
			// Add the table of languages to the DataSet
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[] {"FIN_DFIN_ID","FIN_DPAY_ID",
													"FIN_GRP_ID_ZONE","FIN_GRP_ID_ROUTE", 
													"FIN_STR_ID","FIN_UNI_ID","FIN_USR_ID", 
													"FIN_STATUS", "FIN_STATUSADMON", 
													"FIN_MODEL", "FIN_MANUFACTURER", "FIN_COLOUR" };
			_stValidDeleted			= new string[0];
		}

		#region Specific methods of CmpFinesDB

		public bool ExistsFine(int dfinid, string vehicleid, 
			string model, string manufacturer, string colour, int grpidzone, int grpidroute, int strid, int strnumber,
			DateTime date, string comments, int usrid, int uniid, int dpayid)
		{
			string sql = "SELECT COUNT(*) FROM FINES "
				+ "WHERE " + BuildCondition("FINES", "FIN_DFIN_ID", dfinid==-1)
				+ "AND " + BuildCondition("FINES", "FIN_VEHICLEID", vehicleid==null)
				+ "AND " + BuildCondition("FINES", "FIN_MODEL", model==null)
				+ "AND " + BuildCondition("FINES", "FIN_MANUFACTURER", manufacturer==null)
				+ "AND " + BuildCondition("FINES", "FIN_COLOUR", colour==null)
				+ "AND " + BuildCondition("FINES", "FIN_GRP_ID_ZONE", grpidzone == -1)
				+ "AND " + BuildCondition("FINES", "FIN_GRP_ID_ROUTE", grpidroute == -1)
				+ "AND " + BuildCondition("FINES", "FIN_STR_ID", strid==-1)
				+ "AND " + BuildCondition("FINES", "FIN_STRNUMBER", strnumber==-1)
				+ "AND " + BuildCondition("FINES", "FIN_DATE", date==DateTime.MinValue)
				+ "AND " + BuildCondition("FINES", "FIN_COMMENTS", comments==null)
				+ "AND " + BuildCondition("FINES", "FIN_USR_ID", usrid==-1)
				+ "AND " + BuildCondition("FINES", "FIN_UNI_ID", uniid==-1)
				+ "AND " + BuildCondition("FINES", "FIN_DPAY_ID", dpayid==-1);

			Array values = Array.CreateInstance(typeof(object), 0);
			values = BuildValue(values, dfinid, dfinid==-1);
			values = BuildValue(values, vehicleid, vehicleid==null);
			values = BuildValue(values, model, model==null);
			values = BuildValue(values, manufacturer, manufacturer==null);
			values = BuildValue(values, colour, colour==null);
			values = BuildValue(values, grpidzone, grpidzone ==-1);
			values = BuildValue(values, grpidroute , grpidroute ==-1);
			values = BuildValue(values, strid, strid==-1);
			values = BuildValue(values, strnumber, strnumber==-1);
			values = BuildValue(values, date, date==DateTime.MinValue);
			values = BuildValue(values, comments, comments==null);
			values = BuildValue(values, usrid, usrid==-1);
			values = BuildValue(values, uniid, uniid==-1);
			values = BuildValue(values, dpayid, dpayid==-1);

			Database d = DatabaseFactory.GetDatabase();
			return Convert.ToInt32(d.ExecuteScalar(sql, (object[])values)) > 0;
		}

		private string BuildCondition(string table, string field, bool isNull)
		{
			return " (" + field + " " + (isNull ? "IS NULL) " : "= @" + table + "." + field + "@) ");
		}

		private Array BuildValue(Array a, object o, bool isNull)
		{
			if (isNull)
				return a;
			
			Array valuesTemp = Array.CreateInstance(typeof(object), a.Length + 1);
			a.CopyTo(valuesTemp, 0);
			valuesTemp.SetValue(o, valuesTemp.Length-1);
			return valuesTemp;
		}

		/// <summary>
		/// Inserts a new register in the FINES table
		/// </summary>
		/// <param name="dfinid">FIN_DFIN_ID value. Cannot be NULL</param>
		/// <param name="vehicleid">FIN_VEHICLEID value. May be NULL</param>
		/// <param name="model">FIN_MODEL value. May be NULL</param>
		/// <param name="manufacturer">FIN_MANUFACTURER value. May be NULL</param>
		/// <param name="colour">FIN_COLOUR value. May be NULL</param>
		/// <param name="grpidzone">FIN_GRP_ID_ZONE value. Use -1 for NULL</param>
		/// <param name="grpidroute">FIN_GRP_ID_ROUTE value. Use -1 for NULL</param>
		/// <param name="strid">FIN_STR_ID value. Use -1 for NULL</param>
		/// <param name="strnumber">FIN_STRNUMBER value. Use -1 for NULL</param>
		/// <param name="date">FIN_DATE value. Cannot be NULL</param>
		/// <param name="comments">FIN_COMMENTS value. May be NULL</param>
		/// <param name="usrid">FIN_USR_ID value. Use -1 for NULL</param>
		/// <param name="uniid">FIN_UNI_ID value. Use -1 for NULL</param>
		/// <param name="payed">FIN_PAYED value. Use -1 for NULL</param>
		/// <param name="dpayid">FIN_DPAY_ID value. Use -1 for NULL</param>
		/// <param name="status">FIN_STATUS value. Use -1 for NULL</param>
		/// <param name="finfinid">FIN_FIN_ID value. Use -1 for NULL</param>
		public void InsertFine(int dfinid, string vehicleid, 
			string model, string manufacturer, string colour, int grpidzone, int grpidroute, int strid, int strnumber,
			DateTime date, string comments, int usrid, int uniid, int dpayid, int status, int finfinid)
		{
			if (ExistsFine(dfinid, vehicleid, model, manufacturer, colour, grpidzone, grpidroute, strid, strnumber,
				date, comments, usrid, uniid, dpayid))
				return;

			Database d = DatabaseFactory.GetDatabase();
			int id = Convert.ToInt32(d.ExecuteScalar("SELECT NVL(MAX(FIN_ID),0) FROM FINES", new object[] {}));
			id++;

			string ssql = "INSERT INTO FINES (FIN_ID,FIN_DFIN_ID,FIN_VEHICLEID," 
				+ "FIN_MODEL,FIN_MANUFACTURER,FIN_COLOUR,FIN_GRP_ID_ZONE,  FIN_GRP_ID_ROUTE,FIN_STR_ID,FIN_STRNUMBER,FIN_DATE,"
				+ "FIN_COMMENTS,FIN_USR_ID,FIN_UNI_ID,FIN_DPAY_ID,FIN_COD_ID, FIN_FIN_ID, FIN_STATUS) VALUES ("
				+ "@FINES.FIN_ID@,@FINES.FIN_DFIN_ID@,@FINES.FIN_VEHICLEID@,"
				+ "@FINES.FIN_MODEL@,@FINES.FIN_MANUFACTURER@,@FINES.FIN_COLOUR@,@FINES.FIN_GRP_ID_ZONE@,@FINES.FIN_GRP_ID_ROUTE@,@FINES.FIN_STR_ID@,@FINES.FIN_STRNUMBER@,@FINES.FIN_DATE@,"
				+ "@FINES.FIN_COMMENTS@,@FINES.FIN_USR_ID@,@FINES.FIN_UNI_ID@,@FINES.FIN_DPAY_ID@,@FINES.FIN_COD_ID@, @FINES.FIN_FIN_ID@, @FINES.FIN_STATUS@)";
			d.ExecuteNonQuery(ssql, 
				id, 
				dfinid, 
				(vehicleid==null ? DBNull.Value : (object)vehicleid), 
				(model==null ? DBNull.Value : (object)model), 
				(manufacturer==null ? DBNull.Value : (object)manufacturer), 
				(colour==null ? DBNull.Value : (object)colour), 
				(grpidzone ==-1 ? DBNull.Value : (object)grpidzone), 
				(grpidroute ==-1 ? DBNull.Value : (object)grpidroute), 
				(strid==-1 ? DBNull.Value : (object)strid), 
				(strnumber==-1 ? DBNull.Value : (object)strnumber),
				date, 
				(comments==null ? DBNull.Value : (object)comments), 
				(usrid==-1 ? DBNull.Value : (object)usrid),
				(uniid==-1 ? DBNull.Value : (object)uniid),
				(dpayid==-1 ? DBNull.Value : (object)dpayid), 
				DBNull.Value,
				(finfinid == -1 ? DBNull.Value : (object)finfinid),
				(status == -1 ? DBNull.Value : (object)status) );
		}

		#endregion
	}
}
