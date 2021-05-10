using System;
using System.Data;

using System.Text;

namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpVwRolPermissions.
	/// </summary>
	public class CmpVwRolPermissionsDB: CmpGenericBase
	{
		public CmpVwRolPermissionsDB()
		{
			_standardFields		= new string[] {"VELE_VIE_ID","VELE_ELEMENTNUMBER","RPER_ROL_ID","RPER_UPDALLOWED","RPER_INSALLOWED","RPER_EXEALLOWED","RPER_DELALLOWED"};
			_standardPks		= new string[] {"VELE_VIE_ID","VELE_ELEMENTNUMBER","RPER_ROL_ID"};
			_standardTableName	= "VW_ROL_PERMISSIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[] {"RPER_UPDALLOWED","RPER_INSALLOWED","RPER_EXEALLOWED","RPER_DELALLOWED"};
			_standardRelationTables	= new string[] {};
			_stValidDeleted			= new string[] {};
		}
	}
}


