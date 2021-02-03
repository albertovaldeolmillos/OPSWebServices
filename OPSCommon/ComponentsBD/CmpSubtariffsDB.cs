namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for SUBTARIFFS.
	/// </summary>
	public class CmpSubtariffsDB : CmpGenericBase
	{
		public CmpSubtariffsDB()
		{
			_standardFields		= new string[] {"STAR_ID", "STAR_DESCSHORT", "STAR_DESCLONG"};
			_standardPks		= new string[] {"STAR_ID"};
			_standardTableName	= "SUBTARIFFS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"STAR_VALID", "STAR_DELETED"};
		}
	}
}