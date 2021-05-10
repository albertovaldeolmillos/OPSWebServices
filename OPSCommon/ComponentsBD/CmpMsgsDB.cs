namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS.
	/// </summary>
	public class CmpMsgsDB : CmpGenericBase
	{
//		private static System.Threading.Mutex m = new System.Threading.Mutex();

		public CmpMsgsDB()
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
			_standardRelationFileds	= new string[0];
				//{ "MSG_MMSG_ID",
				//"MSG_MSG_ID",
				//"MSG_UNI_ID",
				//"MSG_MANDATORY" };
			_standardRelationTables	= new string[0];
				//{ "OPS.Components.Data.CmpMsgsMediaDB,ComponentsBD",
				//"OPS.Components.Data.CmpMsgsDB,ComponentsBD",
				//"OPS.Components.Data.CmpUnitsDB,ComponentsBD",
				//"OPS.Components.Data.CmpCodesDB,ComponentsBD" };
			_stValidDeleted			= new string[0];
		}

//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//			// Get the table of GroupsDef
//			DataTable dtMsgsMedia = new CmpMsgsMediaDB().GetData();
//			ds.Tables.Add (dtMsgsMedia);
//			// Get the table of Units
//			DataTable dtUnits = new CmpUnitsDB().GetData();
//			ds.Tables.Add (dtUnits);
//
//
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtMsgsMedia.PrimaryKey)[0],parent.Columns["MSG_MMSG_ID"]);
//			ds.Relations.Add ((parent.PrimaryKey)[0],parent.Columns["MSG_MSG_ID"]);
//			ds.Relations.Add ((dtUnits.PrimaryKey)[0], parent.Columns["MSG_UNI_ID"]);
//		}
//
//		public override Int64 LastPKValue
//		{
//			get 
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(MSG_ID),0) FROM MSGS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//		
//		public override string MainTable  
//		{
//			get { return "MSGS"; }
//		} 
	}
}


