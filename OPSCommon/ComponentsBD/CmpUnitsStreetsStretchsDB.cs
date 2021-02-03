namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STREETS.
	/// </summary>
	public class CmpUnitsStreetsStretchsDB : CmpGenericBase
	{
		public CmpUnitsStreetsStretchsDB() 
		{
			_standardFields				= new string[] { "USS_ID", "USS_UNI_ID", "USS_SS_ID"};
			_standardPks				= new string[] {"USS_ID"};
			_standardTableName			= "UNITS_STREETS_STRETCHS";
			_standardOrderByField		= "USS_ID";
			_standardOrderByAsc			= "ASC";
		
			_standardRelationFileds		= new string[0]; 
			_standardRelationTables		= new string[0];
			_stValidDeleted				= new string[0];
		}
	}
}