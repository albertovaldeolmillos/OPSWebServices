namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for TIMETABLES.
	/// </summary>
	public class CmpTimetablesDB : CmpGenericBase
	{
		public CmpTimetablesDB() 
		{
			_standardFields		= new string[] {"TIM_ID", "TIM_DESC", "TIM_INI", "TIM_END"};
			_standardPks		= new string[] {"TIM_ID"};
			_standardTableName	= "TIMETABLES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"TIM_VALID", "TIM_DELETED"};
		}
	}
}