using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
using Oracle.ManagedDataAccess.Client;


namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to handle de m7 message.
	/// </summary>
	public sealed class Msg07 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m7)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg07()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m7"; } }
		#endregion

		#region Variables, creation and parsing


		public const int DEF_CREDIT_CARD_UNDEF = -999;
		public const int DEF_CREDIT_CARD_OK = 1;
		public const int DEF_CREDIT_CARD_NOK = 0;
		public const int DEF_CREDIT_CARD_BL = -1;
		public const int DEF_CREDIT_CARD_BIN_NOT_FOUND = -2;
		public const int DEF_CREDIT_CARD_NO_BALANCE = -3;
		public const int DEF_CREDIT_CARD_CHECK_NO_POSIBLE = -4;
		public const int DEF_CREDIT_CARD_QUANTITY_TOO_LARGE = -5;
		public const int DEF_CREDIT_CARD_QUANTITY_TOO_SMALL = -6;
		public const int DEF_CREDIT_CARD_INVALID_CARD = -7;

		public const int DEF_BIN_FORMAT_4B =1;
		public const int DEF_BIN_FORMAT_LA_CAIXA =2;
		public const int DEF_BIN_FORMAT_TRANZCOM =3;
		public const int DEF_BIN_FORMAT_CARDEASEXML =4;
		public const int DEF_BIN_FORMAT_EMV_CREDITCALL =5;
		public const int DEF_BIN_FORMAT_EMV_TAS =6;
		public const int DEF_BIN_FORMAT_TRANSAX =7;
		public const int DEF_BIN_FORMAT_TRANSAX_ONLINE =8;



		public const int DEF_CARD_TYPE_CREDIT_CARD =0;
		public const int DEF_CARD_TYPE_CHIP_CARD =1;

		public const int DEF_CARD_BLACKLIST_TYPE_CHIPCARD =3;


		

		private int			_unit;
		private DateTime	_date;
		private DateTime	_dtExpirDate	= DateTime.MinValue;
		private string		_szCCNumber		= "";
		private string		_szCCCodServ		= "";
		private int			_cardType = DEF_CARD_TYPE_CREDIT_CARD;
		private int			_binType = -1;


		/// <summary>
		/// Constructs a new Msg07 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg07(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "tn":	_szCCNumber		= n.InnerText;									break;
					case "td":	_dtExpirDate	= OPS.Comm.Dtx.StringToDtx(n.InnerText);	break;
					case "ts":  _szCCCodServ	= n.InnerText;									break;
					case "ct":  _cardType=Convert.ToInt32(n.InnerText); break;
					case "bt":	_binType	=	Convert.ToInt32(n.InnerText);				break;
						


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
			int responseResult=DEF_CREDIT_CARD_UNDEF;
			int iInfLimit=DEF_CREDIT_CARD_UNDEF;
			int iSupLimit=DEF_CREDIT_CARD_UNDEF;
			int iBinInfLimit=DEF_CREDIT_CARD_UNDEF;
			int iBinSupLimit=DEF_CREDIT_CARD_UNDEF;

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;

				if (_cardType==DEF_CARD_TYPE_CHIP_CARD)
				{
					if (IsCardInBlackList(logger,_szCCNumber))
					{
						responseResult = DEF_CREDIT_CARD_BL;
					}
					else
					{
						responseResult = DEF_CREDIT_CARD_OK;
					}
				}
				else if (_cardType==DEF_CARD_TYPE_CREDIT_CARD)
				{

					if (IsCardInBlackList(logger,_szCCNumber))
					{
						responseResult = DEF_CREDIT_CARD_BL;
					}
					else
					{

						CmpParametersDB cmpParam = new CmpParametersDB();
						string strCheckBines = cmpParam.GetParameter("P_CHECK_BINES");
						string strBinFormat = cmpParam.GetParameter("P_BIN_FORMAT");					
						string strMaxMoneyCardDay = cmpParam.GetParameter("P_MAX_MONEY_CARD_DAY");
						bool bCheckBines=true;
						int	 iBinFormat=DEF_BIN_FORMAT_4B;
						int  iMaxMoneyCardDay=DEF_CREDIT_CARD_UNDEF;

						if (strCheckBines=="")
						{
							bCheckBines=true;
						}
						else
						{
							bCheckBines=(strCheckBines!="0");
						}


						if (_binType==-1)
						{				
							if (strBinFormat!="")
							{
								iBinFormat=Convert.ToInt32(strBinFormat);
							}
						}
						else
						{
							iBinFormat=_binType;						
						}

						bCheckBines = (bCheckBines&&
							(iBinFormat!=DEF_BIN_FORMAT_CARDEASEXML)&&
							(iBinFormat!=DEF_BIN_FORMAT_EMV_CREDITCALL)&&
							(iBinFormat!=DEF_BIN_FORMAT_EMV_TAS)&&
							(iBinFormat!=DEF_BIN_FORMAT_TRANSAX));

						if (strMaxMoneyCardDay!="")
						{
							iMaxMoneyCardDay=Convert.ToInt32(strMaxMoneyCardDay);;
						}
						

						if (ListaNegra(logger,_szCCNumber,iBinFormat))
						{
							responseResult=DEF_CREDIT_CARD_BL;
						}
						else
						{

							if (bCheckBines)
							{

								switch(iBinFormat)
								{
									case DEF_BIN_FORMAT_LA_CAIXA:
										
										if (!BinesLaCaixa(logger,ref iBinInfLimit, ref iBinSupLimit))
										{
											responseResult = DEF_CREDIT_CARD_BIN_NOT_FOUND;
										}
										else
										{
											responseResult = DEF_CREDIT_CARD_OK;
										}
										break;			
									case DEF_BIN_FORMAT_4B:
									default:

										if (!Bines4B(logger))
										{
											responseResult = DEF_CREDIT_CARD_BIN_NOT_FOUND;
										}
										else
										{
											responseResult = DEF_CREDIT_CARD_OK;
										}

										break;

								}
							}
							else if (!bCheckBines)
							{
								responseResult = DEF_CREDIT_CARD_OK;
							}
						}


						if (responseResult == DEF_CREDIT_CARD_OK)
						{

							if (iMaxMoneyCardDay>0)
							{
								iMaxMoneyCardDay -= SaldoConsumidoHoy(logger);

								if (iMaxMoneyCardDay<0)
									iMaxMoneyCardDay=0;
							}

							iInfLimit=iBinInfLimit;
							iSupLimit=iBinSupLimit;

							if (iSupLimit>=0)
							{
								if (iMaxMoneyCardDay>=0)
								{
									iSupLimit = Math.Min(iSupLimit,iMaxMoneyCardDay);
								}
								
							}
							else
							{
								if (iMaxMoneyCardDay>=0)
								{
									iSupLimit = iMaxMoneyCardDay;
								}
							}

							if ((iSupLimit>=0)&&(iInfLimit<0))
							{
								iInfLimit=0;
							}


							if ((iSupLimit>=0)&&(iInfLimit>=0)&&(iInfLimit>iSupLimit))
							{
								responseResult = DEF_CREDIT_CARD_NO_BALANCE;
							}
							else if ((iInfLimit>=0)&&(iSupLimit==0))
							{
								responseResult = DEF_CREDIT_CARD_NO_BALANCE;
							}
						}
					}
				}
			}				
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg07:Process]: Error: "+e.Message,LoggerSeverities.Error);
				ret = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				return ret;
			}



			// Build response
			string response = "<r>" + Convert.ToString(responseResult) + "</r>";
			if (responseResult == DEF_CREDIT_CARD_OK)
			{
				CultureInfo culture = new CultureInfo("", false);
				response += "<q1>" + Convert.ToString(iInfLimit, (IFormatProvider)culture.NumberFormat) + "</q1>";
				response += "<q2>" + Convert.ToString(iSupLimit, (IFormatProvider)culture.NumberFormat) + "</q2>";
			}

			logger.AddLog("[Msg07:Process]: Response: "+response,LoggerSeverities.Debug);
			ret.Add(new AckMessage(_msgId, response).ToString());
			return ret;

		}


		static public bool ListaNegra(ILogger logger,string szCCNumber, int iBinFormat)
		{
			bool bRet=false;
			try
			{
				if (szCCNumber.Length>0)
				{					
					if ((iBinFormat!=DEF_BIN_FORMAT_CARDEASEXML)&&
						(iBinFormat!=DEF_BIN_FORMAT_EMV_CREDITCALL)&&
						(iBinFormat!=DEF_BIN_FORMAT_EMV_TAS)&&
						(iBinFormat!=DEF_BIN_FORMAT_TRANSAX))
					{
						Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
						System.Data.IDbConnection DBCon=d.GetNewConnection();
						OracleCommand cmd = new OracleCommand();
						try
						{
							DBCon.Open();
							cmd.Connection= (OracleConnection)DBCon;
							cmd.CommandText = String.Format("select count(*) "+
								"from  blacklist_cards b "+
								"where b.bc_number = '{0}' ", szCCNumber );
							if (Convert.ToInt32(cmd.ExecuteScalar())>0)
							{
								if (logger!=null)
								{
									logger.AddLog("[Msg07:ListaNegra]: Credit Card "+szCCNumber+ " is in blacklist",LoggerSeverities.Debug);
								}
								bRet=true;
							}
						}
						catch
						{
						}
						finally
						{
							cmd.Dispose();
							DBCon.Close();
							DBCon.Dispose();
						}
					}
				}

			}
			catch
			{
				
			}
				
			return bRet;
		}
		
		private bool Bines4B(ILogger logger)
		{
			bool bRet=false;
			try
			{
				if (_szCCNumber.Length>=6)
				{
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					OracleCommand cmd = new OracleCommand();
					try
					{
						DBCon.Open();
						cmd.Connection= (OracleConnection)DBCon;
						cmd.CommandText = String.Format("select count(*) from ebin_cards " +
							"where substr(ec_bin, 5, 6) = '{0}' ", _szCCNumber.Substring(0,6) );
						if (Convert.ToInt32(cmd.ExecuteScalar())==0)
						{
							if (logger!=null)
							{
								logger.AddLog("[Msg07:Process]: Credit Card "+_szCCNumber+ " isn't in bin list",LoggerSeverities.Debug);
							}
						}
						else
						{							 
							bRet=true;
						}
					}
					catch
					{
					}
					finally
					{
						cmd.Dispose();
						DBCon.Close();
						DBCon.Dispose();
					}
				}

			}
			catch
			{
				
			}
				
			return bRet;
		}


		private bool BinesLaCaixa(ILogger logger, ref int iInfLimit, ref int iSupLimit)
		{
			bool bRet=false;
			try
			{
				if (_szCCNumber.Length>=6)
				{
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					OracleCommand cmd = new OracleCommand();
					OracleDataReader	dr		= null;


					try
					{
						DBCon.Open();
						cmd.Connection= (OracleConnection)DBCon;
						string strBin = _szCCNumber.Substring(0,6);
						bool bEnd=false;
						int iBinNumChars=6;
						string strAction;


						do
						{
							strAction = "";
							cmd.CommandText = String.Format("select substr(ec_bin, 1, 6) BIN, "+
															"substr(ec_bin, 7, 3) COD_SERV, "+
															"substr(ec_bin, 10, 3) INF_LIM, "+
															"substr(ec_bin, 13, 3) SUP_LIM, "+
															"substr(ec_bin, 17, 1) ACTION "+
															"from ebin_cards t "+
															"where substr(ec_bin, 1, 6)='{0}' "+
															"   and substr(ec_bin, 7, 3)='{1}'", strBin ,_szCCCodServ);


							dr = cmd.ExecuteReader();

							if (dr.Read())
							{
								strAction = dr.GetString(dr.GetOrdinal("ACTION"));
								if (strAction=="A")
								{
									iInfLimit = Convert.ToInt32(dr.GetString(dr.GetOrdinal("INF_LIM")))*100;
									iSupLimit = Convert.ToInt32(dr.GetString(dr.GetOrdinal("SUP_LIM")))*100;
									if (iInfLimit>iSupLimit)
									{
										int iTemp=iInfLimit;
										iInfLimit=iSupLimit;
										iSupLimit=iTemp;
									}

								}

							}

							dr.Close();
							dr=null;


							if (strAction=="")
							{

								cmd.CommandText = String.Format("select substr(ec_bin, 1, 6) BIN, "+
									"substr(ec_bin, 7, 3) COD_SERV, "+
									"substr(ec_bin, 10, 3) INF_LIM, "+
									"substr(ec_bin, 13, 3) SUP_LIM, "+
									"substr(ec_bin, 17, 1) ACTION "+
									"from ebin_cards t "+
									"where substr(ec_bin, 1, 6)='{0}' "+
									"   and substr(ec_bin, 7, 3)='{1}'", strBin ,"***");


								dr = cmd.ExecuteReader();

								if (dr.Read())
								{
									strAction = dr.GetString(dr.GetOrdinal("ACTION"));
									if (strAction=="A")
									{
										iInfLimit = Convert.ToInt32(dr.GetString(dr.GetOrdinal("INF_LIM")))*100;
										iSupLimit = Convert.ToInt32(dr.GetString(dr.GetOrdinal("SUP_LIM")))*100;
										if (iInfLimit>iSupLimit)
										{
											int iTemp=iInfLimit;
											iInfLimit=iSupLimit;
											iSupLimit=iTemp;
										}

									}

								}

								dr.Close();
								dr=null;
							}
							
							bEnd= (strAction!="");

							if (!bEnd)
							{
								iBinNumChars--;
								if (iBinNumChars>0)
								{
									strBin=strBin.Substring(0,iBinNumChars).PadRight(6,'*');
								}
				
							}

						}
						while ((!bEnd)&&(iBinNumChars>0));	

						bRet=(strAction=="A");

						if((strAction=="R")||(strAction=="C"))
						{
							if (logger!=null)
							{
								logger.AddLog("[Msg07:Process]: Credit Card "+_szCCNumber+ " rejected in bin list",LoggerSeverities.Debug);
							}
						}
						else if((strAction==""))
						{
							if (logger!=null)
							{
								logger.AddLog("[Msg07:Process]: Credit Card "+_szCCNumber+ " isn't in bin list",LoggerSeverities.Debug);
							}
						}					
					}
					catch
					{
					}
					finally
					{
						if (dr!=null)
						{
							dr.Close();
							dr=null;
						}
						cmd.Dispose();
						DBCon.Close();
						DBCon.Dispose();
					}
									
				}

			}
			catch
			{
				
			}
				
			return bRet;
		}

		private int SaldoConsumidoHoy(ILogger logger)
		{
			int iRet=0;
			try
			{
				if (_szCCNumber.Length>0)
				{
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					OracleCommand cmd = new OracleCommand();
					try
					{
						DBCon.Open();
						cmd.Connection= (OracleConnection)DBCon;
						cmd.CommandText = String.Format("select SUM(OPE_VALUE) "+
													  "from operations o, credit_cards cc "+
													  "where o.ope_id = cc.cc_ope_id "+
													  " and pkg_crypto.crypt('{0}', to_char(cc_id)) = "+
													  "		replace(cc_number, '%&!()=', '') "+
													  " and to_char(ope_movdate, 'ddmmyy') = '{1}'", _szCCNumber, OPS.Comm.Dtx.DtxToString(_date).Substring(6,6));



						iRet = Convert.ToInt32(cmd.ExecuteScalar());
					}
					catch
					{
						iRet=0;
					}									
					finally
					{
						cmd.Dispose();
						DBCon.Close();
						DBCon.Dispose();
					}
				}

			}
			catch
			{
				
			}
				
			return iRet;
		}

		static public bool IsCardInBlackList(ILogger logger,string strCardNumber)
		{
			bool bRet=false;
			try
			{
				if (strCardNumber.Length>0)
				{
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					OracleCommand cmd = new OracleCommand();
					try
					{
						DBCon.Open();
						cmd.Connection= (OracleConnection)DBCon;
						cmd.CommandText = String.Format("select count(*) "+
														"from black_lists t " +
														"where blis_dblis_id = {0} "+
														"and TRIM(blis_value) = '{1}' "+
														"and blis_Valid = 1 "+
														"and blis_deleted = 0",DEF_CARD_BLACKLIST_TYPE_CHIPCARD, strCardNumber.Trim() );
						if (Convert.ToInt32(cmd.ExecuteScalar())>0)
						{
							if (logger!=null)
							{
								logger.AddLog("[Msg07:IsCardInBlackList]: Card "+strCardNumber+ " is in blacklist",LoggerSeverities.Debug);
							}
							bRet=true;
						}
					}
					catch
					{
					}
					finally
					{
						cmd.Dispose();
						DBCon.Close();
						DBCon.Dispose();
					}
				}

			}
			catch
			{
				
			}
				
			return bRet;
		}



		#endregion
	}
}
