using System;
using System.Data;
using System.Xml;
using System.Collections.Specialized;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
//using Oracle.DataAccess.Client;
using System.Web.Services;
using System.ComponentModel;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Handles the M05 message: Query of the data of a fine.
	/// Used by the PDM before requesting a fine payment.
	/// </summary>
	internal sealed class Msg05 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m5)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m5"; } }
		#endregion

		#region Static stuff

		private static int FINES_DEF_CODES_FINE;
		private const int SETEX_FINE_LENGTH_CORDOBA=8;
		private const int SETEX_FINE_LENGTH_CASTROURDIALES=8;
		private const int EYSA_FINE_LENGTH_DONOSTIA=10;
		private const int EYSA_FINE_LENGTH_GETXO=15;

		/// <summary>
		/// Static constructor. Initializes values global to all M05.
		/// </summary>
		static Msg05()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			FINES_DEF_CODES_FINE = (int)appSettings.GetValue("FinesDefCodes.Fine", typeof(int));
		}

		#endregion

		#region Variables, creation and parsing

		private string _fineNumber;
		private DateTime _date;
		private bool _bIsMobileM5;

		/// <summary>
		/// Constructs a new M05 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg05(XmlDocument msgXml) : base(msgXml) {}

		public Msg05(string fineNumber,DateTime date,bool bIsMobileM5, long msgId, string msgDest)
		{
			_fineNumber = fineNumber;
			_date = date;
			_bIsMobileM5 =  bIsMobileM5;
			_msgId = msgId;
			_msgDest = msgDest;
		
		}

		protected override void DoParseMessage()
		{
			_bIsMobileM5=false;
			foreach (XmlNode node in _root.ChildNodes)
			{
				switch (node.Name)
				{
					case "f": _fineNumber = node.InnerText; break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
					case "m": _bIsMobileM5 = (Convert.ToInt32(node.InnerText)==1); break; // is mobile m5
				}
			}
		}

		#endregion

		#region IRecvMessage Members

		public System.Collections.Specialized.StringCollection Process()
		{

			StringCollection ret=null;
			int		 iSystemIdentifier=-1;
			string	 strSystemIdentifier="";

			CmpParametersDB cmpParam = new CmpParametersDB();
			strSystemIdentifier = cmpParam.GetParameter("P_SYSTEM_IDENTIFIER");
			if (strSystemIdentifier.Length>0)
			{
				try
				{
					iSystemIdentifier=Convert.ToInt32(strSystemIdentifier);
				}
				catch
				{
					iSystemIdentifier=-1;
				}
			}
			//throw new Exception("SystemIdentifier: [" + strSystemIdentifier + "]");

			switch(iSystemIdentifier)
			{
				case SYSTEM_IDENTIFIER_HONDARRIBIA:
				{
					ret=ProcessHondarribia();
					break;
				}
				case SYSTEM_IDENTIFIER_DURANGO:
				{
					ret=ProcessDurango();
					break;
				}
				case SYSTEM_IDENTIFIER_CASTROURDIALES:
				{
					ret=ProcessCastroUrdiales();
					break;
				}
				case SYSTEM_IDENTIFIER_CORDOBA:
				{
					ret=ProcessCordoba();
					break;
				}
				case SYSTEM_IDENTIFIER_DONOSTIA:
				{
					ret=ProcessDonostia();
					break;
				}				
				case SYSTEM_IDENTIFIER_BANYOLES:
				{
					ret=ProcessGiropark();
					break;
				}
				case SYSTEM_IDENTIFIER_GETXO:
				{
					ret=ProcessGetxo();
					break;
				}
				default:
				{
					ret=ProcessGeneric();
					break;
				}
			}
			
			return ret;
		}


		private System.Collections.Specialized.StringCollection ProcessGeneric()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseFineDefId = -1; // May be NULL
			string	 responseVehicleId = null; // May be NULL
			double	 responseQuantity = -1.0; // May be NULL
			DateTime responseDate = DateTime.MinValue; // May be NULL
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;
			int		 responseGrpId = 0;
			string	 strDayCode = "";
			// Auxiliar variables
			int payInPdm;

			CmpFinesDB fdb = new CmpFinesDB();
			string sql = string.Format("SELECT FIN_DFIN_ID, "
						+ "       FIN_VEHICLEID, "
						+ "       FIN_DATE, "
						+ "       fdq.DFINQ_VALUE, "
						+ "       fd1.DFIN_PAYINPDM, "
						+ "		  FIN_STATUSADMON, "
						+ "		  FIN_GRP_ID_ZONE, "
						+ "		  DDAY_CODE, "
						+ "		  DFINQ_INI_MINUTE, "
						+ "		  DFINQ_END_MINUTE, "
						+ "		  trunc((to_date('{0}', 'HH24MISSDDMMYY') - f.fin_date) * 24 * 60) ELAPSED_MINUTES  "
						+ " FROM DAYS_DEF dd, FINES f "
						+ " INNER JOIN FINES_DEF fd1 ON f.FIN_DFIN_ID = fd1.DFIN_ID, FINES_DEF fd2 "
						+ " INNER JOIN FINES_DEF_QUANTITY fdq ON fd2.DFIN_ID = fdq.DFINQ_ID "
						+ " WHERE FIN_ID = @FINES.FIN_NUMBER@ and fd1.dfin_pay_dday_id=dday_id "
						+ "   AND fd1.DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@ "
						+ "   and fd1.dfin_id = fd2.dfin_id "
						+ "   and f.fin_date >= fdq.dfinq_inidate "
						+ "   and f.fin_date < fdq.dfinq_endate",
							OPS.Comm.Dtx.DtxToString(_date));
			
