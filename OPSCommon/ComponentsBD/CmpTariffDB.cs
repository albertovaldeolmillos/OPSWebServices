namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for TARIFFS.
	/// </summary>
	public class CmpTariffsDB : CmpGenericBase
	{
		public CmpTariffsDB()
		{
			_standardFields		= new string[] {"TAR_ID", "TAR_NUMBER", "TAR_DESCSHORT"," TAR_DESCLONG", "TAR_TAR_ID", "TAR_DISCOUNT", "TAR_STAR_ID", "TAR_DDAY_ID", "TAR_DAY_ID", "TAR_TIM_ID", "TAR_INIDATE", "TAR_ENDDATE", "TAR_SIGN", "TAR_NEXTDAY", "TAR_NEXTBLOCK", "TAR_RNEXTBLOCKTIME", "TAR_RNEXTBLOCKINT", "TAR_RNEXTDAYINT", "TAR_RNEXTDAYTIME", "TAR_ADDFREETIME", "TAR_NB_CONDITIONAL_VALUE", "TAR_MAXTIMEFORNOTAPPLYREENTRY", "TAR_ROUNDTOENDOFDAY", "TAR_NUMDAYS_PASSED", "TAR_UNIQUE_ID"};
			_standardPks		= new string[] {"TAR_UNIQUE_ID"};
			_standardTableName	= "TARIFFS";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"TAR_DDAY_ID","TAR_STAR_ID","TAR_TIM_ID","TAR_DAY_ID"};
			_standardRelationTables	= new string[0]; //{"DAYS_DEF","SUBTARIFFS","TIMETABLES","DAYS"};
			_stValidDeleted			= new string[] {"TAR_VALID", "TAR_DELETED"};
		}
		
//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//			DataTable dtCmpDaysDefDB = new CmpDaysDefDB().GetData();
//			DataTable dtCmpSubtariffsDB = new CmpSubtariffsDB().GetData();
//			DataTable dtCmpTimeTablesDB = new CmpTimeTablesDB().GetData();
//			DataTable dtCmpDaysDB = new CmpDaysDB().GetData();
//			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
//
//			ds.Tables.Add (dtCmpDaysDefDB);
//			ds.Tables.Add (dtCmpSubtariffsDB);
//			ds.Tables.Add (dtCmpTimeTablesDB);
//			ds.Tables.Add (dtCmpDaysDB);
//			ds.Tables.Add (dtCmpCodesDB);
//
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtCmpDaysDefDB.PrimaryKey)[0],parent.Columns["TAR_DDAY_ID"]);
//			ds.Relations.Add ((dtCmpSubtariffsDB.PrimaryKey)[0], parent.Columns["TAR_STAR_ID"]);
//			ds.Relations.Add ((dtCmpTimeTablesDB.PrimaryKey)[0],parent.Columns["TAR_TIM_ID"]);
//			ds.Relations.Add ((dtCmpDaysDB.PrimaryKey)[0],parent.Columns["TAR_DAY_ID"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_NEXTDAY"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_NEXTBLOCK"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_RNEXTBLOCKTIME"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_RNEXTBLOCKINT"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_RNEXTDAYINT"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_RNEXTDAYTIME"]);
//			ds.Relations.Add ((dtCmpCodesDB.PrimaryKey)[0],parent.Columns["TAR_ADDFREETIME"]);
//		}
	}
}