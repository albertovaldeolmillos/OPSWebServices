namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for PDA_USER_STAT.
	/// </summary>
	public class CmpPdaUserStatDB : CmpGenericBase
	{
		public CmpPdaUserStatDB() 
		{
			_standardFields			= new string[] 
				{"SPDA_ID", "SPDA_UNI_ID", "SPDA_USER_ID", "SPDA_INIDATE", "SPDA_ENDDATE",
				"SPDA_IDLETIME", "SPDA_NUMVEHICLES", "SPDA_NUMFINES", "SPDA_NUMREPVEH_1", "SPDA_NUMREP_1",
				"SPDA_NUMREPVEH_2", "SPDA_NUMREP_2", "SPDA_NUMREPVEH_3", "SPDA_NUMREP_3", "SPDA_COMMENT"};
			_standardPks			= new string[] {"SPDA_ID"};
			_standardTableName		= "PDA_USER_STAT";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"SPDA_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}	
	}
}
