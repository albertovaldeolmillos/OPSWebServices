using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ARTICLES.
	/// </summary>
	public class CmpArticlesDB : CmpGenericBase
	{
		public CmpArticlesDB()
		{
			_standardFields		= new string[] {"ART_ID", "ART_DART_ID", "ART_VEHICLEID", "ART_CUS_ID", "ART_INIDATE", "ART_ENDDATE", "ART_STATUS"};
			_standardPks		= new string[] {"ART_ID"};
			_standardTableName	= "ARTICLES";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds		= new string[0]; //{"ART_CUS_ID","ART_DART_ID","ART_DSTA_ID"};
			_standardRelationTables		= new string[0]; //{"OPS.Components.Data.CmpCustomersDB,ComponentsBD","OPS.Components.Data.CmpArticlesDefDB,ComponentsBD","OPS.Components.Data.CmpStatusDefDB,ComponentsBD"};
			_stValidDeleted				= new string[] {"ART_VALID", "ART_DELETED"};
		}
		
		#region Custom Functions (GetArticle & ArticleCanBeNull)
		public DataTable GetArticle (string vehicleID, DateTime date, int OkValue)
		{
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable (
				"SELECT ART_ID, ART_DART_ID, ART_CUS_ID, ART_INIDATE, ART_ENDDATE FROM ARTICLES WHERE ART_STATUS = @ARTICLES.ART_STATUS@ AND ART_VEHICLEID = @ARTICLES.ART_VEHICLEID@ AND (ART_INIDATE IS NULL OR ART_INIDATE <= @ARTICLES.ART_INIDATE@) AND (ART_ENDDATE IS NULL OR ART_ENDDATE >= @ARTICLES.ART_ENDDATE@)", "ARTICLES", OkValue, vehicleID, date, date);
			return dt;
		}

		/// <summary>
		/// Returns if an article type (ARTICLES_DEF entry) must have an associated ARTICLES entry
		/// </summary>
		/// <param name="arttype">DART_ID to check</param>
		/// <returns>true if DART_ID doesn't have to have an ARTICLES entry (if DART_REQUIERED ==0)</returns>
		public bool ArticleCanBeNull (int arttype)
		{
			Database d = DatabaseFactory.GetDatabase();
			int ival =  Convert.ToInt32(
				d.ExecuteScalar ("SELECT DART_REQUIRED FROM ARTICLES_DEF WHERE DART_ID = @ARTICLES_DEF.DART_ID@", arttype));

			return ival == 0;
		}
		#endregion
	}
}