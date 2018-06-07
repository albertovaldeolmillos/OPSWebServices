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
	/// Summary description for OPSSwitchBoardAccount.
	/// </summary>
	public partial class OPSSwitchBoardAccount : System.Web.UI.Page
	{
		/*
			 # URL of web service
			$this->m_fullURL  = $this->m_baseURL."/OPSSwitchBoardAccount.aspx"."?"."TelNumber=".$this->m_callidnum."&Date=".$this->m_calldt;
				<?xml version="1.0" encoding="ISO-8859-1" ?> 
				<r>
					<usr>0</usr>  ( -1: No existe usuario o no tiene matricula, 
									-2: Tarjeta caducada
									-3: Cuenta no activa
									-4: Saldo inferior al mínimo (en este momento 0)
									 0: Todo ok
									 1: tarjeta proxima a caducar
									 2: Saldo superior al minimo pero inferior a un valor )
					<m>2345AAA</m>  (matricula)
					<m2>2345AAA</m2>  (matricula 2)
					<bal>1234</bal>  (saldo en centimos)
  					<tn>1234********5678</tn> (tarjeta de credito)
     				<td>03/12</td> 			(fecha caducidad tarjeta MM/YY)
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
				string strTelNumber = (string)Request.QueryString["TelNumber"];
				//string strFechaOp = (string)Request.QueryString["Date"];
				//bool bSaveOp = (((string)Request.QueryString["SaveOp"])=="1");

				COPSSwitchBoardHelper.GetAccountData( strTelNumber,  ref stRdo, ref stError);
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
