namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS_DEF.
	/// </summary>
	public class CmpOperationsDefDB : CmpGenericBase
	{
		public CmpOperationsDefDB() 
		{
			_standardFields		= new string[] {"DOPE_ID", "DOPE_DESCSHORT", "DOPE_DESCLONG", "DOPE_SIGN", "DOPE_STAT", "DOPE_NUMTICKETS"};
			_standardPks		= new string[] {"DOPE_ID"};
			_standardTableName	= "OPERATIONS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DOPE_STAT"};
			_standardRelationTables = new string[0]; //{"OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}