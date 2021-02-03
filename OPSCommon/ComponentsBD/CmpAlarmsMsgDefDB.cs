namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ALARMS_MSGS_DEF.
	/// </summary>
	public class CmpAlarmsMsgDefDB : CmpGenericBase
	{
		public CmpAlarmsMsgDefDB()
		{
			_standardFields		= new string[] {"DMALA_ID", "DMALA_DALA_ID", "DMALA_GRP_ID", "DMALA_DMSG_ID", "DMALA_LEVEL"};
			_standardPks		= new string[] {"DMALA_ID"};
			_standardTableName	= "ALARMS_MSGS_DEF";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
				//{ "DMALA_DALA_ID",
				//"DMALA_DMSG_ID",
				//"DMALA_GRP_ID",
				//"DMALA_LEVEL" };
			_standardRelationTables	= new string[0];
				//{ "OPS.Components.Data.CmpAlarmsDefDB,ComponentsBD",
				//"OPS.Components.Data.CmpMsgsDefDB,ComponentsBD",
				//"OPS.Components.Data.CmpGroupsDB,ComponentsBD",
				//"OPS.Components.Data.CmpCodesDB,ComponentsBD" };
			_stValidDeleted			= new string[0];
		}
	}
}

