namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS_HIS.
	/// </summary>
	public class CmpAlarmsHisDB : CmpGenericBase
	{
		public CmpAlarmsHisDB()
		{
			_standardFields		= new string[] {"HALA_ID", "HALA_DALA_ID", "HALA_UNI_ID", "HALA_INIDATE", "HALA_ENDDATE"};
			_standardPks		= new string[] {"HALA_ID"};
			_standardTableName	= "ALARMS_HIS";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"HALA_DALA_ID","HALA_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpAlarmsDefDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}