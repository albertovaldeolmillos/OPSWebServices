namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for PAYTYPES_DEF.
	/// </summary>
	public class CmpCCTransactionStateDefDB : CmpGenericBase
	{
		public CmpCCTransactionStateDefDB()
		{
			_standardFields		= new string[] {"CCTSD_ID", "CCTSD_DES", "CCTSD_LIT_ID"};
			_standardPks		= new string[] {"CCTSD_ID"};
			_standardTableName	= "CC_TRANSACTION_STATE_DEF";
			_standardOrderByField	= "CCTSD_ID";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

	}
}
