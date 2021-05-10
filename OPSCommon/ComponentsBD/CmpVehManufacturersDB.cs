namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VEHMANUFACTURERS.
	/// </summary>
	public class CmpVehManufacturersDB : CmpGenericBase
	{
		public CmpVehManufacturersDB()
		{
			_standardFields		= new string[] {"VMAN_ID", "VMAN_DESCSHORT", "VMAN_DESCLONG" };
			_standardPks		= new string[] {"VMAN_ID"};
			_standardTableName	= "VEHMANUFACTURERS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds		= new string[0];
			_standardRelationTables		= new string[0];
			_stValidDeleted				= new string[] {"VMAN_VALID", "VMAN_DELETED"};
		}
	}
}

