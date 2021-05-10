namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS_DEF.
	/// </summary>
	public class CmpAlarmsLevelDefDB : CmpGenericBase
	{
		public CmpAlarmsLevelDefDB()
		{
			_standardFields		= new string[] {"DALV_ID", "DALV_DESCSHORT", "DALV_COLOR"};
			_standardPks		= new string[] {"DALV_ID"};
			_standardTableName	= "ALARMS_LEVEL_DEF";
			_standardOrderByField	= "DALV_ID";
			_standardOrderByAsc		= "ASC";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];;
		}
	}
}
