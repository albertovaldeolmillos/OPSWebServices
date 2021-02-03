namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS_DEF.
	/// </summary>
	public class CmpAlarmsDefDB : CmpGenericBase
	{
		public CmpAlarmsDefDB()
		{
			_standardFields		= new string[] {"DALA_ID", "DALA_DESCSHORT", "DALA_DESCLONG", "DALA_LIT_ID", "DALA_DSTA_ID"};
			_standardPks		= new string[] {"DALA_ID"};
			_standardTableName	= "ALARMS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"DALA_VALID", "DALA_DELETED"};
		}
	}
}
