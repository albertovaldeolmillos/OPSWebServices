using System;
using System.Data;
using OPS.Components.Data;
using System.Collections;

using OTS.Framework.Collections;

namespace OPS.Components
{
	/// <summary>
	/// Contains methods to search for operations associated to vehicle, date and group
	/// </summary>
	public class CmpMoneyOffCoupons
	{
		#region Static stuff


		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static CmpMoneyOffCoupons()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			
		}

		#endregion

		/// <summary>
		/// Results codes for CmpMoneyOffCoupons::FindOperationsResults
		/// </summary>


		public enum MoneyOffCouponsResults
		{
			AlreadyUsed=-5,
			Cancelled = -4,
			NotStarted = -3,
			Expired = -2,
			NotActived = -1,			
			DontExist = 0,
			Usable = 1
		}

		public CmpMoneyOffCoupons()
		{			

		}




		public int InsertCouponsCustomer (IDbTransaction tran,string strDescription,string strAddress, 
					string strCity, string strZipcode, string strEmail, string strTelephone,ref uint uiId)
		{			
			CmpMoneyOffCouponsCustomersDB cmpCouponCustomer = new CmpMoneyOffCouponsCustomersDB();
			uiId=cmpCouponCustomer.GetMaximumPk(tran);

			return cmpCouponCustomer.InsertCouponsCustomer(tran, uiId, strDescription,strAddress, strCity, strZipcode, strEmail, strTelephone);
		
		}


		public int UpdateCouponsCustomer (IDbTransaction tran,uint uiId, string strDescription,string strAddress, 
			string strCity, string strZipcode, string strEmail, string strTelephone)
		{			
			CmpMoneyOffCouponsCustomersDB cmpCouponCustomer = new CmpMoneyOffCouponsCustomersDB();
		
			return cmpCouponCustomer.UpdateCouponsCustomer(tran, uiId, strDescription,strAddress, strCity, strZipcode, strEmail, strTelephone);
		
		}


		public int GenerateBooksOfCoupons(IDbTransaction tran,int iNumOfBookOfCoupons, 
										int iNumOfCouponsInABookOfCoupons,
										CmpMoneyOffCouponsDB.MoneyOffCouponsStates State,
										DateTime startDate, DateTime expDate, 
										DateTime actDate,uint uiCustomerId )
		{

			int result=-1;

			try
			{

				CmpMoneyOffCouponsDB cmpCoupons = new CmpMoneyOffCouponsDB();
				CmpMoneyOffBookOfCouponsDB cmpBookCoupons = new CmpMoneyOffBookOfCouponsDB();
				int iNumberOfMinutesGiven=-1;
				int iNumberOfCentsGiven=-1;
				int iRes=1;

				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				try
				{
					iNumberOfMinutesGiven = (int)appSettings.GetValue("MoneyOffCouponsDefaultNumberOfMinutesGiven", typeof(int));
				}
				catch
				{
					iNumberOfMinutesGiven=-1;				
				}

				try
				{
					iNumberOfCentsGiven = (int)appSettings.GetValue("MoneyOffCouponsDefaultNumberOfCentsGiven", typeof(int));
				}
				catch
				{
					iNumberOfCentsGiven=-1;				
				}

				int iNumOfCurrBooks=0;

				if (((iNumberOfMinutesGiven>0)&&(iNumberOfCentsGiven==-1))||
					((iNumberOfMinutesGiven==-1)&&(iNumberOfCentsGiven>0)))
				{

					if ((State==CmpMoneyOffCouponsDB.MoneyOffCouponsStates.PendingActivation)||(State==CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Actived))
					{
						if (iNumOfBookOfCoupons>0)
						{
							while ((iNumOfCurrBooks<iNumOfBookOfCoupons)&&(iRes>0))
							{	

								uint uiBookID=cmpBookCoupons.GetMaximumPk(tran)+1;
							
								if (iNumOfCouponsInABookOfCoupons>0)
								{
									if (cmpBookCoupons.InsertBookOfCoupons(tran, uiBookID)>0)
									{

										int iNumOfCurrCoupons=0;
										uint uiFirstCouponID=cmpCoupons.GetMaximumPk(tran)+1;
										uint uiCouponID=uiFirstCouponID;								
										string strCode="";

										while ((iNumOfCurrCoupons<iNumOfCouponsInABookOfCoupons)&&(iRes>0))
										{							
											strCode=GenerateUniqueCode(tran);
											if (strCode.Length>0)
											{
												iRes=cmpCoupons.InsertCoupon(tran, uiCouponID,strCode,iNumberOfMinutesGiven,iNumberOfCentsGiven,uiBookID);
											}
											else
											{
												iRes=-1;								
											}

											if ((iRes>0)&&(State==CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Actived))
											{
												iRes=SetCouponAsActived(tran,uiCouponID, startDate, expDate,actDate,uiCustomerId );

											}

											iNumOfCurrCoupons++;
											if ((iRes>0)&&(iNumOfCurrCoupons<iNumOfCouponsInABookOfCoupons))
											{
												uiCouponID= cmpCoupons.GetMaximumPk(tran)+1;
											}
										}


										if (iRes>0)
										{
											iRes=cmpBookCoupons.UpdateBookOfCoupons(tran,uiBookID,uiFirstCouponID,uiCouponID);

										}

									}
								}

								iNumOfCurrBooks++;
							}
							
							result=iRes;

						}
					}
				}			

			}
			catch
			{
				result=-1;

			}


			return result;
		}

//		public int InsertCoupon (IDbTransaction tran, string strCode, int iNumberOfMinutesGiven, 
//			int iNumberOfCentsGiven, uint uiIdBookofCoupons, ref uint uiId)
//		{			
//			CmpMoneyOffCouponsDB cmpCoupon = new CmpMoneyOffCouponsDB();
//			uiId=cmpCoupon.GetMaximumPk(tran);
//
//			return cmpCoupon.InsertCoupon( uiId, strCode, iNumberOfMinutesGiven, iNumberOfCentsGiven, uiIdBookofCoupons );
//		
//		}

