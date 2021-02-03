namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GPRS_TRAFFIC.
	/// </summary>
	public class CmpGprsTrafficDB : CmpGenericBase
	{
		public CmpGprsTrafficDB()
		{
			_standardFields		= new string[] {"GTRA_ID", "GTRA_UNI_ID", "GTRA_UNI_ID_TARGET", "GTRA_INIDATE", "GTRA_ENDDATE", "GTRA_BYTES_OK_IN", "GTRA_BYTES_NOK_IN", "GTRA_NUMERRORS_IN", "GTRA_BYTES_OK_OUT", "GTRA_BYTES_NOK_OUT", "GTRA_NUMERRORS_OUT", "GTRA_NUMSESSIONS_OK", "GTRa_NUMSESSIONS_NOK", "GTRA_SENT"};
			_standardPks		= new string[] {"GTRA_ID"};
			_standardTableName	= "GPRS_TRAFFIC";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}