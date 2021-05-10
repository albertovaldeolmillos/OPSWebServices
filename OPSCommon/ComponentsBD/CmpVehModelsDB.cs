namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for VEHMODELS.
	/// </summary>
	public class CmpVehModelsDB : CmpGenericBase
	{
		public CmpVehModelsDB()
		{
			_standardFields		= new string[] {"VMOD_ID", "VMOD_DESCSHORT", "VMOD_DESCLONG", "VMOD_VMAN_ID"};
			_standardPks		= new string[] {"VMOD_ID"};
			_standardTableName	= "VEHMODELS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"VMOD_VMAN_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpVehManufacturersDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"VMOD_VALID", "VMOD_DELETED"};
		}
	}
}


