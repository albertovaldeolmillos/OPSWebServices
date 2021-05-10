using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for RESIDENTS.
	/// </summary>
	public class CmpResidentsDB : CmpGenericBase
	{
		public CmpResidentsDB()
		{
			_standardFields		= new string[] {"RES_ID", "RES_VEHICLEID", "RES_GRP_ID", 
												   "RES_DART_ID" };
			_standardPks		= new string[] {"RES_ID"};
			_standardTableName	= "RESIDENTS";
			_standardOrderByField	= "RES_VEHICLEID";
			_standardOrderByAsc		= "";
		
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