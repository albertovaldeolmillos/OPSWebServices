namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_MEDIA.
	/// </summary>
	public class CmpMsgsMediaDB : CmpGenericBase
	{
		public CmpMsgsMediaDB()
		{
			_standardFields		= new string[] {"MMSG_ID", "MMSG_DESCSHORT", "MMSG_DESCLONG", "MMSG_LIT_ID"};
			_standardPks		= new string[] {"MMSG_ID"};
			_standardTableName	= "MSGS_MEDIA";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"MMSG_VALID", "MMSG_DELETED"};
		}
	}
}
