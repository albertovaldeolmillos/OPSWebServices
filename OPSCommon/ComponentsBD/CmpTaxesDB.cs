namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for TAXES.
	/// </summary>
	public class CmpTaxesDB : CmpGenericBase
	{
		public CmpTaxesDB()
		{
			_standardFields		= new string[] {"TAX_ID", "TAX_DESCSHORT", "TAX_DESCLONG", "TAX_TYPE", "TAX_VALUE", "TAX_DEFAULT"};
			_standardPks		= new string[] {"TAX_ID"};
			_standardTableName	= "TAXES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"TAX_DEFAULT"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"TAX_VALID","TAX_DELETED"};
		}
	}
}
