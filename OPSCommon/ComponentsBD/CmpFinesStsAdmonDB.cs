using System;

namespace OPS.Components.Data
{
	/// <summary>
	///  Virtual-filter-compatible executant class for FINES_STSADMON_DEF.
	/// </summary>
	public class CmpFinesStsAdmonDB: CmpGenericBase
	{
		public CmpFinesStsAdmonDB()
		{
			_standardFields		= new string[] { "DSAFIN_ID", "DSAFIN_DESCSHORT", "DESAFIN_DESCLONG" };
			_standardPks		= new string[] {"DSAFIN_ID"};
			_standardTableName	= "FINES_STSADMON_DEF";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; 
			_standardRelationTables	= new string[0]; 
			_stValidDeleted			= new string[] {"DSAFIN_VALID", "DSAFIN_DELETED"};
		}
	}
}

