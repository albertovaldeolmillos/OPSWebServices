namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_CONFIG.
	/// </summary>
	public class CmpMsgsConfigDB : CmpGenericBase
	{
		public CmpMsgsConfigDB()
		{
			_standardFields		= new string[] {"CMSG_ID", "CMSG_DMSG_ID", "CMSG_DESCSHORT", "CMSG_DESCLONG", "CMSG_DLUNI_ID", "CMSG_LIT_ID"};
			_standardPks		= new string[] {"CMSG_ID"};
			_standardTableName	= "MSGS_CONFIG";
			_standardOrderByField = "";
			_standardOrderByAsc = "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"CMSG_DMSG_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpMsgsDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

