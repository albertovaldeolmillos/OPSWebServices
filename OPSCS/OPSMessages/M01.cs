using System;
using System.Collections.Specialized;
using System.Collections;
using System.Data;
using System.Text;
using System.Xml;
//using OTS.Framework.Collections;
using OPS.Components;
using OPS.Components.Data;
using OPS.Comm;
using CS_OPS_TesM1;
////using Oracle.DataAccess.Client;
using System.IO;
using Oracle.ManagedDataAccess.Types;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Types;

namespace OPS.Comm.Becs.Messages
{	
	/// <summary>
	/// This is the message that a PDM sends to the CC when asking for a parking
	/// </summary>
	internal sealed class Msg01 : MsgReceived, IRecvMessage
	{
		static int  C_UNDEFINED_VALUE = -999;
		static int  C_MAX_COUPONS=5;
		static int  C_DEFAULT_RESIDENT_MINIMUM_PERIOD_BETWEEN_RENEWALS=62;
		#region DefinedRootTag (m1)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m1"; } }
		#endregion

		#region Static stuff

		// Static "constants"
		private static int OPERATIONS_DEF_PARKING;
		private static int OPERATIONS_DEF_EXTENSION;
		private static int ARTICLE_TYPE_DEFAULT;
		private static int ARTICLE_TYPE_ROTACION;
		private static int ARTICLE_TYPE_VIP;
		private static int STATUS_OK;
		private static int STATUS_OUT_OF_SALES;
		private static int STATUS_OUT_OF_SERVICE;
		// pOutResults (all vars are prepended with error, althougth not all are errors ^_^)
		private static int ERROR_NOERROR;
		private static int ERROR_NOTVALIDFORARTICLE;
		private static int ERROR_operationDefIdNOTVALID;
		private static int ERROR_BLACKVEH;
		private static int ERROR_BLACKUSU;
		private static int ERROR_SYS_OFF;
		private static int ERROR_SYS_NO_SALES;
		private static int ERROR_NOREENTER;
		private static int ERROR_ARTKO;
		private static int BLIS_DBLIS_ID_VEHICLE;
		private static int BLIS_DBLIS_ID_USER;

		private static int M1_CARD_ALREADY_USED=-14; // User Card Already used in a current parking
		private static int M1_NO_ROOM_FOR_PARK=-15; // In a controlled space of parking there is no room for another car
		private static int M1_PLATE_NOT_FOUND=-16; // In a camera controlled entry plate not found in entry list
		

		//CHEK_ARTICLE_DEF (MOUCHEL SYSTEM)
		private static int CAD_UNDEFINED=-1; 
		private static int CAD_LOOK_FOR_PLATES=1; 
		private static int CAD_CHANGE_PLATES=2; 


		private static int CAD_OPERATION_ENTRY = 0;
		private static int CAD_OPERATION_EXIT = 2;
		//State: 10: Waiting Ticket; 20: Ticket Issued; 30: Entry Penalty Issued; 40: Ticket Issued with previous penalty; 50: Exit Penalty Issued; 60: Entry and Exit Penalty Issued; 70: Waiting Exit, 80: Exit Confirmed, 90: Exit without operation';
		//State: 10: Waiting Ticket; 20: Ticket Issued;  30: Exited without ticket: 40: Entry Penalty Issued; 50:Entry Penalty Issued and Exited without ticket;  60: Ticket Issued with previous penalty; 70: Exit Penalty Issued; 80: Entry and Exit Penalty Issued; 90: Waiting Exit, 100: Exit Confirmed, 110: Exit without operation';
		private static int CAD_STATE_WAITING_TICKET = 10;
		private static int CAD_STATE_TICKET_ISSUED = 20;
		private static int CAD_STATE_EXIT_WITHOUT_TICKET = 30;
		private static int CAD_STATE_ENTRY_PENALTY_ISSUED = 40;
		private static int CAD_STATE_ENTRY_PENALTY_ISSUED_EXIT_WITHOUT_TICKET = 50;
		private static int CAD_STATE_ENTRY_PENALTY_ISSUED_AND_TICKET_ISSUED = 60;
		private static int CAD_STATE_EXIT_PENALTY_ISSUED = 70;
		private static int CAD_STATE_ENTRY_PENALTY_ISSUED_EXIT_PENALTY_ISSUED = 80;
		private static int CAD_STATE_WAITING_EXIT = 90;
		private static int CAD_STATE_EXIT_CONFIRMED = 100;
		private static int CAD_STATE_EXIT_WITHOUT_OPERATION = 110;

		// Static variables
		private static int[] _constraints;

		internal const int  CRC_TABLE_LEN=256;
		static	 uint [] gs_plCrc32Table=null;

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg01()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			
			OPERATIONS_DEF_PARKING = (int)appSettings.GetValue ("OperationsDef.Parking", typeof(int));
			OPERATIONS_DEF_EXTENSION = (int)appSettings.GetValue ("OperationsDef.Extension", typeof(int));

			ARTICLE_TYPE_DEFAULT = (int)appSettings.GetValue ("ArticleType.Default", typeof(int));
			ARTICLE_TYPE_ROTACION = (int) appSettings.GetValue ("ArticleType.Rotacion", typeof(int));
			ARTICLE_TYPE_VIP = (int) appSettings.GetValue ("ArticleType.Vip", typeof(int));
			
			STATUS_OK =  (int)appSettings.GetValue ("Status.OK", typeof(int));
			STATUS_OUT_OF_SALES = (int)appSettings.GetValue ("Status.OUT_OF_SALES", typeof(int)); 
			STATUS_OUT_OF_SERVICE = (int)appSettings.GetValue ("Status.OUT_OF_SERVICE", typeof(int)); 

			// Load pOut results
			ERROR_NOTVALIDFORARTICLE = (int)appSettings.GetValue ("M01.ErrorCodes.NoValidaPorArticulo", typeof(int));
			ERROR_operationDefIdNOTVALID = (int)appSettings.GetValue ("M01.ErrorCodes.NoValida", typeof(int));
			ERROR_NOERROR =  (int)appSettings.GetValue("M01.ErrorCodes.NoError", typeof(int));
			ERROR_BLACKVEH = (int)appSettings.GetValue("M01.ErrorCodes.BlackVeh", typeof(int));
			ERROR_BLACKUSU = (int)appSettings.GetValue("M01.ErrorCodes.BlackUsu", typeof(int));
			ERROR_SYS_NO_SALES = (int)appSettings.GetValue("M01.ErrorCodes.FueraVentas", typeof(int));
			ERROR_SYS_OFF  = (int)appSettings.GetValue("M01.ErrorCodes.FueraServicio", typeof(int));
			ERROR_NOREENTER = (int)appSettings.GetValue("M01.ErrorCodes.TiempoReentradaNoSuperado", typeof(int));
			ERROR_ARTKO = (int)appSettings.GetValue("M01.ErrorCodes.ArtKO", typeof(int));

			BLIS_DBLIS_ID_VEHICLE = (int)appSettings.GetValue("BLIS_DBLIS_ID.Vehicle",typeof(int));
			BLIS_DBLIS_ID_USER = (int)appSettings.GetValue("BLIS_DBLIS_ID.User",typeof(int));

			_constraints = new int[5];
			_constraints[0] = (int)appSettings.GetValue("ConstraintsDef.MaxEstancia", typeof(int));
			_constraints[1] = (int)appSettings.GetValue("ConstraintsDef.MinReentrada", typeof(int));
			_constraints[2] = (int)appSettings.GetValue("ConstraintsDef.TiempoCortesia", typeof(int));
			_constraints[3] = (int)appSettings.GetValue("ConstraintsDef.MaxImport", typeof(int));
			_constraints[4] = (int)appSettings.GetValue("ConstraintsDef.MinImport", typeof(int));

		}

