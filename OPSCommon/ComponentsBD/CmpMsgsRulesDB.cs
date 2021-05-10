namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_RULES.
	/// </summary>
	public class CmpMsgsRulesDB : CmpGenericBase
	{
		public CmpMsgsRulesDB()
		{
			_standardFields		= new string[] 
				{"RMSG_ID", "RMSG_DMSG_ID", "RMSG_DLUNI_SOURCE", "RMSG_DLUNI_DESTINATION", "RMSG_FMSG_ID",
				"RMSG_MMSG_ID", "RMSG_MANDATORY", "RMSG_PRIORITY", "RMSG_RETRIES", "RMSG_IPADAPTER",
				"RMSG_PORTADAPTER", "RMSG_PARAM1", "RMSG_PARAM2", "RMSG_PARAM3", "RMSG_STATUS"};
			_standardPks		= new string[] {"RMSG_ID"};
			_standardTableName	= "MSGS_RULES";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"RMSG_DLUNI_DESTINATION","RMSG_DLUNI_SOURCE","RMSG_MANDATORY"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsLogDefDB,ComponentsBD","OPS.Components.Data.CmpUnitsLogDefDB,ComponentsBD","OPS.Components.Data.CmpCodesDB,ComponentsBD"};
			_stValidDeleted			= new string[] {"RMSG_VALID", "RMSG_DELETED"};
		}

//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//
////			* El camp "Formato" (RMSG_FMSG_ID) ha de ser una FK cap a MSGS_FMT. 
////			* El camp "Medio" (RMSG_MMSG_ID) ha de ser una FK cap a MSGS_MEDIA. 
// 
//
//			// Get the table of Units
//			DataTable dtUnitsLog = new CmpUnitsLogDefDB().GetData();
//			ds.Tables.Add (dtUnitsLog);
//			// Get the table of Messages Def
//			DataTable dtMessagesDef = new CmpMsgsDefDB().GetData();
//			ds.Tables.Add (dtMessagesDef);
//			// Get the table of Messages Def
//			DataTable dtMessagesMedia = new CmpMsgsMediaDB().GetData();
//			ds.Tables.Add (dtMessagesMedia);
//			// Get the table of Messages Def
//			DataTable dtMessagesFmt = new CmpMsgsFmtDB().GetData();
//			ds.Tables.Add (dtMessagesFmt);
//
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtUnitsLog.PrimaryKey)[0],parent.Columns["RMSG_DLUNI_DESTINATION"]);
//			ds.Relations.Add ((dtUnitsLog.PrimaryKey)[0], parent.Columns["RMSG_DLUNI_SOURCE"]);
//			ds.Relations.Add ((dtMessagesDef.PrimaryKey)[0], parent.Columns["RMSG_DMSG_ID"]);
//			ds.Relations.Add ((dtMessagesMedia.PrimaryKey)[0], parent.Columns["RMSG_MMSG_ID"]);
//			ds.Relations.Add ((dtMessagesFmt.PrimaryKey)[0], parent.Columns["RMSG_FMSG_ID"]);
//		}
	}
}

