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
	/// Summary description for OPSSwitchBoard.
	/// </summary>
	public partial class OPSSwitchBoard : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			string stError="";
			string stRdo="";
			Response.Clear();
			Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.Cache.SetNoStore();

			Response.ContentType="text/xml";
			
			try
			{
				string strTelNumber = (string)Request.QueryString["TelNumber"];
				string strFechaOp = (string)Request.QueryString["Date"];
				bool bSaveOp = (((string)Request.QueryString["SaveOp"])=="1");
				string strGroup="";
				string strMaxTime="";


				try
				{							
					strGroup=(string)Request.QueryString["Group"];
					if (strGroup==null)
						strGroup="";
				}
				catch
				{
					strGroup="";			

				}

				try
				{							
					strMaxTime=(string)Request.QueryString["MaxTime"];
					if (strMaxTime==null)
						strMaxTime="-1";

				}
				catch
				{
					strMaxTime="-1";				
				}

				COPSSwitchBoardHelper.ManageCall( strTelNumber,strFechaOp,bSaveOp,strGroup,strMaxTime,  ref stRdo, ref stError);
				Response.Write(stRdo);

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
		}
		#endregion
	}
}
