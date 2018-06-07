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
	/// Summary description for OPSSwitchBoardZoneSector.
	/// </summary>
	public partial class OPSSwitchBoardZoneSector : System.Web.UI.Page
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
				int iZone;
				int iSector;
				int iSectorId=-1;
				string strPlate = "";

				try
				{
					iZone = Convert.ToInt32((string)Request.QueryString["Zone"]);
				}
				catch
				{
					iZone=-1;
				}

				try
				{
					iSector = Convert.ToInt32((string)Request.QueryString["Sector"]);
				}
				catch
				{
					iSector=-1;
				}

				if ((iZone==0)||(iSector==0))
				{
					iZone=-1;
					iSector=-1;
				}



				bool bError=false;
				if ((iZone==-1)||(iSector==-1))
				{
					COPSSwitchBoardHelper.GetLastSector(strTelNumber,strFechaOp,ref strPlate, ref iSectorId);

					if (strPlate=="")
					{
						stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
						stRdo += "<r>";
						stRdo += "<y>0</y>";
						stRdo += "</r>";
						bError=true;

					}
					else
					{
						if (iSectorId==-1)
						{
							stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
							stRdo += "<r>";
							stRdo += "<y>1</y>";
							stRdo += "<m>"+strPlate+"</m>";
							stRdo += "</r>";
							bError=true;
						}

						
					}
				}
				else
				{
					iSectorId = COPSSwitchBoardHelper.GetSectorId(iZone,iSector);
				}

				if (!bError)
				{
					COPSSwitchBoardHelper.ManageCall( strTelNumber,"", strFechaOp,bSaveOp, iSectorId, -1, -1, ref stRdo, ref stError);

					if ((iZone==-1)||(iSector==-1))
					{
						COPSSwitchBoardHelper.GetZoneSector(iSectorId,ref iZone,ref iSector);

						//añadir la zona y sector a la respuesta
						//acordarse de añadir el error de retorno del m1 en el xml
						stRdo = stRdo.Replace("</r>",string.Format("<z>{0}</z><s>{1}</s></r>",iZone,iSector));

					}

				}
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
