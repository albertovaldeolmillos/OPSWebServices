using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for USR_ACCESS.
	/// </summary>
	public class CmpUserAccessDB : CmpGenericBase
	{
		public CmpUserAccessDB()
		{
			_standardFields		= new string[] {"UACC_USR_ID", "UACC_VIE_ID", "UACC_ALLOWED"};
			_standardPks		= new string[] {"UACC_USR_ID", "UACC_VIE_ID"};
			_standardTableName	= "USR_ACCESS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"UACC_USR_ID","UACC_VIE_ID","UACC_ALLOWED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUsuarioDB,ComponentsBD","OPS.Components.Data.CmpViewsDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"UACC_VALID", "UACC_DELETED"};
		}

		public DataTable GetCompleteDataByUser(int usrId)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable("SELECT VIEWS.VIE_ID,VIEWS.VIE_LIT_ID, USR_ACCESS.UACC_ALLOWED FROM VIEWS INNER JOIN USR_ACCESS ON  USR_ACCESS.UACC_VIE_ID = VIEWS.VIE_ID WHERE USR_ACCESS.UACC_USR_ID = @USR_ACCESS.UACC_USR_ID@","VIEWS",usrId);
			return dt;
		}
	}
}
