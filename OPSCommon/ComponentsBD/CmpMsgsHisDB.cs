namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_HIS.
	/// </summary>
	public class CmpMsgsHisDB : CmpGenericBase
	{
		public CmpMsgsHisDB()
		{
			_standardFields		= new string[] 
				{"HMSG_ID", "HMSG_DMSG_ID", "HMSG_MMSG_ID", "HMSG_DATE", "HMSG_PRIORITY",
				"HMSG_MANDATORY", "HMSG_HMSG_ID", "HMSG_HMSG_ORDER", "HMSG_XML", "HMSG_UNI_ID",
				"HMSG_IPADAPTER", "HMSG_PORTADAPTER", "HMSG_STATUS", "HMSG_NUMRETRIES", "HMSG_LASTRETRY",
				"HMSG_TOTALRETRIES", "HMSG_PARCIALRETRIES", "HMSG_TOTALINTERVAL", "HMSG_PARCIALINTERVAL", "HMSG_TOTALTIME"};
			_standardPks		= new string[] {"HMSG_ID"};
			_standardTableName	= "MSGS_HIS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}