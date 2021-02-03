namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for BLACK_LISTS.
	/// </summary>
	public class CmpBlackListsDB: CmpGenericBase
	{
		public CmpBlackListsDB()
		{
			_standardFields		= new string[] {"BLIS_ID", "BLIS_DBLIS_ID", "BLIS_VALUE", "BLIS_PARAM1", "BLIS_PARAM2", "BLIS_PARAM3"};
			_standardPks		= new string[] {"BLIS_ID"};
			_standardTableName	= "BLACK_LISTS";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"BLIS_DBLIS_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpBlackListsDefDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"BLIS_VALID", "BLIS_DELETED"};
		}
	}
}
