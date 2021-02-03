namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STATUS_DEF.
	/// </summary>
	public class CmpStatusDefDB : CmpGenericBase
	{
		public CmpStatusDefDB()
		{
			_standardFields		= new string[] {"DSTA_ID", "DSTA_DESCSHORT", "DSTA_DESCLONG", "DSTA_LIT_ID"};
			_standardPks		= new string[] {"DSTA_ID"};
			_standardTableName	= "STATUS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DSTA_LIT_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpLiteralesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DSTA_VALID", "DSTA_DELETED"};
		}
	}
}



