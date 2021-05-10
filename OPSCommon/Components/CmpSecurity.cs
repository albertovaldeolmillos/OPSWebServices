using System;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Collections;
using OPS.Components.Data;

namespace OPS.Components
{

	/// <summary>
	/// Represents a user-menu
	/// A user-menu is a two-level menu
	///			First level: Modules
	///			Second level: Views (inside each module)
	/// </summary>
	public class MenuInfo	
	{
		// Represents a module (a module is basically a literal id and an arraylist of ModuleItems).
		public class ModuleInfo
		{
			// Id of the Literal with the module name
			public int litid;
			// Name of the image of the module
			public string ModuleImage;
			protected ArrayList _items;
			/// <summary>
			/// Gets the module specified. Modules are indexed 0-based
			/// </summary>
			public ModuleItem this[int idx]
			{
				get { return (ModuleItem)_items[idx]; }
				set { _items[idx] = value; }
			}
			/// <summary>
			/// Gets the number of modules (1st level entries)
			/// </summary>
			public int Count
			{
				get { return _items.Count;}
			}

			/// <summary>
			/// Adds a new Module to the menu
			/// </summary>
			/// <param name="mi">ModuleItem object containing all info about the module</param>
			public void Add (ModuleItem mi)
			{
				_items.Add (mi);
			}
			/// <summary>
			/// Checks if the view is in the module
			/// </summary>
			/// <param name="viewId">ID of the view to check</param>
			/// <returns>true if view is in the module</returns>
			public bool HasView (int viewId)
			{
				foreach (ModuleItem mi in _items)
				{
					if (mi.VieId == viewId) return true;
				}
				return false;
			}

			/// <summary>
			/// Builds a new-empty Module (1st level entry).
			/// </summary>
			/// <param name="litid">Id of the literal of the module. Literal will be used to display title</param>
			internal ModuleInfo (int litid)
			{
				this.litid = litid;
				_items = new ArrayList();
			}
		}
		/// <summary>
		/// That class represents a item inside a module (in fact, a view)
		/// Views are 2nd level entry
		/// </summary>
		public class ModuleItem
		{
			// ID of the resource containing the name of the View
			public int ViewLitId;
			// URL of the view (will contain an extra GET parameter called vid, with the VIE_ID of the view)
			public string ViewUrl;
			// Name of the imgage for the view
			public string ViewImage;
			// VIE_ID of the view
			public int VieId;
			// MODULE_ORDER of the view
			public int ModOrderId;

			// ITEM_ORDER of the view
			public int ItemOrderId;
		}
		
		// A hashtable containing all modules (instances of ModuleInfo).
		protected Hashtable _modules;
		/// <summary>
		/// Builds a new empty view
		/// </summary>
		public MenuInfo() 
		{
			_modules = new Hashtable();
		}

		/// <summary>
		/// Gets the ModuleInfo object containing the info about the specified module
		/// </summary>
		public ModuleInfo this[int idx]
		{
			get { return (ModuleInfo)_modules[idx];}
		}


		public ICollection ModulesKeys 
		{
			get {return _modules.Keys;}
		}

		/// <summary>
		/// Gets the ModuleItem object with the info of the specified view (module and index inside the module)
		/// </summary>
		public ModuleItem this[int module, int idxView]
		{
			get 
			{
				return ((ModuleInfo)_modules[module])[idxView];
			}
		}

		/// <summary>
		/// Adds a ModuleItem (a view) inside a module
		/// </summary>
		/// <param name="module">Descshort of the module</param>
		/// <param name="modlitid">ID of the literal of the module</param>
		/// <param name="item">ModuleItem with info about the view</param>
		internal void AddItem (string module, string modimage,int modlitid, ModuleItem item)
		{
			ModuleInfo mod = null;
			if (!_modules.ContainsKey(item.ModOrderId))
			{
				mod = new ModuleInfo (modlitid);
				mod.ModuleImage = modimage;
				_modules.Add(item.ModOrderId, mod);
			}
			else 
			{
				mod = (ModuleInfo)_modules[item.ModOrderId];
			}
			mod.Add (item);
		}
		/// <summary>
		/// Checks if a specified view is on the menu.
		/// </summary>
		/// <param name="vieId">VIE_ID of the view</param>
		/// <returns>true if a specified view is on the menu.</returns>
		internal bool HasItem (int vieId)
		{
			ModuleInfo mi = null;
			foreach (DictionaryEntry de in _modules)
			{
				mi = (ModuleInfo)de.Value;
				if (mi.HasView (vieId)) return true;
			}
			return false;
		}
	}
	/// <summary>
	/// Mantains all security-related aspects (excluding internal aspects of authentication and
	/// authorization mantained by ASP.NET).
	/// </summary>
	public class CmpSecurity 
	{
		int _usrId;
		int _rolId;
		MenuInfo _usrMenu;


