namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_RESULTS.
	/// </summary>
	public class CmpUnitsResultsDB : CmpGenericBase
	{
		public CmpUnitsResultsDB()
		{
			_standardFields		= new string[] {"RUNI_UNI_ID", "RUNI_INIDATE", "RUNI_ENDDATE", "RUNI_PARCIAL", "RUNI_PARCIALCASH", "RUNI_TOTAL", "RUNI_SENT"};
			_standardPks		= new string[] {"RUNI_UNI_ID", "RUNI_INIDATE"};
			_standardTableName	= "UNITS_RESULTS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"RUNI_UNI_ID","RUNI_SENT"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}



