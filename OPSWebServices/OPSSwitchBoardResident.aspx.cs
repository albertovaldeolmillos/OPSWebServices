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
	/// Summary description for OPSSwitchBoardResident.
	/// </summary>
	public partial class OPSSwitchBoardResident : System.Web.UI.Page
	{
		/*
		 $this->m_fullURL  = $this->m_baseURL."/OPSSwitchBoardResident.aspx"."?"."Plate=".$Plate."&Date=".$this->m_calldt;
				<?xml version="1.0" encoding="ISO-8859-1" ?> 
				<r>
					<res>1</res>   ( 0: NO, 1: SI )
					<ft>0</ft>		(fines today (multas hoy) 0:no 1:si)
					<resblue>60001</resblue>  (grupo azul del residente)
					<oper>1</oper>  ( 0: NO, 1: ESRO, 2: ESRE )
					<m>2345AAA</m>  (matricula)
					<ds>120002190809</ds>  (fecha de inicio)
					<de>122900180809</de>  (fecha de fin)
				</r>
		 
		*/


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
				string strPlate = (string)Request.QueryString["Plate"];
				string strFechaOp = (string)Request.QueryString["Date"];
				//bool bSaveOp = (((string)Request.QueryString["SaveOp"])=="1");

				COPSSwitchBoardHelper.GetResidentData( strPlate,strFechaOp, ref stRdo, ref stError);
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
