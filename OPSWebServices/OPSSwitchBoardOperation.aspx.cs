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
	/// Summary description for OPSSwitchBoardOperation.
	/// </summary>
	public partial class OPSSwitchBoardOperation : System.Web.UI.Page
	{
		/*
			$this->m_baseURL."/OPSSwitchBoardOperation.aspx"."?"."TelNumber=".$this->m_callidnum."&Date=".$this->m_calldt."&SaveOp=".$SaveOp;
		
			<?xml version="1.0" encoding="ISO-8859-1" ?> 
			<r>
				<y>3</y>  (
						-2: El saldo no es suficiente para realizar la operación  (solo si SaveOp==1)
						-1: No existe usuario o no tiene matricula, 
						0: La operacion no es posible 
					        1: aparcamiento,
						    2: prolongacion 
						3: devolucion )
				<m>2345AAA</m>  (matricula)
				<oper>2</oper>  En caso de amp o dev de dónde es la operación ( 0: NO, 1: ESRO, 2: ESRE )
  				<ds>120002190809</ds>  (fecha de inicio)
				<de>122900180809</de>  (fecha de fin)
     				<dr>123000190809</dr> (en caso de devolucion fecha final tras devolucion)
					<q>40</q> (cantidad en centimos consumida realmente)
				    <tn>1234********5678</tn> (tarjeta de credito)
     				<td>03/12</td> 			(fecha caducidad tarjeta MM/YY)
					<bal>1234</bal>		(saldo después de la operación)
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
				string strFechaOp = (string)Request.QueryString["Date"];
				bool bSaveOp = (((string)Request.QueryString["SaveOp"])=="1");

				string strGroup="";
				string strMaxTime="";
				string strArticleDef="";
				string strPlate="";
				
				try
				{							
					strPlate=(string)Request.QueryString["Plate"];
					if (strPlate==null)
						strPlate="";
				}
				catch
				{
					strPlate="";				
				}

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

				try
				{							
					strArticleDef=(string)Request.QueryString["ArticleDef"];
					if (strArticleDef==null)
						strArticleDef="-1";

				}
				catch
				{
					strArticleDef="-1";				
				}


				COPSSwitchBoardHelper.ManageCall( strTelNumber,strPlate, strFechaOp,bSaveOp, strGroup, strArticleDef, strMaxTime,  ref stRdo, ref stError);
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
