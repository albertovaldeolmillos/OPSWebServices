namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS_HIS.
	/// </summary>
	public class CmpUserEventsDB : CmpGenericBase
	{
		public CmpUserEventsDB()
		{
			_standardFields		= new string[] {"UE_ID", "UE_DUE_ID", "UE_UNI_ID", "UE_DATE", "UE_USER_ID", "UE_OPE_ID"};
			_standardPks		= new string[] {"UE_ID"};
			_standardTableName	= "USER_EVENTS";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"HALA_DALA_ID","HALA_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpAlarmsDefDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}