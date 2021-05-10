using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Components;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
using CS_OPS_TesM1;
using System.Data;
////using Oracle.DataAccess.Client;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to handle de m2 message.
	/// </summary>
	// ORC 21.03.2005 internal sealed class Msg02 : MsgReceived, IRecvMessage
	public sealed class Msg02 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m2)
		#region Static stuff

		// Static "constants"
		private static int OPERATIONS_DEF_PARKING;
		private static int OPERATIONS_DEF_EXTENSION;
		private static int OPERATIONS_DEF_REFUND;
		private static int OPERATIONS_GUARD_CLOCK_IN=100;
		private static int OPERATIONS_UPLOCK_OPEN=105;
		private static int OPERATIONS_DOWNLOCK_OPEN=106;
		private static int OPERATIONS_BILLREADER_REFUNDRECEIPT=107;
		static int  C_MAX_COUPONS=5;


		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg02()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			
			OPERATIONS_DEF_PARKING = (int)appSettings.GetValue ("OperationsDef.Parking", typeof(int));
			OPERATIONS_DEF_EXTENSION = (int)appSettings.GetValue ("OperationsDef.Extension", typeof(int));
			OPERATIONS_DEF_REFUND = (int)appSettings.GetValue ("OperationsDef.Refund", typeof(int));

			
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m2"; } }
		#endregion

		#region Variables, creation and parsing

		private string		_vehicleId		= null;
		private int			_operationDefId;
		private int			_operationId	= -1;
		private int			_articleId		= -1;
		private int			_articleDefId	= -1;
		private int			_groupId		= -1;
		private int			_unitId;
		private int			_paytypeDefId	= -1;
		private DateTime	_date;
		private DateTime	_dateIni		= DateTime.MinValue;
		private DateTime	_dateEnd		= DateTime.MinValue;
		private int			_time			= -1;
		private double		_quantity;
		private int			_mobileUserId	= -1;
		private int		_quantityReturned = -1;
		private double		_quantityReal = -1;
		private int		_onlineMessage = 0;
		private int		_ticketNumber=-1;

		// CARD 
		/*
		private DateTime	_dtCardDateResult = DateTime.MinValue;
		private int			_nStatus		= -1;
		private string		_szTargetId		= "";
		private int			_nTargetType	= -1;
		*/
		private DateTime	_dtExpirDate	= DateTime.MinValue;
		private int			_nStatus		= -1;
		private string		_szCCNumber		= "";
		private string		_szCCName		= "";
		private string		_szCCCodServ	= "";
		private string		_szCCDiscData	= "";
		private string		_szRechargeCCNumber		= "";
		private string		_szRechargeCCName		= "";
		private int			_iPostPay		= -1;
		private double		_dChipCardCredit = -1;
		private ulong		_ulChipCardId	= 0;
		private int			_binType = -1;
		private string		_vaoCard1="";
		private string		_vaoCard2="";
		private string		_vaoCard3="";
		private const int  STATUS_INSERT	= 0;

		// Additional credit card data (initially for Transax)
		private string		_szCCDTransId	= "";
		private string		_szCCDType	= "";
		private double		_dCCDQuantity	= -1;
		private string		_szCCDResult	= "";
		private string		_szCCDResultCode	= "";
		private string		_szCCDCardName	= "";
		private string		_szCCDCardNumber	= "";
		private string		_szCCDExpDate	= "";
		private string		_szCCDBatch	= "";
		private ulong		_ulCCDInvoice	= 0;
		private string		_szCCDResponseId	= "";
		private string		_coid=""; //camera operation id
		private int			_paytypeDefIdVis=-1;
		private uint[]		_couponsId= new uint[C_MAX_COUPONS];
		private int			_iNumCoupons=0;

		// Mobile payment data
		private double		_dLatitud=-999;
		private double		_dLongitud=-999;
		private string		_szReference = "";
		private string		_szCloudId = "";
		private int			_iOS = 0;

		// Parking space management
		private int			_iSpaceId = -1;

		/// <summary>
		/// Constructs a new msg02 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg02(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);
			
			//JTZ - 30/07/2015 : Añadido para intentar localizar la razón de los errores en M2/M4
			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg02:DoParseMessage]",LoggerSeverities.Debug);

			for(int i=0; i<C_MAX_COUPONS; i++)
			{
				_couponsId[i]=0;
			}

			bool bHasFechaMov = false;

			try
			{
				foreach (XmlNode n in _root.ChildNodes)
				{
					switch (n.Name)
					{
						case "m": _vehicleId = n.InnerText; break;
						case "y": _operationDefId = Convert.ToInt32(n.InnerText); break;
						case "o": _operationId = Convert.ToInt32(n.InnerText); break;
						case "a": _articleId = Convert.ToInt32(n.InnerText);break;
						case "ad": _articleDefId = Convert.ToInt32(n.InnerText); break;
						case "g": _groupId = Convert.ToInt32(n.InnerText); break;
						case "u": _unitId = Convert.ToInt32(n.InnerText); break;
						case "p": _paytypeDefId = Convert.ToInt32(n.InnerText); break;
						case "d":
							_date = OPS.Comm.Dtx.StringToDtx(n.InnerText);
							bHasFechaMov = true;
							break;
						case "d1": _dateIni = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
						case "d2": _dateEnd = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
						case "t": _time = Convert.ToInt32(n.InnerText); break;
						case "q":
							_quantity = Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "rq":
							_quantityReal = Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "qr":
							_quantityReturned = Convert.ToInt32(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "mui": _mobileUserId = Convert.ToInt32(n.InnerText); break;
							// CARD
							/*
							* case "tcs": _nStatus = Convert.ToInt32(n.InnerText); break;
							case "tci": _szTargetId = n.InnerText; break;
							case "tct": _nTargetType = Convert.ToInt32(n.InnerText); break;
							case "tcd": _dtCardDateResult = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
							*/
							// CFE - 11/06/05 - Tarjetas de crédito
						case "tn":	_szCCNumber		= n.InnerText;									break;
						case "td":	_dtExpirDate	= OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
						case "tm":  _szCCName		= n.InnerText;									break;
						case "ts":  _szCCCodServ	= n.InnerText;									break;
						case "tdd":  _szCCDiscData	= n.InnerText;									break;
						case "rtn":	_szRechargeCCNumber		= n.InnerText;									break;
						case "rtm": _szRechargeCCName		= n.InnerText;									break;


						case "pp":  _iPostPay		= Convert.ToInt32(n.InnerText); break;
						case "chi": _ulChipCardId	= Convert.ToUInt32(n.InnerText); break;
						case "chc":
							_dChipCardCredit = Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "om": _onlineMessage = Convert.ToInt32(n.InnerText); break;
						case "tcn": _ticketNumber = Convert.ToInt32(n.InnerText); break;
						case "bt":	_binType	=	Convert.ToInt32(n.InnerText);				break;
						case "vci1": _vaoCard1 = n.InnerText;break;
						case "vci2": _vaoCard2 = n.InnerText;break;
						case "vci3": _vaoCard3 = n.InnerText;break;
							// Additional credit card data (initially for Transax)
						case "trid": _szCCDTransId = n.InnerText; break;
						case "trcb": _szCCDCardName = n.InnerText; break;
						case "trpan": _szCCDCardNumber = n.InnerText; break;
						case "trexp": _szCCDExpDate = n.InnerText; break;
						case "tron": _ulCCDInvoice = Convert.ToUInt32(n.InnerText); break;
						case "trbtc": _szCCDBatch = n.InnerText; break;
						case "trot": _szCCDType = n.InnerText; break;
						case "tram":
							_dCCDQuantity = Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "trad": _szCCDResponseId = n.InnerText; break;
						case "tral": _szCCDResult = n.InnerText; break;
						case "trac": _szCCDResultCode = n.InnerText; break;
						case "coid": _coid = n.InnerText; break;
						case "pvis": _paytypeDefIdVis = Convert.ToInt32(n.InnerText); break;
						case "cp1": 	
							_couponsId[_iNumCoupons]= Convert.ToUInt32(n.InnerText);
							_iNumCoupons++;
							break;
						case "cp2": 	
							_couponsId[_iNumCoupons]= Convert.ToUInt32(n.InnerText);
							_iNumCoupons++;
							break;
						case "cp3": 	
							_couponsId[_iNumCoupons]= Convert.ToUInt32(n.InnerText);
							_iNumCoupons++;
							break;
						case "cp4": 	
							_couponsId[_iNumCoupons]=Convert.ToUInt32(n.InnerText);
							_iNumCoupons++;
							break;
						case "cp5": 	
							_couponsId[_iNumCoupons]= Convert.ToUInt32(n.InnerText);
							_iNumCoupons++;
							break;
							// Mobile payment data
						case "lt": 
							_dLatitud  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;				
						case "lg": 
							_dLongitud  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
							break;
						case "ref": _szReference = n.InnerText; break;
						case "cid": _szCloudId = n.InnerText; break;
						case "os": _iOS = Convert.ToInt32(n.InnerText); break;
						case "spcid": _iSpaceId = Convert.ToInt32(n.InnerText); break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.AddLog("[Msg02:DoParseMessage] - ERROR in parse",LoggerSeverities.Debug);
				logger.AddLog("[Msg02:DoParseMessage] " + ex.ToString(),LoggerSeverities.Debug);
				throw ex;
			}
			if (!bHasFechaMov) 
				_date = _dateIni;
			if (_time == -1 && _dateIni != DateTime.MinValue && _dateEnd != DateTime.MinValue)
			{
				TimeSpan ts = _dateEnd.Subtract(_dateIni);
				_time = (int)ts.TotalMinutes;
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
			ILogger logger = null;
			IDbTransaction tran=null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg02:Process]",LoggerSeverities.Debug);

			try
			{

				if (_operationDefId==OPERATIONS_GUARD_CLOCK_IN)
				{
					CmpClockInDB cmp = new CmpClockInDB();
					if(cmp.Insert( Convert.ToInt32(_vehicleId), _unitId, _date)<0)
					{
						if(logger != null)
							logger.AddLog("[Msg02:Process]:ERROR ON INSERT",LoggerSeverities.Debug);
						return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
					}
					else
					{
						if(logger != null)
							logger.AddLog("[Msg02:Process]: RESULT OK",LoggerSeverities.Debug);
						return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}

				}
				else if ((_operationDefId==OPERATIONS_UPLOCK_OPEN)||(_operationDefId==OPERATIONS_DOWNLOCK_OPEN))
				{

					OracleConnection oraDBConn=null;
					OracleCommand oraCmd=null;
					OracleCommand selCmd=null;

					try
					{
						logger.AddLog("[Msg02:Process]: UP OR DOWN LOCK OPENNING",LoggerSeverities.Info);
										
						Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
//						logger = DatabaseFactory.Logger;

						oraDBConn = (OracleConnection) d.GetNewConnection();
						oraDBConn.Open();
						tran = oraDBConn.BeginTransaction(IsolationLevel.Serializable);

						String selectUE = String.Format("select count(*) from USER_EVENTS where ue_uni_id={0} and ue_ope_id={1}",_unitId,_operationId);

						selCmd= new OracleCommand();
						selCmd.Connection=(OracleConnection)oraDBConn;
						selCmd.CommandText = selectUE;
						selCmd.Transaction=(OracleTransaction)tran;

						if (oraDBConn.State == System.Data.ConnectionState.Open)
						{
								int iNumRegs= Convert.ToInt32(selCmd.ExecuteScalar());

								if (iNumRegs==0)
								{


									String updateUE = String.Format("insert into USER_EVENTS (UE_DUE_ID, UE_UNI_ID, UE_DATE, UE_USER_ID, UE_OPE_ID) values "+
										"({0},{1}, to_date('{2}','hh24missddmmyy'),{3},{4})",
										_operationDefId,
										_unitId, 
										OPS.Comm.Dtx.DtxToString(_date),
										_ulChipCardId,
										_operationId);

									oraCmd= new OracleCommand();
									oraCmd.Connection=(OracleConnection)oraDBConn;
									oraCmd.CommandText = updateUE;
									oraCmd.Transaction=(OracleTransaction)tran;

									if (oraCmd.ExecuteNonQuery()!=1)
									{

										if (oraCmd!=null)
										{
											oraCmd.Dispose();
											oraCmd = null;
										}

										if (selCmd!=null)
										{
											selCmd.Dispose();
											selCmd = null;
										}

										RollbackTrans(tran);
										
										if(logger != null)
											logger.AddLog("[Msg02:Process]: Error executing sql "+updateUE,LoggerSeverities.Debug);

										return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
									}
								}
						}
						else
						{
							if (oraCmd!=null)
							{
								oraCmd.Dispose();
								oraCmd = null;
							}

							if (selCmd!=null)
							{
								selCmd.Dispose();
								selCmd = null;
							}

							RollbackTrans(tran);

							if(logger != null)
								logger.AddLog("[Msg02:Process]: Connection not opened",LoggerSeverities.Debug);

							return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}


						if (oraCmd!=null)
						{
							oraCmd.Dispose();
							oraCmd = null;
						}

						if (selCmd!=null)
						{
							selCmd.Dispose();
							selCmd = null;
						}

						CommitTrans(tran);
											
						if(logger != null)
							logger.AddLog("[Msg02:Process]: RESULT OK",LoggerSeverities.Debug);
						return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}
					catch (Exception exc)
					{
						if (oraCmd!=null)
						{
							oraCmd.Dispose();
							oraCmd = null;
						}

						if (selCmd!=null)
						{
							selCmd.Dispose();
							selCmd = null;
						}

						RollbackTrans(tran);

						if(logger != null)
							logger.AddLog("[Msg02:Process]" + exc.Message,LoggerSeverities.Debug);
						return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);;
					}
				}
				else if(_operationDefId==OPERATIONS_BILLREADER_REFUNDRECEIPT)
				{
					CmpBillReaderRefundsDB cmp = new CmpBillReaderRefundsDB();
					if(cmp.Insert( _unitId, _date, Convert.ToInt32(_quantity) )<0)
					{
						if(logger != null)
						logger.AddLog("[Msg02:Process]:ERROR ON INSERT",LoggerSeverities.Debug);
						return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
					}
					else
					{
						if(logger != null)
						logger.AddLog("[Msg02:Process]: RESULT OK",LoggerSeverities.Debug);
						return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}
				}
				else
				{

					if (_groupId == -1)		// If  no group <g> passed search the 1st physical parent...
					{
						_groupId = new CmpGroupsChildsDB().GetFirstPhysicalParent(_unitId);
						if (_groupId == -1)	// If no group found that is an error...
						{
							return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}
					}

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

					CmpOperationsDB cmp = new CmpOperationsDB();
					if (!Msg07.ListaNegra(logger, _szCCNumber, iBinFormat))
					{

						int nNewOperationID = 0;
						int iRealTime = -1;
						int iQuantity = -1;

						if ((_operationDefId == OPERATIONS_DEF_PARKING) || (_operationDefId == OPERATIONS_DEF_EXTENSION))
						{
							GetM2CompData(ref iRealTime, ref iQuantity);
						}
						else if (_operationDefId == OPERATIONS_DEF_REFUND)
						{
							GetM2CompData(ref iRealTime, ref iQuantity);
							_quantityReturned = iQuantity;
						}

						/// Returns 0 = 0K, -1 = ERR, 1 = OPERACION YA EXISTENTE
						int nInsOperRdo = cmp.InsertOperation(_operationDefId, _operationId, _articleId, _groupId, _unitId, _paytypeDefId, _date,
							_dateIni, _dateEnd, _time, _quantity, _vehicleId, _articleDefId, _mobileUserId, _iPostPay, _dChipCardCredit, _ulChipCardId, -1, -1, iRealTime, _quantityReturned, _onlineMessage, _ticketNumber, ref nNewOperationID, out tran);


						//if(nInsOperRdo==1)
						//{
						//	return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
						//}
						//else
						if ((nInsOperRdo == 0) && (_iPostPay == 1))
						//Antes era ( nInsOperRdo != -1 ), pero en el caso de ser 1, el método no le asigna un valor a trans, y por lo tanto las siguientes líneas dan una excepción
						{
							CFineManager oFineManager = new CFineManager();
							oFineManager.SetLogger(logger);

							oFineManager.SetDBTransaction(tran);
							oFineManager.RevokeFinesWithPostpay(nNewOperationID);

						}

						if (nInsOperRdo == 0)
						{


							if (!UpdatePaymentTypeVis(nNewOperationID, tran))
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (_iPostPay == 1)
							{
								CFineManager oFineManager = new CFineManager();
								oFineManager.SetLogger(logger);
								oFineManager.SetDBTransaction(tran);
								oFineManager.RevokeFinesWithPostpay(nNewOperationID);
							}

							if (!UpdateVAOCards(nNewOperationID, tran))
							{

								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (!UpdateCOID(nNewOperationID, tran))
							{

								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}


							if (_iNumCoupons > 0)
							{
								CmpMoneyOffCoupons cmpCoupons = new CmpMoneyOffCoupons();
								for (int i = 0; i < _iNumCoupons; i++)
								{
									if (cmpCoupons.SetCouponAsUsed(tran, _couponsId[i], _date, _vehicleId, _paytypeDefId) <= 0)
									{
										RollbackTrans(tran);
										return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
									}
									if (!UpdateMoneyOffDiscount(nNewOperationID, i + 1, _couponsId[i], tran))
									{
										RollbackTrans(tran);
										return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
									}
								}
								if (!UpdateValueVis(nNewOperationID, _quantityReal, tran))
								{
									RollbackTrans(tran);
									return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
								}

							}

							if (!InsertGPSPosn(nNewOperationID, tran))
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (!UpdateReference(nNewOperationID, tran))
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (!UpdateCloudData(nNewOperationID, tran))
							{
								RollbackTrans(tran);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (!UpdateSpaceInfo(nNewOperationID, tran))
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
										logger.AddLog("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);

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

										if (_szRechargeCCNumber != "")
										{
											if (logger != null)
												logger.AddLog("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);
											_nStatus = STATUS_INSERT;
											if (cmpCreditCard.Insert(tran, nNewOperationID, _szRechargeCCNumber, _szRechargeCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
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


									}
									else if (iBinFormat == Msg07.DEF_BIN_FORMAT_EMV_TAS && _szCCNumber == "TRANSACTION_ID")
									{
										int iTransId = -1;
										if (cmpCreditCardsTransactionsDB.InsertCommitTrans(tran, _szCCName, _date, nNewOperationID, Convert.ToInt32(_quantity), _unitId, _vehicleId, out iTransId) < 0)
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
												logger.AddLog("[Msg02:Process]:ERROR ON INSERT", LoggerSeverities.Debug);
											return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
										}
										else
										{
											if (logger != null)
												logger.AddLog("[Msg02:Process]: RESULT OK", LoggerSeverities.Debug);
										}
									}
								}
								else if (_szRechargeCCNumber != "")
								{
									if (logger != null)
										logger.AddLog("[Msg02:Process]: Operation WITH Card Id", LoggerSeverities.Debug);
									_nStatus = STATUS_INSERT;
									if (cmpCreditCard.Insert(tran, nNewOperationID, _szRechargeCCNumber, _szRechargeCCName, _dtExpirDate, _nStatus, _szCCCodServ, _szCCDiscData) < 0)
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
								else
								{
									if (logger != null)
										logger.AddLog("[Msg02:Process]: Operation WITHOUT Card Id", LoggerSeverities.Debug);
								}
							}
							catch (Exception exc)
							{
								if (logger != null)
									logger.AddLog("[Msg02:Process]" + exc.Message, LoggerSeverities.Debug);
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS); ;
							}
						}
						else if (nInsOperRdo == 1)
						{
							//RollbackTrans(tran);
							if (logger != null)
								logger.AddLog("[Msg02:Process]: Operation already exists in DB", LoggerSeverities.Debug);
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
						RollbackTrans(tran);
						if (!InsertFraudMsgs(logger))
						{
							return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}
					}
					#endregion

					CommitTrans(tran);
					return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
			
			}
			catch (Exception e)
			{
				if (logger != null)
					logger.AddLog("[Msg02:Process] EXCEPTION: " + e.ToString() ,LoggerSeverities.Debug);
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
		}




		private bool GetM2CompData(ref int iResRealTime, ref int iResQuantity)
		{
			bool bRdo=true;
			int iResult=-1;
			int iQuantity=-1;
			int iRealTime=-1;

			try
			{
				string m1Tel;

				m1Tel="<m1 id=\""+_msgId+"\">";
				m1Tel+="<m>"+_vehicleId+"</m>";
				m1Tel+="<g>"+_groupId.ToString()+"</g>";
				m1Tel+="<d>"+OPS.Comm.Dtx.DtxToString(_dateIni)+"</d>";
				m1Tel+="<d2>"+OPS.Comm.Dtx.DtxToString(_dateEnd)+"</d2>";
				m1Tel+="<ad>"+_articleDefId.ToString()+"</ad>";
				m1Tel+="<u>"+_unitId.ToString()+"</u>";
				m1Tel+="<o>1</o><rmon>0</rmon></m1>";				
				
				CS_M1 pCS_M1 = new CS_M1();
				pCS_M1.StrIn = m1Tel;
				pCS_M1.ApplyHistory=false;
				pCS_M1.UseDefaultArticleDef=false;


				if(pCS_M1.Exectue()!= CS_M1.C_RES_OK)
				{
					if(m_logger != null)
						m_logger.AddLog("[Msg02]:Process Parsing " +  "Error Execute",LoggerSeverities.Debug);
					bRdo=false;
					return bRdo;
				}

				string m1Res=pCS_M1.StrOutM50.ToString();
			
				if(m_logger != null)
					m_logger.AddLog("[Msg02]:Process Parsing : Result" +  m1Res,LoggerSeverities.Debug);
			
				XmlDocument xmlM1Res= new XmlDocument();
				xmlM1Res.LoadXml(m1Res);

				XmlNode act;
	

				IEnumerator ienum = xmlM1Res.ChildNodes.Item(0).GetEnumerator();  
		
				while (ienum.MoveNext()) 
				{   
					act = (XmlNode) ienum.Current;
					switch(act.Name)
					{

						case "r":
							iResult=int.Parse(act.InnerText);
							break;
						case "q2":
							iQuantity=int.Parse(act.InnerText);
							break;
						case "rot":
							iRealTime=int.Parse(act.InnerText);
							break;
						default:
							break;

					}
				}

				if (iResult>0)
				{
					if (iQuantity>=0)
					{
						iResQuantity=iQuantity;
					}
					if (iRealTime>=0)
					{
						iResRealTime=iRealTime;
					}
				}
				else
				{
					bRdo=false;
				}
			}
			catch
			{
				bRdo=false;
			}

			return bRdo;

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
					String strSQL = String.Format("insert into MSGS_XML_FRAUD_OPERATIONS (MXF_UNI_ID,MXF_MOVDATE,MXF_NUMBER,MXF_NAME,MXF_XPRTN_DATE,MXF_VEHICLEID,MXF_XML) values "+
						"({0},to_date('{1}','hh24missddmmyy'),'{2}','{3}',to_date('{4}','hh24missddmmyy'),'{5}','{6}')", 
						_unitId, 
						OPS.Comm.Dtx.DtxToString(_date),
						_szCCNumber,
						_szCCName,
						OPS.Comm.Dtx.DtxToString(_dtExpirDate),
						_vehicleId,
						_root.OuterXml);

					logger.AddLog(string.Format("[Msg02:Process]: Credit Card {0} is in blacklist",_szCCNumber),LoggerSeverities.Debug);
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

		bool UpdateVAOCards(int iOperationID, IDbTransaction tran)
		{

			bool bOK=true;

			if ((_vaoCard1.Length>0)||(_vaoCard2.Length>0)||(_vaoCard3.Length>0))
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
						sqlQuery.AppendFormat(	" update operations o "+
												"set o.ope_vaocard1 = '{0}', o.ope_vaocard2 = '{1}', o.ope_vaocard3 = '{2}' "+
												"where ope_id = {3}",_vaoCard1,_vaoCard2,_vaoCard3,iOperationID);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
							
						

					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: "+e.Message,LoggerSeverities.Error);
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


		bool UpdateMoneyOffDiscount(int iOperationID, int iCoupon, uint iCouponId,IDbTransaction tran)
		{

			bool bOK=true;

			


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
					sqlQuery.AppendFormat(	" update operations o "+
						"set o.OPE_COUP_ID_{0} = {1} "+
						"where ope_id = {2}",iCoupon,iCouponId,iOperationID);

					oraCmd.CommandText = sqlQuery.ToString();

					oraCmd.ExecuteNonQuery();
						
					

				}
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: "+e.Message,LoggerSeverities.Error);
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


			return bOK;
		}


		bool UpdateValueVis(int iOperationID,double dValueVis,IDbTransaction tran)
		{

			bool bOK=true;

			


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
					sqlQuery.AppendFormat(	" update operations o "+
						"set o.OPE_VALUE_VIS = {0} "+
						"where ope_id = {1}",dValueVis,iOperationID);

					oraCmd.CommandText = sqlQuery.ToString();

					oraCmd.ExecuteNonQuery();
						
					

				}
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: "+e.Message,LoggerSeverities.Error);
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


			return bOK;
		}



		bool UpdatePaymentTypeVis(int iOperationID, IDbTransaction tran)
		{

			bool bOK=true;

			if (_paytypeDefIdVis!=-1)
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
						sqlQuery.AppendFormat(	" update operations o "+
							"set o.ope_dpay_id_vis = {0} "+
							"where ope_id = {1}",_paytypeDefIdVis,iOperationID);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
							
						

					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:UpdateVAOCards]: Excepcion: "+e.Message,LoggerSeverities.Error);
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

		bool UpdateCOID(int iOperationID, IDbTransaction tran)
		{

			bool bOK=true;

			if ((_coid.Length>0))
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
						sqlQuery.AppendFormat(	" update operations o "+
							"set o.OPE_CAMOP_ID_ENTRY = {1} "+
							"where ope_id = {0}",iOperationID,_coid);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
							
						

					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:UpdateCOID]: Excepcion: "+e.Message,LoggerSeverities.Error);
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

		bool InsertGPSPosn(int iOperationID, IDbTransaction tran)
		{
			bool bOK=true;

			if ( _dLatitud != -999 && _dLongitud != -999 )
			{
				CultureInfo culture = new CultureInfo("", false);
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
						sqlQuery.AppendFormat(	" update operations o " +
							"set o.ope_latitude = {0}, o.ope_longitud = {1} " +
							"where ope_id = {2}", Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat), 
							Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat), iOperationID);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:InsertGPSPosn]: Excepcion: "+e.Message,LoggerSeverities.Error);
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

		bool UpdateReference(int iOperationID, IDbTransaction tran)
		{
			bool bOK=true;

			if ( _szReference.Length > 0 )
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
						sqlQuery.AppendFormat(	" update operations o " +
							"set o.ope_reference = '{0}' " +
							"where ope_id = {1}", _szReference, iOperationID);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:UpdateReference]: Excepcion: "+e.Message,LoggerSeverities.Error);
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
					logger.AddLog("[Msg02:UpdateCloudData]: Excepcion: "+e.Message,LoggerSeverities.Error);
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

		bool UpdateSpaceInfo(int iOperationID, IDbTransaction tran)
		{
			bool bOK=true;

			if ( _iSpaceId > 0 )
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
						sqlQuery.AppendFormat(	" update operations o " +
							"set o.ope_ps_id = {0} " +
							"where ope_id = {1}", _iSpaceId, iOperationID);

						oraCmd.CommandText = sqlQuery.ToString();

						oraCmd.ExecuteNonQuery();
					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg02:UpdateSpaceInfo]: Excepcion: "+e.Message,LoggerSeverities.Error);
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
	}
}
