using System;

using System.Data;

namespace OPS.Components.Data 
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ROL_ACCESS.
	/// </summary>
	public class CmpRolAccessDB : CmpGenericBase
	{
		public CmpRolAccessDB()
		{
			_standardFields		= new string[] {"RACC_ROL_ID", "RACC_VIE_ID", "RACC_ALLOWED"};
			_standardPks		= new string[] {"RACC_ROL_ID", "RACC_VIE_ID"};
			_standardTableName	= "ROL_ACCESS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
	
			_standardRelationFileds	= new string[0]; //{"RACC_ROL_ID","RACC_VIE_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpRolesDB,ComponentsBD","OPS.Components.Data.CmpViewsDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"RACC_VALID", "RACC_DELETED"};
		}

		public DataTable GetCompleteDataByRol(int rolId)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable("SELECT VIEWS.VIE_ID, VIEWS.VIE_LIT_ID, TEMP.RACC_ALLOWED FROM VIEWS LEFT OUTER JOIN (SELECT * FROM ROL_ACCESS WHERE ROL_ACCESS.RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@) TEMP ON VIEWS.VIE_ID = TEMP.RACC_VIE_ID", "VIEWS", rolId);
			return dt;
		}
	}
}
