using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
//using Oracle.DataAccess.Client;
using System.Data;



namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to handle de m8 message.
	/// </summary>
	public sealed class Msg08 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m8)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg08()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m8"; } }
		#endregion

		#region Variables, creation and parsing

		public const int DEF_TRANSAX_AUTHORIZED =10;
		public const int DEF_TRANSAX_DECLINED =20;
		public const int DEF_TRANSAX_CONFIRMED =30;
		public const int DEF_TRANSAX_CANCELED =40;


		private int			_unit;
		private DateTime	_date;
		private double		_quantity;
		private DateTime	_dtExpirDate	= DateTime.MinValue;
		private string		_szCCNumber		= "";
		private string		_szCCName		= "";
		private string		_szCCCodServ	= "";
		private string		_szCCDiscData	= "";
		private string		_szInfoField	= "";
		private int			_binType = -1;


		private string 		_TRXTerminalUser="";
		private string 		_TRXTerminalPass="";
		private string 		_TRXTerminalStore="";
		private string 		_TRXTerminalStation="";
		private string 		_TRXRequestId="";
		private string 		_TRXRequestInvoice="";
		private string 		_TRXRequestAmount="";
		private string 		_TRXRequestTrack2="";
		private int			_TRXRequestAmountNumber=0;

		/// <summary>
		/// Constructs a new Msg07 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg08(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "q":
						_quantity = Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;

					case "tn":	_szCCNumber		= n.InnerText;									break;
					case "td":	_dtExpirDate	= OPS.Comm.Dtx.StringToDtx(n.InnerText);		break;
					case "tm":  _szCCName		= n.InnerText;									break;
					case "ts":  _szCCCodServ	= n.InnerText;									break;
					case "tdd":  _szCCDiscData	= n.InnerText;									break;
					case "i":	_szInfoField	= n.InnerText;									break;
					case "bt":	_binType	=	Convert.ToInt32(n.InnerText);					break;
					
					case "tru":	_TRXTerminalUser = n.InnerText;									break;
					case "trp":	_TRXTerminalPass = n.InnerText;									break;
					case "trs":	_TRXTerminalStore = n.InnerText;								break;
					case "trst":	_TRXTerminalStation = n.InnerText;							break;
					case "trrid":	_TRXRequestId = n.InnerText;								break;
					case "trriv":	_TRXRequestInvoice = n.InnerText;							break;
					case "tra":	_TRXRequestAmountNumber = Convert.ToInt32(n.InnerText);			break;
					case "t2":	_TRXRequestTrack2= n.InnerText;									break; 



					default:
						break;
				}
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Inserts a new register in the OPERATIONS table, and if everything is succesful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection ret = new StringCollection();
			ILogger logger = null;

            #region COMENTADO POR FALTA DLL CardEaseXMLClient
            //int responseResult = Msg07.DEF_CREDIT_CARD_NOK; 
            #endregion

            string strTransId="";
			int	   iTransId=-1;;
			string response="";

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;

				int iBinFormat=-1;
				if (_binType==-1)
				{
					CmpParametersDB cmpParam = new CmpParametersDB();
					string strBinFormat = cmpParam.GetParameter("P_BIN_FORMAT");					
					if (strBinFormat!="")
					{
						iBinFormat=Convert.ToInt32(strBinFormat);
					}
				}
				else
				{
					iBinFormat=_binType;
				}


                #region COMENTADO POR FALTA DLL CardEaseXMLClient
     //           if (iBinFormat == Msg07.DEF_BIN_FORMAT_CARDEASEXML)
     //           {
     //               int iSpecErrorCode = (int)CardEaseXML.ErrorCode.Empty;
     //               string errMessage = "";

     //               if (Msg07.CardEaseAuth(logger, _szCCNumber, _dtExpirDate, ref strTransId, Convert.ToInt32(_quantity), false, true,
     //                   out responseResult, out iSpecErrorCode, out errMessage))
     //               {
     //                   CmpCreditCardsTransactionsDB cmpCreditCardsTransactionsDB = new CmpCreditCardsTransactionsDB();
     //                   if (cmpCreditCardsTransactionsDB.InsertAuthTrans(strTransId, _date, _szCCNumber, _szCCName, _dtExpirDate,
     //                       _szCCCodServ, _szCCDiscData, Convert.ToInt32(_quantity), _unit, _szInfoField, out iTransId) == 1)
     //                   {
     //                       responseResult = Msg07.DEF_CREDIT_CARD_OK;

     //                   }
     //                   else
     //                   {
     //                       responseResult = Msg07.DEF_CREDIT_CARD_CHECK_NO_POSIBLE;

     //                   }

     //               }

     //               // Build response
     //               response = "<r>" + Convert.ToString(responseResult) + "</r>";
     //               if (responseResult == Msg07.DEF_CREDIT_CARD_OK)
     //               {
     //                   CultureInfo culture = new CultureInfo("", false);
     //                   response += "<ti>" + Convert.ToString(iTransId, (IFormatProvider)culture.NumberFormat) + "</ti>";
     //               }

     //               logger.AddLog("[Msg08:Process]: Response: " + response, LoggerSeverities.Debug);
     //               ret.Add(new AckMessage(_msgId, response).ToString());

     //           }
     //           else if (iBinFormat == Msg07.DEF_BIN_FORMAT_TRANSAX_ONLINE)
     //           {
     //               CS_M8_TRANSAX pCS_M8_TRANSAX = new CS_M8_TRANSAX();

     //               pCS_M8_TRANSAX._TRXTerminalUser = _TRXTerminalUser;
     //               pCS_M8_TRANSAX._TRXTerminalPass = _TRXTerminalPass;
     //               pCS_M8_TRANSAX._TRXTerminalStore = _TRXTerminalStore;
     //               pCS_M8_TRANSAX._TRXTerminalStation = _TRXTerminalStation;
     //               pCS_M8_TRANSAX._TRXRequestId = _TRXRequestId;
     //               pCS_M8_TRANSAX._TRXRequestInvoice = _TRXRequestInvoice;
     //               double dAmount = Convert.ToDouble(_TRXRequestAmountNumber) / 100.0;
     //               _TRXRequestAmount = dAmount.ToString().Replace(",", ".");
     //               pCS_M8_TRANSAX._TRXRequestAmount = _TRXRequestAmount;
     //               pCS_M8_TRANSAX._TRXRequestTrack2 = _TRXRequestTrack2;

     //               int iRes = pCS_M8_TRANSAX.TRXAuthorize();


     //               /*
					// * 
					// * 		public int			pCS_M8_TRANSAX._TRXResponseOperStatus=-1;  trxos
					//		public string 		pCS_M8_TRANSAX._TRXResponseId="";  trri
					//		public string 		pCS_M8_TRANSAX._TRXResponseTransStatus="";  trr
					//		public string 		pCS_M8_TRANSAX._TRXResponseISORespCode=""; trrc
					//		public string 		pCS_M8_TRANSAX._TRXResponseApproval=""; trid
					//		public string 		pCS_M8_TRANSAX._TRXResponseBatch=""; trba
					//		public string 		pCS_M8_TRANSAX._TRXResponseInvoice=""; tri
					//		public string 		pCS_M8_TRANSAX._TRXResponseCardName=""; trna
					//		public string 		pCS_M8_TRANSAX._TRXResponseMaskedPAN=""; trnb
					//		public string 		pCS_M8_TRANSAX._TRXResponseAmount=""; tra
					//*/
     //               // Build response
     //               response = "<trrr>" + Convert.ToString(iRes) + "</trrr>";
     //               response += "<tros>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseOperStatus) + "</tros>";
     //               response += "<trri>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseId) + "</trri>";
     //               response += "<trr>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseTransStatus) + "</trr>";
     //               response += "<trrc>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseISORespCode) + "</trrc>";
     //               response += "<trid>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseApproval) + "</trid>";
     //               response += "<trba>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseBatch) + "</trba>";
     //               response += "<tri>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseInvoice) + "</tri>";
     //               response += "<trna>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseCardName) + "</trna>";
     //               response += "<trnb>" + Convert.ToString(pCS_M8_TRANSAX._TRXResponseMaskedPAN) + "</trnb>";
     //               if (pCS_M8_TRANSAX._TRXResponseAmount != "")
     //               {
     //                   NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
     //                   double dResponseAmount = double.Parse(pCS_M8_TRANSAX._TRXResponseAmount, nfi);
     //                   dResponseAmount *= 100;
     //                   response += "<tra>" + Convert.ToInt32(dResponseAmount).ToString() + "</tra>";
     //               }

     //               IDbConnection con = d.GetNewConnection();
     //               con.Open();
     //               // Put the two operations inside a TRANSACTION because we want a block (updates to the
     //               // table cannot be allowed when we are reading the PK and inserting the data).
     //               IDbTransaction tran = con.BeginTransaction(IsolationLevel.Serializable);
     //               CmpCreditCardsDataDB cmpCreditCardsDataDB = new CmpCreditCardsDataDB();




     //               if (cmpCreditCardsDataDB.Insert(tran, -1, pCS_M8_TRANSAX._TRXResponseApproval, pCS_M8_TRANSAX._TRXResponseMaskedPAN, pCS_M8_TRANSAX._TRXResponseCardName,
     //                   "**/**", _quantity, pCS_M8_TRANSAX._TRXResponseBatch, (pCS_M8_TRANSAX._TRXResponseInvoice.Length == 0) ? 0 : Convert.ToUInt32(pCS_M8_TRANSAX._TRXResponseInvoice), pCS_M8_TRANSAX._TRXResponseId,
     //                   "PURCHASE", pCS_M8_TRANSAX._TRXResponseTransStatus, pCS_M8_TRANSAX._TRXResponseISORespCode,
     //                   (pCS_M8_TRANSAX._TRXResponseTransStatus == "AUTHORIZED") ? Msg08.DEF_TRANSAX_AUTHORIZED : Msg08.DEF_TRANSAX_DECLINED) < 0)
     //               {
     //                   RollbackTrans(tran);
     //                   if (logger != null)
     //                       logger.AddLog("[Msg08:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
     //                   return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
     //               }
     //               else
     //               {
     //                   CommitTrans(tran);
     //                   if (logger != null)
     //                       logger.AddLog("[Msg08:Process]: RESULT OK", LoggerSeverities.Debug);
     //               }



     //               logger.AddLog("[Msg08:Process]: Response: " + response, LoggerSeverities.Debug);
     //               ret.Add(new AckMessage(_msgId, response).ToString());

     //           }
     //           else
     //           {
     //               //ret.Add(new AckMessage(_msgId, response).ToString());
     //               ret = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);

     //           } 
                #endregion

                // INFO - Extarido del codigo comentado para que el proceso tenga valor de retorno
                ret = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);


            }				
			catch(Exception e)
			{
				logger.AddLog("[Msg08:Process]: Response: "+response,LoggerSeverities.Error);
				if(logger != null)
					logger.AddLog("[Msg08:Process]: Error: "+e.Message,LoggerSeverities.Error);
				ret = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				return ret;
			}



			return ret;

		}


		void CommitTrans(IDbTransaction tran)
		{

			IDbConnection con=null;
			try
			{
				if (tran!=null)
				{
					con=tran.Connection;
					tran.Commit();		
				}
				//	if (tra
			}
			catch
			{
		
			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();
				}	
				if (con!=null)
				{
					con.Close();
					con.Dispose();
				}

			}
		}

		void RollbackTrans(IDbTransaction tran)
		{

			IDbConnection con=null;
			try
			{
				if (tran!=null)
				{
					con=tran.Connection;
					tran.Rollback();					
				}
				//	if (tra
			}
			catch
			{

			}
			finally
			{
				if (tran!=null)
				{
					tran.Dispose();
				}	
				if (con!=null)
				{
					con.Close();
					con.Dispose();
				}

			}
		}


		#endregion
	}
}
