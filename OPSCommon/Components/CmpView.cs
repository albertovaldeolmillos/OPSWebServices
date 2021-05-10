using System;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// That component has all methods that determine if a view is in use, and which user has the view blocked.
	/// Uses the table VIEWS for its puporse (delegating in methods of CmpViewsDB).
	/// </summary>
	public class CmpView
	{
		/// <summary>
		/// Information about a 'block' of a view (user who has the view blocked, date of the block...).
		/// </summary>
		public class BlockInfo
		{
			private DateTime _date;
			private CmpUsuario _user;
			internal BlockInfo(DateTime dt, CmpUsuario user) 
			{
				_date = dt;
				_user = user;
			}
			public DateTime Date { get { return _date; } }
			public CmpUsuario User { get { return _user; } } 
		}

		public struct ViewInfo
		{
			public readonly string ViewUrl;
			public readonly int VieId;
			public ViewInfo (int id, string url) { VieId = id; ViewUrl = url; }
		}

		protected int _vieId;
		protected BlockInfo _blockInfo;

		// ArrayList containing the ALL views.
		// For each view contains URL and VIE_ID (struct ViewInfo)
		protected static ViewInfo [] _allViews;

		/// <summary>
		/// Loads and stores the ArrayList with ALL views
		/// </summary>
		static CmpView()
		{
			System.Data.DataTable dt =  new CmpViewsDB().GetData (new string[] {"VIE_ID","VIE_URL"});
			if (dt.Rows.Count > 0) _allViews = new ViewInfo[dt.Rows.Count];
			for(int i=0; i< dt.Rows.Count; i++)
			{
				System.Data.DataRow dr = dt.Rows[i];
				_allViews[i] = new ViewInfo (Convert.ToInt32(dr["VIE_ID"]), dr["VIE_URL"].ToString());
			}
		}

		/// <summary>
		/// Gets the VIE_ID of a view, based on the URL
		/// </summary>
		/// <param name="sUrl">Url of the view (can contain extra GET parameters that are not checked)</param>
		/// <returns>VIE_ID of the view if found or -1</returns>
		public static int IdOfView (string sUrl)
		{
			if (_allViews == null) return -1;
			for (int i=0; i<_allViews.Length;i++)
			{
				if (sUrl.IndexOf (_allViews[i].ViewUrl) != -1 ) 
				{
					return _allViews[i].VieId;
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets the VIE_URL of a view.
		/// </summary>
		/// <param name="viewId">The ID of the view.</param>
		/// <returns>A string with the URL.</returns>
		public static string UrlOfView(int viewId)
		{
			if (_allViews == null) return "";
			for (int i=0; i<_allViews.Length; i++)
			{
				if (_allViews[i].VieId == viewId) 
				{
					return _allViews[i].ViewUrl;
				}
			}
			return "";
		}

		public CmpView(int vieId)
		{
			_vieId = vieId;
			GetBlockInfo();
		}

		/// <summary>
		/// Gets information about the block of the view.
		/// </summary>
		public BlockInfo Block { get { return _blockInfo; } }

		// Loads if the view is blocked.
		private void GetBlockInfo()
		{
			// Gets block info about a view.
			CmpViewsDB vdb = new CmpViewsDB();
			System.Data.DataTable dt =  vdb.GetData (new string[] {"VIE_INUSE", "VIE_USR_ID", "VIE_INUSEDATE"}, 
				"VIE_ID = @VIEWS.VIE_ID@",null, new object[] {_vieId});
			if (dt.Rows.Count != 1) 
			{
				_blockInfo = null;				// View not found or more than one view (impossible)
			}
			else 
			{
				System.Data.DataRow dr = dt.Rows[0];
				// DXA DUMP 1202. The VIE_INUSE Field is now a 0-default value (not null
				//if (dr["VIE_INUSE"] != DBNull.Value) 
				if (dr["VIE_INUSE"].ToString() != Convert.ToString(0)) 
				{
					DateTime dateTime = DateTime.Now;
					int usrid = 1;


					if (dr["VIE_INUSEDATE"] != DBNull.Value)
					{
						dateTime = Convert.ToDateTime (dr["VIE_INUSEDATE"]);
					}

					if (dr["VIE_USR_ID"] != DBNull.Value)
					{
						usrid = Convert.ToInt32(dr["VIE_USR_ID"]);
					}

					if (dr["VIE_INUSEDATE"] == DBNull.Value && dr["VIE_USR_ID"] != DBNull.Value)
					{
						_blockInfo = null;
					}
					else
					{
						_blockInfo = new BlockInfo(dateTime, new CmpUsuario (usrid));
					}
				}
				else _blockInfo = null;
			}
		}

		/// <summary>
		/// Blocks the view by the user.
		/// If view is already blocked the property Block can be used to get info about the user which has blocked the view
		/// </summary>
		/// <param name="userId">Id of the user who wants to block the view</param>
		/// <returns>false if the view is already blocked by another user.</returns>
		public bool  BlockView (int userId)
		{
			// Fail if view IS blocked AND is blocked by another user
			if (_blockInfo != null && _blockInfo.User.Id != userId) return false;
			// Otherwise the view is not blocked (or is blocked by us) so we can block the view
			new CmpViewsDB().BlockView (_vieId, userId, true);
			// Updates the view info
			GetBlockInfo();
			return true;
		}

		/// <summary>
		/// Releases ALL views blocked by current user EXCEPT the current view.
		/// Do to the event life-cycle of an ASP.NET page the algorithm used to block views is as follows:
		///		At each view::Page_Load:
		///			1. Check if current view is available and block it if it is.
		///			2. Release the rest of views blocked by us (because we can come from ANOTHER view).
		///				That point is important because when we go to a page xxx.aspx from yyy.aspx,
		///				Unload event of yyy.aspx is NOT fired (because Unload event is ONLY fired on a postback!).
		/// </summary>
		/// <param name="userId">Current user</param>
		public void ReleaseRestOfViews (int userId)
		{
			new CmpViewsDB().ReleaseRestOfViews ( _vieId, userId);
		}

		/// <summary>
		/// Releases the view by the user.
		/// If view is already blocked the property Block can be used to get info about the user which has blocked the view
		/// </summary>
		/// <param name="userId">Id of the user who wants to block the view</param>
		/// <returns>false if the view is already blocked by another user.</returns>
		public bool ReleaseView (int userId)
		{
			// Fail if view IS blocked AND is blocked by another user
			if (_blockInfo != null && _blockInfo.User.Id != userId) return false;

			// If the view is not blocked (_blockinfo is null) we have to do nothing and if is blocked by us we can release it.
			if (_blockInfo != null)
			{
				new CmpViewsDB().BlockView(_vieId, userId, false);
				// Updates the block info
				GetBlockInfo();
			}
			return true;
		}
	}
}