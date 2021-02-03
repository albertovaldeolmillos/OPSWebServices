using System.Data;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for ROL_PERMISSIONS.
	/// </summary>
	public class CmpRolPermissionsDB : CmpGenericBase
	{
		public CmpRolPermissionsDB()
		{
			_standardFields		= new string[] {"RPER_ID", "RPER_ROL_ID", "RPER_VELE_VIE_ID", "RPER_VELE_ELEMENTNUMBER", "RPER_INSALLOWED", "RPER_UPDALLOWED", "RPER_DELALLOWED", "RPER_EXEALLOWED"};
			_standardPks		= new string[] {"RPER_ID"};
			_standardTableName	= "ROL_PERMISSIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"RPER_VALID", "RPER_DELETED"};
		}

		public override void GetForeignData(DataSet ds, string sTable)
		{
			GetForeignData(ds, sTable, null);
		}

		/*public override void GetForeignData(DataSet ds, string sTable, OPS.Components.Globalization.IResourceManager rm) 
		{
			DataTable dtCmpRolesDB = new CmpRolesDB().GetData();
			ds.Tables.Add(dtCmpRolesDB);

			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
			ds.Tables.Add(dtCmpCodesDB);

			DataTable dtCmpViewsDB = new CmpViewsDB().GetData();
			ds.Tables.Add(dtCmpViewsDB);

			/RPER_VELE_VIE_ID, RPER_VELE_ELEMENTNUMBER
			DataTable dtViewElements = new CmpViewsElementsDB().GetData();
			ds.Tables.Add(dtViewElements);

			DataColumn[] columns1 = new DataColumn[2];
			columns1[0] = (dtViewElements.PrimaryKey)[0];
			columns1[1] = (dtViewElements.PrimaryKey)[1];

			DataColumn[] columns2 = new DataColumn[2];
			columns2[0] = ds.Tables[sTable].Columns["RPER_VELE_VIE_ID"];
			columns2[1] = ds.Tables[sTable].Columns["RPER_VELE_ELEMENTNUMBER"];

			DataTable parent = ds.Tables[sTable];
			ds.Relations.Add((dtCmpRolesDB.PrimaryKey)[0], parent.Columns["RPER_ROL_ID"]);
			ds.Relations.Add((dtCmpViewsDB.PrimaryKey)[0], parent.Columns["RPER_VELE_VIE_ID"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["RPER_INSALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["RPER_UPDALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["RPER_DELALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["RPER_EXEALLOWED"]);
			//ds.Relations.Add(columns1, columns2);
		}*/
	}
}
