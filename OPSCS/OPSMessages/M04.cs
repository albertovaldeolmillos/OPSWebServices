using System;
using System.Data;
using System.Xml;
using System.Collections.Specialized;
using OPS.Components;
using OPS.Components.Data;
using OPS.Comm;
using System.Globalization;
using OPS.FineLib;
//using Oracle.DataAccess.Client;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Handles the M04 message: Payment of a fine.
	/// </summary>
	internal sealed class Msg04 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m4)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m4"; } }
		#endregion

		#region Static stuff

		private static string FINES_DEF_CODES_FINE;
		private static int FINES_DEF_PAYMENT;
		private static int OPERATIONS_DEF_PAYMENT;

		// Tipo de operación, hasta el momento en FINE, no había tipo
		// Pongo el equivalente al que hemos hecho para m02

		private static int OPERATIONS_BILLREADER_REFUNDRECEIPT=107;

		

		/// <summary>
		/// Static constructor. Initializes values global to all Msg04.
		/// </summary>
		static Msg04()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			FINES_DEF_CODES_FINE = (string)appSettings.GetValue("FinesDefCodes.Fine", typeof(string));
			FINES_DEF_PAYMENT = (int)appSettings.GetValue("FinesDef.Payment", typeof(int));
			OPERATIONS_DEF_PAYMENT = (int)appSettings.GetValue("OperationsDef.Payment", typeof(int));
		}

		#endregion

		#region Variables, creation and parsing

		private string _fineNumber = null;
		private long _lfineDef;
		private int _paymentDefId;
		private int	_operationId	= -1;
		private double _quantity;
		private int _unitId;
		private DateTime _date;
		private int			_mobileUserId	= -1;
		private int		_onlineMessage = 0;
		private int		_ticketNumber=-1;

		//>> Credit card
		private DateTime	_dtExpirDate	= DateTime.MinValue;
		private int			_nStatus		= -1;
		private string		_szCCNumber		= "";
		private string		_szCCName		= "";
		private string		_szCCCodServ		= "";
		private string		_szCCDiscData	= "";
		private double		_dChipCardCredit = -1;
		private ulong		_ulChipCardId	= 0;
		private int			_binType = -1;
		
		// Type for new OPERATIONS_BILLREADER_REFUNDRECEIPT 
		private int			_operType = -1;

		private const int  STATUS_INSERT	= 0;
		//<< Credit Card

		// Mobile payment data
		private string		_szCloudId = "";
		private int			_iOS = 0;

		/// <summary>
		/// Constructs a new Msg04 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg04(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			//JTZ - 06/08/2015 : Añadido para intentar localizar la razón de los errores en M2/M4
			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg02:DoParseMessage]",LoggerSeverities.Debug);

			try
			{
				foreach (XmlNode node in _root.ChildNodes)
				{
					switch (node.Name)
					{
						case "f": _fineNumber = node.InnerText; break;
						case "y": _lfineDef = Convert.ToInt64(node.InnerText); break;
						case "o": _operationId = Convert.ToInt32(node.InnerText); break;
						case "p": _paymentDefId = Convert.ToInt32(node.InnerText); break;
						case "q": _quantity = Convert.ToDouble(node.InnerText); break;
						case "u": _unitId = Convert.ToInt32(node.InnerText); break;
						case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
						case "mui": _mobileUserId = Convert.ToInt32(node.InnerText); break;

							// CARD
						case "tn":	_szCCNumber		= node.InnerText;									break;
						case "td":	_dtExpirDate	= OPS.Comm.Dtx.StringToDtx(node.InnerText);	break;
						case "tm":  _szCCName		= node.InnerText;									break;
						case "ts":  _szCCCodServ	= node.InnerText;									break;
						case "tdd":  _szCCDiscData	= node.InnerText;									break;

						case "chi": _ulChipCardId	= Convert.ToUInt32(node.InnerText); break;
						case "chc":
							_dChipCardCredit = Convert.ToDouble(node.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "om": _onlineMessage = Convert.ToInt32(node.InnerText); break;
						case "tcn": _ticketNumber = Convert.ToInt32(node.InnerText); break;
						case "bt":	_binType	=	Convert.ToInt32(node.InnerText); break;

							// _operType
						case "ot":  _operType	=	Convert.ToInt32(node.InnerText); break;
						
							// Mobile payment data
						case "cid": _szCloudId = node.InnerText; break;
						case "os": _iOS = Convert.ToInt32(node.InnerText); break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.AddLog("[Msg04:DoParseMessage] - ERROR in M4 parse",LoggerSeverities.Error);
				logger.AddLog("[Msg04:DoParseMessage] " + ex.ToString(),LoggerSeverities.Error);
				throw ex;
			}
		}

		#endregion

		#region IRecvMessage Members

		public System.Collections.Specialized.StringCollection Process()
		{
			//***** Parameters needed for FINES
			//id is not necessary
			//defId is FINE_TYPE_PAYMENT
			//number is the "fineNumber" parameter (optional)
			string vehicleId = null;
			string model = null;
			string manufacturer = null;
			string colour = null;
			int groupId = -1;
			int streetId = -1;
			int streetNumber = -1;
			//date is the "date" parameter
			string comments = null;
			int userId = -1;
			//unitId is the "unitId" parameter
			//payed is TRUE
			//int paymentDefId = -1; // Could be calculated through the "paymentDefDescShort" parameter, but is not used
			//sent is not necessary
			//extrasent is not necessary

			//***** Parameters needed for OPERATIONS (see also parameters for FINES)
			//quantity is the "quantity" parameter

			try
			{	
				Console.WriteLine("M04.cs:Proccess - Inicio del procesado del M04");
				ILogger logger = null;
				IDbTransaction tran=null;
				logger = DatabaseFactory.Logger;
				if(logger != null)
					logger.AddLog("[Msg04:Process]",LoggerSeverities.Debug);

				// this._paymentDefId == OPERATIONS_BILLREADER_REFUNDRECEIPT !!!!!!!!!!!!!!!! chapuza momentanea
				if( _operType == OPERATIONS_BILLREADER_REFUNDRECEIPT || this._paymentDefId == OPERATIONS_BILLREADER_REFUNDRECEIPT )
				{
				
					CmpBillReaderRefundsFineDB cmp = new CmpBillReaderRefundsFineDB();
					if(cmp.Insert( _unitId, _date, Convert.ToInt32(_quantity), _fineNumber, _lfineDef )<0)
					{
						if(logger != null)
							logger.AddLog("[Msg04:Process]:ERROR ON INSERT",LoggerSeverities.Debug);
						return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
					}
					else
					{
						if(logger != null)
							logger.AddLog("[Msg04:Process]: RESULT OK",LoggerSeverities.Debug);
						return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}				
				
				
				}
				else
				{
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

					if (!Msg07.ListaNegra(logger, _szCCNumber, iBinFormat))
					{
						// Step 1: Search for an existing fine
						DataTable dt = null;
						CmpFinesDB fdb = new CmpFinesDB();
						if (_fineNumber != null)
						{
							string sql = "SELECT * "
								+ "FROM FINES "
								+ "INNER JOIN FINES_DEF ON FINES.FIN_DFIN_ID = FINES_DEF.DFIN_ID "
								+ "WHERE FIN_NUMBER = @FINES.FIN_NUMBER@ "
								+ "AND DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@";
							dt = fdb.GetData(sql, new object[] { _fineNumber, FINES_DEF_CODES_FINE });
							if (dt.Rows.Count > 0)
							{
								vehicleId = (dt.Rows[0]["FIN_VEHICLEID"] == DBNull.Value ? null : (string)dt.Rows[0]["FIN_VEHICLEID"]);
								model = (dt.Rows[0]["FIN_MODEL"] == DBNull.Value ? null : (string)dt.Rows[0]["FIN_MODEL"]);
								manufacturer = (dt.Rows[0]["FIN_MANUFACTURER"] == DBNull.Value ? null : (string)dt.Rows[0]["FIN_MANUFACTURER"]);
								colour = (dt.Rows[0]["FIN_COLOUR"] == DBNull.Value ? null : (string)dt.Rows[0]["FIN_COLOUR"]);
								groupId = (dt.Rows[0]["FIN_GRP_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["FIN_GRP_ID"]));
								streetId = (dt.Rows[0]["FIN_STR_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["FIN_STR_ID"]));
								streetNumber = (dt.Rows[0]["FIN_STRNUMBER"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["FIN_STRNUMBER"]));
								comments = (dt.Rows[0]["FIN_COMMENTS"] == DBNull.Value ? null : (string)dt.Rows[0]["FIN_COMMENTS"]);
								userId = (dt.Rows[0]["FIN_USR_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["FIN_USR_ID"]));
								//quantity = Convert.ToDouble(dt.Rows[0]["DFIN_VALUE"]);
							}
							else
							{
								CmpFinesHisDB fhdb = new CmpFinesHisDB();
								sql = "SELECT * "
									+ "FROM FINES_HIS "
									+ "INNER JOIN FINES_DEF ON FINES_HIS.HFIN_DFIN_ID = FINES_DEF.DFIN_ID "
									+ "WHERE HFIN_NUMBER = @FINES_HIS.HFIN_NUMBER@ "
									+ "AND DFIN_COD_ID = @FINES_DEF.DFIN_COD_ID@";
								dt = fhdb.GetData(sql, new object[] { _fineNumber, FINES_DEF_CODES_FINE });
								if (dt.Rows.Count > 0)
								{
									vehicleId = (dt.Rows[0]["HFIN_VEHICLEID"] == DBNull.Value ? null : (string)dt.Rows[0]["HFIN_VEHICLEID"]);
									model = (dt.Rows[0]["HFIN_MODEL"] == DBNull.Value ? null : (string)dt.Rows[0]["HFIN_MODEL"]);
									manufacturer = (dt.Rows[0]["HFIN_MANUFACTURER"] == DBNull.Value ? null : (string)dt.Rows[0]["HFIN_MANUFACTURER"]);
									colour = (dt.Rows[0]["HFIN_COLOUR"] == DBNull.Value ? null : (string)dt.Rows[0]["HFIN_COLOUR"]);
									groupId = (dt.Rows[0]["HFIN_GRP_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["HFIN_GRP_ID"]));
									streetId = (dt.Rows[0]["HFIN_STR_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["HFIN_STR_ID"]));
									streetNumber = (dt.Rows[0]["HFIN_STRNUMBER"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["HFIN_STRNUMBER"]));
									comments = (dt.Rows[0]["HFIN_COMMENTS"] == DBNull.Value ? null : (string)dt.Rows[0]["HFIN_COMMENTS"]);
									userId = (dt.Rows[0]["HFIN_USR_ID"] == DBNull.Value ? -1 : Convert.ToInt32(dt.Rows[0]["HFIN_USR_ID"]));
									//quantity = Convert.ToDouble(dt.Rows[0]["DFIN_VALUE"]);
								}
							}
						}
						//				else
						//				{
						//					// Quantity can be calculated based on _fineDefId parameter
						//					CmpFinesDefDB fddb = new CmpFinesDefDB();
						//					DataTable fddt = fddb.GetData(null, "DFIN_DESCSHORT = @FINES_DEF.DFIN_DESCSHORT@", 
						//						new object[] {_fineDefDescShort});
						//					if (fddt.Rows.Count > 0)
						//						quantity = (double)fddt.Rows[0]["DFIN_VALUE"];
						//				}
						if (groupId == -1)
						{
							// Get the physical groups tree and store it in parentsList
							CmpGroupsChildsDB gcdb = new CmpGroupsChildsDB();
							groupId = gcdb.GetFirstPhysicalParent(_unitId);
						}

						// Step 2: Insert the payed register in the FINES table

						// ESTO NO FUNCIONA Y LO SE, LO SE ... falta la adecuación a la nueva tabla FINES
						/*
						* CFE - 020705 - Elimino inserción en fines de pagos llegados por m4
						fdb.InsertFine(FINES_DEF_PAYMENT,  vehicleId, model, manufacturer,
							colour, groupId, groupId, streetId, streetNumber, _date, comments, userId, _unitId, _paymentDefId,-1,-1);
						*/

						// Step 3: Insert the register in the OPERATIONS table
						CmpOperationsDB odb = new CmpOperationsDB();
						int nNewOperationID = 0;


						/// Returns 0 = 0K, -1 = ERR, 1 = OPERACION YA EXISTENTE
						int nInsOperRdo = odb.InsertOperation(OPERATIONS_DEF_PAYMENT, _operationId, -1, groupId,
							_unitId, _paymentDefId, _date, DateTime.MinValue, DateTime.MinValue, -1, _quantity, vehicleId, -1, _mobileUserId, -1,
							_dChipCardCredit, _ulChipCardId, (_fineNumber == null ? -1 : Convert.ToDouble(_fineNumber)),
							_lfineDef, -1, -1, _onlineMessage, _ticketNumber, ref nNewOperationID, out tran);

						// Smartcode implementation
						if (_fineNumber.Length <= 10)
						{
							if ((nInsOperRdo == 0) && (_fineNumber != null))
							{
								CFineManager oFineManager = new CFineManager();
								oFineManager.SetLogger(logger);
								oFineManager.SetDBTransaction(tran);
								oFineManager.SetFineStatus(int.Parse(_fineNumber));
							}
						}
						else
						{
							string sFineCode = "";
							for (int i = 0; i < 10; i++)
							{
								string sByte = _fineNumber.Substring(i * 2, 2);
								int nValue = Convert.ToInt32(sByte);
								char cByte = (char)nValue;
								sFineCode += cByte.ToString();
							}
							CFineManager oFineManager = new CFineManager();
							oFineManager.SetLogger(logger);
							oFineManager.SetDBTransaction(tran);
							oFineManager.UpdateOperationFineNumber(nNewOperationID, sFineCode);
						}

						if (nInsOperRdo == 0)
						{
							if (!UpdateCloudData(nNewOperationID, tran))
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							OPS.Components.Data.CmpCreditCardDB cmpCreditCard = null;
							cmpCreditCard = new OPS.Components.Data.CmpCreditCardDB();

							if (cmpCreditCard == null)
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							try
							{
								if (_szCCNumber != "")
								{
									if (logger != null)
										logger.AddLog("[Msg04:Process]: Operation WITH CARD Id", LoggerSeverities.Debug);


									CmpCreditCardsTransactionsDB cmpCreditCardsTransactionsDB = new CmpCreditCardsTransactionsDB();

									if (_szCCNumber == "CCZ_OPERATIONID")
									{
										//szCCName contiene el número de transacción
										//nNewOperationID
										OracleConnection oraDBConn = null;
										OracleCommand oraCmd = null;


										Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
										logger = DatabaseFactory.Logger;
										oraDBConn = (OracleConnection)tran.Connection;


										//
										string state = String.Empty;
										string selectMFT = "select mft_status from mifare_transaction where MFT_UNI_TRANS_ID = " + _szCCName;
										selectMFT += " and MFT_UNI_ID = " + _unitId;

										if (oraDBConn.State == System.Data.ConnectionState.Open)
										{
											oraCmd = new OracleCommand();
											oraCmd.Connection = (OracleConnection)oraDBConn;
											oraCmd.CommandText = selectMFT;
											oraCmd.Transaction = (OracleTransaction)tran;

											OracleDataReader rd = oraCmd.ExecuteReader();

											while (rd.Read())
											{
												int i = rd.GetOrdinal("MFT_STATUS");
												state = (rd.GetInt32(rd.GetOrdinal("MFT_STATUS"))).ToString();
											}

										}
										else
										{
											RollbackTrans(tran);
											return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
										}

										if (state == "20")
										{
											string updateMFT = "update mifare_transaction ";
											updateMFT += "set mft_status = 30, mft_ope_id = " + nNewOperationID.ToString();
											updateMFT += "  where MFT_UNI_TRANS_ID= " + _szCCName + " and MFT_UNI_ID = " + _unitId;

											if (oraDBConn.State == System.Data.ConnectionState.Open)
											{
												oraCmd = new OracleCommand();
												oraCmd.Connection = (OracleConnection)oraDBConn;
												oraCmd.CommandText = updateMFT;
												oraCmd.Transaction = (OracleTransaction)tran;
												int numRowsAffected = oraCmd.ExecuteNonQuery();

												if (numRowsAffected == 0)
												{
													RollbackTrans(tran);
													ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
													return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
												}
											}
											else
											{
												RollbackTrans(tran);
												return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);

											}
										}
										else
										{
											RollbackTrans(tran);
											return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
										}

									}
									else if (iBinFormat == Msg07.DEF_BIN_FORMAT_EMV_TAS && _szCCNumber == "TRANSACTION_ID")
									{
										int iTransId = -1;
										if (cmpCreditCardsTransactionsDB.InsertCommitTrans(tran, _szCCName, _date, nNewOperationID, Convert.ToInt32(_quantity), _unitId, _fineNumber, out iTransId) < 0)
										{
											RollbackTrans(tran);
											if (logger != null)
												logger.AddLog("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
											return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
										}
										else
										{
											if (logger != null)
												logger.AddLog("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
										}
									}
									else if (iBinFormat == Msg07.DEF_BIN_FORMAT_EMV_TAS && _szCCNumber != "TRANSACTION_ID")
									{
										RollbackTrans(tran);
										if (logger != null)
											logger.AddLog("[Msg02:Process]:TRANSACTION ID IS NOT ATTACHED", LoggerSeverities.Debug);
										return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
									}
									else
									{
										_nStatus = STATUS_INSERT;
										if (cmpCreditCard.Insert(tran, nNewOperationID, _szCCNumber, _szCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
										{
											RollbackTrans(tran);
											if (logger != null)
												logger.AddLog("[Msg04:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
											return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
										}
										else
										{
											if (logger != null)
												logger.AddLog("[Msg04:Process]: RESULT OK", LoggerSeverities.Debug);
										}
									}
								}
								else
								{
									if (logger != null)
										logger.AddLog("[Msg04:Process]: Operation WITHOUT CARD Id", LoggerSeverities.Debug);
								}
							}
							catch (Exception exc)
							{
								RollbackTrans(tran);
								if (logger != null)
									logger.AddLog("[Msg04:Process]" + exc.Message, LoggerSeverities.Debug);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS); ;
							}
						}
						else if (nInsOperRdo == 1)
						{
							//RollbackTrans(tran);
							if (logger != null)
								logger.AddLog("[Msg04:Process]: Operation already exists in DB", LoggerSeverities.Debug);
							return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
						}
						else
						{
							RollbackTrans(tran);
							return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}

					}
					else
					{
						if (!InsertFraudMsgs(logger))
						{
							return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}

					}
				}

                CommitTrans(tran);
						// Finished.
				return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
			}
			catch (Exception)
			{
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
		}


		private bool InsertFraudMsgs(ILogger logger)
		{
			bool bRet=false;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("insert into MSGS_XML_FRAUD_OPERATIONS (MXF_UNI_ID,MXF_MOVDATE,MXF_NUMBER,MXF_NAME,MXF_XPRTN_DATE,MXF_FIN_ID,MXF_XML) values "+
						"({0},to_date('{1}','hh24missddmmyy'),'{2}','{3}',to_date('{4}','hh24missddmmyy'),{5},'{6}')", 
						_unitId, 
						OPS.Comm.Dtx.DtxToString(_date),
						_szCCNumber,
						_szCCName,
						OPS.Comm.Dtx.DtxToString(_dtExpirDate),
						_fineNumber,
						_root.OuterXml);

					logger.AddLog("[Msg04:Process]: "+strSQL,LoggerSeverities.Debug);
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (cmd.ExecuteNonQuery()==1)
					{
						bRet=true;
					}
					cmd.Dispose();
				}
				catch
				{
				}
								
				DBCon.Close();


			}
			catch
			{
				
			}
				
			return bRet;
		}

		bool UpdateCloudData(int iOperationID, IDbTransaction tran)
		{
			bool bOK=true;

			if (( _mobileUserId > 0 ) && (_szCloudId.Length > 0 || _iOS > 0 ))
			{
				OracleConnection oraDBConn=null;
				OracleCommand oraCmd=null;
				ILogger logger = null;
				try
				{
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					logger = DatabaseFactory.Logger;
					oraDBConn = (OracleConnection)tran.Connection;				
					if (oraDBConn.State == System.Data.ConnectionState.Open)
					{
						oraCmd= new OracleCommand();
						oraCmd.Connection=(OracleConnection)oraDBConn;
						oraCmd.Transaction=(OracleTransaction)tran;

						StringBuilder sqlQuery = new StringBuilder();
						if ( _szCloudId.Length > 0 && _iOS > 0 )
						{
							sqlQuery.AppendFormat(	" update mobile_users m " +
								"set m.mu_cloud_token = '{0}', m.mu_device_os = {1} " +
								"where mu_id = {2}", _szCloudId, _iOS, _mobileUserId);
						}
						else if ( _szCloudId.Length > 0 )
						{
							sqlQuery.AppendFormat(	" update mobile_users m " +
								"set m.mu_cloud_token = '{0}' " +
								"where mu_id = {1}", _szCloudId, _mobileUserId);
						}
						else
						{
							sqlQuery.AppendFormat(	" update mobile_users m " +
								"set m.mu_device_os = {0} " +
								"where mu_id = {1}", _iOS, _mobileUserId);
						}

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg04:UpdateCloudData]: Excepcion: "+e.Message,LoggerSeverities.Error);
					bOK=false;
				}
				finally
				{
					if (oraCmd!=null)
					{
						oraCmd.Dispose();
						oraCmd = null;
					}
				}
			}

			return bOK;
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
