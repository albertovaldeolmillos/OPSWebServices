using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpSecurityDB.
	/// </summary>
	public class CmpSecurityDB
	{
		public CmpSecurityDB() {}

		/// <summary>
		/// Get all views associated to a role (RACC_VIE_ID and VIE_LIT_ID)
		/// </summary>
		/// <param name="rid">Id of the role</param>
		/// <returns>A DataTable with all views associated to specified role</returns>
		public DataTable GetViewsByRole (int rid)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable ("SELECT ROL_ACCESS.RACC_ROL_ID, ROL_ACCESS.RACC_VIE_ID, ROL_ACCESS.RACC_ALLOWED, VIEWS.VIE_LIT_ID from ROL_ACCESS INNER JOIN VIEWS ON ROL_ACCESS.RACC_VIE_ID = VIEWS.VIE_ID WHERE ROL_ACCESS.RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@ AND ROL_ACCESS.RACC_ALLOWED = @ROL_ACCESS.RACC_ALLOWED@","ROL_ACCESS",rid, 1);
			return dt;
		}

		/// <summary>
		/// Get all views associated to a user (only looks at USR_ACCESS table) (UACC_VIE_ID and VIE_LIT_ID)
		/// </summary>
		/// <param name="uid">Id of the user</param>
		/// <param name="allow">If true will get views allowed, if false will get views denied</param>
		/// <returns>A DataTable with all views associated to specified user (only looks at USR_ACCESS table)</returns>
		public DataTable GetViewsByUser(int uid, bool allow)
		{	
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable ("SELECT USR_ACCESS.UACC_USR_ID, USR_ACCESS.UACC_VIE_ID, USR_ACCESS.UACC_ALLOWED, VIEWS.VIE_LIT_ID from USR_ACCESS INNER JOIN VIEWS ON USR_ACCESS.UACC_VIE_ID = VIEWS.VIE_ID WHERE USR_ACCESS.UACC_USR_ID = @USR_ACCESS.UACC_USR_ID@ AND USR_ACCESS.UACC_ALLOWED = @USR_ACCESS.UACC_ALLOWED@","USR_ACCESS",uid, allow?1:0);
			return dt;
		}

		/// <summary>
		/// Get all roles, and for each role get all views associated
		/// </summary>
		/// <returns>A DataTable containing all roles and views associated to each role</returns>
		public DataTable GetViewsAssignedToRoles()
		{
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable("SELECT VIE_LIT_ID, VIE_ID, RACC_ROL_ID, VIE_ID, FROM VW_ROL_MODULES_ACCESS WHERE RACC_ALLOWED = 1 ORDER BY VW_ROL_MODULES_ACCESS.RACC_ROL_ID, VW_ROL_MODULES_ACCESS.VIE_ID", "ViewsAllowed");
			return dt;
		}

		/// <summary>
		/// Get all views
		/// </summary>
		/// <returns>A DataTable with all views (only VIE_ID and VIE_LIT_ID) </returns>
		public DataTable GetAllViews()
		{
			return new CmpViewsDB().GetData (new string[] {"VIE_ID","VIE_LIT_ID"});
		}

		/// <summary>
		///  Returns a DataTable with the following information
		///		MOD_DESCSHORT, MOD_LIT_ID, VIE_LIT_ID and VIE_URL AND MOD_ORDER TO ORDER MENUS
		///		That is: the relation between modules and views filtered by rolid
		/// </summary>
		/// <param name="rolid">Id of a role to filter on</param>
		/// <returns></returns>
		public DataTable GetMenuByRole (int rolid)
		{
			Database d = DatabaseFactory.GetDatabase();
			// We don't use mod image to build menu
			//DataTable dt = d.FillDataTable ("MOD_DESCSHORT, MOD_LIT_ID, VIE_ID, VIE_LIT_ID, VIE_URL, VIE_IMAGE, MOD_IMAGE,MOD_ORDER,VMOD_ORDER", "VW_ROL_MODULES_ACCESS","MOD_ORDER", "RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@ AND RACC_ALLOWED = @ROL_ACCESS.RACC_ALLOWED@","MenuByRole",-1,rolid,1);
			DataTable dt = d.FillDataTable ("MOD_DESCSHORT, MOD_LIT_ID, VIE_ID, VIE_LIT_ID, VIE_URL, VIE_IMAGE, MOD_ORDER,VMOD_ORDER", "VW_ROL_MODULES_ACCESS","MOD_ORDER", "RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@ AND RACC_ALLOWED = @ROL_ACCESS.RACC_ALLOWED@","MenuByRole",-1,rolid,1);
			DataColumn[] pk = new DataColumn [1];
			pk[0] = dt.Columns["VIE_LIT_ID"];
			dt.PrimaryKey = pk;						
			return dt;
		}

		/// <summary>
		///  Returns a DataTable with the following information
		///		MOD_DESCSHORT, MOD_LIT_ID, VIE_LIT_ID and VIE_URL AND MOD_ORDER TO ORDER MENUS
		///		That is: the relation between modules and views filtered by rolid and userid access
		/// </summary>
		/// <param name="rolid">Id of a role to filter on</param>
		/// <param name="userid">Id of a user to filter on</param>
		/// <returns></returns>
		public DataTable GetMenuByRoleUser (int rolid, int userid)
		{
			Database d = DatabaseFactory.GetDatabase();
			// We don't use mod image to build menu
			//DataTable dt = d.FillDataTable ("MOD_DESCSHORT, MOD_LIT_ID, VIE_ID, VIE_LIT_ID, VIE_URL, VIE_IMAGE, MOD_ORDER,VMOD_ORDER", "VW_ROL_MODULES_ACCESS","MOD_ORDER", "RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@ AND RACC_ALLOWED = @ROL_ACCESS.RACC_ALLOWED@","MenuByRole",-1,rolid,1);
			string sSql = "SELECT MOD_DESCSHORT, MOD_LIT_ID, VIE_ID, VIE_LIT_ID, VIE_URL, VIE_IMAGE, MOD_ORDER,VMOD_ORDER FROM";
			sSql += " MODULES INNER JOIN VIEWS_MODULES ON MODULES.MOD_ID = VIEWS_MODULES.VMOD_MOD_ID";
			sSql += " INNER JOIN VIEWS ON VIEWS.VIE_ID = VIEWS_MODULES.VMOD_VIE_ID";
			sSql += " INNER JOIN USR_ACCESS ON USR_ACCESS.UACC_VIE_ID = VIEWS.VIE_ID";
			sSql += " WHERE UACC_USR_ID = @USR_ACCESS.UACC_USR_ID@ AND UACC_ALLOWED = 1";
			sSql += " UNION";
			sSql += " SELECT MOD_DESCSHORT, MOD_LIT_ID, VIE_ID, VIE_LIT_ID, VIE_URL, VIE_IMAGE, MOD_ORDER,VMOD_ORDER FROM";
			sSql += " MODULES INNER JOIN VIEWS_MODULES ON MODULES.MOD_ID = VIEWS_MODULES.VMOD_MOD_ID";
			sSql += " INNER JOIN VIEWS ON VIEWS.VIE_ID = VIEWS_MODULES.VMOD_VIE_ID";
			sSql += " INNER JOIN ROL_ACCESS ON ROL_ACCESS.RACC_VIE_ID = VIEWS.VIE_ID";
			sSql += " WHERE RACC_ROL_ID = @ROL_ACCESS.RACC_ROL_ID@  AND RACC_ALLOWED = 1 AND vie_id NOT IN (SELECT UACC_VIE_ID FROM USR_ACCESS WHERE UACC_USR_ID =@USR_ACCESS.UACC_USR_ID@ and UACC_ALLOWED = 0)";
			sSql += " ORDER BY MOD_ORDER, VMOD_ORDER";
			DataTable dt = d.FillDataTable(sSql,null, new object[]{userid,rolid,userid});
			DataColumn[] pk = new DataColumn [1];
			pk[0] = dt.Columns["VIE_LIT_ID"];
			dt.PrimaryKey = pk;						
			return dt;
		}
		/// <summary>
		/// Returns the permissions of all the subviews of a view, based on a role
		/// </summary>
		/// <param name="rol">Id of the role to filter on</param>
		/// <param name="view">Id of the view to filter on</param>
		/// <returns>DataTable with info requested</returns>
		public DataTable GetPermissionsByRole(int rol, int view)
		{
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable ("SELECT RPER_ROL_ID, RPER_VELE_VIE_ID, RPER_VELE_ELEMENTNUMBER, RPER_INSALLOWED, RPER_UPDALLOWED, RPER_DELALLOWED, RPER_EXEALLOWED FROM ROL_PERMISSIONS INNER JOIN VIEWS_ELEMENTS ON ROL_PERMISSIONS.RPER_VELE_VIE_ID = VIEWS_ELEMENTS.VELE_VIE_ID AND ROL_PERMISSIONS.RPER_VELE_ELEMENTNUMBER = VIEWS_ELEMENTS.VELE_ELEMENTNUMBER WHERE (RPER_ROL_ID = @ROL_PERMISSIONS.RPER_ROL_ID@) AND RPER_VELE_VIE_ID = @ROL_PERMISSIONS.RPER_VELE_VIE_ID@ ORDER BY RPER_VELE_ELEMENTNUMBER","ROL_PERMISSIONS",rol, view);
			return dt;
		}


		/// <summary>
		/// Returns the permissions of all subviews for a user
		/// NOTE: That method only retrieves the value of the USR_PERMISSIONS table, and DOES NOT
		/// any matching of that values with the values of the ROL_PERMISSIONS table.
		/// The datatable includes the VIEW_ELEMENTS.VELE_DESCSHORT field.
		/// </summary>
		/// <param name="user">Id of the user</param>
		/// <param name="view">Id of the view</param>
		/// <returns>DataTable with info requested</returns>
		public DataTable GetPermissionsByUser (int user, int view)
		{
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable ("SELECT UPER_USR_ID,UPER_VELE_VIE_ID, UPER_VELE_ELEMENTNUMBER, VELE_DESCSHORT, UPER_INSALLOWED, UPER_UPDALLOWED, UPER_DELALLOWED, UPER_EXEALLOWED FROM USR_PERMISSIONS INNER JOIN VIEWS_ELEMENTS ON USR_PERMISSIONS.UPER_VELE_VIE_ID = VIEWS_ELEMENTS.VELE_VIE_ID AND USR_PERMISSIONS.UPER_VELE_ELEMENTNUMBER = VIEWS_ELEMENTS.VELE_ELEMENTNUMBER WHERE UPER_USR_ID = @USR_PERMISSIONS.UPER_USR_ID@ AND UPER_VELE_VIE_ID = @USR_PERMISSIONS.UPER_VELE_VIE_ID@ ORDER BY UPER_VELE_ELEMENTNUMBER","USR_PERMISSIONS",user, view);
			dt.TableName = "USR_PERMISSIONS";
			return dt;
		}

		/// <summary>
		/// Saves subviews' permissions for a role (ROL_PERMISSIONS table)
		/// </summary>
		/// <param name="dt">DataTable containint the users to save</param>
		public void SaveViewsAssignedToRoles (DataTable dt) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			IDbDataAdapter da = d.GetDataAdapter ();
			IDbCommand cmd = d.PrepareCommand ("UPDATE ROL_PERMISSIONS SET RPER_INSALLOWED = @ROL_PERMISSIONS.RPER_INSALLOWED@, RPER_UPDALLOWED = @ROL_PERMISSIONS.RPER_UPDALLOWED@, RPER_DELALLOWED= @ROL_PERMISSIONS.RPER_DELALLOWED@, RPER_EXEALLOWED=@ROL_PERMISSIONS.RPER_EXEALLOWED@ WHERE RPER_ROL_ID  = @ROL_PERMISSIONS.RPER_ROL_ID@ AND  RPER_VELE_VIE_ID = @ROL_PERMISSIONS.RPER_VELE_VIE_ID@ AND  RPER_VELE_ELEMENTNUMBER = @ROL_PERMISSIONS.RPER_VELE_ELEMENTNUMBER@", false);
			IDbConnection con = d.GetNewConnection();
			con.Open();
			cmd.Connection = con;
			da.UpdateCommand= cmd;
			d.UpdateDataSet(da,dt);
			con.Close();
			dt.AcceptChanges();
		}
		/// <summary>
		/// Updates the USR_PERMISSIONS table
		/// </summary>
		/// <param name="dt">DataTable to udpate</param>
		public void SaveViewsAssignedToUsers (DataTable dt)
		{
			Database d = DatabaseFactory.GetDatabase ();
			IDbDataAdapter da = d.GetDataAdapter ();
			da.UpdateCommand = d.PrepareCommand ("UPDATE USR_PERMISSIONS SET UPER_INSALLOWED = @USR_PERMISSIONS.UPER_INSALLOWED@, UPER_UPDALLOWED = @USR_PERMISSIONS.UPER_UPDALLOWED@, UPER_DELALLOWED= @USR_PERMISSIONS.UPER_DELALLOWED@, UPER_EXEALLOWED=@USR_PERMISSIONS.UPER_EXEALLOWED@ WHERE UPER_USR_ID  = @USR_PERMISSIONS.UPER_USR_ID@ AND UPER_VELE_VIE_ID = @USR_PERMISSIONS.UPER_VELE_VIE_ID@ AND  UPER_VELE_ELEMENTNUMBER = @USR_PERMISSIONS.UPER_VELE_ELEMENTNUMBER@", false);
			da.InsertCommand = d.PrepareCommand ("INSERT INTO USR_PERMISSIONS (UPER_USR_ID, UPER_VELE_VIE_ID, UPER_VELE_ELEMENTNUMBER, UPER_INSALLOWED,UPER_UPDALLOWED, UPER_DELALLOWED, UPER_EXEALLOWED) VALUES (@USR_PERMISSIONS.UPER_USR_ID@, @USR_PERMISSIONS.UPER_VELE_VIE_ID@, @USR_PERMISSIONS.UPER_VELE_ELEMENTNUMBER@, @USR_PERMISSIONS.UPER_INSALLOWED@,@USR_PERMISSIONS.UPER_UPDALLOWED@, @USR_PERMISSIONS.UPER_DELALLOWED@, @USR_PERMISSIONS.UPER_EXEALLOWED@)", false);
			da.DeleteCommand = d.PrepareCommand ("DELETE FROM USR_PERMISSIONS WHERE UPER_USR_ID = @USR_PERMISSIONS.UPER_USR_ID@  AND UPER_VELE_VIE_ID = @USR_PERMISSIONS.UPER_VELE_VIE_ID@ AND UPER_VELE_ELEMENTNUMBER = @USR_PERMISSIONS.UPER_VELE_ELEMENTNUMBER@",false);
			IDbConnection con = d.GetNewConnection();
			con.Open();
			da.UpdateCommand.Connection = con;
			da.InsertCommand.Connection = con;
			da.DeleteCommand.Connection = con;
			d.UpdateDataSet(da,dt);
			con.Close();
			dt.AcceptChanges();		
		}

	}
}
