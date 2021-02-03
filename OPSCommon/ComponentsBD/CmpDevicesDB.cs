namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for DEVICES.
	/// </summary>
	public class CmpDevicesDB : CmpGenericBase
	{
		public CmpDevicesDB()
		{
			_standardFields		= new string[] {"DEV_ID", "DEV_DESCSHORT", "DEV_DESCLONG", "DEV_DDEV_ID", "DEV_MMSG_ID", "DEV_DPUNI_ID"};
			_standardPks		= new string[] {"DEV_ID"};
			_standardTableName	= "DEVICES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DEV_DDEV_ID","DEV_MMSG_ID","DEV_DPUNI_ID","DEV_STATUS"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpDevicesDefDB,ComponentsBD","OPS.Components.Data.CmpMsgsMediaDB,ComponentsBD","OPS.Components.Data.CmpUnitsPhyDefDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DEV_VALID", "DEV_DELETED"};
		}
	}
}