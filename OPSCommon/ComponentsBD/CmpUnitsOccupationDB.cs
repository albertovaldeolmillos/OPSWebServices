namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_OCCUPATION.
	/// </summary>
	public class CmpUnitsOccupationDB : CmpGenericBase
	{
		public CmpUnitsOccupationDB()
		{
			_standardFields		= new string[] {"OUNI_ID", "OUNI_UNI_ID", "OUNI_LEVEL", "OUNI_INIDATE", "OUNI_ENDDATE"};
			_standardPks		= new string[] {"OUNI_ID"};
			_standardTableName	= "UNITS_OCCUPATION";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OUNI_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}