		#endregion

		#region Variables, creation and parsing

		// Data from the message
		private string _vehicleId = null;
		private DateTime _date;
		private DateTime _dateMax=DateTime.MinValue;
		private int _unitId;
		private int _operationDefId;
		private int _articleId = -1;
		private int _articleDefId;
		private int _groupId;
		private int m_nMaxImport = C_UNDEFINED_VALUE;
		private string _vaoCard1="";
		private string _vaoCard2="";
		private string _vaoCard3="";
		private int _cad=CAD_UNDEFINED; //Check Article def (Mouchel cameras)
		private string _coid=""; //CAMERA OPERATION ID
		private int	_MinEqMax=0;
		private string[] _couponsCode= new string[C_MAX_COUPONS];
		private int _iNumCoupons=0;
		private uint[] _ReturnCouponsId= new uint[C_MAX_COUPONS];
		private int[] _ReturnCouponsError= new int[C_MAX_COUPONS];
		private int _TotalNumFreeMinutes=0;
		private int _TotalNumFreeCents=0;
		private int _lang_rot=0;
		private int _CalculateTimeSteps = 0;
		private int _CalculateQuantitySteps = 0;
		private int _CalculateSteps_StepValue = 0;
		private string _computeDllPath = "";



		// Support members
		private bool _useUnit;

		/// <summary>
		/// Constructs a new Msg01 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg01(XmlDocument msgXml) : base(msgXml)
		{
		}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			_groupId = -1;
			_useUnit = false;
			_articleDefId = ARTICLE_TYPE_DEFAULT;

			foreach (XmlNode node in _root.ChildNodes)
			{
				switch (node.Name)
				{
					case "m": _vehicleId = node.InnerText; break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
					case "d2": _dateMax = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
					case "u": _unitId  = Convert.ToInt32(node.InnerText); break;
					case "o": _operationDefId = Convert.ToInt32(node.InnerText); break;
					case "a": _articleId = Convert.ToInt32(node.InnerText); break;
					case "ad": _articleDefId = Convert.ToInt32(node.InnerText); break;
					case "g": _groupId = Convert.ToInt32(node.InnerText); break;
					case "q": m_nMaxImport = Convert.ToInt32(node.InnerText);break;
					case "vci1": _vaoCard1 = node.InnerText;break;
					case "vci2": _vaoCard2 = node.InnerText;break;
					case "vci3": _vaoCard3 = node.InnerText;break;
					case "cad": _cad = Convert.ToInt32(node.InnerText);break;
					case "coid": _coid = node.InnerText;break;
					case "lr": _lang_rot = Convert.ToInt32(node.InnerText); break;
					case "mineqmax": _MinEqMax = Convert.ToInt32(node.InnerText);break;
					case "cp1": 	
						_couponsCode[_iNumCoupons]= node.InnerText;
						_iNumCoupons++;
						break;
					case "cp2": 	
						_couponsCode[_iNumCoupons]= node.InnerText;
						_iNumCoupons++;
						break;
					case "cp3": 	
						_couponsCode[_iNumCoupons]= node.InnerText;
						_iNumCoupons++;
						break;
					case "cp4": 	
						_couponsCode[_iNumCoupons]=node.InnerText;
						_iNumCoupons++;
						break;
					case "cp5": 	
						_couponsCode[_iNumCoupons]= node.InnerText;
						_iNumCoupons++;
						break;
					case "ctst":
						_CalculateTimeSteps = Convert.ToInt32(node.InnerText) == 1 && _CalculateQuantitySteps == 0 ? 1 : 0;
						break;
					case "cqst":
						{
							_CalculateQuantitySteps = Convert.ToInt32(node.InnerText) == 1 ? 1 : 0;
							if (_CalculateQuantitySteps == 1)
								_CalculateTimeSteps = 0;
						}
						break;
					case "stv":
						_CalculateSteps_StepValue = Convert.ToInt32(node.InnerText);
						break;
					case "dll": _computeDllPath = node.InnerText; break;
				}
			}
			if (_groupId == -1)
			{
				_groupId = _unitId;
				_useUnit = true;
			}

