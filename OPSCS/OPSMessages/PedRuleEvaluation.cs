using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Comm;
using OPS.Components.Data;
using OPS.Components;
using System.Data;
using System.Collections;
using CS_OPS_TesM1;
//using Oracle.DataAccess.Client;
using System.Globalization;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages//OPSMessages
{
	/// <summary>
	/// Summary description for PedRuleEvaluation.
	/// </summary>
	public class PedRuleEvaluation
	{
		public const int NoRule = -1;

		private bool StringToByte(string binaryStr,ref byte byResult)
		{
			bool bRslt = true;
			
			try
			{
				if(binaryStr.Length <= 8)
				{
					byResult = 0;
					int i = 0;
					foreach( char c in binaryStr )
					{
						if( c == '1')
						{
							byResult += (byte)(Math.Pow(2 , i));
						}

						i++;
					}
				}
			}
			catch(Exception e)
			{
				bRslt = false;
			}
			return bRslt;
		}
		private string DayOfWeekMask(System.DayOfWeek dow)
		{
			string mask = "0000000";
			switch(dow)
			{
				case System.DayOfWeek.Friday:
				{
					mask = "0000100";
					break;
				}
				case System.DayOfWeek.Monday:
				{
					mask = "1000000";
					break;
				}
				case System.DayOfWeek.Saturday:
				{
					mask = "0000010";
					break;
				}
				case System.DayOfWeek.Sunday:
				{
					mask = "0000001";
					break;
				}
				case System.DayOfWeek.Thursday:
				{
					mask = "0001000";
					break;
				}
				case System.DayOfWeek.Tuesday:
				{
					mask = "0100000";
					break;
				}
				case System.DayOfWeek.Wednesday:
				{
					mask = "0010000";
					break;
				}
				default:
				{
					mask = "0000000";
					break;
				}
			}

			return mask;
		}
		private bool  IsPredefinedDay(DateTime dt, ref int iDayTypeCode )
		{
			//select DAY_DDAY_ID from days where DAY_DATE = to_date('28/07/2010','dd:mm:yyyy')
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			bool bResult =false;
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
					sqlQuery.AppendFormat("select DAY_DDAY_ID from days where DAY_DATE = to_date('{0}','dd:mm:yyyy')", dt.ToShortDateString());
					
					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("DAY_DDAY_ID");
						iDayTypeCode = dr.GetInt32(iOrdinal); 
						bResult = true;
					}
				}
			}
			catch(Exception e)
			{
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

			return bResult;

		}
		private string GetDayCode(int dday)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string	dayCode = String.Empty;
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
					sqlQuery.AppendFormat("select DDAY_CODE from DAYS_DEF where DDAY_ID = {0}", dday.ToString());
					
					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("DDAY_CODE");
						dayCode = dr.GetString(iOrdinal); 
					}
				}
			}
			catch(Exception e)
			{
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

			return dayCode;
		}
		private int GetTimMinutesMax(int iTimId)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			int	iMinutesMax = -1;
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
					sqlQuery.AppendFormat("select TIM_END from timetables where TIM_ID = {0}", iTimId.ToString() );

					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select TIM_END from timetables where TIM_ID = " + iTimId.ToString();
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("TIM_END");
						iMinutesMax = (int)dr.GetDecimal(iOrdinal); 
					}
				}
			}
			catch(Exception e)
			{
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

			return iMinutesMax;

		}

		private int GetTimMinutesMin(int iTimId)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			int	iMinutesMin = -1;
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
					sqlQuery.AppendFormat("select TIM_INI from timetables where TIM_ID = {0}", iTimId.ToString() );

					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select TIM_INI from timetables where TIM_ID = " + iTimId.ToString();
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("TIM_INI");
						iMinutesMin = (int)dr.GetOracleDecimal(iOrdinal); 
					}
				}
			}
			catch(Exception e)
			{
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

			return iMinutesMin;

		}

		private bool IsTypeOfDay( DateTime dt,int dday )
		{
			bool bRslt = false;

			try

			{
				string dayCode = GetDayCode(dday);

				if( dayCode == "0000000" )
				{
					// Busaca en tabla days
					int iDayTypeCode = -1;
					if(IsPredefinedDay(dt, ref iDayTypeCode ))
					{
						if(iDayTypeCode == dday)
						{
							bRslt = true;
						}
						else
						{
							bRslt = false;
						}
					}
					else
					{
						bRslt = false;
					}
				}
				else
				{
					string dowMask = DayOfWeekMask(dt.DayOfWeek);
					
					byte byDowMask = 0;
					StringToByte(dowMask,ref byDowMask);
					
					byte byDayCode = 0;
					StringToByte(dayCode,ref byDayCode);
					
					
					if( (byDayCode & byDowMask) != 0)
					{
						bRslt = true;
					}
					else
					{
						bRslt = false;
					}
				}
			}
			catch(Exception e)
			{
			}
			return bRslt;
		}


		private bool EvalZonePlateRules(string plate, DateTime dt, ref int iTipoArticulo,int iUnitID,int iGrpID)
		{

			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			bool bValidRuleFound = false;

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
					sqlQuery.AppendFormat("select* from pedestrian_zone_plate_rules where PDPR_VEHICLEID= '{0}'", plate);

					oraCmd.CommandText = sqlQuery.ToString();

					//oraCmd.CommandText = "select* from pedestrian_zone_plate_rules where PDPR_VEHICLEID= '" + plate + "'";

					dr = oraCmd.ExecuteReader();	
			
				
					while(dr.Read())
					{
				
						bValidRuleFound = true;
					
						int iOrdinal = 0;
						if(iUnitID != -1 && iGrpID != -1 )
						{
							iOrdinal = dr.GetOrdinal("PDPR_UNI_ID");
							if( dr.IsDBNull(iOrdinal) == false )
							{
								if( iUnitID != int.Parse(dr.GetInt32(iOrdinal).ToString()))
								{
									bValidRuleFound = false;
								}
							}

							iOrdinal = dr.GetOrdinal("PDPR_GRP_ID");
							if(dr.IsDBNull(iOrdinal) == false)
							{
								if( iGrpID != int.Parse(dr.GetInt32(iOrdinal).ToString()))
								{
									bValidRuleFound = false;
								}
							}
						}
						else
						{
							bValidRuleFound = false; // A NO SER QUE SOLO TENGA UNA CÄMARA DE ENTRADA
						}

						iOrdinal = dr.GetOrdinal("PDPR_DDAY_ID");
						if(dr.IsDBNull(iOrdinal) == false)
						{
							int dday = int.Parse(dr.GetInt32(iOrdinal).ToString());
						
							if( IsTypeOfDay( dt, dday) == false )
							{
								bValidRuleFound = false;
							}
							else
							{
								bool bTimeCheck = false;

								iOrdinal = dr.GetOrdinal("PDPR_DATE_INI");
								if(dr.IsDBNull(iOrdinal) == false)
								{
									string date = dr.GetOracleValue(iOrdinal).ToString();
									date = date.Substring(0,10); // 10 es la longitud de DD/MM/YYYY
									DateTime dtInit = DateTime.Parse(date);

									if(dt < dtInit)
									{
										bValidRuleFound = false;
										bTimeCheck = false;
									}
									else
									{
										bTimeCheck = true;
									}
								}
								else
								{
									bTimeCheck = true;
								}

								iOrdinal = dr.GetOrdinal("PDPR_DATE_END");
								if(dr.IsDBNull(iOrdinal) == false)
								{
									string date = dr.GetOracleValue(iOrdinal).ToString();
									date = date.Substring(0,10); // 10 es la longitud de DD/MM/YYYY
									DateTime dtEnd = DateTime.Parse(date);
								
									if(dt > dtEnd)
									{
										bValidRuleFound = false;
										bTimeCheck = false;
									}
									else
									{
										bTimeCheck = true;
									}
								}
								else
								{
									bTimeCheck = true;
								}

								iOrdinal = dr.GetOrdinal("PDPR_TIM_ID");
								if(dr.IsDBNull(iOrdinal) == false)
								{
									int iTimId = int.Parse(dr.GetInt32(iOrdinal).ToString());
									if(bTimeCheck)
									{
										int iMinutes = dt.Minute + dt.Hour * 60;
										int iMinutesMin = GetTimMinutesMin(iTimId);
										int iMinutesMax = GetTimMinutesMax(iTimId);
							
										if(iMinutesMin <= iMinutes && iMinutes  <= iMinutesMax)
										{
										}
										else
										{
											bValidRuleFound = false;
										}
									}
								}
							}
						}
				
						if(bValidRuleFound == true)
						{
							iOrdinal = dr.GetOrdinal("PDPR_DART_ID");
							iTipoArticulo = dr.GetInt32(iOrdinal);
						}
					
					}
				}
			}
			catch(Exception e)
			{
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

			return bValidRuleFound;
		}
		
		private bool EvaluatePedZoneRules(string plate,DateTime dt,bool bTipoArticulo, int iTipoArticulo, ref int iRuleId)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			bool bRuleFound = false;
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
					

					if(bTipoArticulo) 
					{
						sqlQuery.AppendFormat("select * from pedestrian_zone_rules where PDR_DART_ID = {0}", iTipoArticulo);
						//oraCmd.CommandText = "select * from pedestrian_zone_rules where PDR_DART_ID is not null";
					}
					else
					{
						sqlQuery.Append("select * from pedestrian_zone_rules where PDR_DART_ID is null");
						//oraCmd.CommandText = "select * from pedestrian_zone_rules where PDR_DART_ID is null";
					}
					
					oraCmd.CommandText = sqlQuery.ToString();
					dr = oraCmd.ExecuteReader();	

					

					while(dr.Read() && bRuleFound == false)
					{

						bRuleFound = true;
						
						int iOrdinal = dr.GetOrdinal("PDR_DDAY_ID");
						
						if(dr.IsDBNull(iOrdinal) == false)
						{
							int dday = dr.GetInt32(iOrdinal);
							
							if( IsTypeOfDay( dt, dday) == false )
							{
								bRuleFound = false;
							}
							else
							{


								bool bTimeCheck = false;

								iOrdinal = dr.GetOrdinal("PDR_DATE_INI");
								
								if(dr.IsDBNull(iOrdinal) == false)
								{
									string date = dr.GetOracleValue(iOrdinal).ToString();
									date = date.Substring(0,10); // 10 es la longitud de DD/MM/YYYY
									DateTime dtInit = DateTime.Parse(date);
									//DateTime dtInit = DateTime.Parse(dr.GetString(iOrdinal).ToString());
									
									if(dt < dtInit)
									{
										bRuleFound = false;
										bTimeCheck = false;
									}
									else
									{
										bTimeCheck = true;
									}
								}
								else
								{
									bTimeCheck = true;
								}

								iOrdinal = dr.GetOrdinal("PDR_DATE_END");
								if(dr.IsDBNull(iOrdinal) == false)
								{
									string date = dr.GetOracleValue(iOrdinal).ToString();
									date = date.Substring(0,10); // 10 es la longitud de DD/MM/YYYY
									DateTime dtEnd = DateTime.Parse(date);
									//DateTime dtEnd = DateTime.Parse(dr.GetString(iOrdinal).ToString());

									if(dt > dtEnd)
									{
										bRuleFound = false;
										bTimeCheck = false;
									}
									else
									{
										bTimeCheck = true;
									}
								}
								else
								{
									bTimeCheck = true;
								}


								if(bTimeCheck)
								{
									iOrdinal = dr.GetOrdinal("PDR_TIM_ID");
									int iTimId = dr.GetInt32(iOrdinal);
									
									int iMinutes = dt.Minute + dt.Hour * 60;
									int iMinutesMin = GetTimMinutesMin(iTimId);
									int iMinutesMax = GetTimMinutesMax(iTimId);
									
									if(iMinutesMin <= iMinutes && iMinutes  <= iMinutesMax)
									{
									}
									else
									{
										bRuleFound = false;
									}
								}
							}
						}

						
						


						if( bRuleFound )
						{
							iOrdinal = dr.GetOrdinal("PDR_ID");
							iRuleId = dr.GetInt32(iOrdinal);
						}
						
					}
				}
			}
			catch(Exception e)
			{
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

			return bRuleFound;

		}

		
		public int EvaluateRules(string plate, DateTime dt, int iUnitID, int iGrpID)
		{
			int iRuleId = -1;

			try
			{
				int iTipoArticulo = -1;
				bool bTipoArticulo = EvalZonePlateRules(plate,dt, ref iTipoArticulo, iUnitID, iGrpID);
				
				// Buscar en PEDESTRIAN_ZONE_RULES 				
				bool bRule = EvaluatePedZoneRules(plate,dt,bTipoArticulo, iTipoArticulo, ref iRuleId);

				
			}
			catch(Exception e)
			{
			}
			

			return iRuleId;
			
		}
	}
}
