using System;
using System.Data;

using System.Text;

namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpVwUsrPermissions.
	/// </summary>
	public class CmpVwUsrPermissionsDB: CmpGenericBase
	{
		public CmpVwUsrPermissionsDB()
		{
			_standardFields		= new string[] {"VELE_VIE_ID","VELE_ELEMENTNUMBER","UPER_USR_ID","UPER_UPDALLOWED","UPER_INSALLOWED","UPER_EXEALLOWED","UPER_DELALLOWED"};
			_standardPks		= new string[] {"VELE_VIE_ID","VELE_ELEMENTNUMBER","UPER_USR_ID"};
			_standardTableName	= "VW_USR_PERMISSIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}
	}
}
