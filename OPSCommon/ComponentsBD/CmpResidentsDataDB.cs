using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for RESIDENTS.
	/// </summary>
	public class CmpResidentsDataDB : CmpGenericBase
	{
		public CmpResidentsDataDB()
		{
			_standardFields		= new string[] {"RESD_ID",         
												"RESD_IDDOC",     
												"RESD_REQ_DATE",   
												"RESD_GRP_ID",     
												"RESD_VEHICLEID",  
												"RESD_NAME",       
												"RESD_SURNAME",    
												"RESD_ADDRESS",    
												"RESD_ZIPCODE",    
												"RESD_CITY",      
												"RESD_TELEPHONE1", 
												"RESD_TELEPHONE2", 
												"RESD_REMARKS",    
												"RESD_ACTIVED",
												"RESD_DART_ID"    
												};
			_standardPks		= new string[] {"RESD_ID"};
			_standardTableName	= "RESIDENTS_DATA";
			_standardOrderByField	= "RESD_ID";
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