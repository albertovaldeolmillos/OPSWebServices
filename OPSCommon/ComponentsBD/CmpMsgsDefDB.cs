namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_DEF.
	/// </summary>
	public class CmpMsgsDefDB: CmpGenericBase
	{
		public CmpMsgsDefDB()
		{
			_standardFields		= new string[] {"DMSG_ID", "DMSG_DESCSHORT", "DMSG_LIT_ID", "DMSG_HISMANDATORY", "DMSG_PROCESSTIME", "DMSG_USERTIME", "DMSG_PRIORITY"};
			_standardPks		= new string[] {"DMSG_ID"};
			_standardTableName	= "MSGS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DMSG_HISMANDATORY"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DMSG_VALID", "DMSG_DELETED"};
		}
	}
}
