namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for REPLICATIONS_DEF.
	/// </summary>
	public class CmpReplicationsDefDB : CmpGenericBase
	{
		public CmpReplicationsDefDB()
		{
			_standardFields			= new string[] 
				{"DREP_ID", "DREP_DESCSHORT", "DREP_DESCLONG", "DREP_DLUNI_SOURCE", "DREP_DLUNI_DESTINATION",
				"DREP_DGRP_ID", "DREP_FREQUENCY", "DREP_INIDATE", "DREP_ENDDATE", "DREP_LAST", "DREP_INIT"};
			_standardPks			= new string[] {"DREP_ID"};
			_standardTableName		= "REPLICATIONS_DEF";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"DREP_DGRP_ID","DREP_DLUNI_DESTINATION","DREP_DLUNI_SOURCE","DREP_INIT"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpUnitsLogDefDB,ComponentsBD","OPS.Components.Data.CmpUnitsLogDefDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"DREP_VALID", "DREP_DELETED"};
		}

//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//			// Get the table of GroupsDef
//			DataTable dtGroupsDef = new CmpGroupsDefDB().GetData();
//			ds.Tables.Add (dtGroupsDef);
//			// Get the table of Units
//			DataTable dtUnitsLog = new CmpUnitsLogDefDB().GetData();
//			ds.Tables.Add (dtUnitsLog);
//
//
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtGroupsDef.PrimaryKey)[0],parent.Columns["DREP_DGRP_ID"]);
//			ds.Relations.Add ((dtUnitsLog.PrimaryKey)[0],parent.Columns["DREP_DLUNI_DESTINATION"]);
//			ds.Relations.Add ((dtUnitsLog.PrimaryKey)[0], parent.Columns["DREP_DLUNI_SOURCE"]);
//		}
	}
}
