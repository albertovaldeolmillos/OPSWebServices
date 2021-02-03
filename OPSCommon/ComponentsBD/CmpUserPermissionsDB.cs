using System.Data;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for USR_PERMISSIONS.
	/// </summary>
	public class CmpUserPermissionsDB : CmpGenericBase
	{
		public CmpUserPermissionsDB()
		{
			_standardFields		= new string[] {"UPER_ID", "UPER_USR_ID", "UPER_VELE_VIE_ID", "UPER_VELE_ELEMENTNUMBER", "UPER_INSALLOWED", "UPER_UPDALLOWED", "UPER_DELALLOWED", "UPER_EXEALLOWED"};
			_standardPks		= new string[] {"UPER_ID" };
			_standardTableName	= "USR_PERMISSIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"UPER_USR_ID"};
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"UPER_VALID", "UPER_DELETED"};
		}

		/*public override void GetForeignData(DataSet ds, string sTable)
		{
			GetForeignData(ds, sTable, null);
		}

		public override void GetForeignData(DataSet ds, string sTable, OPS.Components.Globalization.IResourceManager rm) 
		{
			DataTable dtCmpUsuarioDB = new CmpUsuarioDB().GetData();
			ds.Tables.Add(dtCmpUsuarioDB);

			DataTable dtCmpCodesDB = new CmpCodesDB().GetYesNoData();
			ds.Tables.Add(dtCmpCodesDB);

			//UPER_VELE_VIE_ID, UPER_VELE_ELEMENTNUMBER
			DataTable dtViewElements = new CmpViewsElementsDB().GetData();
			ds.Tables.Add(dtViewElements);

			DataColumn[] columns1 = new DataColumn[2];
			columns1[0] = (dtViewElements.PrimaryKey)[0];
			columns1[1] = (dtViewElements.PrimaryKey)[1];

			DataColumn[] columns2 = new DataColumn[2];
			columns2[0] = ds.Tables[sTable].Columns["UPER_VELE_VIE_ID"];
			columns2[1] = ds.Tables[sTable].Columns["UPER_VELE_ELEMENTNUMBER"];

			DataTable parent = ds.Tables[sTable];
			ds.Relations.Add((dtCmpUsuarioDB.PrimaryKey)[0], parent.Columns["UPER_USR_ID"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["UPER_INSALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["UPER_UPDALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["UPER_DELALLOWED"]);
			ds.Relations.Add((dtCmpCodesDB.PrimaryKey)[0], parent.Columns["UPER_EXEALLOWED"]);
			ds.Relations.Add(columns1, columns2);
		}*/
	}
}