		public  CmpMoneyOffCouponsDB.MoneyOffCouponsStates GetMoneyOffCouponState(string strCouponCode, ref DateTime dtStartDate, ref DateTime dtExpDate,
																				  ref int iNumberOfMinutesGiven,ref int iNumberOfCentsGiven,
																				  ref uint uiID )
		{

			CmpMoneyOffCouponsDB.MoneyOffCouponsStates result=CmpMoneyOffCouponsDB.MoneyOffCouponsStates.PendingActivation;
			dtStartDate = DateTime.MaxValue;
			dtExpDate = DateTime.MinValue;
			iNumberOfMinutesGiven=-1;
			iNumberOfCentsGiven=-1;
			uiID=0;

			try
			{

				CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();
				DataTable dt = cmp.GetData (null, "COUP_CODE =  @MONEYOFF_COUPON.COUP_CODE@", 
					new object[] { strCouponCode});


				if (dt.Rows.Count > 0)
				{
					CmpMoneyOffCouponsDB.MoneyOffCouponsStates state = 	(CmpMoneyOffCouponsDB.MoneyOffCouponsStates)Convert.ToInt32(dt.Rows[0]["COUP_STATE"]);
					uiID = Convert.ToUInt32(dt.Rows[0]["COUP_ID"]);
				
					if(dt.Rows[0]["COUP_START_DATE"]  != DBNull.Value)
						dtStartDate = Convert.ToDateTime(dt.Rows[0]["COUP_START_DATE"]);

					if(dt.Rows[0]["COUP_EXP_DATE"]  != DBNull.Value)
						dtExpDate = Convert.ToDateTime(dt.Rows[0]["COUP_EXP_DATE"]);
			
					if(dt.Rows[0]["COUP_DISCOUNT_TIME"]  != DBNull.Value)
						iNumberOfMinutesGiven = Convert.ToInt32(dt.Rows[0]["COUP_DISCOUNT_TIME"]);

					if(dt.Rows[0]["COUP_DISCOUNT_MONEY"]  != DBNull.Value)
						iNumberOfCentsGiven = Convert.ToInt32(dt.Rows[0]["COUP_DISCOUNT_MONEY"]);

				}
			}
			catch
			{

			}

			return result;

		}

