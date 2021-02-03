namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_FMT.
	/// </summary>
	public class CmpMsgsFmtDB : CmpGenericBase
	{
		public CmpMsgsFmtDB()
		{
			_standardFields		= new string[] {"FMSG_ID", "FMSG_DESCSHORT", "FMSG_DESCLONG", "FMSG_LIT_ID", "FMSG_XSL"};
			_standardPks		= new string[] {"FMSG_ID"};
			_standardTableName	= "MSGS_FMT";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"FMSG_VALID", "FMSG_DELETED"};
		}
	}
}
