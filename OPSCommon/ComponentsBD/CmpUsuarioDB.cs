using System;
using System.Data;

using OPS.Components.Data;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for USERS.
	/// </summary>
	public class CmpUsuarioDB : CmpGenericBase
	{
		public CmpUsuarioDB()
		{
			_standardFields		= new string[] {"USR_ID", "USR_NAME", "USR_SURNAME1", "USR_SURNAME2", "USR_ROL_ID", "USR_LAN_ID", "USR_CUS_ID", "USR_LOGIN", "USR_PASSWORD", "USR_STATUS"};
			_standardPks		= new string[] {"USR_ID"};
			_standardTableName	= "USERS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

		// ****************************** Custom Functions ************************************
		 
		/// <summary>
		/// Validates if a login-password record exists on the database
		/// </summary>
		/// <param name="login">login to validated</param>
		/// <param name="pwd">password to validate (encrypted)</param>
		/// <returns>DataSet with the info of the user</returns>
		public DataSet ValidateUser (string login, string pwd) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			return d.FillDataSet ("USR_ID, USR_NAME, USR_SURNAME1, USR_SURNAME2, USR_ROL_ID, USR_LOGIN, USR_PASSWORD, USR_LAN_ID, USR_STATUS, USR_CUS_ID","USERS",null, "USR_LOGIN = @USERS.USR_LOGIN@ AND USR_PASSWORD = @USERS.USR_PASSWORD@ AND USR_STATUS = @USERS.USR_STATUS@", "USERS",-1,login, pwd, 1);
		}

		public DataSet GetUserData (string login) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			return d.FillDataSet ("USR_ID, USR_NAME, USR_SURNAME1, USR_SURNAME2, USR_ROL_ID, USR_LOGIN, USR_PASSWORD, USR_LAN_ID, USR_STATUS, USR_CUS_ID","USERS",null, "USR_LOGIN = @USERS.USR_LOGIN@ AND USR_STATUS = @USERS.USR_STATUS@", "USERS",-1,login, 1);
		}

		/// <summary>
		/// Returns the role of a user
		/// </summary>
		/// <param name="usrid">Id of the user</param>
		/// <returns>Id of the Role of the user</returns>
		public int GetUserRole (int usrid)
		{
			Database d = DatabaseFactory.GetDatabase();
			return Convert.ToInt32(d.ExecuteScalar("SELECT USR_ROL_ID FROM USERS WHERE USR_ID = @USERS.USR_ID@", usrid));
		}

		/// <summary>
		/// Returns a list of users (suitable for read-only lists, such as combos), optionally of a certain role.
		/// </summary>
		/// <param name="roleId">Role to filter (negative number means all).</param>
		/// <returns>A list of users (id, full name), ordered by full name.</returns>
		public DataSet GetUserList(int roleId) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			if (roleId >= 0) 
			{
				return d.FillDataSet ("USR_ID, USR_FULLNAME","VW_USERS_LIST", null, "USR_ROL_ID = @VW_USERS_LIST.USR_ROL_ID", "VW_USERS_LIST",-1, roleId);
			}		
			else
				return d.FillDataSet ("USR_ID, USR_FULLNAME","VW_USERS_LIST", null, null, "VW_USERS_LIST", -1);
		}