/*			string sql = "SELECT FIN_DFIN_ID, FIN_VEHICLEID, FIN_DATE, DFIN_VALUE, DFIN_PAYINPDM "
				+ "FROM FINES "
				+ "INNER JOIN FINES_DEF ON FINES.FIN_DFIN_ID = FINES_DEF.DFIN_ID "
				+ "WHERE FIN_ID = @FINES.FIN_NUMBER@ AND DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@";*/


			DataTable dt = fdb.GetData(sql, new object[] {Convert.ToInt64(_fineNumber), FINES_DEF_CODES_FINE} );
			if (dt.Rows.Count > 0)
			{
				int i=0;
				bool bExit=false;
				
				while ((i<dt.Rows.Count)&&(!bExit))
				{
					responseResult = 0;

					responseFineDefId = Convert.ToInt32(dt.Rows[i]["FIN_DFIN_ID"]);
					responseVehicleId = Convert.ToString(dt.Rows[i]["FIN_VEHICLEID"]);
					responseQuantity  = Convert.ToDouble(dt.Rows[i]["DFINQ_VALUE"]);
					responseDate = Convert.ToDateTime(dt.Rows[i]["FIN_DATE"]);
					payInPdm = Convert.ToInt32(dt.Rows[i]["DFIN_PAYINPDM"]);
					responseGrpId = Convert.ToInt32(dt.Rows[i]["FIN_GRP_ID_ZONE"]);
					strDayCode = dt.Rows[i]["DDAY_CODE"].ToString();

					if (Convert.ToInt32(dt.Rows[i]["FIN_STATUSADMON"])!=CFineManager.C_ADMON_STATUS_PENDIENTE)
					{
						responsePayed=1;
					}
				

					int iFinQIniMinute=Convert.ToInt32(dt.Rows[i]["DFINQ_INI_MINUTE"]);
					int iFinQEndMinute=Convert.ToInt32(dt.Rows[i]["DFINQ_END_MINUTE"]);
					int iElapsedMinutes=Convert.ToInt32(dt.Rows[i]["ELAPSED_MINUTES"]);

					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					DBCon.Open();
					CFineManager oFineManager = new CFineManager();
					oFineManager.SetLogger(logger);
					oFineManager.SetDBConnection(DBCon);
					bool bFinePaymentInTime=oFineManager.IsFinePaymentInTime(responseDate,_date,payInPdm,strDayCode);
					DBCon.Close();

			
					if (payInPdm == 0)
					{
						responseResult = -1;
						bExit=true;
					}
					else if (bFinePaymentInTime)
					{
						if ((iElapsedMinutes>iFinQIniMinute)&&(iElapsedMinutes<=iFinQEndMinute))
						{
							responseResult = 1;
							bExit=true;
						}
						else
						{
							responseResult = -2;
						}
					}
					else
					{
						responseResult = -2;
						bExit=true;
					}

					i++;

				}
			}
			else
			{
				//todavía no existe la multa
				// existe alguna operación de pago de la misma
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
					{
						responsePayed=1;
					}
					cmd.Dispose();
				}
				catch(Exception e)
				{
					logger.AddLog(e);
				}
								
				DBCon.Close();


			}



			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>" + Convert.ToString(responseFineDefId) + "</y>";
				response += "<m>" + responseVehicleId + "</m>";
				response += "<q>" + Convert.ToString(responseQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d>" + OPS.Comm.Dtx.DtxToString(responseDate) + "</d>";
				if (_bIsMobileM5)
				{
					response +=	"<g>" + responseGrpId.ToString() + "</g>";				
				}
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessGeneric]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			StringCollection ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}

		private System.Collections.Specialized.StringCollection ProcessHondarribia()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseFineDefId = -1; // May be NULL
			string	 responseVehicleId = null; // May be NULL
			double	 responseQuantity = -1.0; // May be NULL
			DateTime responseDate = DateTime.MinValue; // May be NULL
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;
			int		 responseGrpId = 0;
			string	 strDayCode = "";
			// Auxiliar variables
			int payInPdm;

			CmpFinesDB fdb = new CmpFinesDB();
			string sql = string.Format("SELECT FIN_DFIN_ID, "
				+ "       FIN_VEHICLEID, "
				+ "       FIN_DATE, "
				+ "       fdq.DFINQ_VALUE, "
				+ "       fd1.DFIN_PAYINPDM, "
				+ "		  FIN_STATUSADMON, "
				+ "		  FIN_GRP_ID_ZONE, "
				+ "		  DDAY_CODE, "
				+ "		  DFINQ_INI_MINUTE, "
				+ "		  DFINQ_END_MINUTE, "
				+ "		  trunc((to_date('{0}', 'HH24MISSDDMMYY') - f.fin_date) * 24 * 60) ELAPSED_MINUTES  "
				+ " FROM DAYS_DEF dd, FINES f "
				+ " INNER JOIN FINES_DEF fd1 ON f.FIN_DFIN_ID = fd1.DFIN_ID, FINES_DEF fd2 "
				+ " INNER JOIN FINES_DEF_QUANTITY fdq ON fd2.DFIN_ID = fdq.DFINQ_ID "
				+ " WHERE FIN_ID = @FINES.FIN_NUMBER@ and fd1.dfin_pay_dday_id=dday_id "
				+ "   AND fd1.DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@ "
				+ "   and fd1.dfin_id = fd2.dfin_id "
				+ "   and f.fin_date >= fdq.dfinq_inidate "
				+ "   and f.fin_date < fdq.dfinq_endate",
				OPS.Comm.Dtx.DtxToString(_date));
			
			/*			string sql = "SELECT FIN_DFIN_ID, FIN_VEHICLEID, FIN_DATE, DFIN_VALUE, DFIN_PAYINPDM "
							+ "FROM FINES "
							+ "INNER JOIN FINES_DEF ON FINES.FIN_DFIN_ID = FINES_DEF.DFIN_ID "
							+ "WHERE FIN_ID = @FINES.FIN_NUMBER@ AND DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@";*/


			DataTable dt = fdb.GetData(sql, new object[] {Convert.ToInt64(_fineNumber), FINES_DEF_CODES_FINE} );
			if (dt.Rows.Count > 0)
			{
				int i=0;
				bool bExit=false;
				
				while ((i<dt.Rows.Count)&&(!bExit))
				{
					responseResult = 0;

					responseFineDefId = Convert.ToInt32(dt.Rows[i]["FIN_DFIN_ID"]);
					responseVehicleId = Convert.ToString(dt.Rows[i]["FIN_VEHICLEID"]);
					responseQuantity  = Convert.ToDouble(dt.Rows[i]["DFINQ_VALUE"]);
					responseDate = Convert.ToDateTime(dt.Rows[i]["FIN_DATE"]);
					payInPdm = Convert.ToInt32(dt.Rows[i]["DFIN_PAYINPDM"]);
					responseGrpId = Convert.ToInt32(dt.Rows[i]["FIN_GRP_ID_ZONE"]);
					strDayCode = dt.Rows[i]["DDAY_CODE"].ToString();

					if (Convert.ToInt32(dt.Rows[i]["FIN_STATUSADMON"])!=CFineManager.C_ADMON_STATUS_PENDIENTE)
					{
						responsePayed=1;
					}
				

					int iFinQIniMinute=Convert.ToInt32(dt.Rows[i]["DFINQ_INI_MINUTE"]);
					int iFinQEndMinute=Convert.ToInt32(dt.Rows[i]["DFINQ_END_MINUTE"]);
					int iElapsedMinutes=Convert.ToInt32(dt.Rows[i]["ELAPSED_MINUTES"]);

					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					DBCon.Open();
					CFineManager oFineManager = new CFineManager();
					oFineManager.SetLogger(logger);
					oFineManager.SetDBConnection(DBCon);
					bool bFinePaymentInTime=oFineManager.IsFinePaymentInTimeHondarribia(responseDate,_date,payInPdm,strDayCode);
					DBCon.Close();

			
					if (payInPdm == 0)
					{
						responseResult = -1;
						bExit=true;
					}
					else if (bFinePaymentInTime)
					{
						if ((iElapsedMinutes>iFinQIniMinute)&&(iElapsedMinutes<=iFinQEndMinute))
						{
							responseResult = 1;
							bExit=true;
						}
						else
						{
							responseResult = -2;
						}
					}
					else
					{
						responseResult = -2;
						bExit=true;
					}

					i++;

				}
			}
			else
			{
				//todavía no existe la multa
				// existe alguna operación de pago de la misma
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
					{
						responsePayed=1;
					}
					cmd.Dispose();
				}
				catch(Exception e)
				{
					logger.AddLog(e);
				}
								
				DBCon.Close();


			}



			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>" + Convert.ToString(responseFineDefId) + "</y>";
				response += "<m>" + responseVehicleId + "</m>";
				response += "<q>" + Convert.ToString(responseQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d>" + OPS.Comm.Dtx.DtxToString(responseDate) + "</d>";
				if (_bIsMobileM5)
				{
					response +=	"<g>" + responseGrpId.ToString() + "</g>";				
				}
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessGeneric]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			StringCollection ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}
					
		public System.Collections.Specialized.StringCollection ProcessDurango()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseFineDefId = -1; // May be NULL
			string	 responseVehicleId = null; // May be NULL
			double	 responseQuantity = -1.0; // May be NULL
			DateTime responseDate = DateTime.MinValue; // May be NULL
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;
			int		 responseGrpId = 0;
			string	 strDayCode = "";

			// Auxiliar variables
			int payInPdm;

			CmpFinesDB fdb = new CmpFinesDB();
			string sql = string.Format("SELECT (ASCII(SUBSTR(fd1.DFIN_DESCSHORT,LENGTH(fd1.DFIN_DESCSHORT),1))-ASCII('A')+1) FIN_DFIN_ID, "
				+ "       FIN_VEHICLEID, "
				+ "       FIN_DATE, "
				+ "       fdq.DFINQ_VALUE, "
				+ "       fd1.DFIN_PAYINPDM, "
				+ "		  FIN_STATUSADMON, "
				+ "		  FIN_GRP_ID_ZONE, "
				+ "		  DDAY_CODE "				
				+ " FROM DAYS_DEF dd, FINES f "
				+ " INNER JOIN FINES_DEF fd1 ON f.FIN_DFIN_ID = fd1.DFIN_ID, FINES_DEF fd2 "
				+ " INNER JOIN FINES_DEF_QUANTITY fdq ON fd2.DFIN_ID = fdq.DFINQ_ID "
				+ " WHERE FIN_ID = @FINES.FIN_NUMBER@ and fd1.dfin_pay_dday_id=dday_id "
				+ "   AND fd1.DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@ "
				+ "   and fd1.dfin_id = fd2.dfin_id "
				+ "   and f.fin_date >= fdq.dfinq_inidate "
				+ "   and f.fin_date < fdq.dfinq_endate"
				+ "	  and ((to_date('{0}','HH24MISSDDMMYY')-f.fin_date)*24*60)>DFINQ_INI_MINUTE"
				+ "	  and ((to_date('{0}','HH24MISSDDMMYY')-f.fin_date)*24*60)<=DFINQ_END_MINUTE",
				OPS.Comm.Dtx.DtxToString(_date));


			/*string sql = "SELECT (ASCII(SUBSTR(DFIN_DESCSHORT,LENGTH(DFIN_DESCSHORT),1))-ASCII('A')+1) FIN_DFIN_ID, FIN_VEHICLEID, FIN_DATE, DFIN_VALUE, DFIN_PAYINPDM "
				+ "FROM FINES "
				+ "INNER JOIN FINES_DEF ON FINES.FIN_DFIN_ID = FINES_DEF.DFIN_ID "
				+ "WHERE FIN_ID = @FINES.FIN_NUMBER@ AND DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@";*/


			DataTable dt = fdb.GetData(sql, new object[] {Convert.ToInt64(_fineNumber), FINES_DEF_CODES_FINE} );
			if (dt.Rows.Count > 0)
			{
				responseFineDefId = Convert.ToInt32(dt.Rows[0]["FIN_DFIN_ID"]);
				responseVehicleId = Convert.ToString(dt.Rows[0]["FIN_VEHICLEID"]);
				responseQuantity  = Convert.ToDouble(dt.Rows[0]["DFINQ_VALUE"]);
				responseDate = Convert.ToDateTime(dt.Rows[0]["FIN_DATE"]);
				payInPdm = Convert.ToInt32(dt.Rows[0]["DFIN_PAYINPDM"]);
				responseGrpId = Convert.ToInt32(dt.Rows[0]["FIN_GRP_ID_ZONE"]);
				strDayCode = dt.Rows[0]["DDAY_CODE"].ToString();

				if (Convert.ToInt32(dt.Rows[0]["FIN_STATUSADMON"])!=CFineManager.C_ADMON_STATUS_PENDIENTE)
				{
					responsePayed=1;
				}


				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				CFineManager oFineManager = new CFineManager();
				oFineManager.SetLogger(logger);
				oFineManager.SetDBConnection(DBCon);
				bool bFinePaymentInTime=oFineManager.IsFinePaymentInTime(responseDate,_date,payInPdm,strDayCode);
				DBCon.Close();

		
				if (payInPdm == 0)
					responseResult = -1;
				else if (bFinePaymentInTime)
					responseResult = 1;
				else
					responseResult = -2;
			}
			else
			{
				//todavía no existe la multa
				// existe alguna operación de pago de la misma
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
					{
						responsePayed=1;
					}
					cmd.Dispose();
				}
				catch(Exception e)
				{
					logger.AddLog(e);
				}
								
				DBCon.Close();


			}



			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>" + Convert.ToString(responseFineDefId) + "</y>";
				response += "<m>" + responseVehicleId + "</m>";
				response += "<q>" + Convert.ToString(responseQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d>" + OPS.Comm.Dtx.DtxToString(responseDate) + "</d>";
				if (_bIsMobileM5)
				{
					response +=	"<g>" + responseGrpId.ToString() + "</g>";				
				}
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessDurango]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			StringCollection ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}

		public System.Collections.Specialized.StringCollection ProcessCastroUrdiales()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			StringCollection ret;
			int iQuantity=0;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;

			

			if (_fineNumber != null)
			{
				if (_fineNumber.Length>SETEX_FINE_LENGTH_CASTROURDIALES)
				{
					_fineNumber = _fineNumber.Substring(0,SETEX_FINE_LENGTH_CASTROURDIALES);
				}
			}

			OPSMessages.WSObtenerImporteAnulacionSETEX_Castrourdiales.SETEX objFineInfoService=null;
			
			try
			{
				objFineInfoService=new OPSMessages.WSObtenerImporteAnulacionSETEX_Castrourdiales.SETEX();	
	
				if (_date.Year<=2010)
				{
					responseResult=objFineInfoService.EsExpedienteAnulable(_fineNumber,OPS.Comm.Dtx.DtxToString(_date));
					logger.AddLog(string.Format("[Msg05:ProcessCastroUrdiales]: FineInfoService.EsExpedienteAnulable Returned {0}. Fine Number: {1}. Date: {2})", responseResult,_fineNumber, OPS.Comm.Dtx.DtxToString(_date)),				
						LoggerSeverities.Info);

				}
				else
				{
					System.Decimal decResponse = objFineInfoService.ObtenerImporteAnulacion(_fineNumber,OPS.Comm.Dtx.DtxToString(_date));
					int iResponse=0;
					string strResponse="0";
					if (decResponse>=1)
					{
						strResponse="1";
						iQuantity=Convert.ToInt32(decResponse*100); //cents
					}
					else
					{
						iResponse=Convert.ToInt32(decResponse);
						strResponse=iResponse.ToString();

					}
					
					logger.AddLog(string.Format("[Msg05:ProcessCastroUrdiales]: FineInfoService.ObtenerImporteAnulacion Returned {0}({3}). Fine Number: {1}. Date: {2})", strResponse,_fineNumber, OPS.Comm.Dtx.DtxToString(_date),iQuantity),				
						LoggerSeverities.Info);

					responseResult=Convert.ToInt32(strResponse);
				}



			}
			catch (Exception e)
			{
				responseResult=0;
				if (_date.Year<=2010)
				{
					logger.AddLog(e);
					logger.AddLog(string.Format("[Msg05:ProcessCastroUrdiales]: Error Calling {2}/FineInfoService.EsExpedienteAnulable. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
						LoggerSeverities.Error);

				}
				else
				{
					if(logger != null)
					{
						logger.AddLog(e);
						logger.AddLog(string.Format("[Msg05:ProcessCastroUrdiales]: Error Calling {2}/FineInfoService.ObtenerImporteAnulacion. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
							LoggerSeverities.Error);
					}
				}

				

			}




			//todavía no existe la multa
			// existe alguna operación de pago de la misma
			Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
			System.Data.IDbConnection DBCon=d.GetNewConnection();
			DBCon.Open();
			try
			{
				String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
				OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
				if (Convert.ToInt32(cmd.ExecuteScalar())>0)
				{
					responsePayed=1;
				}
				cmd.Dispose();
			}
			catch(Exception e)
			{
				logger.AddLog(e);
			}
							
			DBCon.Close();


			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{

				if (_date.Year<=2010)
				{
					DBCon=d.GetNewConnection();
					DBCon.Open();
					try
					{
						String strSQL = String.Format("select dfinq_value "+
							"from fines_def_quantity "+
							"where dfinq_year=20{0} "+
							"and dfinq_id = 1", _fineNumber.Substring(0,2));
						OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
						iQuantity=Convert.ToInt32(cmd.ExecuteScalar());
						cmd.Dispose();
					}
					catch(Exception e)
					{
						logger.AddLog(e);
						DBCon.Close();
						ret = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						return ret;
					}
							
					DBCon.Close();
				}

				CultureInfo culture = new CultureInfo("", false);
				response += "<y>1</y>";
				response += "<m></m>";
				response += "<q>" + Convert.ToString(iQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d></d>";
				
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessCastroUrdiales]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}
					

		public System.Collections.Specialized.StringCollection ProcessCordoba()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			StringCollection ret;
			int iQuantity=0;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;

			

			if (_fineNumber != null)
			{
				if (_fineNumber.Length>SETEX_FINE_LENGTH_CORDOBA)
				{
					_fineNumber = _fineNumber.Substring(0,SETEX_FINE_LENGTH_CORDOBA);
				}
			}

			OPSMessages.WSObtenerImporteAnulacionSETEX_Cordoba.SETEX objFineInfoService=null;
			
			try
			{
				objFineInfoService=new OPSMessages.WSObtenerImporteAnulacionSETEX_Cordoba.SETEX();
				if (_date.Year<=2010)
				{
					responseResult=objFineInfoService.EsExpedienteAnulable(_fineNumber,OPS.Comm.Dtx.DtxToString(_date));
					logger.AddLog(string.Format("[Msg05:ProcessCordoba]: FineInfoService.EsExpedienteAnulable Returned {0}. Fine Number: {1}. Date: {2})", responseResult,_fineNumber, OPS.Comm.Dtx.DtxToString(_date)),				
						LoggerSeverities.Info);

				}
				else
				{
					System.Decimal decResponse = objFineInfoService.ObtenerImporteAnulacion(_fineNumber,OPS.Comm.Dtx.DtxToString(_date));
					int iResponse=0;
					string strResponse="0";
					if (decResponse>=1)
					{
						strResponse="1";
						iQuantity=Convert.ToInt32(decResponse*100); //cents
					}
					else
					{
						iResponse=Convert.ToInt32(decResponse);
						strResponse=iResponse.ToString();

					}
					
					logger.AddLog(string.Format("[Msg05:ProcessCordoba]: FineInfoService.ObtenerImporteAnulacion Returned {0}({3}). Fine Number: {1}. Date: {2})", strResponse,_fineNumber, OPS.Comm.Dtx.DtxToString(_date),iQuantity),				
						LoggerSeverities.Info);

					responseResult=Convert.ToInt32(strResponse);
				}



			}
			catch (Exception e)
			{
				responseResult=0;
				if (_date.Year<=2010)
				{
					logger.AddLog(e);
					logger.AddLog(string.Format("[Msg05:ProcessCordoba]: Error Calling {2}/FineInfoService.EsExpedienteAnulable. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
						LoggerSeverities.Error);

				}
				else
				{
					if(logger != null)
					{
						logger.AddLog(e);
						logger.AddLog(string.Format("[Msg05:ProcessCordoba]: Error Calling {2}/FineInfoService.ObtenerImporteAnulacion. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
							LoggerSeverities.Error);
					}
				}

			}




			//todavía no existe la multa
			// existe alguna operación de pago de la misma
			Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
			System.Data.IDbConnection DBCon=d.GetNewConnection();
			DBCon.Open();
			try
			{
				String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
				OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
				if (Convert.ToInt32(cmd.ExecuteScalar())>0)
				{
					responsePayed=1;
				}
				cmd.Dispose();
			}
			catch(Exception e)
			{
				logger.AddLog(e);
			}
							
			DBCon.Close();


			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{

				if (_date.Year<=2010)
				{
					DBCon=d.GetNewConnection();
					DBCon.Open();
					try
					{
						String strSQL = String.Format("select dfinq_value "+
							"from fines_def_quantity "+
							"where to_date('{0}','HH24MISSDDMMYY')>=DFINQ_INIDATE and "+
							"to_date('{0}','HH24MISSDDMMYY')<DFINQ_ENDATE "+
							"and dfinq_id = 1", OPS.Comm.Dtx.DtxToString(_date));
						OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
						iQuantity=Convert.ToInt32(cmd.ExecuteScalar());
						cmd.Dispose();
					}
					catch(Exception e)
					{
						logger.AddLog(e);
						DBCon.Close();
						ret = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						return ret;
					}
								
					DBCon.Close();
				}

				CultureInfo culture = new CultureInfo("", false);
				response += "<y>1</y>";
				response += "<m></m>";
				response += "<q>" + Convert.ToString(iQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d></d>";
				
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessCordoba]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}
		

		public System.Collections.Specialized.StringCollection ProcessDonostia()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			StringCollection ret;
			int iQuantity=0;
			int iIdContrata=-1;
			int iParkCode=-1;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;

			

			if (_fineNumber != null)
			{
				if (_fineNumber.Length>EYSA_FINE_LENGTH_DONOSTIA)
				{
					_fineNumber = _fineNumber.Substring(0,EYSA_FINE_LENGTH_DONOSTIA);
				}
			}

			OPSMessages.WSObtenerImporteAnulacionEYSA_Donostia.Anulaciones objFineInfoService=null;
			OPSMessages.WSObtenerImporteAnulacionEYSA_Donostia.ConsolaSoapHeader authentication=null;
			
			try
			{
				objFineInfoService=new OPSMessages.WSObtenerImporteAnulacionEYSA_Donostia.Anulaciones();
				authentication = new OPSMessages.WSObtenerImporteAnulacionEYSA_Donostia.ConsolaSoapHeader();

				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				try
				{
					iIdContrata = (int)appSettings.GetValue("IdContrata", typeof(int));
				}
				catch
				{
					iIdContrata=-1;
					logger.AddLog("[Msg05:ProcessDonostia]: Id Contrata not found. Make IdContrata=-1",LoggerSeverities.Error);
				}

				try
				{
					iParkCode = (int)appSettings.GetValue("ParkCode", typeof(int));					
				}
				catch
				{
					iParkCode=-1;
					logger.AddLog("[Msg05:ProcessDonostia]: ParkCode not found. Make ParkCode=-1",LoggerSeverities.Error);
				}

				logger.AddLog(string.Format("[Msg05:ProcessDonostia]: IdContrata={0};ParkCode={1}",iIdContrata,iParkCode) ,LoggerSeverities.Info);

				authentication.IdContrata = iIdContrata;
				objFineInfoService.ConsolaSoapHeaderValue = authentication;

				string strResponse = objFineInfoService.EsExpedienteAnulableConCuantia(_fineNumber,_date);
								
				logger.AddLog(string.Format("[Msg05:ProcessDonostia]: FineInfoService.EsExpedienteAnulableConCuantia Returned {0}. Fine Number: {1}. Date: {2})", strResponse,_fineNumber, OPS.Comm.Dtx.DtxToString(_date)),				
					LoggerSeverities.Info);
						
				/*<ANULACIONES Expediente = "1205453647" CodigoEsAnulable = "-1" Mensaje = "Denuncia encontrada pero el tipo de denuncia no es pagable" Cuantía = "0,00€"></ANULACIONES>
							
				0: no encontrada
				1: ok
				-1: tipo no pagable
				-2: pasado el plazo
				
				-3: ya anulada-->responsePayed=1; responseResult=1;
				-4: ya remesada -->ResponseResult=-2;
				-5: error BD --> responseResult=0;
					
				*/


				XmlDocument xmldoc = new XmlDocument();
				xmldoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"+strResponse);

				XmlNodeList xmlNodeAnulaciones = xmldoc.GetElementsByTagName("ANULACIONES");
				XmlElement xmlElementAnulaciones = (XmlElement)xmlNodeAnulaciones[0];

				string strCodigoEsAnulable= xmlElementAnulaciones.GetAttribute("CodigoEsAnulable");
				string strCuantia= xmlElementAnulaciones.GetAttribute("Cuantía");
				
				strCuantia=strCuantia.Replace("€","");
				
				CultureInfo culture = new CultureInfo("", false);
				double dCuantia=Convert.ToDouble(strCuantia.Replace(",","."), (IFormatProvider)culture.NumberFormat);
				iQuantity=Convert.ToInt32(dCuantia*100);

				int iCodigoEsAnulable=Convert.ToInt32(strCodigoEsAnulable);
			
				logger.AddLog(string.Format("[Msg05:ProcessDonostia]: FineNumber={0}, CodigoEsAnulable={1}, Cuantia={2}",_fineNumber,iCodigoEsAnulable,iQuantity),
					LoggerSeverities.Info);

				switch(iCodigoEsAnulable)
				{
					case -3: //Ya pagado
						responseResult=1;
						responsePayed=1;
						break;
					case -4:  //Expediente ya remesado, traducimos a pasado con retraso
						responseResult=-2;
						break;
					case -5: //Error BD . Traducimos a expediente no encontrado
						responseResult=0;
						break;
					default:
						responseResult=	iCodigoEsAnulable;
						break;
				}			
			}
			catch (Exception e)
			{
				responseResult=0;

				if(logger != null)
				{
					logger.AddLog(e);
					logger.AddLog(string.Format("[Msg05:ProcessDonostia]: Error Calling {2}/FineInfoService.EsExpedienteAnulableConCuantia. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
						LoggerSeverities.Error);
				}

			}


			if (responsePayed==0)
			{
				//todavía no existe la multa
				// existe alguna operación de pago de la misma
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
					{
						responsePayed=1;
					}
					cmd.Dispose();
				}
				catch(Exception e)
				{
					logger.AddLog(e);
				}
								
				DBCon.Close();
			}


			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{

				
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>1</y>";
				response += "<m></m>";
				response += "<q>" + Convert.ToString(iQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d></d>";
				
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				

			logger.AddLog(string.Format("[Msg05:ProcessDonostia]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);


			ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}
		

		public System.Collections.Specialized.StringCollection ProcessGiropark()
		{

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			StringCollection ret;
			int iQuantity=0;

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;
			int		 iCodiPoblacio=-1;

			
			//OPSMessages.WSObtenerImporteAnulacionGiropark.anulaciones objFineInfoService=null;
			OPSMessages.WSGetFineAmountGiropark.ServiceNotify objFineInfoService=null;
			
			try
			{

				try
				{

					CmpParametersDB cmpParam = new CmpParametersDB();
					//iCodiPoblacio = Convert.ToInt32(cmpParam.GetParameter("P_CODI_POB_GIROPARK"));
					iCodiPoblacio = Convert.ToInt32(cmpParam.GetParameter("P_CODE_INST_GIROPARK"));
				}
				catch
				{

				}


				objFineInfoService=new OPSMessages.WSGetFineAmountGiropark.ServiceNotify();
				string strDate = System.DateTime.Now.ToString("yyyyMMdd");
				string strHour = System.DateTime.Now.ToString("HHmmss");
	
				long lHash = CalculateHash(iCodiPoblacio.ToString() + _fineNumber + " " + strDate + strHour + "0" + " " + " " );
				// *** TEMP - For testing result code - 1
				//long lHash = CalculateHash(iCodiPoblacio.ToString() + _fineNumber + "0000" + "20151109" + "132000" + "0" + "0" + "0" );

				logger.AddLog(string.Format("[Msg05:ProcessGiropark]: FineInfoService Codi Poblacio:{0} Fine Number: {1} Date: {2} Hour: {3} Amount: {4} Hash: {5})", iCodiPoblacio, _fineNumber, strDate, strHour, iQuantity, lHash),				
					LoggerSeverities.Info);

				int iResponse = objFineInfoService.wsAnularDenunciaZB(iCodiPoblacio.ToString(), _fineNumber, " ", strDate, strHour, "0", " ", " ", lHash.ToString());
				// *** TEMP - For testing result code -1
				//int iResponse = objFineInfoService.wsAnularDenunciaZB(iCodiPoblacio.ToString(), _fineNumber, "0000", "20151109", "132000", "0", "0", "0", lHash.ToString());

				/*
				 *  a. >0: Importe de la denuncia
				 *	b. 0: No se ha encontrado el número de denuncia
					c. -1: Denuncia encontrada pero el periodo de pago ha expirado.
				*/
				
				string strResponse="0";
				if (iResponse>0)
				{
					strResponse="1";
					iQuantity=iResponse; //cents
				}
				else
				{					
					// Convert result code to OPS result code
					if ( iResponse == -1 )
						iResponse = -2;
						
					strResponse=iResponse.ToString();
				}

				responseResult=Convert.ToInt32(strResponse);

				logger.AddLog(string.Format("[Msg05:ProcessGiropark]: FineInfoService.wsAnularDenunciaZB Received {0} Returned {1})", iResponse, strResponse),				
					LoggerSeverities.Info);
			}
			catch (Exception e)
			{
				responseResult=0;

				if(logger != null)
				{
					logger.AddLog(e);
					logger.AddLog(string.Format("[Msg05:ProcessGiropark]: Error Calling {3}/FineInfoService.ObtenerImporteAnulacion. Codi Poblacio:{0}. Fine Number: {1}. Date: {2})", iCodiPoblacio, _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
						LoggerSeverities.Error);
				}
				

			}




			//todavía no existe la multa
			// existe alguna operación de pago de la misma
			Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
			System.Data.IDbConnection DBCon=d.GetNewConnection();
			DBCon.Open();
			try
			{
				String strSQL = String.Format("SELECT count(*) FROM operations WHERE ope_fin_id = {0}", Convert.ToInt64(_fineNumber));
				OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
				if (Convert.ToInt32(cmd.ExecuteScalar())>0)
				{
					responsePayed=1;
				}
				cmd.Dispose();
			}
			catch(Exception e)
			{
				logger.AddLog(e);
			}
							
			DBCon.Close();



			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>1</y>";
				response += "<m></m>";
				response += "<q>" + Convert.ToString(iQuantity, (IFormatProvider)culture.NumberFormat) + "</q>";
				response += "<d></d>";
				
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				
			logger.AddLog(string.Format("[Msg05:ProcessGiropark]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}

		public System.Collections.Specialized.StringCollection ProcessGetxo()
		{
			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			StringCollection ret;
			string strCuantia = "";
			int iIdContrata=-1;
			string sAppCode = "";
			string sOperData = "";
			int iFineType = 1;
			string strPlate = "";

			// Data to include in the response
			string	 responseFineNumber = _fineNumber;
			int		 responseResult = 0; // Cannot be NULL. Default result is Not Found
			int		 responsePayed = 0;

			if (_fineNumber != null)
			{
				if (_fineNumber.Length>EYSA_FINE_LENGTH_GETXO)
				{
					_fineNumber = _fineNumber.Substring(0,EYSA_FINE_LENGTH_GETXO);
				}
			}

			OPSMessages.WSObtenerImporteAnulacionEYSA_Getxo.Anulaciones objFineInfoService=null;
			OPSMessages.WSObtenerImporteAnulacionEYSA_Getxo.ConsolaSoapHeader authentication=null;
			
			try
			{
				objFineInfoService = new OPSMessages.WSObtenerImporteAnulacionEYSA_Getxo.Anulaciones();
				authentication = new OPSMessages.WSObtenerImporteAnulacionEYSA_Getxo.ConsolaSoapHeader();

				try
				{
					CmpParametersDB cmpParam = new CmpParametersDB();
					iIdContrata = Convert.ToInt32(cmpParam.GetParameter("P_CODE_INST_GETXO"));
				}
				catch
				{
					iIdContrata=-1;
					logger.AddLog("[Msg05:ProcessGetxo]: Id Contrata not found. Make IdContrata=-1",LoggerSeverities.Error);
				}

				try
				{
					CmpParametersDB cmpParam = new CmpParametersDB();
					sAppCode = cmpParam.GetParameter("P_APP_CODE_GETXO").ToString();
				}
				catch
				{
					sAppCode = "OPS";
					logger.AddLog("[Msg05:ProcessGetxo]: AppCode not found. Make AppCode=OPS",LoggerSeverities.Error);
				}

				logger.AddLog(string.Format("[Msg05:ProcessGetxo]: IdContrata={0};AppCode={1}",iIdContrata,sAppCode) ,LoggerSeverities.Info);

				authentication.IdContrata = iIdContrata;
				authentication.LocalTime = DateTime.Now;
				authentication.NomUsuario = sAppCode;
				authentication.IdUsuario = MsgOrig;
				objFineInfoService.ConsolaSoapHeaderValue = authentication;

				logger.AddLog(string.Format("[Msg05:ProcessGetxo]: Query fine {0}", _fineNumber), LoggerSeverities.Info);

				sOperData = "<ipark_in><f>" + _fineNumber + "</f><city_id>" + iIdContrata.ToString() + "</city_id><d>" + _date.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "</d><vers>1.0</vers><ah></ah><em>" + sAppCode + "</em></ipark_in>";

				string strResponse = objFineInfoService.rdPQueryFinePaymentQuantity( sOperData );
				// For testing response
				//string strResponse = "<ipark_out><r>1</r><q>265</q><lp>8238DTW</lp><d>2015-05-05T12:38:34.000</d><df>2015-05-06T12:38:47.000</df><ta>Art. 12B</ta><dta>ESTACIONAR EN ZONA O.R.A. CON EL DISTINTIVO QUE LOAUTORIZA REBASANDO EL TIEMPO PERMITIDO </dta></ipark_out>";
								
				logger.AddLog(string.Format("[Msg05:ProcessGetxo]: FineInfoService.EsExpedienteAnulableConCuantia Returned {0}. Fine Number: {1}. Date: {2})", strResponse,_fineNumber, OPS.Comm.Dtx.DtxToString(_date)),				
					LoggerSeverities.Info);
						
				/*<ANULACIONES Expediente = "1205453647" CodigoEsAnulable = "-1" Mensaje = "Denuncia encontrada pero el tipo de denuncia no es pagable" Cuantía = "0,00€"></ANULACIONES>
							
				1: ok				
				-4: ya remesada --> ResponseResult = -2
				-5: no encontrada --> ResponseResult = 0
				-6: encontrada, pero no anulable --> ResponseResult = -1
				-7: pasado el plazo --> ResponseResult = -2
				-8: ya anulada--> ResponsePayed = 1, responseResult = 1
				-9: error BD --> ResponseResult = 0;
					
				*/

				XmlDocument xmldoc = new XmlDocument();
				xmldoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"+strResponse);

				XmlNodeList xmlNodeResult = xmldoc.GetElementsByTagName("r");
				string strResult = xmlNodeResult[0].InnerText;
				int iCodigoEsAnulable = Convert.ToInt32(strResult);

				XmlNodeList xmlNodeAmount = xmldoc.GetElementsByTagName("q");
				strCuantia = xmlNodeAmount[0].InnerText;
								
				logger.AddLog(string.Format("[Msg05:ProcessGetxo]: FineNumber={0}, CodigoEsAnulable={1}, Cuantia={2}",_fineNumber,iCodigoEsAnulable,strCuantia),
					LoggerSeverities.Info);

				switch(iCodigoEsAnulable)
				{
					case -4:  //Expediente ya remesado, traducimos a pasado con retraso
						responseResult = -2;
						break;
					case -5:  // No encontrada
						responseResult = 0;
						break;
					case -6:  // Encontrada, pero no anulable
						responseResult = -1;
						break;
					case -7:  // Pasado el plazo
						responseResult = -2;
						break;
					case -8: // Ya pagado
						responseResult = 1;
						responsePayed = 1;
						break;
					case -9: //Error BD, traducimos a expediente no encontrado
						responseResult = 0;
						break;
					default:
						responseResult = iCodigoEsAnulable;
						break;
				}
			
				// If fine is payable, get the type
				if ( iCodigoEsAnulable == 1 )
				{
					XmlNodeList xmlNodeType = xmldoc.GetElementsByTagName("ta");
					string strType = xmlNodeType[0].InnerText;

					switch( strType )
					{
						case "Art. 14 a":
								iFineType = 1;
							break;
						case "Art. 14 b":
								iFineType = 2;
							break;
						case "Art. 14 c":
								iFineType = 3;
							break;
						case "Art. 14 d":
								iFineType = 4;
							break;
						case "Art. 14 e":
								iFineType = 5;
							break;
						case "Art. 14 f":
								iFineType = 6;
							break;
						case "Art. 14 g":
								iFineType = 7;
							break;
						case "Art. 14 h":
								iFineType = 8;
							break;
						case "Art. 14 i":
								iFineType = 9;
							break;
						case "Art. 14 j":
								iFineType = 10;
							break;
						case "Art. 14 k":
								iFineType = 11;
							break;
						default:
							logger.AddLog(string.Format("[Msg05:ProcessGetxo]: Fine type not found {0}, using default", strType), LoggerSeverities.Info);
								iFineType = 1;
							break;
					}

					XmlNodeList xmlNodePlate = xmldoc.GetElementsByTagName("lp");
					strPlate = xmlNodePlate[0].InnerText;
				}
			}
			catch (Exception e)
			{
				responseResult=0;

				if(logger != null)
				{
					logger.AddLog(e);
					logger.AddLog(string.Format("[Msg05:ProcessGetxo]: Error Calling {2}/FineInfoService.EsExpedienteAnulableConCuantia. Fine Number: {0}. Date: {1})", _fineNumber, OPS.Comm.Dtx.DtxToString(_date),objFineInfoService.Url),		
						LoggerSeverities.Error);
				}
			}

			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			response += "<f>" + responseFineNumber + "</f>";
			if (responseResult != 0)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<y>" + iFineType.ToString() + "</y>";
				response += "<m>" + strPlate + "</m>";
				response += "<q>" + strCuantia + "</q>";
				response += "<d></d>";
			}
			response +=	"<p>" + responsePayed.ToString() + "</p>";				

			logger.AddLog(string.Format("[Msg05:ProcessGetxo]: FineNumber={0}, Response={1}",_fineNumber,response),LoggerSeverities.Info);

			ret = new StringCollection();
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;
		}

		private long CalculateHash(string strText)
		{
			// Sum of all ASCII values multiplied by 23
			long lResult = 0;

			byte[] array = Encoding.ASCII.GetBytes(strText);

			foreach (byte element in array)
			{
				lResult += element;
			}

			lResult *= 23;

			return lResult;
		}
	#endregion
	}
}
