namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_PHY_DEF.
	/// </summary>
	public class CmpUnitsPhyDefDB : CmpGenericBase
	{
		public CmpUnitsPhyDefDB() 
		{
			_standardFields		= new string[] {"DPUNI_ID", "DPUNI_DESC", "DPUNI_DLUNI_ID"};
			_standardPks		= new string[] {"DPUNI_ID"};
			_standardTableName	= "UNITS_PHY_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DPUNI_DLUNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsLogDefDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DPUNI_VALID", "DPUNI_DELETED"};
		}
	}
}
