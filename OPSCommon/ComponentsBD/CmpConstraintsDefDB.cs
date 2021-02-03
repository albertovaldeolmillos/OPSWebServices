namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for CONSTRAINTS_DEF.
	/// </summary>
	public class CmpConstraintsDefDB : CmpGenericBase
	{
		public CmpConstraintsDefDB()
		{
			_standardFields		= new string[] {"DCON_ID", "DCON_DESCSHORT", "DCON_DESCLONG"};
			_standardPks		= new string[] {"DCON_ID"};
			_standardTableName	= "CONSTRAINTS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DCON_VALID", "DCON_DELETED"};
		}
	}
}