		public CmpSecurity (int usrid, int rolid) 
		{
			_usrId = usrid;
			_rolId = rolid;
			_usrMenu = null;
		}

		/// <summary>
		///  Gets the ID (only the ID) of the user (current logged user)
		/// </summary>
		public int UsrId { get { return _usrId;} }
		/// <summary>
		///  Gets the ID (only the ID) of the Role of current logged user
		/// </summary>
		public int RolId { get { return _rolId;} }
		/// <summary>
		/// Returns a structure containing the user's menu
		/// </summary>
		public MenuInfo UserMenu
		{
			get 
			{
				if (_usrMenu == null) BuildUserMenu();
				return _usrMenu;
			}
		}

		/// <summary>
		/// Checks if the view is allowed by the current user
		/// </summary>
		/// <param name="vid">ID of the view to check</param>
		/// <returns>true if the view is allowed by the current user</returns>
		public bool ViewAllowed (int vid)
		{
			return _usrMenu.HasItem(vid);
		}

		/// <summary>
		/// Returns if a view is available (and CAN be blocked) by the current user
		/// </summary>
		/// <param name="v">View to test</param>
		/// <returns>true if view is available to block</returns>
		public bool ViewAvailable (CmpView v)
		{
			if (v.Block == null) return true;			// View is free
			return v.Block.User.Id == _usrId;			// View is blocked by the current user, so we can release it
		}

		private void BuildUserMenu()
		{
			_usrMenu = new MenuInfo();
			DataTable t = new CmpSecurityDB().GetMenuByRoleUser (_rolId,_usrId);
			foreach (DataRow dr in t.Rows)
			{
				MenuInfo.ModuleItem item = new MenuInfo.ModuleItem();
				item.ViewLitId = Convert.ToInt32(dr["VIE_LIT_ID"]);
				item.ViewUrl = dr["VIE_URL"].ToString();
				item.VieId = Convert.ToInt32(dr["VIE_ID"]);
				item.ModOrderId = Convert.ToInt32(dr["MOD_ORDER"]);
				item.ItemOrderId =Convert.ToInt32(dr["VMOD_ORDER"]);
				item.ViewImage = dr["VIE_IMAGE"] != DBNull.Value ? dr["VIE_IMAGE"].ToString() : null;
				// We don't use mod image to build menu
				//string smodimg = dr["MOD_IMAGE"] != DBNull.Value ? dr["MOD_IMAGE"].ToString() : null;
				// Items don't have icon now.
				//_usrMenu.AddItem (dr["MOD_DESCSHORT"].ToString(), smodimg,Convert.ToInt32(dr["MOD_LIT_ID"]),item);
				_usrMenu.AddItem (dr["MOD_DESCSHORT"].ToString(), "",Convert.ToInt32(dr["MOD_LIT_ID"]),item);
			}
		}



		/// <summary>
		/// Cheks the permissions to the view, user permissions and role permissions, that cheks validate read, insert, write & execute
		/// </summary>
		/// <param name="vid">Id of The View</param>
		/// <param name="subviewid">Id of sub-view</param>
		/// <param name="UsrId">Id of the user</param>
		/// <param name="RolId">Id of the role</param>
		/// <returns>A byte codified as 0000and 1 or 0 depending permissions on insert, update, delete & execute</returns>
		public static byte SubviewAllowed(int vid, int subviewid, int UsrId, int RolId)
		{
			byte bRolPermissions = 0;
			byte bUsrPermissions = 0;
			string sWhereRol	= "";
			string sWhereUsr	= "";

			string[] sFieldsRol = new string [] {"RPER_UPDALLOWED","RPER_INSALLOWED","RPER_EXEALLOWED","RPER_DELALLOWED"};
			string[] sFieldsUsr = new string [] {"UPER_UPDALLOWED","UPER_INSALLOWED","UPER_EXEALLOWED","UPER_DELALLOWED"};

			sWhereRol	= "VELE_VIE_ID = '" + vid + "' AND VELE_ELEMENTNUMBER= '" + subviewid + "' AND RPER_ROL_ID = '" + RolId+ "'";
			sWhereUsr	= "VELE_VIE_ID = '" + vid + "' AND VELE_ELEMENTNUMBER= '" + subviewid + "' AND UPER_USR_ID = '" + UsrId+ "'";
			DataTable dtRolPermissions = new OPS.Components.Data.CmpVwRolPermissionsDB().GetData(sFieldsRol,sWhereRol,null,null);
			DataTable dtUserPermissions = new OPS.Components.Data.CmpVwUsrPermissionsDB().GetData(sFieldsUsr,sWhereUsr,null,null);

			if (dtRolPermissions.Rows.Count > 0)
			{
				int RolUpdallowed	= Int32.Parse(dtRolPermissions.Rows[0]["RPER_UPDALLOWED"].ToString());
				int RolInsallowed	= Int32.Parse(dtRolPermissions.Rows[0]["RPER_INSALLOWED"].ToString());
				int RolExeallowed	= Int32.Parse(dtRolPermissions.Rows[0]["RPER_EXEALLOWED"].ToString());
				int RolDelallowed	= Int32.Parse(dtRolPermissions.Rows[0]["RPER_DELALLOWED"].ToString());
				int inumberRol =  RolUpdallowed + RolInsallowed*2 + RolExeallowed*4 + RolDelallowed*8;
				bRolPermissions |= (byte)inumberRol;
			}

			if (dtUserPermissions.Rows.Count > 0)
			{
				int UsrUpdallowed	= Int32.Parse(dtUserPermissions.Rows[0]["UPER_UPDALLOWED"].ToString());
				int UsrInsallowed	= Int32.Parse(dtUserPermissions.Rows[0]["UPER_INSALLOWED"].ToString());
				int UsrExeallowed	= Int32.Parse(dtUserPermissions.Rows[0]["UPER_EXEALLOWED"].ToString());
				int UsrDelallowed	= Int32.Parse(dtUserPermissions.Rows[0]["UPER_DELALLOWED"].ToString());
				int inumberUsr =  UsrUpdallowed + UsrInsallowed*2 + UsrExeallowed*4 + UsrDelallowed*8;

				bUsrPermissions |= (byte)inumberUsr;
			}


			bUsrPermissions |= (byte)((byte)bUsrPermissions | (byte)bRolPermissions);

			return bUsrPermissions;
		}


