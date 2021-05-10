namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_LOG_DEF.
	/// </summary>
	public class CmpUnitsLogDefDB : CmpGenericBase
	{
		public CmpUnitsLogDefDB()
		{
			_standardFields		= new string[] {"DLUNI_ID", "DLUNI_DESC", "DLUNI_PORT"};
			_standardPks		= new string[] {"DLUNI_ID"};
			_standardTableName	= "UNITS_LOG_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table

			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DLUNI_VALID", "DLUNI_DELETED"};
		}
	}
}

