using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for CODES.
	/// </summary>
	public class CmpCodesDB: CmpGenericBase
	{
		public CmpCodesDB() 
		{

			_standardFields		= new string[] {"COD_ID", "COD_CATEGORY", "COD_DESCSHORT", "COD_DESCLONG", "COD_LIT_ID"};
			_standardPks		= new string[] {"COD_ID" };
			_standardTableName	= "CODES";
		
			_standardRelationFileds = new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"COD_VALID", "COD_DELETED"};
		}
	
		public DataTable GetYesNoData () 
		{
			Database d = DatabaseFactory.GetDatabase ();
			string[] fields = new string[] { "COD_ID", "COD_LIT_ID" };//, "COD_DESCSHORT" };
			string[] pk = _standardPks;
			string where = "COD_ID=0 OR COD_ID=1";
			System.Text.StringBuilder sb = base.ProcessFields(fields, pk);
			return base.DoGetData(sb.ToString(), where, null, null, _standardTableName, _standardTableName, pk);
		}
	
		public DataTable GetYesNoData(string[] fields) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			if (fields == null) fields = new string[] { "COD_ID", "COD_LIT_ID" };//, "COD_DESCSHORT" };
			string[] pk = _standardPks;
			string where = "COD_ID=0 OR COD_ID=1";
			System.Text.StringBuilder sb = base.ProcessFields(fields, pk);
			return base.DoGetData(sb.ToString(), where, null, null, _standardTableName, _standardTableName, pk);
		}		
	}
}