		/// <summary>
		/// Returns a DataSet with 2 tables containing
		///		Table 1: All views and roles associated to them
		///		Table 2: All roles
		/// </summary>
		/// <returns></returns>
		public static DataSet ViewsAssignedToRoles()
		{
			CmpSecurityDB cmpSec = new CmpSecurityDB();
			DataSet ds = new DataSet();
			// Adds a table with relations between roles and views
			DataTable dt = cmpSec.GetViewsAssignedToRoles();
			ds.Tables.Add(dt);
			// Adds a table with all roles
			CmpRolesDB cmpRol = new CmpRolesDB();
			dt = cmpRol.GetData();
			ds.Tables.Add (dt);
			ds.Relations.Add ("RolViews", ds.Tables["ROLES"].Columns["ROL_ID"], ds.Tables["ViewsAllowed"].Columns["RACC_ROL_ID"]);
			return ds;
		}

		/// <summary>
		/// Gets all views allowed by a specified user
		/// Note: A view is allowed to a user if and only if:
		///		1.  USR_ACCESS.UACC_ALLOWED is 1 for the view and user
		///						OR
		///		2a. USR_ACCESS.UACC_ALLOWED is null for the view and user
		///						AND
		///		2b. ROL_ACCESS.RACC_ALLOWED is 1 for the view and user's role
		/// </summary>
		/// <param name="userid">Id of the user</param>
		/// <returns>DataTable with ONLY one column (VIE_ID) with all views allowed by the user</returns>
		public static DataTable GetViewsAllowedByUser (int userid)
		{
			CmpSecurityDB cdb = new CmpSecurityDB();
			CmpUsuarioDB udb = new CmpUsuarioDB();
			// STEP 1: Get the role of the user
			int roleid = udb.GetUserRole (userid);
			// STEP 2: Get all the views associated by the role
			DataTable dtViewsByRole = cdb.GetViewsByRole(roleid);
			// STEP 3: Get all views associated by the user
			DataTable dtViewsAllowedByUser = cdb.GetViewsByUser (userid, true);
			// STEP 4: Get all views denied by the user
			DataTable dtViewsDeniedByUser = cdb.GetViewsByUser(userid, false);
			// STEP 5: Join all the current info in a new DataTable and return it...
			DataTable dtRet = new DataTable("ViewsByUser");
			dtRet.Columns.Add (new DataColumn("VIE_ID",Type.GetType("System.Int32")));
			dtRet.Columns.Add (new DataColumn("VIE_LIT_ID",Type.GetType("System.Int32")));
			// 5.1 All views allowed by user are allowed
			foreach (DataRow dr in dtViewsAllowedByUser.Rows)
			{
				DataRow nrow = dtRet.NewRow();
				nrow["VIE_ID"] = dr["UACC_VIE_ID"];
				nrow["VIE_LIT_ID"]  = dr["VIE_LIT_ID"];
				dtRet.Rows.Add (nrow);
			}
			// 5.2 All views allowed by role AND not denied by user are also added
			foreach (DataRow dr in dtViewsByRole.Rows)
			{
				string vieid = dr["RACC_VIE_ID"].ToString();
				DataRow[] draSelected = dtViewsDeniedByUser.Select("UACC_VIE_ID = " + vieid);
				if (draSelected != null && draSelected.Length > 0)
				{
					// 5.2.1 The view was not denied by the user...
					DataRow[] draSelected2 = dtRet.Select ("VIE_ID = " + vieid);
					if (draSelected2 != null && draSelected2.Length	> 0)
					{
						// 5.2.2 ... and was not previously added in dtRet, so we can add it
						DataRow nrow = dtRet.NewRow();
						nrow["VIE_ID"] = dr["RACC_VIE_ID"];
						nrow["VIE_LIT_ID"] = dr["VIE_LIT_ID"];
						dtRet.Rows.Add (nrow);
					}
				}
			}
			dtRet.AcceptChanges();
			return dtRet;
		}

