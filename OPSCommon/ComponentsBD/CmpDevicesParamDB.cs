namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for DEVICES_PARAM.
	/// </summary>
	public class CmpDevicesParamDB : CmpGenericBase
	{
		public CmpDevicesParamDB()
		{
			_standardFields		= new string[] {"PDEV_ID", "PDEV_DESCSHORT", "PDEV_DESCLONG", "PDEV_VALTYPE", "PDEV_VALSTRING", "PDEV_VALINTEGER", "PDEV_VALFLOAT"};
			_standardPks		= new string[] {"PDEV_ID"};
			_standardTableName	= "DEVICES_PARAM";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"PDEV_VALID", "PDEV_DELETED"};
		}
	}
}
