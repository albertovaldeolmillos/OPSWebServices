using System;
using System.Collections.Specialized;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using System.Data;
//using Oracle.DataAccess.Client;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Configuration;
using ZLibNet;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	public  enum CARD_TYPE
	{
		PRE_PAY = 1,
		POST_PAY = 2
	};

	public  enum TRANSACTION_STATUS
	{	
		REQUEST_SENT = 10,
		RESPONSE_RECEIVED = 20,
		CONFIRMATION_RECEIVED_FROM_PDM = 30,
		CONFIRMATION_SENT_TO_WD = 40,
		CONFIRMATION_RESPONSE_RECEIVED = 50
	};



	internal sealed class Msg90 : MsgReceived, IRecvMessage
	{

		private enum TRANSACTION_STATUS
		{	
			REQUEST_SENT = 10,
			RESPONSE_RECEIVED = 20,
			CONFIRMATION_RECEIVED_FROM_PDM = 30,
			CONFIRMATION_SENT_TO_WD = 40,
			CONFIRMATION_RESPONSE_RECEIVED = 50
		};

		
		
		private enum OPE_TYPE
		{
			OPEINS = 1,
			RECINS = 2,
			TARSAL = 3
		};

		private const int NUM_CHARS = 1536; // 48 bloques x 32 chars/bloque

		private const string  DEF_BLACKLIST_MSG = "TARJETA RECHAZADA";

		#region DefinedRootTag (m90)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m90"; } }
		#endregion


		#region Variables, creation and parsing



		private int _unit;
		private DateTime _date;
		CARD_TYPE _cardType;
		OPE_TYPE  _opeType;
		uint		  _cardId;
		int		  _amount;
		string	  _bytes;



		/// <summary>
		/// Constructs a new msg90 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg90(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{	
			foreach (XmlNode node in _root.ChildNodes)
			{
				switch (node.Name)
				{
					case "u":  _unit = Convert.ToInt32(node.InnerText); break;

					case "d":  _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;

					case "tt": 
					{
						if(node.InnerText == "1")
						{
							_cardType = CARD_TYPE.PRE_PAY;
						}
						else if( node.InnerText == "2")
						{
							_cardType = CARD_TYPE.POST_PAY;
						}
						break;
					}

					case "to":
					{
						switch(node.InnerText)
						{
							case "1": { _opeType = OPE_TYPE.OPEINS; break; }
							case "2": { _opeType = OPE_TYPE.RECINS; break; }
							case "3": { _opeType = OPE_TYPE.TARSAL; break; }
						}

						break; 
					}

					case "t": { _cardId = uint.Parse(node.InnerText); break; }

					case "q": { _amount = int.Parse(node.InnerText); break; }

					case "bl": 
					{
						// descomprimir
						int rest = node.InnerText.Length %2;
						if(rest == 0)// es par
						{
							byte [] byCompressed = new byte[node.InnerText.Length/2];
							byCompressed = StringToByteArray(node.InnerText);
/*
							bool bCompressed = false;
							byte [] byCmpr = CompressBodyToSend(byCompressed, ref bCompressed);
							string sCmpr = BitConverter.ToString(byCmpr);
							sCmpr = sCmpr.Replace("-","");
*/
							byte [] byUncompressed = UnCompress(byCompressed);

							string result = BitConverter.ToString(byUncompressed);
							result = result.Replace("-","");


							if(result.Length == NUM_CHARS)
							{
								_bytes = result;
							}
							else
							{
								// Error bad format
							}
							

						}
						break;
					}
				}
			}
		}

		public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

		#endregion
		
		
		private int GetIntFromTransactionStatus(TRANSACTION_STATUS te)
		{
			int iRes = -1;

			
			switch(te)
			{
				case TRANSACTION_STATUS.CONFIRMATION_RECEIVED_FROM_PDM:
				{
					iRes = 30;
					break;
				}
				case TRANSACTION_STATUS.CONFIRMATION_RESPONSE_RECEIVED:
				{
					iRes = 50;
					break;
				}
				case TRANSACTION_STATUS.CONFIRMATION_SENT_TO_WD:
				{
					iRes = 40;
					break;
				}
				case TRANSACTION_STATUS.REQUEST_SENT:
				{
					iRes = 10;
					break;
				}
				case TRANSACTION_STATUS.RESPONSE_RECEIVED:
				{
					iRes = 20;
					break;
				}
			}

			return iRes;
		}


		private int GetIntFromCardType( CARD_TYPE ct )
		{
			int iRes = -1;

			switch(ct)
			{
				case CARD_TYPE.PRE_PAY:
				{
					iRes = 1;
					break;
				}

				case CARD_TYPE.POST_PAY:
				{
					iRes = 2;
					break;
				}
			}
			return iRes;
		}


		private int GetIntFromOpeType( OPE_TYPE ot )
		{
			int iRes = -1;

			switch(ot)
			{
				case OPE_TYPE.OPEINS:
				{
					iRes = 1;
					break;
				}
				
				case OPE_TYPE.RECINS:
				{
					iRes = 2;
					break;
				}

				case OPE_TYPE.TARSAL:
				{
					iRes = 3;
					break;
				}
			}

			return iRes;
		}


		private MESSAGE_TYPE GetMessageTypeFromOperation()
		{
			MESSAGE_TYPE message_type = MESSAGE_TYPE.MSG_OPEINS;
			switch(_opeType)
			{
				case OPE_TYPE.OPEINS:
				{
					message_type = MESSAGE_TYPE.MSG_OPEINS;
					break;
				}
						
				case OPE_TYPE.RECINS:
				{
					message_type = MESSAGE_TYPE.MSG_RECINS;
					break;
				}
						
				case OPE_TYPE.TARSAL:
				{
					message_type = MESSAGE_TYPE.MSG_TARSAL;
					break;
				}
			}
			return message_type;
		}

		
		private string GetInsertMFTSQL(int iTransId)
		{
			string sInsert = String.Empty;
			sInsert = "insert into mifare_transaction (";
			sInsert += "mft_req_date,";
			sInsert += "mft_uni_id,";
			sInsert += "mft_status,";
			sInsert += "mft_card_type,";
			sInsert += "mft_operation_type,";
			sInsert += "mft_card_id,";
			sInsert += "mft_op_amount,";
			sInsert += "mft_result,";
			sInsert += "mft_message,";
			sInsert += "mft_balance,";
			sInsert += "mft_update,";
			sInsert += "mft_ope_id,";
			sInsert += "mft_confirm_date,";
			sInsert += "mft_confirm_retries,";
			sInsert += "MFT_UNI_TRANS_ID)";
					
			sInsert += "values(";

			sInsert += "to_date('" + _date.ToString("dd/MM/yyyy HH:mm:ss") + "','dd/mm/yyyy hh24:mi:ss'),";
			sInsert += _unit.ToString() + ",";
			sInsert += GetIntFromTransactionStatus(TRANSACTION_STATUS.REQUEST_SENT).ToString() + ",";
			sInsert += GetIntFromCardType(_cardType).ToString() + ",";
			sInsert += GetIntFromOpeType(_opeType).ToString() + ",";
			sInsert += _cardId.ToString() + ",";
			sInsert += _amount.ToString() + ",";
			sInsert += 0 + ",";
			sInsert += "null,";
			sInsert += "0,";
			sInsert += " null,";
			sInsert += "null,";
			sInsert += "null,";
			sInsert += "0, " + iTransId.ToString() + ")  returning MFT_ID into :ID";

			return  sInsert;
		}


		private int GetNextTransactionID(OracleConnection oraDBConn,OracleTransaction transaction)
		{
			OracleCommand oraCmd= new OracleCommand();
			oraCmd.Connection=(OracleConnection)oraDBConn;
		
			oraCmd.Transaction = transaction;

			
			int iTransId = -1;
			string sGetTransactID = "select MTI_NEXT_TRANS_ID ID from MIFARE_TRANSACTION_ID where MTI_UNI_ID = " + _unit.ToString();
					
			oraCmd.CommandText = sGetTransactID;
			OracleDataReader rd = oraCmd.ExecuteReader();
										
			while (rd.Read())
			{
				if(rd.IsDBNull(rd.GetOrdinal("ID")) == false)
				{
					iTransId = rd.GetInt32(rd.GetOrdinal("ID"));
					int iNextTransId = iTransId + 1;
					string sSetTransactID = "update MIFARE_TRANSACTION_ID set MTI_NEXT_TRANS_ID = " + iNextTransId.ToString();
					sSetTransactID += " where MTI_UNI_ID = " + _unit.ToString();
					oraCmd.CommandText = sSetTransactID;
					oraCmd.ExecuteNonQuery();
				}
				else
				{
					iTransId = 1;
				}
			}
			
			return iTransId;
		}

		public StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleTransaction transaction = null;

			bool bApOK = false;

			try
			{
				res = new StringCollection();
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();
				string sResponse = String.Empty;
							
				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{
					logger.AddLog("[Msg90:Process]: Database opened",LoggerSeverities.Debug);

                    #region COMENTADO POR FALRA DLL CardEaseXMLClient

                    //if (Msg07.IsCardInBlackList(logger, _cardId.ToString()))
                    //{
                    //    sResponse += "<r>0</r>";
                    //    sResponse += "<m>" + DEF_BLACKLIST_MSG + "</m>";
                    //    res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //}
                    //else
                    //{

                    //    oraCmd = new OracleCommand();
                    //    oraCmd.Connection = (OracleConnection)oraDBConn;

                    //    transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //    oraCmd.Transaction = transaction;

                    //    // Get Transaction id
                    //    int iTransId = -1;
                    //    MESSAGE_TYPE message_type = GetMessageTypeFromOperation();
                    //    if (MESSAGE_TYPE.MSG_TARSAL != message_type)
                    //    {
                    //        iTransId = GetNextTransactionID(oraDBConn, transaction);
                    //    }

                    //    //Insert to Mifare transaction table
                    //    OracleCommand oraCmd2 = new OracleCommand();
                    //    oraCmd2.Connection = (OracleConnection)oraDBConn;

                    //    oraCmd2.Transaction = transaction;

                    //    oraCmd2.CommandText = GetInsertMFTSQL(iTransId);


                    //    OracleParameter p = oraCmd2.Parameters.Add(":ID", OracleDbType.Decimal);
                    //    p.Direction = ParameterDirection.Output;

                    //    int numRowsAffected = oraCmd2.ExecuteNonQuery();

                    //    if (numRowsAffected == 0)
                    //    {
                    //        logger.AddLog("[Msg90:Process]__Error performing insert: " + oraCmd.CommandText, LoggerSeverities.Debug);
                    //    }

                    //    int returningId = int.Parse(p.Value.ToString());




                    //    Message msg = WDMessageFactory.CreateMessage(message_type);

                    //    //MESSAGE_TYPE.MSG_OPEINS

                    //    msg.SetTagValue(TagName.DispositivoID, _unit);
                    //    msg.SetTagValue(TagName.TarjetaID, _cardId);

                    //    switch (message_type)
                    //    {
                    //        case MESSAGE_TYPE.MSG_OPEINS:
                    //            {
                    //                logger.AddLog("[Msg90:Process]__Building Message: MESSAGE_TYPE.MSG_OPEINS", LoggerSeverities.Debug);
                    //                msg.SetTagValue(TagName.NumeroOperacionID, iTransId);
                    //                msg.SetTagValue(TagName.Importe, _amount);
                    //                break;
                    //            }
                    //        case MESSAGE_TYPE.MSG_OPENOTI:
                    //            {
                    //                logger.AddLog("[Msg90:Process]__Building Message: MESSAGE_TYPE.MSG_OPENOTI", LoggerSeverities.Debug);
                    //                msg.SetTagValue(TagName.NumeroOperacionID, iTransId);
                    //                break;
                    //            }

                    //        case MESSAGE_TYPE.MSG_RECINS:
                    //            {
                    //                logger.AddLog("[Msg90:Process]__Building Message: MESSAGE_TYPE.MSG_RECINS", LoggerSeverities.Debug);
                    //                msg.SetTagValue(TagName.NumeroOperacionID, iTransId);
                    //                msg.SetTagValue(TagName.Importe, _amount);
                    //                break;
                    //            }

                    //        case MESSAGE_TYPE.MSG_RECNOTI:
                    //            {
                    //                logger.AddLog("[Msg90:Process]__Building Message: MESSAGE_TYPE.MSG_RECNOTI", LoggerSeverities.Debug);
                    //                msg.SetTagValue(TagName.NumeroOperacionID, iTransId);
                    //                break;
                    //            }

                    //        case MESSAGE_TYPE.MSG_TARSAL:
                    //            {
                    //                logger.AddLog("[Msg90:Process]__Building Message: MESSAGE_TYPE.MSG_TARSAL", LoggerSeverities.Debug);

                    //                break;
                    //            }
                    //    }

                    //    int iOffset = 0;

                    //    TagHelper tagHelper = new TagHelper();

                    //    TagName[] blockArray = tagHelper.GetTagBlockArray();

                    //    foreach (TagName blockName in blockArray)
                    //    {
                    //        int iTagLength = tagHelper.GetTagLength(blockName);
                    //        msg.SetTagValue(blockName, _bytes.Substring(iOffset, iTagLength));
                    //        iOffset += iTagLength;
                    //    }

                    //    string messageText = msg.GetFullMessage();

                    //    WDSender sender = new WDSender();

                    //    sender.PREPAY_SERVER_ADDRESS = ConfigurationSettings.AppSettings["PrePayServerIP"].ToString();//"93.92.169.118";
                    //    sender.POSTPAY_SERVER_ADDRESS = ConfigurationSettings.AppSettings["PostPayServerIP"].ToString();//"93.92.169.118";
                    //    sender.PREPAY_SERVER_PORT = int.Parse(ConfigurationSettings.AppSettings["PrePayServerPort"].ToString());//8081;
                    //    sender.POSTPAY_SERVER_PORT = int.Parse(ConfigurationSettings.AppSettings["PostPayServerPort"].ToString());
                    //    try
                    //    {
                    //        sender.TIMEOUT = int.Parse(ConfigurationSettings.AppSettings["TimeoutServerComm"].ToString());
                    //    }
                    //    catch
                    //    {
                    //        sender.TIMEOUT = 10;
                    //    }
                    //    string response = null;

                    //    Socket socket = null;

                    //    bool bSent = sender.SendMessage(msg.GetFullMessage(), _cardType, out socket);

                    //    //<ap id=\"1\"/>_msgId

                    //    if (bSent)
                    //    {
                    //        transaction.Commit();

                    //        logger.AddLog("[Msg90:Process]__Message sent: " + msg.GetFullMessage(), LoggerSeverities.Debug);

                    //        bool bReceived = sender.WaitForResponse(out response, socket);

                    //        if (bReceived)
                    //        {
                    //            logger.AddLog("[Msg90:Process]__Message received: " + response, LoggerSeverities.Debug);
                    //            Message msgReceived = null;
                    //            bool bResParse = WDMessageFactory.ParseResponseStream(response, out msgReceived, message_type);

                    //            if (bResParse == true)
                    //            {
                    //                switch (msgReceived.GetMessageType())
                    //                {
                    //                    case MESSAGE_TYPE.MSG_OPEINS_RESP_OK:
                    //                        {
                    //                            #region OPEINS_RESP_OK
                    //                            oraCmd = new OracleCommand();
                    //                            oraCmd.Connection = (OracleConnection)oraDBConn;

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd.Transaction = transaction;

                    //                            int iSaldo = int.Parse(msgReceived.GetTagByName(TagName.Saldo).TAG_VALUE);

                    //                            // Update transaction y AP
                    //                            oraCmd.CommandText = "update mifare_transaction set mft_status = ";
                    //                            oraCmd.CommandText += GetIntFromTransactionStatus(TRANSACTION_STATUS.RESPONSE_RECEIVED);
                    //                            oraCmd.CommandText += ",mft_result = 1";
                    //                            oraCmd.CommandText += ",mft_balance = " + iSaldo.ToString();
                    //                            oraCmd.CommandText += ",mft_actualizacion ='" + msgReceived.GetTagByName(TagName.Actualizacion).TAG_VALUE + "'";
                    //                            oraCmd.CommandText += " where mft_id = " + returningId;



                    //                            int numRowsUpdated = oraCmd.ExecuteNonQuery();

                    //                            if (numRowsUpdated == 0)
                    //                            {
                    //                                res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    //                            }
                    //                            else
                    //                            {
                    //                                transaction.Commit();
                    //                                logger.AddLog("[Msg90:Process]__Transaction  mft_id = " + returningId + "  Updated ", LoggerSeverities.Debug);
                    //                            }


                    //                            //sResponse = "<ap id=\"" + _msgId + "\">";
                    //                            sResponse += "<r>1</r>";
                    //                            sResponse += "<tid>" + iTransId + "</tid>";
                    //                            sResponse += "<b>" + iSaldo.ToString() + "</b>";
                    //                            //sResponse += "<a>" + msgReceived.GetTagByName(TagName.Actualizacion).TAG_VALUE + "</a>";

                    //                            sResponse += "<bl>";

                    //                            byte[] byArray = new byte[NUM_CHARS];
                    //                            int iPos = 0;


                    //                            foreach (TagName blockName in blockArray)
                    //                            {
                    //                                Tag tag = msgReceived.GetTagByName(blockName);
                    //                                if (tag != null)
                    //                                {
                    //                                    string sNum = tagHelper.GetBlockNum(blockName);
                    //                                    int iSector = System.Int32.Parse(sNum.Substring(0, 2));
                    //                                    int iBloque = System.Int32.Parse(sNum.Substring(2, 1));
                    //                                    byte[] bySectorId = BitConverter.GetBytes(iSector);
                    //                                    byte[] byBloqueId = BitConverter.GetBytes(iBloque);

                    //                                    byte[] byCompressed = StringToByteArray(tag.TAG_VALUE);

                    //                                    byArray[iPos++] = bySectorId[0];
                    //                                    byArray[iPos++] = byBloqueId[0];

                    //                                    foreach (byte by in byCompressed)
                    //                                    {
                    //                                        byArray[iPos++] = by;
                    //                                    }

                    //                                }
                    //                            }

                    //                            if (iPos > 0)
                    //                            {
                    //                                byte[] byCmpr = CompressBodyToSend(byArray, iPos);
                    //                                string sCmpr = BitConverter.ToString(byCmpr);
                    //                                sCmpr = sCmpr.Replace("-", "");
                    //                                sResponse += sCmpr;
                    //                            }

                    //                            sResponse += "</bl>";
                    //                            //sResponse += "</ap>";

                    //                            res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            #endregion
                    //                            break;
                    //                        }
                    //                    case MESSAGE_TYPE.MSG_OPEINS_RESP_ERR:
                    //                        {
                    //                            #region MSG_OPEINS_RESP_ERR
                    //                            //<nb id="1" />
                    //                            //sResponse = "<ap id=\"" + _msgId + "\">";
                    //                            sResponse += "<r>0</r>";
                    //                            string sMessage = msgReceived.GetTagByName(TagName.Mensaje).TAG_VALUE;
                    //                            sMessage = sMessage.Trim();
                    //                            sResponse += "<m>" + sMessage + "</m>";

                    //                            sResponse += "<bl>";
                    //                            foreach (TagName blockName in blockArray)
                    //                            {
                    //                                Tag tag = msgReceived.GetTagByName(blockName);
                    //                                if (tag != null)
                    //                                {
                    //                                    string sNum = tagHelper.GetBlockNum(blockName);
                    //                                    sResponse += sNum + tag.TAG_VALUE;
                    //                                }
                    //                            }
                    //                            sResponse += "</bl>";

                    //                            //res.Add(sResponse); //ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);

                    //                            //Update de Mensaje
                    //                            OracleCommand oraCmd3 = new OracleCommand();
                    //                            oraCmd3.Connection = (OracleConnection)oraDBConn;

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd3.Transaction = transaction;

                    //                            // Update transaction y AP
                    //                            oraCmd3.CommandText = "update mifare_transaction set mft_status = 20, mft_message = '" + sMessage + "' where mft_id = " + returningId;
                    //                            int iNum = oraCmd3.ExecuteNonQuery();

                    //                            if (iNum != 0)
                    //                            {
                    //                                transaction.Commit();
                    //                                res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            }
                    //                            #endregion
                    //                            break;
                    //                        }
                    //                    case MESSAGE_TYPE.MSG_TARSAL_RESP_OK:
                    //                        {
                    //                            #region MSG_TARSAL_RESP_OK
                    //                            OracleCommand oraCmd4 = new OracleCommand();
                    //                            oraCmd4.Connection = (OracleConnection)oraDBConn;

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd4.Transaction = transaction;


                    //                            int iSaldo = int.Parse(msgReceived.GetTagByName(TagName.Saldo).TAG_VALUE);

                    //                            oraCmd4.CommandText = "update mifare_transaction set mft_status = ";
                    //                            oraCmd4.CommandText += GetIntFromTransactionStatus(TRANSACTION_STATUS.RESPONSE_RECEIVED);
                    //                            oraCmd4.CommandText += ", mft_balance = " + iSaldo.ToString();
                    //                            oraCmd4.CommandText += ", MFT_RESULT = 1";
                    //                            oraCmd4.CommandText += " where mft_id = " + returningId;

                    //                            int iRes = oraCmd4.ExecuteNonQuery();

                    //                            if (iRes != 0)
                    //                            {
                    //                                transaction.Commit();
                    //                                sResponse += "<r>1</r>";
                    //                                sResponse += "<b>" + iSaldo.ToString() + "</b>";
                    //                                //sResponse += "<b>" + msgReceived.GetTagByName(TagName.Saldo).TAG_VALUE + "</b>";

                    //                                res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            }
                    //                            else
                    //                            {
                    //                                res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    //                            }
                    //                            #endregion
                    //                            break;
                    //                        }
                    //                    case MESSAGE_TYPE.MSG_TARSAL_RESP_ERR:
                    //                        {
                    //                            #region MSG_TARSAL_RESP_ERR
                    //                            OracleCommand oraCmd4 = new OracleCommand();
                    //                            oraCmd4.Connection = (OracleConnection)oraDBConn;

                    //                            string sMessage = msgReceived.GetTagByName(TagName.Mensaje).TAG_VALUE;
                    //                            sMessage = sMessage.Trim();

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd4.Transaction = transaction;

                    //                            oraCmd4.CommandText = "update mifare_transaction set mft_status = ";
                    //                            oraCmd4.CommandText += GetIntFromTransactionStatus(TRANSACTION_STATUS.RESPONSE_RECEIVED);
                    //                            oraCmd4.CommandText += ", mft_message = '" + sMessage + "'";
                    //                            oraCmd4.CommandText += " where mft_id = " + returningId;

                    //                            int iRes = oraCmd4.ExecuteNonQuery();

                    //                            if (iRes != 0)
                    //                            {
                    //                                transaction.Commit();
                    //                                sResponse += "<r>0</r>";
                    //                                sResponse += "<m>" + sMessage + "</m>";
                    //                                res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            }
                    //                            else
                    //                            {
                    //                                res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    //                            }
                    //                            #endregion
                    //                            break;
                    //                        }
                    //                    case MESSAGE_TYPE.MSG_RECINS_RESP_OK:
                    //                        {
                    //                            #region MSG_RECINS_RESP_OK
                    //                            OracleCommand oraCmd4 = new OracleCommand();
                    //                            oraCmd4.Connection = (OracleConnection)oraDBConn;

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd4.Transaction = transaction;

                    //                            int iSaldo = int.Parse(msgReceived.GetTagByName(TagName.Saldo).TAG_VALUE);

                    //                            Tag tagImporte = msgReceived.GetTagByName(TagName.Importe);
                    //                            int iImporte = -1;

                    //                            if (tagImporte != null)
                    //                            {
                    //                                iImporte = int.Parse(msgReceived.GetTagByName(TagName.Importe).TAG_VALUE);
                    //                            }

                    //                            oraCmd4.CommandText = "update mifare_transaction set mft_status = ";
                    //                            oraCmd4.CommandText += GetIntFromTransactionStatus(TRANSACTION_STATUS.RESPONSE_RECEIVED);
                    //                            oraCmd4.CommandText += ", mft_balance = " + iSaldo.ToString();
                    //                            oraCmd4.CommandText += ",mft_actualizacion ='" + msgReceived.GetTagByName(TagName.Actualizacion).TAG_VALUE + "'";
                    //                            oraCmd4.CommandText += ", MFT_RESULT = 1";

                    //                            if (tagImporte != null)
                    //                            {
                    //                                oraCmd4.CommandText += ", mft_op_amount = " + iImporte.ToString();
                    //                            }

                    //                            oraCmd4.CommandText += " where mft_id = " + returningId;

                    //                            int numRowsUpdated = oraCmd4.ExecuteNonQuery();

                    //                            if (numRowsUpdated == 0)
                    //                            {
                    //                                res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    //                            }
                    //                            else
                    //                            {
                    //                                transaction.Commit();
                    //                                logger.AddLog("[Msg90:Process]__Transaction  mft_id = " + returningId + "  Updated ", LoggerSeverities.Debug);
                    //                            }


                    //                            //sResponse = "<ap id=\"" + _msgId + "\">";
                    //                            sResponse += "<r>1</r>";
                    //                            sResponse += "<tid>" + iTransId + "</tid>";
                    //                            sResponse += "<b>" + iSaldo.ToString() + "</b>";
                    //                            //sResponse += "<a>" + msgReceived.GetTagByName(TagName.Actualizacion).TAG_VALUE + "</a>";

                    //                            sResponse += "<bl>";

                    //                            byte[] byArray = new byte[18 * 48 * 2];
                    //                            int iPos = 0;


                    //                            foreach (TagName blockName in blockArray)
                    //                            {
                    //                                Tag tag = msgReceived.GetTagByName(blockName);
                    //                                if (tag != null)
                    //                                {
                    //                                    string sNum = tagHelper.GetBlockNum(blockName);
                    //                                    int iSector = System.Int32.Parse(sNum.Substring(0, 2));
                    //                                    int iBloque = System.Int32.Parse(sNum.Substring(2, 1));
                    //                                    byte[] bySectorId = BitConverter.GetBytes(iSector);
                    //                                    byte[] byBloqueId = BitConverter.GetBytes(iBloque);

                    //                                    byte[] byCompressed = StringToByteArray(tag.TAG_VALUE);

                    //                                    byArray[iPos++] = bySectorId[0];
                    //                                    byArray[iPos++] = byBloqueId[0];

                    //                                    foreach (byte by in byCompressed)
                    //                                    {
                    //                                        byArray[iPos++] = by;
                    //                                    }

                    //                                }
                    //                            }

                    //                            if (iPos > 0)
                    //                            {
                    //                                byte[] byCmpr = CompressBodyToSend(byArray, iPos);
                    //                                string sCmpr = BitConverter.ToString(byCmpr);
                    //                                sCmpr = sCmpr.Replace("-", "");
                    //                                sResponse += sCmpr;
                    //                            }

                    //                            sResponse += "</bl>";
                    //                            //sResponse += "</ap>";

                    //                            res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            #endregion
                    //                            break;
                    //                        }
                    //                    case MESSAGE_TYPE.MSG_RECINS_RESP_ERR:
                    //                        {
                    //                            #region MSG_RECINS_RESP_ERR
                    //                            OracleCommand oraCmd4 = new OracleCommand();
                    //                            oraCmd4.Connection = (OracleConnection)oraDBConn;

                    //                            string sMessage = msgReceived.GetTagByName(TagName.Mensaje).TAG_VALUE;
                    //                            sMessage = sMessage.Trim();

                    //                            transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    //                            oraCmd4.Transaction = transaction;

                    //                            oraCmd4.CommandText = "update mifare_transaction set mft_status = ";
                    //                            oraCmd4.CommandText += GetIntFromTransactionStatus(TRANSACTION_STATUS.RESPONSE_RECEIVED);
                    //                            oraCmd4.CommandText += ", mft_message = '" + sMessage + "'";
                    //                            oraCmd4.CommandText += " where mft_id = " + returningId;

                    //                            int iRes = oraCmd4.ExecuteNonQuery();

                    //                            if (iRes != 0)
                    //                            {
                    //                                transaction.Commit();
                    //                                sResponse += "<r>0</r>";
                    //                                sResponse += "<m>" + sMessage + "</m>";
                    //                                res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //                            }
                    //                            else
                    //                            {
                    //                                res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
                    //                            }
                    //                            #endregion
                    //                            break;
                    //                        }

                    //                }

                    //                logger.AddLog("[Msg90:Process]__Message To Return: " + sResponse, LoggerSeverities.Debug);
                    //            }
                    //            else
                    //            {
                    //                if (logger != null)
                    //                    logger.AddLog("[Msg90:Process]__Message Error: Received Msg has an invalid Format.", LoggerSeverities.Error);
                    //                // El mensaje no comple con el formato esperado
                    //                bApOK = false;
                    //                sResponse = "<r>0</r>";
                    //                res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (logger != null)
                    //                logger.AddLog("[Msg90:Process]__Message Error: Timeout Waiting Response.", LoggerSeverities.Error);
                    //            // No se ha recibido respuesta en el tiempo especificado
                    //            bApOK = false;
                    //            sResponse = "<r>0</r>";
                    //            res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (logger != null)
                    //            logger.AddLog("[Msg90:Process]__Message Error: Message could not be sent.", LoggerSeverities.Error);
                    //        transaction.Rollback();
                    //        bApOK = false;
                    //        sResponse = "<r>0</r>";
                    //        res.Add(new AckMessage(_msgId, sResponse).ToString());
                    //    }

                    //}

                    #endregion
                }
				else
				{
					if(logger != null)
						logger.AddLog("[Msg90:Process]: Error: BD is not opened",LoggerSeverities.Error);
					sResponse = "<r>0</r>";
					res.Add(new AckMessage(_msgId, sResponse).ToString());
				}

			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg90:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);

				if(transaction != null)
				{
					transaction.Rollback();
					transaction.Dispose();
					transaction = null;
				}

			}
			finally
			{
				if (oraCmd!=null)
				{
					oraCmd.Dispose();
					oraCmd = null;
				}

				if (oraDBConn!=null)
				{
					oraDBConn.Close();
					oraDBConn.Dispose();
					oraDBConn = null;
				}

			}

			return res;
		}

		protected static void CopyStream(System.IO.BinaryReader dataReader, System.IO.BinaryWriter dataWriter, int iLen)
		{
			byte[] buffer = new byte[iLen];
			int len;
			
			while ((len = dataReader.Read(buffer, 0, iLen)) > 0)
			{
				dataWriter.Write(buffer, 0, len);
			}
		}

		protected static void CopyStream(System.IO.BinaryReader dataReader, System.IO.BinaryWriter dataWriter)
		{
			byte[] buffer = new byte[2048];
			int len;
			while ((len = dataReader.Read(buffer, 0, 2048)) > 0)
			{
				dataWriter.Write(buffer, 0, len);
			}
		}
		
		public static void CopyStream(System.IO.Stream input, System.IO.BinaryWriter dataWriter)
		{
			byte[] buffer = new byte[2048];
			int len;
			while ((len = input.Read(buffer, 0, 2048)) > 0)
			{
				dataWriter.Write(buffer, 0, len);
			}
		}

		protected byte [] CompressBodyToSend(byte [] body, int iLen)
		{
			byte [] byRes=null;
			byte [] byTemp=null;
			System.IO.MemoryStream inMemStream = new System.IO.MemoryStream(body,0,iLen);
			System.IO.MemoryStream outMemStream = new System.IO.MemoryStream();

			try
			{
                ZLibStream zipStream = new ZLibStream(outMemStream, CompressionMode.Compress, CompressionLevel.Level9);
                //ManagedZLib.CompressionOptions option = ManagedZLib.CompressionStream.GetLevel( 9 );

				System.IO.BinaryReader dataReader;
				System.IO.BinaryWriter dataWriter;
				dataReader = new System.IO.BinaryReader(inMemStream);
				dataWriter = new BinaryWriter(zipStream);

                CopyStream(dataReader,dataWriter);
				dataWriter.Flush();
				byTemp=outMemStream.GetBuffer();

				
				byRes=new byte[outMemStream.Length];
				Array.Copy (byTemp,0, byRes,0, outMemStream.Length);
				
				
				dataReader.Close();
				dataWriter.Close();


			}
			finally
			{
				outMemStream.Close();
				inMemStream.Close();
			}

			return byRes;

		}
		

		protected byte [] UnCompress(byte [] body)
		{
			byte [] byRes=null;
			byte [] byTemp=null;
			System.IO.MemoryStream inMemStream = new System.IO.MemoryStream(body,0,body.Length);
			System.IO.MemoryStream outMemStream = new System.IO.MemoryStream();

			try
			{
				System.IO.BinaryWriter dataWriter;
				System.IO.BinaryReader dataReader;

                ZLibStream zipStream = new ZLibStream(inMemStream, CompressionMode.Decompress, CompressionLevel.Level9);

                dataReader = new BinaryReader(zipStream);
                dataWriter = new System.IO.BinaryWriter( outMemStream );
				CopyStream(dataReader,dataWriter);
				dataWriter.Flush();
				byTemp=outMemStream.GetBuffer();

				byRes=new byte[outMemStream.Length];
				Array.Copy (byTemp,0, byRes,0, outMemStream.Length);

				dataWriter.Close();
				dataReader.Close();
			}
			catch (Exception e)
			{
				byRes=null;	
			}
			finally
			{
				outMemStream.Close();
				inMemStream.Close();
			}

			return byRes;

		}		
		
	}
	

		


}