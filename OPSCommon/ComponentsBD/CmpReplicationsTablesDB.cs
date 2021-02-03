namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for REPLICATIONS_TABLES.
	/// </summary>
	public class CmpReplicationsTablesDB : CmpGenericBase
	{
		public CmpReplicationsTablesDB()
		{
			_standardFields		= new string[] {"TREP_ID", "TREP_DREP_ID", "TREP_COD_ID", "TREP_TABLE", "TREP_NEWVERSION", "TREP_OLDVERSION"};
			_standardPks		= new string[] {"TREP_ID"};
			_standardTableName	= "REPLICATIONS_TABLES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"TREP_DREP_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpReplicationsDefDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"TREP_VALID", "TREP_DELETED"};
		}
	}
}