			if (_vehicleId == null)
			{
				_vehicleId = "ROT" + DateTime.Now.Ticks.ToString();
			}
		}

		#endregion

		#region IRecvMessage Members

		public StringCollection Process()
		{
		  //  StringCollection result = null;
		//	result = new StringCollection();
		//	result.Add("<p><m1><r>1</r><q1>0</q1><q2>554</q2><t>202</t><d>180849171104</d></m1></p>");
		//	return result;
			StringCollection result = new StringCollection();
			m_logger = DatabaseFactory.Logger;
			try
			{
				if(m_logger != null)
					m_logger.AddLog("[Msg01]:Processing Message",LoggerSeverities.Debug);
			
				string outxml="";
				int iError=ERROR_NOERROR;
				DateTime originalDate=_date;
				bool bVAOCardsOK=CheckVAOCards();
				bool bCamaraCheck=CameraCheckArticleDef(ref outxml,ref iError);				


				if ((bVAOCardsOK)&&(bCamaraCheck))
				{
					if (iError==ERROR_NOERROR)
					{

						CS_M1 pCS_M1 = new CS_M1();

						string strIn=this._docXml.InnerXml;
						int startPos=strIn.IndexOf("<d>");
						int endPos;
					
						if (startPos>0)
						{
							startPos+=3;
							endPos=strIn.IndexOf("</d>",startPos);

							if (endPos>0)
							{
								endPos-=1;

								strIn =	strIn.Substring(0,startPos)+
									Dtx.DtxToString(_date)+
									strIn.Substring(endPos+1,strIn.Length-endPos-1);
							}

						}

						startPos=strIn.IndexOf("<ad>");

						if (startPos>0)
						{
							startPos+=4;
							endPos=strIn.IndexOf("</ad>",startPos);

							if (endPos>0)
							{
								endPos-=1;

								strIn =	strIn.Substring(0,startPos)+
									_articleDefId.ToString()+
									strIn.Substring(endPos+1,strIn.Length-endPos-1);
							}

						}


						if (_dateMax>DateTime.MinValue)
						{
							TimeSpan ts=_dateMax-_date;
							if (Math.Abs(ts.TotalSeconds)<60)
							{
								TimeSpan tsSubs=new TimeSpan(0,0,0,60,0);
								_date=_date.Subtract(tsSubs);

								startPos=strIn.IndexOf("<d>");
					
								if (startPos>0)
								{
									startPos+=3;
									endPos=strIn.IndexOf("</d>",startPos);

									if (endPos>0)
									{
										endPos-=1;

										strIn =	strIn.Substring(0,startPos)+
											Dtx.DtxToString(_date)+
											strIn.Substring(endPos+1,strIn.Length-endPos-1);
									}

								}

							}

						}


						strIn=ApplyCoupons(strIn);


						pCS_M1.StrIn = strIn;
						pCS_M1.ApplyHistory=true;
						pCS_M1.UseDefaultArticleDef=false;
						if (_computeDllPath.Length > 0)
							pCS_M1.StrComputeDllPath = _computeDllPath;

						if (m_logger != null)
							m_logger.AddLog("[Msg01]:Process Parsing" +  pCS_M1.StrIn.ToString(),LoggerSeverities.Debug);

						if(pCS_M1.Exectue()!= CS_M1.C_RES_OK)
						{
							if(m_logger != null)
								m_logger.AddLog("[Msg01]:Process Parsing " +  "Error Execute",LoggerSeverities.Debug);
							return ReturnNack(NackMessage.NackTypes.NACK_SEMANTIC);
						}
						if(m_logger != null)
							m_logger.AddLog("[Msg01]:Process Parsing : Result" +  pCS_M1.StrOut.ToString(),LoggerSeverities.Debug);

						ProcessParticularSystemIdentifiers(ref outxml);

						//StringCollection result = 
						string strRes=pCS_M1.StrOut.ToString().Replace("</ap>",outxml+"</ap>");

						//Has plate fines

						if (_vehicleId.Length>0)
						{
							CalculateInfoAboutPlateFines(ref outxml);
							strRes=strRes.Replace("</ap>",outxml+"</ap>");
						}

						
						if (_cad!=CAD_UNDEFINED)
						{

							startPos=strRes.IndexOf("<Ad>");
						
							if (startPos>0)
							{
								startPos+=4;
								endPos=strRes.IndexOf("</Ad>",startPos);

								if (endPos>0)
								{
									endPos-=1;

									strRes =	strRes.Substring(0,startPos)+
										Dtx.DtxToString(_date)+
										strRes.Substring(endPos+1,strRes.Length-endPos-1);
								}

							}
							else
							{
								strRes=strRes.Replace("</ap>","<Ad>"+Dtx.DtxToString(_date)+"</Ad></ap>");

							}
						}



/*
 * 		private int _iNumCoupons=0;
		private uint[] _ReturnCouponsId= new uint[C_MAX_COUPONS];
		private int[] _ReturnCouponsError= new int[C_MAX_COUPONS];
		private int _TotalNumFreeMinutes=0;
		private int _TotalNumFreeCents=0;
 */

						if (_iNumCoupons>0)
						{
							for(int i=0; i<_iNumCoupons; i++)
							{
								strRes=strRes.Replace("</ap>",string.Format("<Acpi{0}>{1}</Acpi{0}><Acpr{0}>{2}</Acpr{0}></ap>",i+1,_ReturnCouponsId[i],_ReturnCouponsError[i]));
							}
						}


						result.Add(strRes);

						if(m_logger != null)
							m_logger.AddLog("[Msg01]:Process Parsing : Result StringCollection" +  result.ToString(),LoggerSeverities.Debug);

					}
					else
					{
						System.Text.StringBuilder ret = new System.Text.StringBuilder();
						ret.Append("<Ar>" + iError.ToString() + "</Ar>");	
						ret.Append(outxml);
						string strResult =new AckMessage(_msgId, ret.ToString()).ToString();
						result.Add(strResult);
						if(m_logger != null)
							m_logger.AddLog("[Msg01]:Process Parsing : Result" + strResult,LoggerSeverities.Debug);


					}

				}
				else if (!bVAOCardsOK)
				{
					System.Text.StringBuilder ret = new System.Text.StringBuilder();
					iError=M1_CARD_ALREADY_USED;			
					ret.Append("<Ar>" + iError.ToString() + "</Ar>");	
					string strResult =new AckMessage(_msgId, ret.ToString()).ToString();
					result.Add(strResult);
					if(m_logger != null)
						m_logger.AddLog("[Msg01]:Process Parsing : Result" + strResult,LoggerSeverities.Debug);

				}
				else
				{
					System.Text.StringBuilder ret = new System.Text.StringBuilder();
					iError=-1;			
					ret.Append("<Ar>" + iError.ToString() + "</Ar>");	
					string strResult =new AckMessage(_msgId, ret.ToString()).ToString();
					result.Add(strResult);
					if(m_logger != null)
						m_logger.AddLog("[Msg01]:Process Parsing : Result" + strResult,LoggerSeverities.Debug);

				}

				
			}
			catch(Exception e)
			{
				if(m_logger != null)
					m_logger.AddLog("[Msg01]:Process:Exception " +  e.Message ,LoggerSeverities.Debug);
			}
	 
			return result;

		}

		string ApplyCoupons(string strIn)
		{
			ILogger logger = null;
			string strRes = strIn;
			int		 iSystemIdentifier=-1;
			string	 strSystemIdentifier="";

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();

				if (_iNumCoupons>0)
				{
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

					switch(iSystemIdentifier)
					{
						case SYSTEM_IDENTIFIER_BANYOLES:
						{
							ApplyCouponsGiropark();
							break;
						}

						default:
						{
							ApplyCouponsGeneric();
							break;
						}
					}

					if (_TotalNumFreeMinutes>0)
					{
						strRes=strRes.Replace("</m1>",string.Format("<aft>{0}</aft></m1>",_TotalNumFreeMinutes));					
					}

					if (_TotalNumFreeCents>0)
					{
						strRes=strRes.Replace("</m1>",string.Format("<afm>{0}</afm></m1>",_TotalNumFreeCents));					
					}
				}
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg01:ApplyCoupons]: Excepcion: "+e.Message,LoggerSeverities.Error);
			}

			return strRes;
		}

		void ApplyCouponsGeneric()
		{
			ILogger logger = null;

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();

				CmpMoneyOffCoupons cmpCoupons = new CmpMoneyOffCoupons();
				/*	private string[] _couponsCode= new string[C_MAX_COUPONS];
					private int _iNumCoupons=0;
					private uint[] _ReturnCouponsId= new uint[C_MAX_COUPONS];
					private int[] _ReturnCouponsError= new int[C_MAX_COUPONS];
		
				*/
				int iNumFreeMinutes=0;
				int iNumFreeCents=0;
				uint uiCouponId=0;

				for (int i=0; i<_iNumCoupons;i++)
				{
					CmpMoneyOffCoupons.MoneyOffCouponsResults couponResult=
						cmpCoupons.IsMoneyOffCouponUsable(_couponsCode[i],_date,ref iNumFreeMinutes, ref iNumFreeCents,ref uiCouponId);

					_ReturnCouponsId[i]=uiCouponId;
					_ReturnCouponsError[i]=(int)couponResult;

					if (couponResult==CmpMoneyOffCoupons.MoneyOffCouponsResults.Usable)
					{
						_TotalNumFreeMinutes+= iNumFreeMinutes;
						_TotalNumFreeCents+= iNumFreeCents;
					}
				}
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg01:ApplyCouponsGeneric]: Excepcion: "+e.Message,LoggerSeverities.Error);
			}

			return;			
		}

		void ApplyCouponsGiropark()
		{
			ILogger logger = null;

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();

				CmpMoneyOffCoupons.MoneyOffCouponsResults couponResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.DontExist;

				uint uiCouponId=0;
				int	iCodiPoblacio=-1;

				try
				{
					CmpParametersDB cmpParam = new CmpParametersDB();
					iCodiPoblacio = Convert.ToInt32(cmpParam.GetParameter("P_CODI_POB_GIROPARK"));
				}
				catch
				{
				}

				OPSMessages.WSGestionarBonosGiropark.bonos objCouponInfoService=null;
				objCouponInfoService=new OPSMessages.WSGestionarBonosGiropark.bonos();

				// Determine if the coupon is valid
				try
				{
					// First check its state in the OPS DB
					CmpMoneyOffCoupons.MoneyOffCouponsResults couponOPSResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.DontExist;
					CmpMoneyOffCoupons cmpCoupons = new CmpMoneyOffCoupons();
					int iNumOPSMinutes = 0;
					int iNumOPSValue = 0;
					uint uiOPSCouponId = 0;
					couponOPSResult = cmpCoupons.IsMoneyOffCouponUsable( _couponsCode[0], _date, ref iNumOPSMinutes, ref iNumOPSValue, ref uiOPSCouponId );

					// If the coupon has not been used, then check using the web service
					if ( couponOPSResult != CmpMoneyOffCoupons.MoneyOffCouponsResults.AlreadyUsed )
					{
						int iResponse = objCouponInfoService.ExisteBono( iCodiPoblacio, _couponsCode[0], OPS.Comm.Dtx.DtxToString(_date) );

						/*
							*  a. >0: Coupon ID
							*	b. 0: Coupon code not found
							c. -1: Coupon found, but expired
							d. -2: Coupon already used
						*/
		
						logger.AddLog(string.Format("[Msg01:ApplyCouponsGiropark]: CouponInfoService.ExisteBono Returned {0}. Codi Poblacio:{1}. Coupon Code: {2}. Date: {3})", 
							iResponse,iCodiPoblacio,_couponsCode[0], OPS.Comm.Dtx.DtxToString(_date)), LoggerSeverities.Info);

						// Convert Giropark response to OTS result
						if ( iResponse > 0 )
						{
							uiCouponId = (uint) iResponse;
							couponResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.Usable;
						} // end of if
						else if ( iResponse == 0 )
							couponResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.DontExist;
						else if ( iResponse == -1 )
							couponResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.Expired;
						else if ( iResponse == -2 )
							couponResult = CmpMoneyOffCoupons.MoneyOffCouponsResults.AlreadyUsed;
					} // end of if
					else
					{
						// Coupon has already been used
						couponResult = couponOPSResult;
						uiCouponId = uiOPSCouponId;

						logger.AddLog(string.Format("[Msg01:ApplyCouponsGiropark]: The coupon has already been used in the OPS System. Coupon Code: {0})", 
							_couponsCode[0]), LoggerSeverities.Info);
					} // end of else

					_ReturnCouponsId[0]=uiCouponId;
					_ReturnCouponsError[0]=(int)couponResult;
				}
				catch (Exception e)
				{
					if(logger != null)
					{
						logger.AddLog(e);
						logger.AddLog(string.Format("[Msg01:ApplyCouponsGiropark]: Error Calling {3}/CouponInfoService.ExisteBono. Codi Poblacio:{0}. Coupon Code: {1}. Date: {2})", 
							iCodiPoblacio, _couponsCode[0], OPS.Comm.Dtx.DtxToString(_date),objCouponInfoService.Url), LoggerSeverities.Error);
					}
				}

				// Get the time associated with the coupon if it is valid
				if (couponResult==CmpMoneyOffCoupons.MoneyOffCouponsResults.Usable)
				{
					try
					{
						int iResponse = objCouponInfoService.ObtenerTiempoBono( iCodiPoblacio, uiCouponId, OPS.Comm.Dtx.DtxToString(_date) );

						/*
							*  a. >0: Number of minutes that the coupon is worth
							*	b. 0: Coupon code not found
							c. -1: Coupon found, but expired
							d. -2: Coupon already used
						*/
	
						logger.AddLog(string.Format("[Msg01:ApplyCouponsGiropark]: CouponInfoService.ObtenerTiempoBono Returned {0}. Codi Poblacio:{1}. Coupon ID: {2}. Date: {3})", 
							iResponse,iCodiPoblacio,uiCouponId, OPS.Comm.Dtx.DtxToString(_date)), LoggerSeverities.Info);

						if ( iResponse > 0 )
							_TotalNumFreeMinutes += iResponse;
					}
					catch (Exception e)
					{
						if(logger != null)
						{
							logger.AddLog(e);
							logger.AddLog(string.Format("[Msg01:ApplyCouponsGiropark]: Error Calling {3}/CouponInfoService.ObtenerTiempoBono. Codi Poblacio:{0}. Coupon ID: {1}. Date: {2})", 
								iCodiPoblacio, uiCouponId, OPS.Comm.Dtx.DtxToString(_date),objCouponInfoService.Url), LoggerSeverities.Error);
						}
					}

					// Delete the coupon if it exists
					CmpMoneyOffCouponsDB cmpCouponsDB = new CmpMoneyOffCouponsDB();
					cmpCouponsDB.DeleteCoupon( uiCouponId );

					// Insert the new coupon in the database
					if ( cmpCouponsDB.InsertCoupon( uiCouponId, _couponsCode[0], _TotalNumFreeMinutes, 0, 1 ) < 0 )
					{
						if(logger != null)
							logger.AddLog("[Msg01:ApplyCouponsGiropark]:ERROR inserting new coupon",LoggerSeverities.Debug);
						throw( new Exception("[Msg01:ApplyCouponsGiropark]:ERROR inserting new coupon") );
					}
					else
					{
						if(logger != null)
							logger.AddLog("[Msg01:ApplyCouponsGiropark]: Inserted new coupon OK",LoggerSeverities.Debug);
					}
				} // end of if
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg01:ApplyCouponsGiropark]: Excepcion: "+e.Message,LoggerSeverities.Error);
			}

			return;			
		}

		void CalculateInfoAboutPlateFines(ref string outxml)
		{
			outxml="";


			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr=null;
			ILogger logger = null;
			try
			{

				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();
				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;

					StringBuilder sqlQuery = new StringBuilder();
					sqlQuery.AppendFormat(	"select fin_id from fines t "+
											"WHERE TO_char(FIN_DATE,'DDMMYY')='{0:ddMMyy}' "+
											"AND fin_vehicleid='{1}' "+
											"AND t.fin_statusadmon=0 and t.fin_status=30 "+ //pendiente de pago y generada
											"ORDER BY FIN_DATE DESC", _date,_vehicleId);

					oraCmd.CommandText = sqlQuery.ToString();

					dr=oraCmd.ExecuteReader();

					if (dr.Read())
					{
						string fin_id= dr["fin_id"].ToString();
						string strResult="";
						string strPayed="";
						Msg05 m5 = new Msg05(fin_id,_date,false,_msgId,_msgDest);

						StringCollection retM5=m5.Process();
						if (retM5.Count>0)
						{
							string xmlRet=retM5[0];
							logger.AddLog("[Msg01:CalculateInfoAboutPlateFines]: m5 response for fin_id="+fin_id+": "+xmlRet,LoggerSeverities.Debug);


							XmlDocument xmldoc = new XmlDocument();
							xmldoc.LoadXml(xmlRet);

							XmlNodeList xnList = xmldoc.SelectNodes("/ap");
							
							if (xnList.Count>0)
							{
								XmlNode apNode=xnList[0];
								strResult=apNode["r"].InnerText;
								strPayed=apNode["p"].InnerText;								
							}
					
						}

						if (strResult.Length>0)
						{
							outxml="<Afi>"+fin_id+"</Afi><Afr>"+strResult+"</Afr><Afp>"+strPayed+"</Afp>";

						}

					}					
				}

			}
			catch(Exception e)
			{
				logger.AddLog("[Msg01:CalculateInfoAboutPlateFines]: Excepcion: "+e.Message,LoggerSeverities.Error);

			}
			finally
			{

				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr=null;;
				}


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

			


			

		}



		void ProcessParticularSystemIdentifiers(ref string outxml)
		{
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

			if (iSystemIdentifier==SYSTEM_IDENTIFIER_ZARAGOZA)
			{

				OracleConnection oraDBConn=null;
				OracleCommand oraCmd=null;
				OracleDataReader	dr		= null;
				ILogger logger = null;
				try
				{

					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					logger = DatabaseFactory.Logger;
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					oraDBConn = (OracleConnection)DBCon;
					oraDBConn.Open();
					if (oraDBConn.State == System.Data.ConnectionState.Open)
					{

						oraCmd= new OracleCommand();
						oraCmd.Connection=(OracleConnection)oraDBConn;

						StringBuilder sqlQuery = new StringBuilder();
						/*sqlQuery.AppendFormat(	"select rwmc_msg0, rwmc_msg1, rwmc_msg2 "+
							"from residents_data rd, residents_warning_msg_conf rw  "+
							"where rd.resd_vehicleid = '{0}'  "+
							"and rd.resd_actived = 1  "+
							"and rd.resd_grp_id = rw.rwmc_grp_id  "+
							"and to_date('{1}', 'hh24missddmmyy') between  "+
							"	to_date(rw.rwmc_inidate || '{2}' || '000000', 'ddmmyyyyhh24miss') and  "+
							"	to_date(rw.rwmc_enddate || '{2}' || '235959', 'ddmmyyyyhh24miss')  and "+
							"   to_number(to_char(resd_req_date,'yyyy'))<{2} "+
							"order by rw.rwmc_inidate, rw.rwmc_enddate", _vehicleId,OPS.Comm.Dtx.DtxToString(_date),_date.Year);*/


						System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

						int iMinimumPeriodBetweenRenewals=C_DEFAULT_RESIDENT_MINIMUM_PERIOD_BETWEEN_RENEWALS;
								
						try
						{
							iMinimumPeriodBetweenRenewals= (int)appSettings.GetValue ("RESIDENT_MINIMUM_PERIOD_BETWEEN_RENEWALS", typeof(int));
						}
						catch
						{
							iMinimumPeriodBetweenRenewals=C_DEFAULT_RESIDENT_MINIMUM_PERIOD_BETWEEN_RENEWALS;
						}



						sqlQuery.AppendFormat(	"select rwmc_msg0, "+
							"       rwmc_msg1, "+
							"       rwmc_msg2, "+
							"       CASE "+
							"         WHEN MONTH_CURR <= MONTH_EXP THEN "+
							"          YEAR_CURR "+
							"         ELSE "+
							"          YEAR_CURR + 1 "+
							"       END rwmc_year "+
							"  from (select R.RESD_REQ_DATE, "+
							"               R.RESD_VEHICLEID, "+
							"               R.RESD_ACTIVED, "+
							"               RESD_GRP_ID, "+
							"               TO_NUMBER(TO_CHAR(RESD_REQ_DATE, 'MM')) MONTH_EXP, "+
							"               TO_NUMBER(TO_CHAR(TO_DATE('{1}', 'HH24MISSDDMMYY'), "+
							"                                 'MM')) MONTH_CURR, "+
							"               TO_NUMBER(TO_CHAR(TO_DATE('{1}', 'HH24MISSDDMMYY'), "+
							"                                 'YYYY')) YEAR_CURR, "+
							"               MOD(MOD(TO_NUMBER(TO_CHAR(R.RESD_REQ_DATE, 'MM')) - 1, 12) - 1 + 12, "+
							"                   12) + 1 MONTH_INF, "+
							"               MOD(MOD(TO_NUMBER(TO_CHAR(R.RESD_REQ_DATE, 'MM')) - 1, 12) + 1 + 12, "+
							"                   12) + 1 MONTH_SUP "+
							"          from RESIDENTS_DATA R) rd, "+
							"       residents_warning_msg_conf rw "+
							" where RESD_ACTIVED = 1 and RWMC_ACTIVED=1 "+
							"   AND ((MONTH_INF < MONTH_SUP AND MONTH_CURR >= MONTH_INF AND "+
							"       MONTH_CURR < MONTH_SUP) OR "+
							"       (MONTH_INF > MONTH_SUP AND MONTH_CURR < MONTH_INF AND "+
							"       MONTH_CURR >= MONTH_INF - 12 AND MONTH_CURR < MONTH_SUP) OR "+
							"       (MONTH_INF > MONTH_SUP AND MONTH_CURR >= MONTH_INF AND "+
							"       MONTH_CURR - 12 >= MONTH_INF - 12 AND MONTH_CURR - 12 < MONTH_SUP)) "+
							"   AND rd.resd_vehicleid = '{0}'  "+
							"   AND rw.rwmc_enddate = lpad(to_char(MONTH_EXP), 2, '0') "+
							"   AND (TO_DATE('{1}', 'HH24MISSDDMMYY')-RESD_REQ_DATE)>{2} ",
								_vehicleId,OPS.Comm.Dtx.DtxToString(_date),iMinimumPeriodBetweenRenewals);



						oraCmd.CommandText = sqlQuery.ToString();
						
						dr = oraCmd.ExecuteReader();	
						int iNumMsg=0;
						string strMsg0="";
						string strMsg1="";
						string strMsg2="";
						string strYear="";
						string strFormat="";
						string strAppend="";


						while (dr.Read()) 
						{
							strMsg0=dr.GetString(dr.GetOrdinal("RWMC_MSG0")).ToString();
							strMsg1=dr.GetString(dr.GetOrdinal("RWMC_MSG1")).ToString();
							strMsg2=dr.GetString(dr.GetOrdinal("RWMC_MSG2")).ToString();
							strYear=dr.GetInt32(dr.GetOrdinal("RWMC_YEAR")).ToString();

							
							switch(_lang_rot)
							{
								case 0:
									strFormat="<sm"+iNumMsg.ToString()+">"+strMsg0+"</sm"+iNumMsg.ToString()+">";
									break;
								case 1:
									strFormat="<sm"+iNumMsg.ToString()+">"+strMsg1+"</sm"+iNumMsg.ToString()+">";
									break;
								case 2:
									strFormat="<sm"+iNumMsg.ToString()+">"+strMsg2+"</sm"+iNumMsg.ToString()+">";
									break;
								default:
									strFormat="<sm"+iNumMsg.ToString()+">"+strMsg0+"</sm"+iNumMsg.ToString()+">";
									break;								

							}

							strAppend = string.Format(strFormat,strYear);
							outxml+=strAppend;
							iNumMsg++;

						}


						if (iNumMsg==0)
						{
						
							if( dr != null )
							{
								dr.Close();
								dr = null;
							}

							StringBuilder sqlQuery2 = new StringBuilder();
							sqlQuery2.AppendFormat(	"SELECT rwmc_msg0, rwmc_msg1, rwmc_msg2 "+
													"FROM RESIDENTS_WARNING_MSG_CONF rw, RESIDENTS_DATA rd "+
													"where rw.rwmc_actived = 0 "+
													"and rd.resd_actived = 0 "+
													"and rd.resd_vehicleid = '{0}'",
													_vehicleId);



							oraCmd.CommandText = sqlQuery2.ToString();
						
							dr = oraCmd.ExecuteReader();	
							iNumMsg=0;
							strMsg0="";
							strMsg1="";
							strMsg2="";


							while (dr.Read()) 
							{
								strMsg0=dr.GetString(dr.GetOrdinal("RWMC_MSG0")).ToString();
								strMsg1=dr.GetString(dr.GetOrdinal("RWMC_MSG1")).ToString();
								strMsg2=dr.GetString(dr.GetOrdinal("RWMC_MSG2")).ToString();

							
								switch(_lang_rot)
								{
									case 0:
										strAppend="<sm"+iNumMsg.ToString()+">"+strMsg0+"</sm"+iNumMsg.ToString()+">";
										break;
									case 1:
										strAppend="<sm"+iNumMsg.ToString()+">"+strMsg1+"</sm"+iNumMsg.ToString()+">";
										break;
									case 2:
										strAppend="<sm"+iNumMsg.ToString()+">"+strMsg2+"</sm"+iNumMsg.ToString()+">";
										break;
									default:
										strAppend="<sm"+iNumMsg.ToString()+">"+strMsg0+"</sm"+iNumMsg.ToString()+">";
										break;								

								}
								
								outxml+=strAppend;
								iNumMsg++;

							}




						}

					}

				}
				catch(Exception e)
				{
					logger.AddLog("[Msg01:ProcessParticularSystemIdentifiers]: Excepcion: "+e.Message,LoggerSeverities.Error);					
				}
				finally
				{


					if( dr != null )
					{
						dr.Close();
						dr = null;
					}

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




			}
		}



		bool CheckVAOCards()
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
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					oraDBConn = (OracleConnection)DBCon;
					oraDBConn.Open();
					if (oraDBConn.State == System.Data.ConnectionState.Open)
					{

						oraCmd= new OracleCommand();
						oraCmd.Connection=(OracleConnection)oraDBConn;

						if (_vaoCard1.Length>0)
						{
							StringBuilder sqlQuery = new StringBuilder();
							sqlQuery.AppendFormat(	"select count(*) "+
								"from (select * "+
								"		from (select * "+
								"				from operations o "+
								"				where (o.ope_vaocard1 = '{0}' or o.ope_vaocard2 = '{0}' or "+
								"					o.ope_vaocard3 = '{0}') "+
								"				order by ope_movdate desc) "+
								"		where rownum <= 1) "+
								"where ope_enddate > to_date('{1}','hh24missddmmyy')", _vaoCard1,OPS.Comm.Dtx.DtxToString(_date));

							oraCmd.CommandText = sqlQuery.ToString();
							
							if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
							{
								bOK=false;
							}
						}

						if ((bOK)&&(_vaoCard2.Length>0))
						{
							StringBuilder sqlQuery = new StringBuilder();
							sqlQuery.AppendFormat(	"select count(*) "+
								"from (select * "+
								"		from (select * "+
								"				from operations o "+
								"				where (o.ope_vaocard1 = '{0}' or o.ope_vaocard2 = '{0}' or "+
								"					o.ope_vaocard3 = '{0}') "+
								"				order by ope_movdate desc) "+
								"		where rownum <= 1) "+
								"where ope_enddate > to_date('{1}','hh24missddmmyy')", _vaoCard2,OPS.Comm.Dtx.DtxToString(_date));

							oraCmd.CommandText = sqlQuery.ToString();
							
							if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
							{
								bOK=false;
							}
						}					

						if ((bOK)&&(_vaoCard3.Length>0))
						{
							StringBuilder sqlQuery = new StringBuilder();
							sqlQuery.AppendFormat(	"select count(*) "+
								"from (select * "+
								"		from (select * "+
								"				from operations o "+
								"				where (o.ope_vaocard1 = '{0}' or o.ope_vaocard2 = '{0}' or "+
								"					o.ope_vaocard3 = '{0}') "+
								"				order by ope_movdate desc) "+
								"		where rownum <= 1) "+
								"where ope_enddate > to_date('{1}','hh24missddmmyy')", _vaoCard3,OPS.Comm.Dtx.DtxToString(_date));

							oraCmd.CommandText = sqlQuery.ToString();
							
							if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
							{
								bOK=false;
							}
						}					
					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg01:CheckVAOCards]: Excepcion: "+e.Message,LoggerSeverities.Error);
					bOK=true;

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
			}

			return bOK;
		}


		bool CameraCheckArticleDef(ref string outxml,ref int iError)
		{

			bool bOK=true;
			int  iNumMinutesToGoBack;
			int  iMaxPhotosToReturn;
			int  iDefaultArticleDef;
			string strFTPPhotoFolder;
			string strNMOperationID;

			OPSMessages.WSNewPark.WSOPS2NewPark WSOPS2NewPark=null;
			OPSMessages.WSNewPark.VehiclePDMOperation oCallOperation=null;
			OPSMessages.WSNewPark.VehiclePDMOperationAnswer oAnswerOperation=null;	

			outxml="";
			iError=ERROR_NOERROR;

			if (_cad!=CAD_UNDEFINED)
			{
				OracleConnection oraDBConn=null;
				OracleCommand oraCmd=null;
				OracleDataReader dr=null;
				ILogger logger = null;
				try
				{

					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					logger = DatabaseFactory.Logger;
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					oraDBConn = (OracleConnection)DBCon;
					oraDBConn.Open();
					if (oraDBConn.State == System.Data.ConnectionState.Open)
					{

						oraCmd= new OracleCommand();
						oraCmd.Connection=(OracleConnection)oraDBConn;
						System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

						iDefaultArticleDef = ARTICLE_TYPE_DEFAULT;

						if (_cad==CAD_LOOK_FOR_PLATES)
						{
			
							try
							{
								iNumMinutesToGoBack = (int)appSettings.GetValue ("CAMERA_TIME_TO_GO_BACK_IN_MINUTES", typeof(int));
							}
							catch
							{
								iNumMinutesToGoBack=5;
							}


							StringBuilder sqlQuery = new StringBuilder();

							sqlQuery.AppendFormat(	"select count(*) "+
								"from nm_operations t "+
								"where nm_ope_plate = '{0}' "+
								"and (to_date('{1}', 'hh24missddmmyy') - nm_ope_date) * 24 * 60 <= {2} "+
								"and nm_ope_state in ({3},{4})",
								_vehicleId,OPS.Comm.Dtx.DtxToString(_date),iNumMinutesToGoBack,CAD_STATE_WAITING_TICKET,CAD_STATE_ENTRY_PENALTY_ISSUED);

							oraCmd.CommandText = sqlQuery.ToString();
							
							if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
							{
							
								sqlQuery.Remove(0,sqlQuery.Length);
								sqlQuery.AppendFormat(	"select NM_OPE_INT_ID,to_char(NM_OPE_DATE,'hh24missddmmyy') NM_OPE_DATE_CHAR,NM_OPE_TARIFF_ID "+
									"from nm_operations t "+
									"where nm_ope_plate = '{0}' "+
									"and (to_date('{1}', 'hh24missddmmyy') - nm_ope_date) * 24 * 60 <= {2} "+
									"and nm_ope_state in ({3},{4}) order by nm_ope_date desc",
									_vehicleId,OPS.Comm.Dtx.DtxToString(_date),iNumMinutesToGoBack,CAD_STATE_WAITING_TICKET,CAD_STATE_ENTRY_PENALTY_ISSUED);

								oraCmd.CommandText = sqlQuery.ToString();
					
								dr = oraCmd.ExecuteReader();	

								if (dr.Read())
								{
									_date=OPS.Comm.Dtx.StringToDtx(dr.GetString(dr.GetOrdinal("NM_OPE_DATE_CHAR")));
									if (_articleDefId==iDefaultArticleDef)
									{
										_articleDefId=dr.GetInt32(dr.GetOrdinal("NM_OPE_TARIFF_ID"));
									}

									if (_articleDefId<0)
									{
										_articleDefId=iDefaultArticleDef;
									}
									outxml="<Acoid>"+dr.GetInt32(dr.GetOrdinal("NM_OPE_INT_ID")).ToString()+"</Acoid>";
								}

							}
							else
							{
								

								sqlQuery.Remove(0,sqlQuery.Length);
								sqlQuery.AppendFormat(	"select PLATE_SIMILARITY_WEIGHT('{0}',NM_OPE_PLATE),NM_OPE_PLATE,NM_OPE_INT_ID,to_char(NM_OPE_DATE,'hh24missddmmyy') NM_OPE_DATE_CHAR,NM_OPE_TARIFF_ID "+
									"from nm_operations t "+
									"where (to_date('{1}', 'hh24missddmmyy') - nm_ope_date) * 24 * 60 <= {2} "+
									"and nm_ope_state in ({3},{4}) order by 1 DESC, NM_OPE_DATE DESC",
									_vehicleId,OPS.Comm.Dtx.DtxToString(_date),iNumMinutesToGoBack,CAD_STATE_WAITING_TICKET,CAD_STATE_ENTRY_PENALTY_ISSUED);

								oraCmd.CommandText = sqlQuery.ToString();
					
								dr = oraCmd.ExecuteReader();	

								try
								{
									iMaxPhotosToReturn = (int)appSettings.GetValue ("CAMERA_MAX_PHOTOS_TO_RETURN", typeof(int));
								}
								catch
								{
									iMaxPhotosToReturn=4;
								}

 
								try
								{
									strFTPPhotoFolder = (string)appSettings.GetValue ("CAMERA_FTP_PHOTO_FOLDER", typeof(string));
								}
								catch
								{
									strFTPPhotoFolder="C:\\TEMP\\Plates\\";
								}

								int iCurrPhoto=0;
								string strFileName="1.jpg";
								bool bExist=false;
								uint crc=1;
								string strPlate="";
								string strLastPlate="";

								while ((dr.Read())&&(iCurrPhoto<iMaxPhotosToReturn))
								{
									bExist=false;
									crc=0;
									strPlate=dr.GetString(dr.GetOrdinal("NM_OPE_PLATE"));
									if (strPlate!=strLastPlate)
									{										
										strFileName=_unitId.ToString()+"_"+_msgId.ToString()+"_"+dr.GetString(dr.GetOrdinal("NM_OPE_DATE_CHAR"));
										//GetFirstPhoto(dr.GetInt32(dr.GetOrdinal("NM_OPE_INT_ID")),strFTPPhotoFolder,ref strFileName,ref bExist,ref crc);
										bExist=true;
										if (bExist)
										{
											outxml+="<Am"+iCurrPhoto.ToString()+">"+dr.GetString(dr.GetOrdinal("NM_OPE_PLATE"))+"</Am"+iCurrPhoto.ToString()+">";
											outxml+="<Ad"+iCurrPhoto.ToString()+">"+dr.GetString(dr.GetOrdinal("NM_OPE_DATE_CHAR"))+"</Ad"+iCurrPhoto.ToString()+">";
											outxml+="<Aid"+iCurrPhoto.ToString()+">"+dr.GetInt32(dr.GetOrdinal("NM_OPE_INT_ID")).ToString()+"</Aid"+iCurrPhoto.ToString()+">";
											outxml+="<Af"+iCurrPhoto.ToString()+">"+strFileName+"</Af"+iCurrPhoto.ToString()+">";
											outxml+="<Acs"+iCurrPhoto.ToString()+">"+crc.ToString()+"</Acs"+iCurrPhoto.ToString()+">";																
											iCurrPhoto++;
											strLastPlate=strPlate;
										}
									}
									
								}

								if (dr!=null)
								{
									dr.Close();
									dr.Dispose();
									dr = null;
								}


								if (iCurrPhoto>0)
								{
									outxml="<Appn>"+iCurrPhoto.ToString()+"</Appn>"+outxml;
									iError=M1_PLATE_NOT_FOUND;
								}
								else
								{  //No photos for showing to the user. We need to ask
																		
									try
									{

										sqlQuery.Remove(0,sqlQuery.Length);
										sqlQuery.Append("select SEQ_NM_OPERATIONS.NEXTVAL FROM DUAL");

										oraCmd.CommandText = sqlQuery.ToString();

										int iInternalId = Convert.ToInt32(oraCmd.ExecuteScalar());

										WSOPS2NewPark = new OPSMessages.WSNewPark.WSOPS2NewPark();
										oCallOperation=new OPSMessages.WSNewPark.VehiclePDMOperation();

										oCallOperation.SID=iInternalId.ToString();
										oCallOperation.NType=2; //Any
										oCallOperation.SPlate=_vehicleId;
										
										string sPatt = @"dd/MM/yyyy HH:mm:ss";										
										oCallOperation.SPaymentDate= _date.ToString(sPatt);
										oCallOperation.SExpiryDate= _date.ToString(sPatt);
										oCallOperation.NQuantity=0;
										oCallOperation.NTariffID=-1;
										oCallOperation.BOnline=true;

										oAnswerOperation = WSOPS2NewPark.NewVehicleOperation(oCallOperation);

										if ((oAnswerOperation.NTariffID>=0)&&(_articleDefId==iDefaultArticleDef))
										{
											_articleDefId=oAnswerOperation.NTariffID;
										}
										/*else
										{
											_articleDefId=iDefaultArticleDef;
										}*/

										
	
									}
									catch(Exception e)
									{
										oAnswerOperation= new OPSMessages.WSNewPark.VehiclePDMOperationAnswer();
										oAnswerOperation.SID=oCallOperation.SID;
										logger.AddLog("[Msg01:CameraCheckArticleDef]: Excepcion: "+e.Message,LoggerSeverities.Error);
										
									}

									strNMOperationID=(oAnswerOperation.SID!=oCallOperation.SID)?oAnswerOperation.SID:"NULL";
									//insert new operation
									InsertNewOperation(oCallOperation,"NULL");
									outxml="<Acoid>"+oCallOperation.SID+"</Acoid>";


								}

								outxml="<Aad>"+_articleDefId.ToString()+"</Aad>"+outxml;


							}
						}

						else if (_cad==CAD_CHANGE_PLATES)
						{
							
							if (_coid=="")
							{
								//the user has selected none of the photos showed
								try
								{

									StringBuilder sqlQuery = new StringBuilder();
									sqlQuery.Append("select SEQ_NM_OPERATIONS.NEXTVAL FROM DUAL");

									oraCmd.CommandText = sqlQuery.ToString();

									int iInternalId = Convert.ToInt32(oraCmd.ExecuteScalar());

									WSOPS2NewPark = new OPSMessages.WSNewPark.WSOPS2NewPark();
									oCallOperation=new OPSMessages.WSNewPark.VehiclePDMOperation();

									oCallOperation.SID=iInternalId.ToString();
									oCallOperation.NType=2; //Any
									oCallOperation.SPlate=_vehicleId;
										
									string sPatt = @"dd/MM/yyyy HH:mm:ss";										
									oCallOperation.SPaymentDate= _date.ToString(sPatt);
									oCallOperation.SExpiryDate= _date.ToString(sPatt);
									oCallOperation.NQuantity=0;
									oCallOperation.NTariffID=-1;
									oCallOperation.BOnline=true;

									oAnswerOperation = WSOPS2NewPark.NewVehicleOperation(oCallOperation);

									if ((oAnswerOperation.NTariffID!=-1)&&(_articleDefId==iDefaultArticleDef))
									{
										_articleDefId=oAnswerOperation.NTariffID;
									}
									/*else
									{
										_articleDefId=iDefaultArticleDef;
									}*/
									

								}
								catch(Exception e)
								{
									
									logger.AddLog("[Msg01:CameraCheckArticleDef]: Excepcion: "+e.Message,LoggerSeverities.Error);
								
								}

								
								strNMOperationID=(oAnswerOperation.SID!=oCallOperation.SID)?oAnswerOperation.SID:"NULL";
								//insert new operation
								InsertNewOperation(oCallOperation,"NULL");
								outxml="<Acoid>"+oCallOperation.SID+"</Acoid>";


							}
							else
							{
								try
								{

									//we need to change plate of selected coid
									WSOPS2NewPark = new OPSMessages.WSNewPark.WSOPS2NewPark();
									oCallOperation=new OPSMessages.WSNewPark.VehiclePDMOperation();

									oCallOperation.SID=_coid;
									oCallOperation.NType=1; //User (user selects from images)
									oCallOperation.SPlate=_vehicleId;
											
									string sPatt = @"dd/MM/yyyy HH:mm:ss";										
									oCallOperation.SPaymentDate= _date.ToString(sPatt);
									oCallOperation.SExpiryDate= _date.ToString(sPatt);
									oCallOperation.NQuantity=0;
									oCallOperation.NTariffID=_articleDefId;
									oCallOperation.BOnline=true;

/*									oAnswerOperation = WSOPS2NewPark.NewVehicleOperation(oCallOperation);

									if (oAnswerOperation.NTariffID!=-1)
									{
										_articleDefId=oAnswerOperation.NTariffID;
									}
									else
									{
										_articleDefId=iDefaultArticleDef;
									}		
*/
									

								}
								catch(Exception e)
								{
									logger.AddLog("[Msg01:CameraCheckArticleDef]: Excepcion: "+e.Message,LoggerSeverities.Error);									
								}

								UpdateOperation(oCallOperation,_coid);
								outxml="<Acoid>"+_coid+"</Acoid>";

							}

						}

					}
				}
				catch(Exception e)
				{
					logger.AddLog("[Msg01:CameraCheckArticleDef]: Excepcion: "+e.Message,LoggerSeverities.Error);
					bOK=false;

				}
				finally
				{

					
					if (dr!=null)
					{
						dr.Close();
						dr.Dispose();
						dr = null;
					}

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
			}

			return bOK;
		}


		bool  InsertNewOperation(OPSMessages.WSNewPark.VehiclePDMOperation oCallOperation,string strNMOperationID)

		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			bool bRes=false;


			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
				
					oraCmd.CommandText = String.Format("INSERT INTO NM_OPERATIONS (NM_OPE_INT_ID,NM_OPE_ID, NM_OPE_TYPE, NM_OPE_PLATE, NM_OPE_DATE, NM_OPE_NUM_IMG, NM_OPE_TARIFF_ID, NM_OPE_STATE,NM_OPE_LAST_STATE_CHANGE) " +
						"VALUES ({0},{1},{2},'{3}',to_date('{4}','DD/MM/yyyy HH24:MI:SS'),{5},{6},{7},SYSDATE)",
						oCallOperation.SID,
						strNMOperationID,
						0,
						oCallOperation.SPlate,
						oCallOperation.SPaymentDate,
						0,
						_articleDefId,
						CAD_STATE_WAITING_TICKET);
								
						oraCmd.ExecuteNonQuery();
						bRes=true;
							

					
				}

				if (!bRes)
				{
					if(logger != null)
						logger.AddLog("[Msg01:InsertNewOperation]: Error.",LoggerSeverities.Error);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg01:InsertNewOperation]: Error: "+e.Message,LoggerSeverities.Error);
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

			return bRes;
		}


		bool  UpdateOperation(OPSMessages.WSNewPark.VehiclePDMOperation oCallOperation,string coid)

		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			bool bRes=false;


			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
				
					oraCmd.CommandText=	string.Format("update nm_operations "+
										"set nm_ope_subst_plate      = nm_ope_plate, "+
										"	nm_ope_subst_date      = nm_ope_date, "+
										"	nm_ope_subst_tariff_id = nm_ope_tariff_id, "+
										"	nm_ope_plate= '{1}', "+
										"	nm_ope_date= to_date('{2}','DD/MM/yyyy HH24:MI:SS'), "+
										"	nm_ope_tariff_id= {3} "+
										"where nm_ope_int_id = {0}",
										_coid,
										oCallOperation.SPlate,
										oCallOperation.SPaymentDate,
										_articleDefId);
								
					oraCmd.ExecuteNonQuery();
					bRes=true;
							

					
				}

				if (!bRes)
				{
					if(logger != null)
						logger.AddLog("[Msg01:InsertNewOperation]: Error.",LoggerSeverities.Error);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg01:InsertNewOperation]: Error: "+e.Message,LoggerSeverities.Error);
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

			return bRes;
		}



		bool GetFirstPhoto(int iOperId,string strFTPPhotoFolder,ref string strFileName,ref bool bExist,ref uint crc)
		{

			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr=null;
			ILogger logger = null;
			bExist=false;
			crc=0;
			

			try
			{

				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();
				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
						

					StringBuilder sqlQuery = new StringBuilder();

					sqlQuery.AppendFormat(	"select nm_opimg_raw, nm_opimg_format "+
											"from nm_operations_img t "+
											"where t.nm_opimg_ope_int_id={0} "+
											"order by nm_opimg_num asc",iOperId);

					oraCmd.CommandText = sqlQuery.ToString();
					

					dr = oraCmd.ExecuteReader();	

					if (dr.Read())
					{						
						OracleBlob PhotoClob= dr.GetOracleBlob(dr.GetOrdinal("NM_OPIMG_RAW"));
						string strFileFormat=dr.GetString(dr.GetOrdinal("NM_OPIMG_FORMAT"));
						if (PhotoClob!=null)
						{
							strFileName=strFileName+"."+strFileFormat;
							//
							/*byte [] byPhotoClob=new byte[PhotoClob.Length];
							if (PhotoClob.Read(byPhotoClob,0,Convert.ToInt32(PhotoClob.Length))==Convert.ToInt32(PhotoClob.Length))
							{
								System.Text.Encoding enc = System.Text.Encoding.Text;
								string base64String = enc.GetString(byPhotoClob );
								Base64ToFile(base64String,strFTPPhotoFolder+strFileName);
							}*/
							
							if (Base64ToFile(PhotoClob.Value.ToString(),strFTPPhotoFolder+strFileName,ref crc))
							{
								bExist=true;

							}


						}
						
					}

					if (bExist)
					{
						FileInfo f = new FileInfo(strFTPPhotoFolder+strFileName);
						bExist=(f.Length>0);
						if (!bExist)
						{
							File.Delete(strFTPPhotoFolder+strFileName);							
						}
					}
				}
			}
			catch(Exception e)
			{
				logger.AddLog("[Msg01:CameraCheckArticleDef]: Excepcion: "+e.Message,LoggerSeverities.Error);
				bExist=false;

			}
			finally
			{

				
				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

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
		

			return bExist;
		}


		public  bool Base64ToFile(string base64String,string strFullPath,ref uint crc)
		{
			bool bOK=true;
			try
			{
				// Convert Base64 String to byte[]
				byte[] imageBytes = Convert.FromBase64String(base64String);
				crc=CalculateCRC32(imageBytes,imageBytes.Length);
				FileStream fs   = new FileStream(strFullPath, FileMode.Create);
				BinaryWriter bw = new BinaryWriter(fs);			
				bw.Write(imageBytes);
				bw.Close();
				fs.Close();
			}
			catch(Exception e)
			{
				bOK=false;

			}

			return bOK;

		}
		
		private void InitCRC32Table()
		{
			gs_plCrc32Table=new uint[CRC_TABLE_LEN];
			Array.Clear(gs_plCrc32Table,0,gs_plCrc32Table.Length);
			uint i, j;
			uint	uiCrc;
			for(i = 0; i < CRC_TABLE_LEN; i++)
			{
				uiCrc = i;
				for(j = 8; j > 0; j--)
				{
					if((uiCrc & 1)!=0)
						uiCrc = (uiCrc >> 1) ^ 3988292384;
					else
						uiCrc >>= 1;
				}
				gs_plCrc32Table[i] = uiCrc;
			}
		}

		private uint CalculateCRC32(byte [] body, int iLengthForCalc)
		{
			uint uiRes=0;
			if (gs_plCrc32Table==null)
			{	
				InitCRC32Table();
			}

			for( uint i = 0; i < iLengthForCalc; i++ )
			{
				uiRes=CalculateCRC32( body[i], uiRes );
			}

			return uiRes;
		}

		private uint CalculateCRC32(byte by,uint uiCurrCRC)
		{
			uint uiRes;

			uiRes =(uiCurrCRC >> 8) ^ gs_plCrc32Table[(by) ^ (uiCurrCRC & 0x000000FF)];

			return uiRes;
		}

		#endregion
	
	}
}