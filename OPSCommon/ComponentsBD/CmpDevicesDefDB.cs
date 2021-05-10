namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for DEVICES_DEF.
	/// </summary>
	public class CmpDevicesDefDB: CmpGenericBase
	{
		public CmpDevicesDefDB()
		{
			_standardFields		= new string[] {"DDEV_ID", "DDEV_DESCSHORT", "DDEV_DESCLONG"};
			_standardPks		= new string[] {"DDEV_ID"};
			_standardTableName	= "DEVICES_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DDEV_VALID", "DDEV_DELETED"};
		}
	}
}

