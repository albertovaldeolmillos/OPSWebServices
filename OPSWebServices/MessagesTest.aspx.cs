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
using OPSWebServices.OPSWebServices;
using System.Security;
using System.Security.Permissions;

namespace OPSWebServices
{

	/// <summary>
	/// Summary description for MessagesTest.
	/// </summary>
	public partial class MessagesTest : System.Web.UI.Page
	{
	
		public static bool CanCallUnmanagedCode
		{
			get
			{
				try
				{
					new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
				}
				catch(SecurityException)
				{
					return false;
				}

				return true;
			}
		}
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			
			


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

		protected void BttnCall_Click(object sender, System.EventArgs e)
		{

			if (!CanCallUnmanagedCode)
			{
				TxtBxQry.Text = "This page can NOT call unmanaged code.";
				return;
			}
			
			
		

			if( IsPostBack )
			{
				try
				{

					Messages wsMessages = new Messages();
				
					string strRdo = wsMessages.Message(TxtBxQry.Text.ToString());

					// Very simple checking
					if( strRdo == null )
						throw new Exception("NULL result");

					if( strRdo[0] != '<' )
						throw new Exception(strRdo);

					XmlQry.DocumentContent = TxtBxQry.Text.ToString();
					XmlQry.TransformSource = ".\\Style\\xmlverbatimwrapper.xsl";

					//XmlRdo.DocumentContent = "<m3 id=\"328\" dst=\"4\"><u>126</u><a>200</a><s>0</s><d>185037120305</d></m3>";
					XmlRdo.DocumentContent = strRdo;
					XmlRdo.TransformSource = ".\\Style\\xmlverbatimwrapper.xsl";

					//XmlRdo2.DocumentContent = "<m3 id=\"328\" dst=\"4\"><u>126</u><a>200</a><s>0</s><d>185037120305</d></m3>";
					XmlRdo2.DocumentContent = strRdo;
					XmlRdo2.TransformSource = ".\\Style\\tree-view.xsl";
				}
				catch(Exception excp)
				{
					string strAlert = "<br><br><br><br><br><br><br><br><br><br><br><br><br>";
					strAlert += excp.ToString();
					
					Response.Write(strAlert);
				}

			
				BttnCall.Visible = false;
				TxtBxQry.Visible = false;
			}
		}
	}
}