		/// <summary>
		/// Gets the PERMISIONS of a user, for all subviews of a view
		/// That method makes two things:
		///		1. Calls CmpSecurityDB::GetPermissionsByUser to get all registers in USR_PERMISSIONS
		///		   (for the specified user and view).
		///		2. Get all subviews of the view, and foreach subview that IS not in the USR_PERMISSIONS
		///		   table, adds it to the DataTable returned with null value
		///	The DataTable returned has ALWAYS, ALL the subviews of the specified view.
		/// </summary>
		/// <param name="user">ID of the User (USERS.USR_ID)</param>
		/// <param name="view">ID of the view (VIEWS.VIE_ID)</param>
		/// <returns>DataTable with permisions (exe, del, upd, ins)</returns>
		public static DataTable GetPermissionsByUser(int user, int view)
		{
			CmpSecurityDB csdb = new CmpSecurityDB();
			CmpViewsElementsDB cvedb  = new CmpViewsElementsDB();
			// Step 1: Get data of USR_PERMISSIONS
			DataTable dtUserPermissions = csdb.GetPermissionsByUser(user, view);
			// Step 2: Get all subviews of a view
			DataTable dtSubViews = cvedb.GetData (new string[] {"VELE_VIE_ID","VELE_ELEMENTNUMBER, VELE_DESCSHORT"},
					"VELE_VIE_ID = @VIEWS_ELEMENTS.VELE_VIE_ID@",null, new object[] {view});
			// Step 3: Merge the results...
			// Foreach subview search it in the table of permissioins and if does not appear
			// insert a new row with null ('not set' values).
			foreach (DataRow dr in dtSubViews.Rows)
			{
				DataRow[] selectResult = dtUserPermissions.Select ("UPER_VELE_ELEMENTNUMBER = " + dr["VELE_ELEMENTNUMBER"]);
				if (selectResult == null || selectResult.Length == 0)
				{
					DataRow nrow = dtUserPermissions.NewRow();
					nrow["UPER_USR_ID"] = user;
					nrow["VELE_DESCSHORT"] = dr["VELE_DESCSHORT"];
					nrow["UPER_VELE_VIE_ID"] = view;
					nrow["UPER_VELE_ELEMENTNUMBER"] = dr["VELE_ELEMENTNUMBER"];
					nrow["UPER_INSALLOWED"] = DBNull.Value;
					nrow["UPER_DELALLOWED"] = DBNull.Value;
					nrow["UPER_UPDALLOWED"] = DBNull.Value;
					nrow["UPER_EXEALLOWED"] = DBNull.Value;
					dtUserPermissions.Rows.Add (nrow);
				}
			}
			// don't accept changes because we want that rows to be with 'added' state
			// so it will be included in the table when user saves
			return dtUserPermissions;
		}

		/// <summary>
		/// Saves data about subviews assigned to a role (ROL_PERMISSIONS)
		/// </summary>
		/// <param name="dt">DataTable to update</param>
		public static void SaveViewsAssignedToRoles (DataTable dt)
		{
			new CmpSecurityDB().SaveViewsAssignedToRoles(dt);
		}

		/// <summary>
		/// aves data about subviews assigned to a user (USR_PERMISSIONS)
		/// </summary>
		/// <param name="dt">DataTable to udpate</param>
		public static void SaveViewsAssignedToUsers (DataTable dt)
		{
			// Do a preprocess of the table (a row with all NULLS can be deleted!).
			ArrayList ar = new ArrayList();
			foreach (DataRow dr in dt.Rows)
			{
				if (dr["UPER_INSALLOWED"] == DBNull.Value &&
					dr["UPER_DELALLOWED"] == DBNull.Value &&
					dr["UPER_UPDALLOWED"] == DBNull.Value &&
					dr["UPER_EXEALLOWED"] == DBNull.Value)
					ar.Add(dr);
			}
			for (int i=0; i<ar.Count;i++) { ((DataRow)ar[i]).Delete(); }
			new CmpSecurityDB().SaveViewsAssignedToUsers(dt);
		}

	}
}
