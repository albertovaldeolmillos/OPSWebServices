using System;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VEHCOLORS.
	/// </summary>
	public class CmpVehColorDB: CmpGenericBase 
	{
		public CmpVehColorDB()
		{
			_standardFields		= new string[] {"VCOL_ID", "VCOL_DESCSHORT", "VCOL_DESCLONG"};
			_standardPks		= new string[] {"VCOL_ID"};
			_standardTableName	= "VEHCOLORS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; 
			_standardRelationTables	= new string[0]; 
			_stValidDeleted			= new string[] {"VCOL_VALID", "VCOL_DELETED"};
		}
	}
}




