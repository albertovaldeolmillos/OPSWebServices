namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for FINES_DEF.
	/// </summary>
	public class CmpFinesDefDB: CmpGenericBase
	{
		public CmpFinesDefDB() 
		{
			_standardFields		= new string[] {"DFIN_ID", "DFIN_COD_ID", "DFIN_DESCSHORT", "DFIN_DESCLONG", "DFIN_VALUE", "DFIN_STATUS", "DFIN_SIGN", "DFIN_PAYINPDM", "DFIN_NUMTICKETS"};
			_standardPks		= new string[] {"DFIN_ID"};
			_standardTableName	= "FINES_DEF";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DFIN_STATUS"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DFIN_VALID", "DFIN_DELETED"};
		}
	}
}
