using System;
using OPS.Comm;
//using Oracle.DataAccess.Client;
using CS_OPS_TesM1;
using OPS.Components.Data;
using System.Xml;
using System.Collections;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;

namespace RealDurationCalculator
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class OPSRealDurationCalculator
	{
		static ILogger _logger;
		private static string DB_CONNECTION_STRING;
		private static OracleConnection OPSBDCon=null;

		private static string		_vehicleId		= null;
		private static int			_operationId	= -1;
		private static int			_articleDefId	= -1;
		private static int			_groupId		= -1;
		private static int			_unitId;
		private static int			_dopeid;
		private static DateTime		_dateMov		= DateTime.MinValue;
		private static DateTime		_dateIni		= DateTime.MinValue;
		private static DateTime		_dateEnd		= DateTime.MinValue;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[MTAThread]
		static void Main(string[] args)
		{


			if (Init())
			{
				CalculateOperationsDuration();
			}

			Logger_AddLogMessage("Exiting application", LoggerSeverities.Info);
		}

		private static bool Init()
		{
			bool bReturn=false;
			try
			{
                //_logger = new FileLogger(LoggerSeverities.Debug, "c:\\{0}_OPSRealDurationCalculator.log");

                _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);

                LoggerSeverities logSeverity = ReadLoggerSeverity();
				Logger_AddLogMessage(string.Format("Setting logger severity to: {0} ", logSeverity.ToString()), LoggerSeverities.Info);
				
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				DB_CONNECTION_STRING = (string)appSettings.GetValue("ConnectionString", typeof(string));
				Logger_AddLogMessage(string.Format("Connection String: {0} ", DB_CONNECTION_STRING.ToString()), LoggerSeverities.Info);
				bReturn=true;

			}
			catch (Exception e)
			{
				Logger_AddLogException(e);
				bReturn=false;
			}

			return bReturn;
		}

		private static void Logger_AddLogMessage(string msg, LoggerSeverities severity)
		{
			_logger.AddLog(msg, severity);
		}

		private static void Logger_AddLogException(Exception ex)
		{
			_logger.AddLog(ex);
		}

		/// <summary>
		/// Reads logger severity level from the app.config file
		/// </summary>
		private static LoggerSeverities ReadLoggerSeverity()
		{
			LoggerSeverities logSeverity = LoggerSeverities.Error;
			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				string logLevel = (string)appSettings.GetValue("LoggerSeverity", typeof(string));
				if (logLevel != null)
				{
					logSeverity = (LoggerSeverities) Enum.Parse(
						typeof(LoggerSeverities), logLevel, true);
				}
			}
			catch {}

			return logSeverity;
		}



		public static bool CalculateOperationsDuration()
		{
			bool bReturn=true;
			OracleDataReader	dr		= null;
			OracleCommand		selOpsCmd = null;
			int					iRealTime=-1;
			int					iQuantity=-1;
			int					iOpeBaseId=-1;
			
			try
			{
				if (OpenDBs())
				{
					selOpsCmd= new OracleCommand();
					selOpsCmd.Connection = OPSBDCon;

					selOpsCmd.CommandText =	"select ope_id, ope_vehicleid, ope_dart_id, "+
						"ope_grp_id, ope_uni_id, ope_dope_id, "+
						"to_char(ope_inidate,'HH24MISSDDMMYY') ope_inidate , "+
						"to_char(ope_enddate,'HH24MISSDDMMYY') ope_enddate, "+
						"to_char(ope_movdate,'HH24MISSDDMMYY') movdate "+
						"from operations "+
						"where ope_dope_id in (1,2,3) "+
//						"and (ope_realduration is null or ope_id=ope_base_ope_id) "+
						"order by ope_movdate asc";
					dr = selOpsCmd.ExecuteReader();	


					while (dr.Read()) 
					{
				
						try
						{

							_operationId=dr.GetInt32(dr.GetOrdinal("OPE_ID"));
							_vehicleId=dr.GetString(dr.GetOrdinal("OPE_VEHICLEID"));
							_articleDefId=dr.GetInt32(dr.GetOrdinal("OPE_DART_ID"));
							_groupId=dr.GetInt32(dr.GetOrdinal("OPE_GRP_ID"));
							_unitId=dr.GetInt32(dr.GetOrdinal("OPE_UNI_ID"));
							_dopeid=dr.GetInt32(dr.GetOrdinal("OPE_DOPE_ID"));

							_dateMov=OPS.Comm.Dtx.StringToDtx(dr.GetString(dr.GetOrdinal("MOVDATE")));
							_dateIni=OPS.Comm.Dtx.StringToDtx(dr.GetString(dr.GetOrdinal("OPE_INIDATE")));
							_dateEnd=OPS.Comm.Dtx.StringToDtx(dr.GetString(dr.GetOrdinal("OPE_ENDDATE")));

							switch(_dopeid)
							{

								case 1:
									UpdateBaseOpeId(_operationId);
									Logger_AddLogMessage(string.Format("Estacionamiento {0} con ope_base={1}",_operationId,_operationId), LoggerSeverities.Info);
									break;
								case 2:
									iOpeBaseId=GetOpeBaseIdAmp();
									if (iOpeBaseId<0)
										iOpeBaseId=_operationId;
									
									UpdateBaseOpeId(iOpeBaseId);
									Logger_AddLogMessage(string.Format("Prolongacion {0} con ope_base={1}",_operationId,iOpeBaseId), LoggerSeverities.Info);
									
									break;
								case 3:
									GetM2CompData(ref iRealTime, ref iQuantity);
									iOpeBaseId=GetOpeBaseIdDev();
									if (iOpeBaseId<0)
										iOpeBaseId=_operationId;

									UpdateRealTime(_operationId,iRealTime,iQuantity,iOpeBaseId);
									UpdateBaseOpeIdDev(iOpeBaseId);
									Logger_AddLogMessage(string.Format("Devolucion {0} con tiempo real={1} minutos, cantidad={2} y ope_base={3}",_operationId,iRealTime,iQuantity,iOpeBaseId), LoggerSeverities.Info);

									/*UpdateBaseOpeId(iOpeBaseId);
									UpdateBaseOpeIdDev(iOpeBaseId);
									Logger_AddLogMessage(string.Format("Devolucion{0} con ope_base={1}",_operationId,iOpeBaseId), LoggerSeverities.Info);*/
									break;
								default:
									break;
							}
						}						
						catch(Exception e)
						{
							Logger_AddLogException(e);
						}
					} 
				}
				else
					bReturn=false;

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
				bReturn=false;
			}

			finally
			{

				if( dr != null )
				{
					dr.Close();
					dr = null;
				}
				if( selOpsCmd != null )
				{
					selOpsCmd.Dispose();
					selOpsCmd = null;
				}

				CloseDBs();
			}

			return bReturn;			
		}

		private static bool OpenDBs()
		{
			bool bReturn=true;
			try
			{
				OPSBDCon = new OracleConnection(DB_CONNECTION_STRING);
				OPSBDCon.Open();

				if (OPSBDCon.State!=System.Data.ConnectionState.Open)
				{
					bReturn=false;
					OPSBDCon.Close();
					OPSBDCon.Dispose();
					Logger_AddLogMessage("Error openning OPS Database", LoggerSeverities.Error);
				}
			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
				bReturn=false;
			}

			return bReturn;			
		}


		private static bool CloseDBs()
		{
			bool bReturn=true;
			try
			{

				if (OPSBDCon!=null)
				{
					OPSBDCon.Close();
					OPSBDCon.Dispose();
				}

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
				bReturn=false;
			}

			return bReturn;			
		}

		private static bool GetM2CompData(ref int iResRealTime, ref int iResQuantity)
		{
			bool bRdo=true;
			int iResult=-1;
			int iQuantity=-1;
			int iRealTime=-1;

			try
			{
				string m1Tel;

				m1Tel="<m1 id=\"0\">";
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
					if(_logger != null)
						_logger.AddLog("[Msg02]:Process Parsing " +  "Error Execute",LoggerSeverities.Debug);
					bRdo=false;
					return bRdo;
				}

				string m1Res=pCS_M1.StrOutM50.ToString();
			
				if(_logger != null)
					_logger.AddLog("[Msg02]:Process Parsing : Result" +  m1Res,LoggerSeverities.Debug);
			
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



		private static void UpdateRealTime(int iOpeId,int iRealTime,int iQuantity,int iOpeBaseId) 
		{
			OracleCommand		updateCmd = null;

			try			
			{
				updateCmd= new OracleCommand();
				updateCmd.Connection=OPSBDCon;
				updateCmd.CommandText=string.Format("update operations set ope_realduration={0}, ope_value_in_return={2}, ope_base_ope_id={3} where ope_id={1}",iRealTime,iOpeId,iQuantity,iOpeBaseId);
				updateCmd.ExecuteNonQuery();

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
			}
			finally
			{
				if( updateCmd != null )
				{
					updateCmd.Dispose();
					updateCmd = null;
				}
			}			
		}



		private static int GetOpeBaseIdAmp() 
		{
			OracleCommand		updateCmd = null;
			int iOpeBaseId=-1;

			try			
			{
				updateCmd= new OracleCommand();
				updateCmd.Connection=OPSBDCon;
				updateCmd.CommandText=string.Format(" select nvl(max(ope_base_ope_id), -1) "+
					"from operations o "+
					"where o.ope_vehicleid = '{0}' "+
					"and o.ope_grp_id = {1} "+
					"and o.ope_dart_id = {2} "+
					"and TO_CHAR(o.ope_enddate,'HH24MISSDDMMYY') = '{3}'  "+
					"and ope_base_ope_id is not null "+
					"and o.ope_dope_id in (1,2,3)",_vehicleId,_groupId.ToString(),_articleDefId.ToString(),OPS.Comm.Dtx.DtxToString(_dateIni));

				
				iOpeBaseId =Convert.ToInt32(updateCmd.ExecuteScalar());

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
			}
			finally
			{
				if( updateCmd != null )
				{
					updateCmd.Dispose();
					updateCmd = null;
				}
			}	
		
			return iOpeBaseId;
		}




		private static void UpdateBaseOpeId(int iOpeBaseId) 
		{
			OracleCommand		updateCmd = null;

			try			
			{
				updateCmd= new OracleCommand();
				updateCmd.Connection=OPSBDCon;
				updateCmd.CommandText=string.Format("update operations "+
					"set ope_base_ope_id = {0} "+
					"where ope_id={1}",iOpeBaseId,_operationId);
				updateCmd.ExecuteNonQuery();

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
			}
			finally
			{
				if( updateCmd != null )
				{
					updateCmd.Dispose();
					updateCmd = null;
				}
			}			
		}

		private static int GetOpeBaseIdDev() 
		{
			OracleCommand		updateCmd = null;
			int iOpeBaseId=-1;

			try			
			{

				updateCmd= new OracleCommand();
				updateCmd.Connection=OPSBDCon;
				updateCmd.CommandText=string.Format(" select nvl(max(ope_base_ope_id), -1) "+
					"from operations o "+
					"where o.ope_vehicleid = '{0}' "+
					"and o.ope_grp_id = {1} "+
					"and o.ope_dart_id = {2} "+
					"and (TO_CHAR(o.ope_inidate,'HH24MISSDDMMYY') = '{3}' or TO_CHAR(o.ope_movdate,'HH24MISSDDMMYY') = '{3}') "+
					"and ope_base_ope_id is not null "+
					"and o.ope_dope_id in (1, 3)",_vehicleId,_groupId.ToString(),_articleDefId.ToString(),OPS.Comm.Dtx.DtxToString(_dateIni));

				
				iOpeBaseId =Convert.ToInt32(updateCmd.ExecuteScalar());

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
			}
			finally
			{
				if( updateCmd != null )
				{
					updateCmd.Dispose();
					updateCmd = null;
				}
			}	
		
			return iOpeBaseId;
		}


		private static void UpdateBaseOpeIdDev(int iOpeBaseId) 
		{
			OracleCommand		updateCmd = null;

			try			
			{
				updateCmd= new OracleCommand();
				updateCmd.Connection=OPSBDCon;
				updateCmd.CommandText=string.Format("update operations "+
					"set ope_base_ope_id = null "+
					"where ope_movdate<to_date('{0}','HH24MISSDDMMYY') "+
					"AND OPE_DOPE_ID IN (1, 2, 3) "+
					"and ope_base_ope_id = {1}",OPS.Comm.Dtx.DtxToString(_dateMov), iOpeBaseId);
				updateCmd.ExecuteNonQuery();

			}
			catch(Exception e)		
			{
				Logger_AddLogException(e);
			}
			finally
			{
				if( updateCmd != null )
				{
					updateCmd.Dispose();
					updateCmd = null;
				}
			}			
		}
	
	}
}
