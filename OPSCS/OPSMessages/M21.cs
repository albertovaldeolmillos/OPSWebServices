using System;
using System.Xml;
using OPS.Components.Data;
using OPS.Comm.Becs.Messages;
using System.Collections.Specialized;
using OPS.Comm;
////using Oracle.DataAccess.Client;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{	

	public class MsgCommand
	{
		#region Variables, creation and parsing
		private  long   m_lUnit = -1;
	
		public  MsgCommand()
		{
			
		}

		public long UnitSnd
		{
			get
			{
				return this.m_lUnit;
			}
			set
			{
				this.m_lUnit = value;
			}
		}

		#endregion

	}
	/// <summary>
	/// Recaudación
	/// </summary>
 
	public sealed class Msg20: MsgReceived, IRecvMessage
	{
		
		#region DefinedRootTag (m1)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m20"; } }
		#endregion

		#region Variables, creation and parsing

		public struct stCoinData
		{
			public int m_iCoinValue;
			public int m_iCoinNumber;
		};

		public const int MAX_NUM_COINS=15;

		public int m_iMsgVersion = 1;
		public double m_dRepFactor = 1.0;
		public string m_strCoinSymbol = "";
		public long m_lUnit  = -1;
		public long m_lColNum = -1;
		public  string m_dtDateIni;
		public  string m_dtDateEnd;
		public long m_lBackColTotal = -1;

		// CASH 
		public int m_nCashPark = -1;
		public int m_nCashParkOps = -1;
		public int m_nCashFine = -1;
		public int m_nCashFineOps = -1;
		public int m_nCashRecharge = -1;
		public int m_nCashRechargeOps = -1;
		public int m_nCashResPay = -1;
		public int m_nCashResPayOps = -1;
		public int m_nCashPowerRecharge = -1;
		public int m_nCashPowerRechargeOps = -1;
		public int m_nCashBycing = -1;
		public int m_nCashBycingOps = -1;
		public int m_nCashTotal = -1;
		public int m_nCashTotalOps = -1;

		
		public stCoinData[] m_CoinList;


		// CREDIT_CARD
		public int m_nCrCardPark = -1;
		public int m_nCrCardParkOps = -1;
		public int m_nCrCardFine = -1;
		public int m_nCrCardFineOps = -1;
		public int m_nCrCardRecharge = -1;
		public int m_nCrCardRechargeOps = -1;
		public int m_nCrCardResPay = -1;
		public int m_nCrCardResPayOps = -1;
		public int m_nCrCardPowerRecharge = -1;
		public int m_nCrCardPowerRechargeOps = -1;
		public int m_nCrCardBycing = -1;
		public int m_nCrCardBycingOps = -1;
		public int m_nCrCardTotal = -1;
		public int m_nCrCardTotalOps = -1;

		// CHIP_CARD
		public int m_nChCardPark = -1;
		public int m_nChCardParkOps = -1;
		public int m_nChCardFine = -1;
		public int m_nChCardFineOps = -1;
		public int m_nChCardReturn = -1;
		public int m_nChCardReturnOps = -1;
		public int m_nChCardResPay = -1;
		public int m_nChCardResPayOps = -1;
		public int m_nChCardPowerRecharge = -1;
		public int m_nChCardPowerRechargeOps = -1;
		public int m_nChCardBycing = -1;
		public int m_nChCardBycingOps = -1;

		public int m_nChCardTotal = -1;
		public int m_nChCardTotalOps = -1;		

		// CMI
		public int m_nCMI_050 = -1;
		public int m_nCMI_050_075 = -1;
		public int m_nCMI_075_100 = -1;
		public int m_nCMI_100_150 = -1;
		public int m_nCMI_150_200 = -1;
		public int m_nCMI_200_250 = -1;
		public int m_nCMI_250_300 = -1;
		public int m_nCMI_300_400 = -1;
		public int m_nCMI_400_500 = -1;
		public int m_nCMI_500 = -1;

		// CTI
		public int m_nCTI_1000 = -1;
		public int m_nCTI_1000_1200 = -1;
		public int m_nCTI_1200_1400 = -1;
		public int m_nCTI_1400_1600 = -1;
		public int m_nCTI_1600_1800 = -1;
		public int m_nCTI_1800_2000 = -1;
		public int m_nCTI_2000_2200 = -1;
		public int m_nCTI_2200_0000 = -1;

		public int m_nCouponsFreeTime	=	-1;
		public int m_nCouponsFreeAmount	=	-1;
		public int m_nCouponsNum		=	-1;

		//private XmlDocument m_root = null;

		public Msg20(XmlDocument msgXml) : base(msgXml)
		{		
		}
 


		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr= null;
			bool bRes=false;
			bool bExist=false;
			bool bDeleted=false;

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg20::DoParseMessage]",LoggerSeverities.Debug);

			try
			{

				if (m_lColNum!= -1)
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
					
						if (!ExistCollecting(ref bExist,ref oraDBConn,ref oraCmd))
						{
							bRes=false;
						}
						else
						{
							bRes=true;
						}

						if ((bRes)&&(bExist))
						{
							if (!DeleteCollecting(ref bDeleted,ref oraDBConn,ref oraCmd))
							{
								bRes=false;
							}
							else
							{
								bRes=true;
							}
						}

						if (bRes)
						{
							bRes=false;
							NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
							nfi.NumberDecimalSeparator = ".";
							oraCmd.CommandText =	string.Format(nfi, "insert into COLLECTINGS("+
								"COL_UNI_ID,"+
								"COL_NUM,"+
								"COL_INIDATE,"+
								"COL_ENDDATE,"+
								"COL_BACK_COL_TOTAL,"+
								"COL_CASH_PARK,"+
								"COL_CASH_FINE,"+
								"COL_CASH_RECHARGE,"+
								"COL_CASH_TOTAL,"+
								"COL_CASH_PARK_OPS,"+
								"COL_CASH_FINE_OPS,"+
								"COL_CASH_RECHARGE_OPS,"+
								"COL_CASH_TOTAL_OPS,"+							
								"COL_CASH_COINS_V1,"+
								"COL_CASH_COINS_V2,"+
								"COL_CASH_COINS_V3,"+
								"COL_CASH_COINS_V4,"+
								"COL_CASH_COINS_V5,"+
								"COL_CASH_COINS_V6,"+
								"COL_CASH_COINS_V7,"+
								"COL_CASH_COINS_V8,"+
								"COL_CASH_COINS_V9,"+
								"COL_CASH_COINS_V10,"+
								"COL_CASH_COINS_V11,"+
								"COL_CASH_COINS_V12,"+
								"COL_CASH_COINS_V13,"+
								"COL_CASH_COINS_V14,"+
								"COL_CASH_COINS_V15,"+
								"COL_CASH_COINS_Q1,"+
								"COL_CASH_COINS_Q2,"+
								"COL_CASH_COINS_Q3,"+
								"COL_CASH_COINS_Q4,"+
								"COL_CASH_COINS_Q5,"+
								"COL_CASH_COINS_Q6,"+
								"COL_CASH_COINS_Q7,"+
								"COL_CASH_COINS_Q8,"+
								"COL_CASH_COINS_Q9,"+
								"COL_CASH_COINS_Q10,"+
								"COL_CASH_COINS_Q11,"+
								"COL_CASH_COINS_Q12,"+
								"COL_CASH_COINS_Q13,"+
								"COL_CASH_COINS_Q14,"+
								"COL_CASH_COINS_Q15,"+
								"COL_CRCARD_PARK,"+
								"COL_CRCARD_FINE,"+
								"COL_CRCARD_RECHARGE,"+
								"COL_CRCARD_TOTAL,"+
								"COL_CRCARD_PARK_OPS,"+
								"COL_CRCARD_FINE_OPS,"+
								"COL_CRCARD_RECHARGE_OPS,"+
								"COL_CRCARD_TOTAL_OPS,"+
								"COL_CHCARD_PARK,"+
								"COL_CHCARD_FINE,"+
								"COL_CHCARD_RETURN,"+
								"COL_CHCARD_TOTAL,"+
								"COL_CHCARD_PARK_OPS,"+
								"COL_CHCARD_FINE_OPS,"+
								"COL_CHCARD_RETURN_OPS,"+
								"COL_CHCARD_TOTAL_OPS,"+
								"COL_CMI_050,"+
								"COL_CMI_050_075,"+
								"COL_CMI_075_100,"+
								"COL_CMI_100_075,"+
								"COL_CMI_150_075,"+
								"COL_CMI_200_075,"+
								"COL_CMI_250_075,"+
								"COL_CMI_300_075,"+
								"COL_CMI_400_075,"+
								"COL_CMI_500,"+
								"COL_CTI_1000,"+
								"COL_CTI_1000_1200,"+
								"COL_CTI_1200_1400,"+
								"COL_CTI_1400_1600,"+
								"COL_CTI_1600_1800,"+
								"COL_CTI_1800_2000,"+
								"COL_CTI_2000_2200,"+
								"COL_CTI_2200_0000,"+
								"COL_COIN_REP_FACTOR,"+
								"COL_COIN_SYMBOL) "+
								"values ({0}, {1}, to_date('{2}', 'HH24MISSDDMMYY'), to_date('{3}', 'HH24MISSDDMMYY'), "+ 
								"{4}, {5}, {6}, {7}, {8}, {9},{10}, {11}, {12}, "+								
								"{13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, "+
								"{23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, "+
								"{33}, {34}, {35}, {36}, {37}, {38}, {39}, {40}, {41}, {42}, "+
								"{43}, {44}, {45}, {46}, {47}, {48}, {49}, {50}, {51}, {52}, {53}, "+
								"{54}, {55}, {56}, {57}, {58}, {59}, {60}, {61}, {62}, {63}, "+
								"{64}, {65}, {66}, {67}, {68}, {69}, {70}, {71}, {72}, {73}, "+
								"{74}, {75}, {76}, {77}, '{78}')",
								m_lUnit ,
								m_lColNum,
								m_dtDateIni,
								m_dtDateEnd,
								m_lBackColTotal,
								m_nCashPark,
								m_nCashFine,
								m_nCashRecharge,
								m_nCashTotal,
								m_nCashParkOps,
								m_nCashFineOps,
								m_nCashRechargeOps,
								m_nCashTotalOps,
								m_CoinList[0].m_iCoinValue,
								m_CoinList[1].m_iCoinValue,
								m_CoinList[2].m_iCoinValue,
								m_CoinList[3].m_iCoinValue,
								m_CoinList[4].m_iCoinValue,
								m_CoinList[5].m_iCoinValue,
								m_CoinList[6].m_iCoinValue,
								m_CoinList[7].m_iCoinValue,
								m_CoinList[8].m_iCoinValue,
								m_CoinList[9].m_iCoinValue,
								m_CoinList[10].m_iCoinValue,
								m_CoinList[11].m_iCoinValue,
								m_CoinList[12].m_iCoinValue,
								m_CoinList[13].m_iCoinValue,
								m_CoinList[14].m_iCoinValue,
								m_CoinList[0].m_iCoinNumber,
								m_CoinList[1].m_iCoinNumber,
								m_CoinList[2].m_iCoinNumber,
								m_CoinList[3].m_iCoinNumber,
								m_CoinList[4].m_iCoinNumber,
								m_CoinList[5].m_iCoinNumber,
								m_CoinList[6].m_iCoinNumber,
								m_CoinList[7].m_iCoinNumber,
								m_CoinList[8].m_iCoinNumber,
								m_CoinList[9].m_iCoinNumber,
								m_CoinList[10].m_iCoinNumber,
								m_CoinList[11].m_iCoinNumber,
								m_CoinList[12].m_iCoinNumber,
								m_CoinList[13].m_iCoinNumber,
								m_CoinList[14].m_iCoinNumber,
								m_nCrCardPark,
								m_nCrCardFine,
								m_nCrCardRecharge,
								m_nCrCardTotal,
								m_nCrCardParkOps,
								m_nCrCardFineOps,
								m_nCrCardRechargeOps,
								m_nCrCardTotalOps,
								m_nChCardPark,
								m_nChCardFine,
								m_nChCardReturn,
								m_nChCardTotal,
								m_nChCardParkOps,
								m_nChCardFineOps,
								m_nChCardReturnOps,
								m_nChCardTotalOps,		
								m_nCMI_050,
								m_nCMI_050_075,
								m_nCMI_075_100,
								m_nCMI_100_150,
								m_nCMI_150_200,
								m_nCMI_200_250,
								m_nCMI_250_300,
								m_nCMI_300_400,
								m_nCMI_400_500,
								m_nCMI_500,
								m_nCTI_1000,
								m_nCTI_1000_1200,
								m_nCTI_1200_1400,
								m_nCTI_1400_1600,
								m_nCTI_1600_1800,
								m_nCTI_1800_2000,
								m_nCTI_2000_2200,
								m_nCTI_2200_0000,
								m_dRepFactor,
								m_strCoinSymbol);
								
							oraCmd.ExecuteNonQuery();

							if (m_nCashResPayOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
																			"set COL_CASH_RESPAY = {0},"+
																			"    COL_CASH_RESPAY_OPS = {1} "+
																			"where COL_UNI_ID = {2} "+
																			"  and COL_NUM = {3} "+
																			"  and COL_BACK_COL_TOTAL = {4}",
								m_nCashResPay, m_nCashResPayOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}



							if (m_nCashPowerRechargeOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CASH_POWRECHARGE = {0},"+
									"    COL_CASH_POWRECHARGE_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nCashPowerRecharge, m_nCashPowerRechargeOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}

							if (m_nCashBycingOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CASH_BYCING = {0},"+
									"    COL_CASH_BYCING_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nCashBycing, m_nCashBycingOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}


							if (m_nCrCardResPayOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CRCARD_RESPAY = {0},"+
									"    COL_CRCARD_RESPAY_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nCrCardResPay, m_nCrCardResPayOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}



							if (m_nCrCardPowerRechargeOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CRCARD_POWRECHARGE = {0},"+
									"    COL_CRCARD_POWRECHARGE_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nCrCardPowerRecharge, m_nCrCardPowerRechargeOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}

							if (m_nCrCardBycingOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CRCARD_BYCING = {0},"+
									"    COL_CRCARD_BYCING_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nCrCardBycing, m_nCrCardBycingOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}

							if (m_nChCardResPayOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CHCARD_RESPAY = {0},"+
									"    COL_CHCARD_RESPAY_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nChCardResPay, m_nChCardResPayOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}



							if (m_nChCardPowerRechargeOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CHCARD_POWRECHARGE = {0},"+
									"    COL_CHCARD_POWRECHARGE_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nChCardPowerRecharge, m_nChCardPowerRechargeOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}

							if (m_nChCardBycingOps>-1)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_CHCARD_BYCING = {0},"+
									"    COL_CHCARD_BYCING_OPS = {1} "+
									"where COL_UNI_ID = {2} "+
									"  and COL_NUM = {3} "+
									"  and COL_BACK_COL_TOTAL = {4}",
									m_nChCardBycing, m_nChCardBycingOps,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}


							if (m_nCouponsNum>0)
							{
								oraCmd.CommandText =	string.Format(nfi, "update COLLECTINGS " + 
									"set COL_NUM_COUPONS = {0},"+
									"    COL_COUPONS_VALUE = {1}, "+
									"    COL_COUPONS_DURATION = {2} "+
									"where COL_UNI_ID = {3} "+
									"  and COL_NUM = {4} "+
									"  and COL_BACK_COL_TOTAL = {5}",
									m_nCouponsNum, m_nCouponsFreeAmount,m_nCouponsFreeTime,m_lUnit ,m_lColNum, m_lBackColTotal);

								oraCmd.ExecuteNonQuery();


							}							
							

							bRes=true;
								

						}
					}
				}
				else
				{
					//el mensaje de recaudación es el viejo y por tanto inutil
					bRes=true;
				}


				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg20:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg20:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
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

			return res;
		}

		bool ExistCollecting(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bExist=false;

			try
			{
				oraCmd.CommandText =	string.Format("select count(*) "+    
					"from   COLLECTINGS "+
					"where  COL_NUM = {0} "+
					"  and  COL_UNI_ID = {1} "+
					"  and  TO_CHAR(COL_INIDATE,'HH24MISSDDMMYY') = '{2}'",
					m_lColNum, m_lUnit, m_dtDateIni );
				


				if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
				{
					bExist=true;
				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool DeleteCollecting(ref bool bDeleted, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bDeleted=false;

			try
			{
				oraCmd.CommandText =	string.Format("delete "+    
					"from   COLLECTINGS "+
					"where  COL_NUM = {0} "+
					"  and  COL_UNI_ID = {1} "+
					"  and  TO_CHAR(COL_INIDATE,'HH24MISSDDMMYY') = '{2}'",
					m_lColNum, m_lUnit, m_dtDateIni );
				


				if (oraCmd.ExecuteNonQuery()>0)
				{
					bDeleted=true;
				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}



		public void DoParseMessageCC()
		{
			this.DoParseMessage();
		}

		protected override void DoParseMessage()
		{
			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg20::DoParseMessage]",LoggerSeverities.Debug);
			

			if(logger != null)
				logger.AddLog("[Msg20::DoParseMessage]" + _root.FirstChild.InnerText,LoggerSeverities.Debug);
			
			NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
			nfi.NumberDecimalSeparator = ".";

			foreach (XmlNode n in _root)
			{
				if(logger != null)
					logger.AddLog("[Msg20::DoParseMessage] " + n.Name ,LoggerSeverities.Debug);
			
				switch (n.Name)
				{
					case "u": m_lUnit = Convert.ToInt32(n.InnerText); break;
					case "d1": m_dtDateIni = n.InnerText; break;
					case "d2": m_dtDateEnd = n.InnerText; break;
					case "c": m_lColNum= Convert.ToInt32(n.InnerText); break;
					case "q": m_lBackColTotal= Convert.ToInt32(n.InnerText); break;
					case "v": m_iMsgVersion= Convert.ToInt32(n.InnerText); break;
					case "rf": m_dRepFactor= double.Parse(n.InnerText,nfi); break;
					case "cs": m_strCoinSymbol= n.InnerText; break;
					
					case "CA": 
						
						foreach(XmlNode nodeCash in n.ChildNodes)
						{
							switch (nodeCash.Name)
							{
								case "P":
									foreach (XmlNode nodeCashPark in nodeCash.ChildNodes)
									{
										switch (nodeCashPark.Name)
										{
											case "q": m_nCashPark = Convert.ToInt32(nodeCashPark.InnerText); break;
											case "o": m_nCashParkOps = Convert.ToInt32(nodeCashPark.InnerText); break;
										}
									}
									break;
								case "F":
									foreach (XmlNode nodeCashFine in nodeCash.ChildNodes)
									{
										switch (nodeCashFine.Name)
										{
											case "q": m_nCashFine = Convert.ToInt32(nodeCashFine.InnerText); break;
											case "o": m_nCashFineOps = Convert.ToInt32(nodeCashFine.InnerText); break;
										}
									}
									break;
								case "RC":
									foreach (XmlNode nodeCashRchge in nodeCash.ChildNodes)
									{
										switch (nodeCashRchge.Name)
										{
											case "q": m_nCashRecharge = Convert.ToInt32(nodeCashRchge.InnerText); break;
											case "o": m_nCashRechargeOps = Convert.ToInt32(nodeCashRchge.InnerText); break;
										}
									}
									break;
								case "RP":
									foreach (XmlNode nodeCashResPay in nodeCash.ChildNodes)
									{
										switch (nodeCashResPay.Name)
										{
											case "q": m_nCashResPay = Convert.ToInt32(nodeCashResPay.InnerText); break;
											case "o": m_nCashResPayOps = Convert.ToInt32(nodeCashResPay.InnerText); break;
										}
									}
									break;
								case "PR":
									foreach (XmlNode nodeCashPowerRchge in nodeCash.ChildNodes)
									{
										switch (nodeCashPowerRchge.Name)
										{
											case "q": m_nCashPowerRecharge = Convert.ToInt32(nodeCashPowerRchge.InnerText); break;
											case "o": m_nCashPowerRechargeOps = Convert.ToInt32(nodeCashPowerRchge.InnerText); break;
										}
									}
									break;
								case "BI":
									foreach (XmlNode nodeCashBycing in nodeCash.ChildNodes)
									{
										switch (nodeCashBycing.Name)
										{
											case "q": m_nCashBycing = Convert.ToInt32(nodeCashBycing.InnerText); break;
											case "o": m_nCashBycingOps = Convert.ToInt32(nodeCashBycing.InnerText); break;
										}
									}
									break;
								case "T":
									foreach (XmlNode nodeCashTotal in nodeCash.ChildNodes)
									{
										switch (nodeCashTotal.Name)
										{
											case "q": m_nCashTotal = Convert.ToInt32(nodeCashTotal.InnerText); break;
											case "o": m_nCashTotalOps = Convert.ToInt32(nodeCashTotal.InnerText); break;
										}
									}
									break;					
								case "CO":

									m_CoinList = new stCoinData[MAX_NUM_COINS];	
									for(int i=0; i<MAX_NUM_COINS; i++)
									{
										m_CoinList[i].m_iCoinNumber=0;
										m_CoinList[i].m_iCoinValue=0;									
									}

									if (m_iMsgVersion==1)
									{
										m_dRepFactor = 0.01;
										m_strCoinSymbol = "EUR";

										foreach (XmlNode nodeCashCoins in nodeCash.ChildNodes)
										{
											switch (nodeCashCoins.Name)
											{
												case "_005": 
													m_CoinList[0].m_iCoinValue=5;	
													m_CoinList[0].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;

												case "_010": 
													m_CoinList[1].m_iCoinValue=10;	
													m_CoinList[1].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;

												case "_020": 
													m_CoinList[2].m_iCoinValue=20;	
													m_CoinList[2].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;

												case "_050": 
													m_CoinList[3].m_iCoinValue=50;	
													m_CoinList[3].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;

												case "_100": 
													m_CoinList[4].m_iCoinValue=100;	
													m_CoinList[4].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;

												case "_200": 
													m_CoinList[5].m_iCoinValue=200;	
													m_CoinList[5].m_iCoinNumber=Convert.ToInt32(nodeCashCoins.InnerText);
													break;


											}
										}
									}
									else if (m_iMsgVersion==2)
									{

										foreach (XmlNode nodeCashCoins in nodeCash.ChildNodes)
										{
											int iIndex=Convert.ToInt32(nodeCashCoins.Name.Substring(1));
											string strField=nodeCashCoins.Name.Substring(0,1);

											try
											{
												if (strField=="v")
												{
													m_CoinList[iIndex].m_iCoinValue	= Convert.ToInt32(nodeCashCoins.InnerText);
												}
												else if (strField=="q")
												{
													m_CoinList[iIndex].m_iCoinNumber	= Convert.ToInt32(nodeCashCoins.InnerText);
												}
											}
											catch
											{
											}
										}
									}
										
									break;					
								}
							
						}
						
						break;
					case "CR":
						
						foreach(XmlNode nodeCrCard in n.ChildNodes)
						{
							switch (nodeCrCard.Name)
							{
								case "P":
									foreach (XmlNode nodeCrCardPark in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardPark.Name)
										{
											case "q": m_nCrCardPark = Convert.ToInt32(nodeCrCardPark.InnerText); break;
											case "o": m_nCrCardParkOps = Convert.ToInt32(nodeCrCardPark.InnerText); break;
										}
									}
									break;
								case "F":
									foreach (XmlNode nodeCrCardFine in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardFine.Name)
										{
											case "q": m_nCrCardFine = Convert.ToInt32(nodeCrCardFine.InnerText); break;
											case "o": m_nCrCardFineOps = Convert.ToInt32(nodeCrCardFine.InnerText); break;
										}
									}
									break;
								case "RC":
									foreach (XmlNode nodeCrCardRchge in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardRchge.Name)
										{
											case "q": m_nCrCardRecharge = Convert.ToInt32(nodeCrCardRchge.InnerText); break;
											case "o": m_nCrCardRechargeOps = Convert.ToInt32(nodeCrCardRchge.InnerText); break;
										}
									}
									break;

								case "RP":
									foreach (XmlNode nodeCrCardResPay in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardResPay.Name)
										{
											case "q": m_nCrCardResPay = Convert.ToInt32(nodeCrCardResPay.InnerText); break;
											case "o": m_nCrCardResPayOps = Convert.ToInt32(nodeCrCardResPay.InnerText); break;
										}
									}
									break;
								case "PR":
									foreach (XmlNode nodeCrCardPowerRchge in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardPowerRchge.Name)
										{
											case "q": m_nCrCardPowerRecharge = Convert.ToInt32(nodeCrCardPowerRchge.InnerText); break;
											case "o": m_nCrCardPowerRechargeOps = Convert.ToInt32(nodeCrCardPowerRchge.InnerText); break;
										}
									}
									break;
								case "BI":
									foreach (XmlNode nodeCrCardBycing in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardBycing.Name)
										{
											case "q": m_nCrCardBycing = Convert.ToInt32(nodeCrCardBycing.InnerText); break;
											case "o": m_nCrCardBycingOps = Convert.ToInt32(nodeCrCardBycing.InnerText); break;
										}
									}
									break;

								case "T":
									foreach (XmlNode nodeCrCardTotal in nodeCrCard.ChildNodes)
									{
										switch (nodeCrCardTotal.Name)
										{
											case "q": m_nCrCardTotal = Convert.ToInt32(nodeCrCardTotal.InnerText); break;
											case "o": m_nCrCardTotalOps = Convert.ToInt32(nodeCrCardTotal.InnerText); break;
										}
									}
									break;							
							}
							
						}
						break;

					case "CC":
						
						foreach(XmlNode nodeChCard in n.ChildNodes)
						{
							switch (nodeChCard.Name)
							{
								case "P":
									foreach (XmlNode nodeChCardPark in nodeChCard.ChildNodes)
									{
										switch (nodeChCardPark.Name)
										{
											case "q": m_nChCardPark = Convert.ToInt32(nodeChCardPark.InnerText); break;
											case "o": m_nChCardParkOps = Convert.ToInt32(nodeChCardPark.InnerText); break;
										}
									}
									break;
								case "F":
									foreach (XmlNode nodeChCardFine in nodeChCard.ChildNodes)
									{
										switch (nodeChCardFine.Name)
										{
											case "q": m_nChCardFine = Convert.ToInt32(nodeChCardFine.InnerText); break;
											case "o": m_nChCardFineOps = Convert.ToInt32(nodeChCardFine.InnerText); break;
										}
									}
									break;
								case "RT":
									foreach (XmlNode nodeChCardReturn in nodeChCard.ChildNodes)
									{
										switch (nodeChCardReturn.Name)
										{
											case "q": m_nChCardReturn = Convert.ToInt32(nodeChCardReturn.InnerText); break;
											case "o": m_nChCardReturnOps = Convert.ToInt32(nodeChCardReturn.InnerText); break;
										}
									}
									break;
								case "RP":
									foreach (XmlNode nodeChCardResPay in nodeChCard.ChildNodes)
									{
										switch (nodeChCardResPay.Name)
										{
											case "q": m_nChCardResPay = Convert.ToInt32(nodeChCardResPay.InnerText); break;
											case "o": m_nChCardResPayOps = Convert.ToInt32(nodeChCardResPay.InnerText); break;
										}
									}
									break;
								case "PR":
									foreach (XmlNode nodeChCardPowerRchge in nodeChCard.ChildNodes)
									{
										switch (nodeChCardPowerRchge.Name)
										{
											case "q": m_nChCardPowerRecharge = Convert.ToInt32(nodeChCardPowerRchge.InnerText); break;
											case "o": m_nChCardPowerRechargeOps = Convert.ToInt32(nodeChCardPowerRchge.InnerText); break;
										}
									}
									break;
								case "BI":
									foreach (XmlNode nodeChCardBycing in nodeChCard.ChildNodes)
									{
										switch (nodeChCardBycing.Name)
										{
											case "q": m_nChCardBycing = Convert.ToInt32(nodeChCardBycing.InnerText); break;
											case "o": m_nChCardBycingOps = Convert.ToInt32(nodeChCardBycing.InnerText); break;
										}
									}
									break;


								case "T":
									foreach (XmlNode nodeChCardTotal in nodeChCard.ChildNodes)
									{
										switch (nodeChCardTotal.Name)
										{
											case "q": m_nChCardTotal = Convert.ToInt32(nodeChCardTotal.InnerText); break;
											case "o": m_nChCardTotalOps = Convert.ToInt32(nodeChCardTotal.InnerText); break;
										}
									}
									break;							
							}
							
						}
						break;
					
					case "CMI":
						
						foreach(XmlNode nodeCMI in n.ChildNodes)
						{
							switch (nodeCMI.Name)
							{
								case "lt_050": m_nCMI_050 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_050": m_nCMI_050_075 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_075": m_nCMI_075_100 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_100": m_nCMI_100_150 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_150": m_nCMI_150_200 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_200": m_nCMI_200_250 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_250": m_nCMI_250_300 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_300": m_nCMI_300_400 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "_400": m_nCMI_400_500 = Convert.ToInt32(nodeCMI.InnerText); break;
								case "mt_500": m_nCMI_500 = Convert.ToInt32(nodeCMI.InnerText); break;
							}
						}
						break;

					case "CTI":
						
						foreach(XmlNode nodeCTI in n.ChildNodes)
						{
							switch (nodeCTI.Name)
							{
								case "lt_1000": m_nCTI_1000 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_1000": m_nCTI_1000_1200 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_1200": m_nCTI_1200_1400 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_1400": m_nCTI_1400_1600 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_1600": m_nCTI_1600_1800 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_1800": m_nCTI_1800_2000 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_2000": m_nCTI_2000_2200 = Convert.ToInt32(nodeCTI.InnerText); break;
								case "_2200": m_nCTI_2200_0000 = Convert.ToInt32(nodeCTI.InnerText); break;
							}
						}
						break;

					case "CMCI":
						foreach(XmlNode nodeCTI in n.ChildNodes)
						{
							switch (nodeCTI.Name)
							{
								case "n": m_nCouponsNum = Convert.ToInt32(nodeCTI.InnerText); break;
								case "t": m_nCouponsFreeTime = Convert.ToInt32(nodeCTI.InnerText); break;
								case "q": m_nCouponsFreeAmount = Convert.ToInt32(nodeCTI.InnerText); break;
							}
						}
						break;


				}
			}	
		}


		#endregion

	}
	/// <summary>
	///  Consulta Estado
	/// </summary>
	/// 
	sealed public class Msg21 
	{
		
		#region Variables, creation and parsing

		private long m_lUnit = -1;
		private int m_LogicStatePDM = -1;
		private int m_StateMachinePDM = -1;
		private long m_lAlarms = -1;
		private int m_BateryLevel = -1;
		private int m_StateSolarPanel = -1;
		private XmlDocument m_root = null;

		
		public Msg21()
		{

		}
		/// <summary>
		/// Constructs a new msg21 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg21(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		
		public long UnitSnd
		{
			get
			{
				return this.m_lUnit;
			}
			set
			{
				this.m_lUnit = value;
			}
		}

		public int LogicalStatus
		{
			get
			{
				return m_LogicStatePDM;
			}
			set
			{
				m_LogicStatePDM = value;
			}
		}

		public void DoParseMessage()
		{
		
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "l": m_LogicStatePDM = Convert.ToInt32(n.InnerText); break;
					case "s": m_StateMachinePDM = Convert.ToInt32(n.InnerText); break;
					case "a": m_lAlarms = Convert.ToInt32(n.InnerText); break;
					case "b": m_BateryLevel = Convert.ToInt32(n.InnerText); break;
					case "ps": m_StateSolarPanel = Convert.ToInt32(n.InnerText); break;
				}
			}	
		}

		public override string ToString()
		{
			string szResult = "";
			szResult = "<p><m21 id=\"" + m_lUnit+ "\"></m21></p>"; 
			return szResult;
		}

		#endregion

	}



	
	/// <summary>
	/// Forzar Estado
	/// </summary>
	sealed public class Msg22
	{
		
		#region Variables, creation and parsing


		private int m_LogicStatePDM = -1;
		private int m_StateMachinePDM = -1;
		private	 long m_TypeMessage = -1;
		private  long   m_lUnit = -1;
		private  DateTime   m_dtReplyDate = DateTime.MinValue;
		private XmlDocument m_root = null;


		public long UnitSnd
		{
			get
			{
				return this.m_lUnit;
			}
			set
			{
				this.m_lUnit = value;
			}
		}


		public Msg22()  
		{
			
		}
		/// <summary>
		/// Constructs a new Msg28 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg22(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		
		public DateTime ReplyDate
		{
			get
			{
				return m_dtReplyDate;
			}
			set 
			{
				m_dtReplyDate = value;
			}
		}
		public void DoParseMessage()
		{
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "t":	m_TypeMessage = Convert.ToInt32(n.InnerText); break;
					case "d":	m_dtReplyDate = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					
				}
			}	
			
		}
		public override string  ToString()
		{
			string szResult = "";
			szResult = "<p><m22 id=\"" + m_lUnit+ "\"></m22></p>"; 
			return szResult;
		}

		#endregion

	}


	/// <summary>
	/// Enviar mensaje de salida
	/// </summary>
	sealed public class Msg24 : MsgCommand
	{
		
		#region Variables, creation and parsing


		private  string m_szMessage = "";
		private  long   m_lIdentifier = -1;
		private  long   m_lType = -1;
		private  long   m_lUnit = 34;
		private XmlDocument m_root = null;

		/// <summary>
		/// Constructs a new msg21 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg24(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		
		public Msg24()  
		{
		
		}
		
		public string Message
		{
			get
			{
				return m_szMessage;
			}
			set 
			{
				m_szMessage = value;
			}
		}
		public long Identifier
		{
			get
			{
				return m_lIdentifier;
			}
			set 
			{
				m_lIdentifier = value;
			}

		}

		public long MessageType
		{
			get
			{
				return m_lType;
			}
			set 
			{
				m_lType = value;
			}

		}

		public void DoParseMessage()
		{
		
			
		}
		public override string  ToString()
		{
			System.Text.StringBuilder  szResult = new System.Text.StringBuilder();
			szResult.Append("<p>");
			string szDummy = "";
			szDummy = "<m24 id=\"" + this.m_lUnit + "\">"; 
			szResult.Append(szDummy);
			szDummy = "<t>" + this.m_szMessage + "</t>";
			szResult.Append(szDummy);
			szDummy = "<l>" + this.m_lIdentifier + "</l>";
			szResult.Append(szDummy);
			szDummy = "<m>" + this.m_lType + "</m>";
			szResult.Append(szDummy);
			szResult.Append("</m24>");
			szResult.Append("</p>");
			
			return szResult.ToString();
		}

		#endregion

	}

	/// <summary>
	/// Consulta version Software
	/// </summary>
	sealed public class Msg26 : MsgCommand
	{
		
		#region Variables, creation and parsing
private long m_lUnit = -1;
		private string m_szVersion = "";
		private XmlDocument m_root = null;

		/// <summary>
		/// Constructs a new msg21 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg26(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		public Msg26()  
		{
			
		}

		public void DoParseMessage()
		{
		
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "v": m_szVersion = n.InnerText; break;
				}
			}	
		}


		public override string  ToString()
		{
			string szResult = "";
			szResult = "<p><m26 id=\"" + this.UnitSnd + "\"></m26></p>"; 
			return szResult;
		}

		#endregion

	}

	/// <summary>
	/// Consulta hora
	/// </summary>
	sealed public class Msg27 :MsgCommand
	{
		
		#region Variables, creation and parsing


		private  DateTime m_dtDate = DateTime.MinValue;
		private XmlDocument m_root = null;
		

		/// <summary>
		/// Constructs a new msg21 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg27(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		
		public Msg27()  
		{
			
		}

		public DateTime DateIn
		{
			get
			{
				return m_dtDate;
			}
			set 
			{
				m_dtDate = value;
			}
		}
		public void DoParseMessage()
		{
		
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "d": m_dtDate = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					
				}
			}	
		}


		public override string  ToString()
		{
			string szResult = "";
			szResult = "<p><m27 id=\"" + this.UnitSnd + "\"></m27></p>"; 
			return szResult;
		}
		#endregion

	}
	/// <summary>
	/// Forzar Hora
	/// </summary>
	sealed public class Msg28 : MsgCommand
	{
		
		#region Variables, creation and parsing


		private  bool		m_bFixedHour  = false;
		private  DateTime   m_dtOldDate = DateTime.MinValue;
		private  DateTime   m_dtNewDate = DateTime.MinValue;
		private XmlDocument m_root = null;

		/// <summary>
		/// Constructs a new Msg28 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg28(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		public Msg28()  
		{
			
		}
		public DateTime NewDateIn
		{
			get
			{
				return m_dtNewDate;
			}
			set 
			{
				m_dtNewDate = value;
			}
		}
		
		public void DoParseMessage()
		{
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "t":	m_bFixedHour = (n.InnerText == "1"); break;
					case "od":	m_dtOldDate = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "nd":	m_dtNewDate = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
				}
			}	
			
		}
		public override string  ToString()
		{
			System.Text.StringBuilder  szResult = new System.Text.StringBuilder();
			szResult.Append("<p>");
			string szDummy = "";
			szDummy = "<m28 id=\"" + this.UnitSnd + "\">"; 
			szResult.Append(szDummy);
			szDummy = "<nd>" + OPS.Comm.Dtx.DtxToString(this.m_dtNewDate) + "</nd>";
			szResult.Append(szDummy);
			szResult.Append("</m28>");
			szResult.Append("</p>");
			
			return szResult.ToString();
		}

		#endregion

	}

	

	/// <summary>
	/// Forzar Estado
	/// </summary>
	sealed public class Msg29 : MsgCommand
	{
		
		#region Variables, creation and parsing


		private int m_LogicStatePDM = -1;
		private int m_StateMachinePDM = -1;
		private	 long m_TypeMessage = -1;
		private  DateTime   m_dtReplyDate = DateTime.MinValue;
		private XmlDocument m_root = null;

		/// <summary>
		/// Constructs a new Msg28 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg29(XmlDocument msgXml)  
		{
			if(msgXml != null)
			{
				m_root = msgXml;
			}
		}
		public Msg29()  
		{
			
		}

		public int LogicSatePDM
		{
			get
			{
				return m_LogicStatePDM;
			}
			set
			{
				m_LogicStatePDM = value;
			}
		}

		public int StateMachinePDM
		{
			get
			{
				return m_StateMachinePDM;
			}
			set
			{
				m_StateMachinePDM = value;
			}
		}

		public DateTime ReplyDate
		{
			get
			{
				return m_dtReplyDate;
			}
			set 
			{
				m_dtReplyDate = value;
			}
		}
		
		/// <summary>
		/// DoParseMessage : Parsing the result
		/// t 
		/// </summary>
		public void DoParseMessage()
		{
			XmlNode child = m_root.FirstChild;
			XmlNode child2 = child.FirstChild;
	
			foreach (XmlNode n in child2.ChildNodes)
			{
				switch (n.Name)
				{
					case "t":	m_TypeMessage = Convert.ToInt32(n.InnerText); break;
					case "d":	m_dtReplyDate = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					
				}
			}	
			
		}
		/// <summary>
		/// Converts a Message29 to XML
		/// </summary>
		/// <returns></returns>
		public override string  ToString()
		{
			System.Text.StringBuilder  szResult = new System.Text.StringBuilder();
			szResult.Append("<p>");
			string szDummy = "";
			szDummy = "<m29 id=\"" + this.UnitSnd + "\">"; 
			szResult.Append(szDummy);
			szResult.Append(szDummy);
			szDummy = "<l>" + this.m_LogicStatePDM + "</l>";
			szResult.Append(szDummy);
			szDummy = "<s>" + this.m_StateMachinePDM  + "</s>";
			szResult.Append(szDummy);
			szResult.Append("</m29>");
			szResult.Append("</p>");
			
			return szResult.ToString();
		}

		#endregion

	}


}

