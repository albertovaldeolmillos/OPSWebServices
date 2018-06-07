using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace OPSWebServices
{
	/// <summary>
	/// Summary description for OPSSwitchBoardGetDateTime.
	/// </summary>
	public class OPSSwitchBoardGetDateTime : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			string stError="";
			string stRdo="";
			Response.Clear();
			Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.Cache.SetNoStore();

			Response.ContentType="text/html";
			
			try
			{
				string strDateTime="";

				COPSSwitchBoardHelper.GetDateTime(ref strDateTime);
				Response.Write(strDateTime);

			}
			catch
			{
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
