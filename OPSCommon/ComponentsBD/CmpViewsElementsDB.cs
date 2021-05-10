using System;
namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VIEWS_ELEMENTS.
	/// </summary>
	public class CmpViewsElementsDB : CmpGenericBase
	{
		public CmpViewsElementsDB()
		{
			_standardFields		= new string[] {"VELE_ID", "VELE_VIE_ID", "VELE_ELEMENTNUMBER", "VELE_DESCSHORT", "VELE_DESCLONG"};
			_standardPks		= new string[] {"VELE_ID"};
			_standardTableName	= "VIEWS_ELEMENTS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"VELE_VALID", "VELE_DELETED"};
		}

		/*public override Int64 LastPKValue
		{
			get 
			{
				return Convert.ToInt64(-2); 
			}
		}*/
	}
}



