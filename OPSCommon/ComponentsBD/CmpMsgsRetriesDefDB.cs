namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_RETRIES_DEF.
	/// </summary>
	public class CmpMsgsRetriesDefDB : CmpGenericBase
	{
		public CmpMsgsRetriesDefDB()
		{
			_standardFields		= new string[] 
				{"DRMSG_ID", "DRMSG_DMSG_ID", "DRMSG_MMSG_ID", "DRMSG_DREP_ID", "DRMSG_TOTALRETRIES",
				"DRMSG_PARCIALRETRIES", "DRMSG_TOTALINTERVAL", "DRMSG_TOTALTIME", "DRMSG_PARCIALINTERVAL"};
			_standardPks		= new string[] {"DRMSG_ID"};
			_standardTableName	= "MSGS_RETRIES_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DRMSG_DMSG_ID","DRMSG_DREP_ID","DRMSG_MMSG_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpMsgsDefDB,ComponentsBD","OPS.Components.Data.CmpReplicationsDefDB,ComponentsBD","OPS.Components.Data.CmpMsgsMediaDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DRMSG_VALID", "DRMSG_DELETED"};
		}
	}
}