		public  MoneyOffCouponsResults IsMoneyOffCouponUsable(string strCouponCode, DateTime dtTime,  ref int iNumberOfMinutesGiven,
															ref int iNumberOfCentsGiven,ref uint uiID )
		{
			MoneyOffCouponsResults result=MoneyOffCouponsResults.DontExist;
			iNumberOfMinutesGiven=-1;
			iNumberOfCentsGiven=-1;
			uiID=0;


			try
			{

				CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();
				DataTable dt = cmp.GetData (null, "COUP_CODE =  @MONEYOFF_COUPON.COUP_CODE@", 
					new object[] { strCouponCode});


				if (dt.Rows.Count > 0)
				{
					CmpMoneyOffCouponsDB.MoneyOffCouponsStates state = 	(CmpMoneyOffCouponsDB.MoneyOffCouponsStates)Convert.ToInt32(dt.Rows[0]["COUP_STATE"]);

					switch (state)
					{
						case CmpMoneyOffCouponsDB.MoneyOffCouponsStates.PendingActivation:
							result= MoneyOffCouponsResults.NotActived;
							break;
						case CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Used:
							result= MoneyOffCouponsResults.AlreadyUsed;
							break;
						case CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Cancelled:
							result= MoneyOffCouponsResults.Cancelled;
							break;
						case CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Actived:
						{
							DateTime dtStartDate;
							DateTime dtExpDate;

							if(dt.Rows[0]["COUP_START_DATE"]  != DBNull.Value)
								dtStartDate = Convert.ToDateTime(dt.Rows[0]["COUP_START_DATE"]);
							else
								dtStartDate = DateTime.MaxValue;

							if(dt.Rows[0]["COUP_EXP_DATE"]  != DBNull.Value)
								dtExpDate = Convert.ToDateTime(dt.Rows[0]["COUP_EXP_DATE"]);
							else
								dtExpDate = DateTime.MinValue;

							if (dtTime>dtStartDate)
							{
								if (dtTime<dtExpDate)
								{

									result= MoneyOffCouponsResults.Usable;
									uiID = Convert.ToUInt32(dt.Rows[0]["COUP_ID"]);

									if(dt.Rows[0]["COUP_DISCOUNT_TIME"]  != DBNull.Value)
										iNumberOfMinutesGiven = Convert.ToInt32(dt.Rows[0]["COUP_DISCOUNT_TIME"]);
									else
										iNumberOfMinutesGiven = -1;

									if(dt.Rows[0]["COUP_DISCOUNT_MONEY"]  != DBNull.Value)
										iNumberOfCentsGiven = Convert.ToInt32(dt.Rows[0]["COUP_DISCOUNT_MONEY"]);
									else
										iNumberOfCentsGiven = -1;


								}
								else
								{
									result= MoneyOffCouponsResults.Expired;
								}

							}
							else
							{
								result= MoneyOffCouponsResults.NotStarted;
							}							
								
						}
						break;

						
					}

				}
			}
			catch
			{

			}

			return result;
		}



		public int SetCouponAsActived(IDbTransaction tran,uint uiID, DateTime startDate, DateTime expDate, 
										DateTime actDate,uint uiCustomerId )
		{

			int result=-1;

			try
			{

				CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();

				result = cmp.UpdateState(tran,uiID,(int)CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Actived,startDate,expDate,actDate,DateTime.MinValue,DateTime.MinValue,uiCustomerId,"",-1);

			}
			catch
			{

			}

			return result;

		}

		public int SetCouponAsUsed(IDbTransaction tran,uint uiID, DateTime useDate,  string strUsePlate, int iDPayId)
		{
			int result=-1;

			try
			{

				CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();

				result = cmp.UpdateState(tran,uiID,(int)CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Used,DateTime.MinValue,
										DateTime.MinValue,DateTime.MinValue,useDate,DateTime.MinValue,0,
										strUsePlate,iDPayId);

			}
			catch
			{

			}

			return result;
		}

		public int SetCouponAsCancelled(IDbTransaction tran,uint uiID,  DateTime cancelDate)
		{
			int result=-1;

			try
			{

				CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();

				result = cmp.UpdateState(tran,uiID,(int)CmpMoneyOffCouponsDB.MoneyOffCouponsStates.Cancelled,DateTime.MinValue,
					DateTime.MinValue,DateTime.MinValue,DateTime.MinValue,cancelDate,0,
					"",-1);

			}
			catch
			{

			}

			return result;
		}

		protected  string GenerateUniqueCode(IDbTransaction tran)
		{
			int iCodeLength=-1;
			string strCodeValidChars="";
			int iMaxRetries=-1;
			int iCurrRetries=0;
			
			string strRes="";
			

			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				try
				{
					iCodeLength = (int)appSettings.GetValue("MoneyOffCouponsCodeLength", typeof(int));
				}
				catch
				{
					iCodeLength=5;				
				}

				try
				{
					strCodeValidChars = (string)appSettings.GetValue("MoneyOffCouponsCodeValidChars", typeof(string));
				}
				catch
				{
					strCodeValidChars="";				
				}

				try
				{
					iMaxRetries = (int)appSettings.GetValue("MoneyOffCouponsRetriesToObtainUniqueCode", typeof(int));
				}
				catch
				{
					iMaxRetries=5;				
				}


				if (strCodeValidChars.Length>0)
				{

					Random rand= new Random (Convert.ToInt32(DateTime.Now.Ticks%Convert.ToInt64(Int32.MaxValue)));
					CmpMoneyOffCouponsDB cmp = new CmpMoneyOffCouponsDB();
					int iChar;
			
					do
					{
				
						for (int i=0; i<iCodeLength; i++)
						{
							iChar=rand.Next(0,strCodeValidChars.Length);
							strRes+=strCodeValidChars[iChar];
						}

						strRes = strRes.ToUpper();
						

						if (cmp.ExistCode(tran,strRes))
						{
							strRes="";
							iCurrRetries++;
						}


					}
					while ((iCurrRetries<iMaxRetries)&&(strRes.Length==0));


				}


			}
			catch
			{

			}

			return strRes;
		}




	}
}
