using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_MEASURES.
	/// </summary>
	public class CmpUnitsMeasuresHisDB : CmpGenericBase
	{
		public CmpUnitsMeasuresHisDB()
		{
			_standardFields		= new string[] {"HMUNI_ID", "HMUNI_DATE", "HMUNI_UNI_ID", 
												   "HMUNI_VALUE1", 
												   "HMUNI_VALUE2", 
												   "HMUNI_VALUE3",
												   "HMUNI_VALUE4", 
												   "HMUNI_VALUE5",
												   "HMUNI_VALUE6", 
												   "HMUNI_VALUE7", 
												   "HMUNI_VALUE8",
												   "HMUNI_VALUE9", 
												   "HMUNI_VALUE10"};
			_standardPks		= new string[] {"HMUNI_ID"};
			_standardTableName	= "UNITS_MEASURES_HIS";
			_standardOrderByField	= "HMUNI_DATE";
			_standardOrderByAsc		= "DESC";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"HMUNI_UNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpUnitsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}

		//		public override void GetForeignData(DataSet ds, string sTable)
		//		{
		//			DataTable dtCmpUnitsDB = new CmpUnitsDB().GetData();
		//			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
		//
		//			ds.Tables.Add (dtCmpUnitsDB);
		//			ds.Tables.Add (dtCmpCodesDB);
		//
		//			DataTable parent = ds.Tables[sTable];
		//			ds.Relations.Add ((dtCmpUnitsDB.PrimaryKey)[0],parent.Columns["MUNI_UNI_ID"]);
		//
		//		}

	}
}