//		// ****************************** ICmpDataSource implementation ************************
//		public override DataTable GetData (string[] fields, string where, string orderby,object[] values) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"USR_ID", "USR_NAME", "USR_SURNAME1", "USR_SURNAME2", "USR_ROL_ID", "USR_LAN_ID", "USR_CUS_ID", "USR_LOGIN", "USR_PASSWORD", "USR_STATUS"};
//			}
//			string[] pk = new string[] {"USR_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetData (sb.ToString(),where, orderby,values,"USERS","USERS",pk);
//		}
//
//		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			if (fields == null) 
//			{
//				fields = new string[] {"USR_ID", "USR_NAME", "USR_SURNAME1", "USR_SURNAME2", "USR_ROL_ID", "USR_LAN_ID", "USR_CUS_ID", "USR_LOGIN", "USR_PASSWORD", "USR_STATUS"};
//			}
//			string[] pk = new string[] {"USR_ID"};
//			StringBuilder sb = base.ProcessFields(fields,pk);
//			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,"USERS","USERS",pk);
//		}
//
//		public override void SaveData (DataTable dt) 
//		{
//			Database d = DatabaseFactory.GetDatabase ();
//			IDbDataAdapter da = d.GetDataAdapter ();
//			da.InsertCommand = d.PrepareCommand ("INSERT INTO USERS (USR_ID, USR_NAME, USR_SURNAME1, USR_SURNAME2, USR_ROL_ID, USR_LOGIN, USR_PASSWORD, USR_LAN_ID, USR_STATUS, USR_CUS_ID) VALUES (@USERS.USR_ID@, @USERS.USR_NAME@, @USERS.USR_SURNAME1@, @USERS.USR_SURNAME2@ ,@USERS.USR_ROL_ID@, @USERS.USR_LOGIN@, @USERS.USR_PASSWORD@, @USERS.USR_LAN_ID@, @USERS.USR_STATUS@, @USERS.USR_CUS_ID@)",false);
//			da.UpdateCommand = d.PrepareCommand ("UPDATE USERS SET USR_NAME = @USERS.USR_NAME@, USR_SURNAME1 = @USERS.USR_SURNAME1@, USR_SURNAME2 = @USERS.USR_SURNAME2@, USR_ROL_ID = @USERS.USR_ROL_ID@, USR_LOGIN = @USERS.USR_LOGIN@, USR_PASSWORD = @USERS.USR_PASSWORD@, USR_LAN_ID = @USERS.USR_LAN_ID@, USR_STATUS = @USERS.USR_STATUS@, USR_CUS_ID = @USERS.USR_CUS_ID@ WHERE USR_ID = @USERS.USR_ID@", false);
//			da.DeleteCommand = d.PrepareCommand ("DELETE FROM USERS WHERE USR_ID = @USERS.USR_ID@", false);
//			IDbConnection con = d.GetNewConnection();
//			con.Open();
//			da.InsertCommand.Connection = con;
//			da.UpdateCommand.Connection = con;
//			da.DeleteCommand.Connection = con;
//			d.UpdateDataSet(da,dt);
//			con.Close();
//		}
//
//		public override long GetCount(string where, params object[] values)
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			string sql = "SELECT COUNT(*) FROM USERS";
//			if (where != null) 
//			{
//				sql = sql + " WHERE " + where;
//			}
//			return Convert.ToInt64(d.ExecuteScalar(sql, values));
//		}
////		public Int64 GetMaximumPk ()
////		{
////			Database d = DatabaseFactory.GetDatabase();
////			return Convert.ToInt64(d.ExecuteScalar("select MAX (USR_ID) from USERS"));
////		}
//
//		public override void GetForeignData(DataSet ds, string sTable)
//		{
//			// Get the table of customers
//			DataTable dtCustomers = new CmpCustomersDB().GetData();
//			ds.Tables.Add (dtCustomers);
//			// Get the (full) table of roles
//			DataTable dtRoles = new CmpRolesDB().GetData();
//			ds.Tables.Add (dtRoles);
//			// Get the (full) table of Languages
//			DataTable dtLanguages = new CmpLanguagesBD().GetData();
//			ds.Tables.Add (dtLanguages);
//
//			// Get the (full) table of Languages
//			DataTable dtCodes = new CmpCodesDB().GetYesNoData();
//			ds.Tables.Add(dtCodes);
//			// Ok. Now add the DataRelations, between sTable and the two new tables.
//			//	1. DataRelation between the table Users (sTable) and Roles
//			DataTable parent = ds.Tables[sTable];
//			ds.Relations.Add ((dtLanguages.PrimaryKey)[0],parent.Columns["USR_LAN_ID"]);
//			ds.Relations.Add ((dtRoles.PrimaryKey)[0], parent.Columns["USR_ROL_ID"]);
//			ds.Relations.Add ((dtCustomers.PrimaryKey)[0], parent.Columns["USR_CUS_ID"]);
//			ds.Relations.Add ((dtCodes.PrimaryKey)[0], parent.Columns["USR_STATUS"]);
//		}
//
//		public override string MainTable
//		{
//			get {return "USERS";}
//		}
//		public override Int64 LastPKValue 
//		{
//			get
//			{
//				Database d = DatabaseFactory.GetDatabase();
//				m.WaitOne();
//				long l = Convert.ToInt64(d.ExecuteScalar("SELECT NVL(MAX(USR_ID),0) FROM USERS"));
//				m.ReleaseMutex();
//				return l;
//			}
//		}
	}
}
