using System;
using System.Data;


namespace OPS.Components.Data
{
	public class CmpDeleteCommandsDB : CmpGenericBase
	{
		public CmpDeleteCommandsDB()
		{
			_standardFields		= new string[]
				{ "MSG_ID", "MSG_DMSG_ID", "MSG_MMSG_ID", "MSG_DATE", "MSG_PRIORITY",
				"MSG_MANDATORY", "MSG_MSG_ID", "MSG_MSG_ORDER", "MSG_XML", "MSG_UNI_ID",
				"MSG_IPADAPTER", "MSG_PORTADAPTER", "MSG_STATUS", "MSG_NUMRETRIES", "MSG_LASTRETRY",
				"MSG_TOTALRETRIES", "MSG_PARCIALRETRIES", "MSG_TOTALINTERVAL", "MSG_PARCIALINTERVAL", "MSG_TOTALTIME",
				"MSG_HISMANDATORY" };
			_standardPks		= new string[] { "MSG_ID" };
			_standardTableName	= "MSGS";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[]
				{ "MSG_MMSG_ID",
				"MSG_MSG_ID",
				"MSG_UNI_ID" };
			_standardRelationTables	= new string[]
				{ "OPS.Components.Data.CmpMsgsMediaDB,ComponentsBD",
				"OPS.Components.Data.CmpMsgsDB,ComponentsBD",
				"OPS.Components.Data.CmpUnitsDB,ComponentsBD" };
			_stValidDeleted			= new string[0];

		}

		public override void GetForeignData(DataSet ds, string sTable)
		{
			// Get the table of GroupsDef
			DataTable dtMsgsMedia = new CmpMsgsMediaDB().GetData();
			ds.Tables.Add (dtMsgsMedia);
			// Get the table of Units
			DataTable dtUnits = new CmpUnitsDB().GetData();
			ds.Tables.Add (dtUnits);


			DataTable parent = ds.Tables[sTable];
			ds.Relations.Add ((dtMsgsMedia.PrimaryKey)[0],parent.Columns["MSG_MMSG_ID"]);
			ds.Relations.Add ((parent.PrimaryKey)[0],parent.Columns["MSG_MSG_ID"]);
			ds.Relations.Add ((dtUnits.PrimaryKey)[0], parent.Columns["MSG_UNI_ID"]);
		}
	}
}


