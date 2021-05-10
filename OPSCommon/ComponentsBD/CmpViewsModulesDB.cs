using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VIEWS_MODULES.
	/// </summary>
	public class CmpViewsModulesDB : CmpGenericBase
	{
		public CmpViewsModulesDB()
		{
			_standardFields		= new string[] {"VMOD_MOD_ID", "VMOD_VIE_ID", "VMOD_ORDER"};
			_standardPks		= new string[] {"VMOD_MOD_ID", "VMOD_VIE_ID"};
			_standardTableName	= "VIEWS_MODULES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

		public DataTable GetViewsByModule(int moduleId)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable("SELECT VIEWS.VIE_ID,VIEWS.VIE_LIT_ID FROM VIEWS INNER JOIN VIEWS_MODULES ON  VIEWS_MODULES.VMOD_VIE_ID = VIEWS.VIE_ID WHERE VIEWS_MODULES.VMOD_MOD_ID = @VIEWS_MODULES.VMOD_MOD_ID@ ORDER BY VIEWS_MODULES.VMOD_ORDER","VIEWS",moduleId);
			return dt;
		}
	}
}

