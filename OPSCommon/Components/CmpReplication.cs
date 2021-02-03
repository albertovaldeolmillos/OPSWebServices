using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Handles all the replication logic.
	/// </summary>
	public class CmpReplication
	{
		public CmpReplication()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public void ProcessTableRequest(int unitId, string table, int version)
		{
			// TODO
		}

		public DataTable ReplicationVersionQuery(int replicationDefId)
		{
			// TODO: Falta el cas de OPERATIONS
			CmpReplicationsTablesDB rtdb = new CmpReplicationsTablesDB();
			string rawsql = "SELECT VER_TABLE, VER_VALUE FROM VERSIONS "
				+ "INNER JOIN REPLICATIONS_TABLES ON VERSIONS.VER_TABLE = REPLICATIONS_TABLES.TREP_TABLE "
				+ "WHERE TREP_DREP_ID = @REPLICATIONS_TABLES.TREP_DREP_ID@";
			DataTable dt = rtdb.GetData(rawsql, new object[] { replicationDefId });
			return dt;
		}
	}
}
