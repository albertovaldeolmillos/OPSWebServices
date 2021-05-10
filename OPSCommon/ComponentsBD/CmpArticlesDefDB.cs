using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ARTICLES_DEF.
	/// </summary>
	public class CmpArticlesDefDB : CmpGenericBase
	{
		public CmpArticlesDefDB()
		{
			_standardFields		= new string[] {"DART_ID", "DART_DESCSHORT", "DART_DESCLONG", "DART_TAX_ID", "DART_REQUIRED"};
			_standardPks		= new string[] {"DART_ID"};
			_standardTableName	= "ARTICLES_DEF";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DART_TAX_ID","DART_REQUIRED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpTaxesDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DART_VALID", "DART_DELETED"};
		}
	}
}
