using System;
using System.Data;
using System.Text;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS_CHILDS.
	/// </summary>
	public class CmpGroupsChildsDB : CmpGenericBase
	{
		public CmpGroupsChildsDB()
		{
			_standardFields		= new string[] {"CGRP_ID", "CGRP_TYPE", "CGRP_CHILD", "CGRP_ORDER", "CGRP_UNIQUE_ID"};
			_standardPks		= new string[] {"CGRP_UNIQUE_ID"};
			_standardTableName	= "GROUPS_CHILDS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[] {"CGRP_VALID", "CGRP_DELETED"};
		}

//		private static System.Threading.Mutex m = new System.Threading.Mutex();
//
//		public CmpGroupsChildsDB() {}
//
//		#region Implementation of CmpDataSourceAdapter 
//		
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			string[] pk = new string[] {"CGRP_ID,CGRP_TYPE"};
//			if (fields == null) 
//			{
//				fields = new string[] {"CGRP_ID","CGRP_TYPE","CGRP_CHILD","CGRP_ORDER"};
//			}
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby, values,"GROUPS_CHILDS","GROUPS_CHILDS", pk);
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			if (fields == null) 
//			{
//				fields = new string[] {"CGRP_ID","CGRP_TYPE","CGRP_CHILD","CGRP_ORDER"};
//			}
//			string[] pk = new string[] {"CGRP_ID,CGRP_TYPE"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(),orderByField,orderByAsc,where,rowstart,rowend,"GROUPS_CHILDS","GROUPS_CHILDS",pk);
//		}
//
//		public override void SaveData (DataTable dt)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO GROUPS_CHILDS (CGRP_ID, CGRP_TYPE, CGRP_CHILD, CGRP_ORDER, LAN_DEFAULT) VALUES (@GROUPS_CHILDS.CGRP_ID@, @GROUPS_CHILDS.CGRP_TYPE@, @GROUPS_CHILDS.CGRP_CHILD@, @GROUPS_CHILDS.CGRP_ORDER@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE GROUPS_CHILDS SET CGRP_TYPE = @GROUPS_CHILDS.CGRP_TYPE@, CGRP_CHILD = @GROUPS_CHILDS.CGRP_CHILD@, CGRP_ORDER = @GROUPS_CHILDS.CGRP_ORDER@ WHERE CGRP_ID = @GROUPS_CHILDS.CGRP_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM GROUPS_CHILDS WHERE CGRP_ID = @GROUPS_CHILDS.CGRP_ID@ AND CGRP_TYPE = @GROUPS_CHILDS.CGRP_TYPE@", false);
//			IDbConnection con = d.GetNewConnection();
//			con.Open();
//			da.InsertCommand.Connection = con;
//			da.UpdateCommand.Connection = con;
//			da.DeleteCommand.Connection = con;
//			d.UpdateDataSet(da,dt);
//			dt.AcceptChanges();
//			con.Close();
//
//		}
//		public override void GetForeignData(DataSet ds, string sTable) {}
//		public override string MainTable  {get { return "GROUPS_CHILDS"; }} 
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM GROUPS_CHILDS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
//
//		public override Int64 LastPKValue
//		{
//			get 
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(CGRP_ID),0) FROM GROUPS_CHILDS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
//		#endregion

		/// <summary>
		/// Returns the ID of the 1st Physical parent of the unit specified.
		/// The 1st physical parent is the physical parent with the lowest CGRP_ORDER
		/// </summary>
		/// <param name="uniid">UNI_ID of the unit</param>
		/// <returns>GRP_ID of the parent sarched or -1 if not register found</returns>
		public int GetFirstPhysicalParent (int uniid)
		{
			Database d = DatabaseFactory.GetDatabase();
			string ssql =	"select CGRP_ID FROM  GROUPS_CHILDS INNER JOIN GROUPS  ON CGRP_ID = GRP_ID  INNER JOIN GROUPS_DEF " + 
				"ON GRP_DGRP_ID = DGRP_ID WHERE CGRP_CHILD = @GROUPS_CHILDS.CGRP_CHILD@ AND DGRP_PHYORDER IS NOT NULL ORDER BY CGRP_ORDER ASC";
			IDataReader dr = null;
			try 
			{
				dr = d.ExecQuery (ssql, uniid);
				return dr.Read() ? Convert.ToInt32(dr["CGRP_ID"]) : -1;
			}
			finally 
			{
				if (dr!=null) dr.Close();
			}

		}

		/// <summary>
		/// Retrieves the parents of the UNIT or GROUP passed
		/// </summary>
		/// <param name="grpid">Id of the Unit OR GROUP passed</param>
		/// <param name="isUnit">If true means that grpid is a UNIT (otherwise grpid is a GROUP)</param>
		/// <param name="dgrpid">At the exit of the function contains the group_def of the parent (or -1 if not aplicable)</param>
		/// <param name="searchOnlyPhyGroups">If true only groups with DGRP_PHYORDER NOT NULL will be considered.</param>
		/// <returns>DataTable with all data requested. Note that if searchOnlyPhyGroups is FALSE the column DGRP_PHYORDER will contain -1 for non physical items</returns>
		public DataTable GetParentsGroup (int grpid, bool isUnit, bool searchOnlyPhyGroups)
		{
			Database d = DatabaseFactory.GetDatabase();
			// Get the physical parent of the element
			string ssql =	"select CGRP_ID, CGRP_TYPE, CGRP_ORDER, DGRP_ID, DGRP_PHYORDER  FROM GROUPS_CHILDS " + 
							"INNER JOIN GROUPS ON CGRP_ID = GRP_ID INNER JOIN GROUPS_DEF ON GRP_DGRP_ID = DGRP_ID " + 
							"WHERE CGRP_CHILD = @GROUPS_CHILDS.CGRP_CHILD@";
			if (searchOnlyPhyGroups) ssql = ssql + "  AND DGRP_PHYORDER IS NOT NULL";
			IDataReader dr = d.ExecQuery (ssql, grpid);
			DataTable dt = new DataTable("Parents");
			dt.Columns.Add ("CGRP_ID",typeof(int));
			dt.Columns.Add ("DGRP_ID",typeof(int));
			dt.Columns.Add ("DGRP_PHYORDER", typeof (int));
			while  (dr.Read())
			{
				// Have one or more physical parent. We must check that the CGRP_TYPE is "U" when isUnit is true
				// That is because the IDs of UNITS and GROUPS can be the same, so if we are the GROUP X
				// and find one parent Y, we must check that the CGRP_TYPE is not "U" (because X is a GROUP
				// not a Unit!
				bool bAddRegister = false;
				string stype = dr["CGRP_TYPE"].ToString();
				if (isUnit)
				{
					if (stype.Equals("U")) bAddRegister = true;
				}
				else
				{
					if (!stype.Equals("U")) bAddRegister = true;
				}

				// Adds the current register to the DataTable
				if (bAddRegister)
				{
					DataRow row = dt.NewRow ();
					row["CGRP_ID"] = dr["CGRP_ID"];
					row["DGRP_ID"] = dr["DGRP_ID"];
					row["DGRP_PHYORDER"] = dr["DGRP_PHYORDER"] != DBNull.Value ? dr["DGRP_PHYORDER"] : -1;
					dt.Rows.Add (row);
				}				
			}
			dr.Close();
			dt.AcceptChanges();
			return dt;
		}

		/// <summary>
		/// Checks if uniid is a unit or not (if is not a unit is a group).
		/// </summary>
		/// <param name="uniid">ID to check</param>
		/// <returns>true if ID is a UNIT (if is not a group is a GROUP)</returns>
		public bool IsUnit (int uniid)
		{
			Database d = DatabaseFactory.GetDatabase();
			string ssql = "select UNI_ID from UNITS where UNI_ID = @UNITS.UNI_ID@";
			IDataReader dr = d.ExecQuery(ssql, uniid);
			try 
			{
				return dr.Read();
			}
			finally 
			{
				dr.Close();
				dr.Dispose();
			}
		}

		/// <summary>
		/// Checks if grpid is a group or not (if is not a group is a unit).
		/// </summary>
		/// <param name="grpid">ID to check</param>
		/// <returns>true if ID is a GROUP (if is not a group is a UNIT)</returns>
		public bool IsGroup (int grpid)
		{
			Database d = DatabaseFactory.GetDatabase();
			string ssql = "select GRP_ID from GROUPS where GRP_ID = @GROUPS.GRP_ID@";
			IDataReader dr = d.ExecQuery(ssql, grpid);
			
			try 
			{
				return dr.Read();
			}
			finally 
			{
				dr.Close();
				dr.Dispose();
			}
		}
	}
}
