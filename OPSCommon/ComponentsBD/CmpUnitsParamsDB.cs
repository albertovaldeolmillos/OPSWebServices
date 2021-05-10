namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_PARAMS.
	/// </summary>
	public class CmpUnitsParamsDB : CmpGenericBase
	{
		public CmpUnitsParamsDB() 
		{
			_standardFields		= new string[] {"PUNI_ID", "PUNI_DLUNI_ID", "PUNI_DESCSHORT", "PUNI_DESCLONG", "PUNI_VALUE", "PUNI_CATEGORY"};
			_standardPks		= new string[] {"PUNI_ID"};
			_standardTableName	= "UNITS_PARAMS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"PUNI_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"PUNI_VALID", "PUNI_DELETED"};
		}
	}
}
