using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for UNITS_MEASURES.
	/// </summary>
	public class CmpModBusDataDB : CmpGenericBase
	{
		public CmpModBusDataDB()
		{
			_standardFields		= new string[] {"DM_ID", "DM_UNI_ID", "DM_ADDRESS", 
												   "DM_DESC", 
												   "DM_VALUEIN", 
												   "DM_VALUEOUT"};
			_standardPks		= new string[] {"DM_ID"};
			_standardTableName	= "MODBUS_DATA";
			_standardOrderByField	= "DM_ADDRESS";
			_standardOrderByAsc		= "ASC";
		
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