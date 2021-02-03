using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
//using Oracle.DataAccess.Client;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{


	/// <summary>
	/// Class to handle de m9 message.
	/// </summary>
	public sealed class Msg63 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m63)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg63()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m63"; } }
		#endregion

		#region Variables, creation and parsing

		private int			_unit;
		
    
		private string _plate;
		private string _conf1;   
		private string _conf2;    
		private string _conf3;    
		private string _conf4;    
		private string _conf5;    
		private string _conf6;    
		private string _conf7;    
		private string _conf8;    
		private string _conf9;	
		private string _conf10;   
		private string _date;		
		private string _time;     
		private string _jpeg_path;



		/// <summary>
		/// Constructs a new msg06 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg63(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					/*
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "m": _vehicleId = n.InnerText; break;
					*/

					case "u":	   _unit  = Convert.ToInt32(n.InnerText); break;    
					case "Plate":  
					{
						_plate = n.InnerText;  
						_plate = _plate.Replace(" ",""); 
						break;   
					}
					case "Conf1":  _conf1 = n.InnerText; break;      
					/*
					case "Conf2":  _conf2 = n.InnerText; break;   
					case "Conf3":  _conf3 = n.InnerText; break;   
					case "Conf4":  _conf4 = n.InnerText; break;   
					case "Conf5":  _conf5 = n.InnerText; break;   
					case "Conf6":  _conf6 = n.InnerText; break;   
					case "Conf7":  _conf7 = n.InnerText; break;  
					case "Conf8":  _conf8 = n.InnerText; break;   
					case "Conf9":  _conf9 = n.InnerText; break;	
					case "Conf10": _conf10 = n.InnerText; break; 
					*/ 
					case "Date":   _date = n.InnerText; break;	
					case "Time":   _time = n.InnerText; break;	
					case "jpeg_path": _jpeg_path = n.InnerText; break;

				}
			}
		}

		#endregion

		#region IRecvMessage Members



		//select PZOD_ID from units, pedestrian_zone_op_def where  uni_dpuni_id = PZOD_DPUNI_ID and uni_id = 10000

		private string GetTypeOfOperation(string unit)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string	typeOp = String.Empty;
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
					//oraCmd.CommandText = "select PZOD_ID from units, pedestrian_zone_op_def where  uni_dpuni_id = PZOD_DPUNI_ID and uni_id = " + unit.ToString();
					oraCmd.CommandText = "select PZOD_ID from pedestrian_zone_op_def INNER JOIN units ON ( PZOD_DPUNI_ID = uni_dpuni_id ) and uni_id = " + unit.ToString();


					dr = oraCmd.ExecuteReader();	

					
					while (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("PZOD_ID");
						typeOp = dr.GetInt32(iOrdinal).ToString(); 
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

			return typeOp;
		}

		
		private int GetNumPlateOperations(string plate, int minutes)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			int	num = 0;
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

					oraCmd.CommandText = "select count(*) NUM from pedestrian_zone_operations where pzo_capture_date > (sysdate - ((1/1440)*"+ minutes.ToString() + ")) and pzo_vehicleid = '" + plate + "'"; 
					
					dr = oraCmd.ExecuteReader();	

					
					while (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("NUM");
						num = Convert.ToInt32(dr.GetInt32(iOrdinal).ToString()); 
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

			return num;
		}

		private string GetUnitGroup(string unit)
		{

			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string	grp_id = String.Empty;
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

					oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					while (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("CGRP_ID");
						grp_id = dr.GetInt32(iOrdinal).ToString(); 
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

			return grp_id;
		}

		private bool PayedOperation(string plate)
		{
			bool bPayed = false;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			
			logger = logger = DatabaseFactory.Logger;

			logger.AddLog("Searching for a payed operation for plate: " + plate, LoggerSeverities.Debug);

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

					oraCmd.CommandText = "select OPE_MOVDATE from operations where OPE_VEHICLEID = '" + plate +"'";
					logger.AddLog("Select: " + oraCmd.CommandText, LoggerSeverities.Debug);
					dr = oraCmd.ExecuteReader();	

					
					while (dr.Read())
					{
									
						int iOrdinal = dr.GetOrdinal("OPE_MOVDATE");
						string date = dr.GetOracleDate(iOrdinal).ToString();

						DateTime dt = DateTime.Parse(date);
						TimeSpan tSp = DateTime.Now - dt;
						int iMinutes = int.Parse(ConfigurationSettings.AppSettings["Barrier_lastop_minutes"].ToString());
						if( tSp.TotalMinutes < iMinutes )
						{
							bPayed = true;
						}
					}

					if(bPayed)
					{
						logger.AddLog("There is a payed operation", LoggerSeverities.Debug);
					}
					else
					{
						logger.AddLog("There is not a payed operation", LoggerSeverities.Debug);
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

			return bPayed;
		}


		bool OpenBarrier()
		{
			bool bRslt = false;
			Socket socket = null;
			ILogger logger = null;
			logger = logger = DatabaseFactory.Logger;

			logger.AddLog("Generatin OpenBarrier command ...",LoggerSeverities.Debug);
			try
			{
				IPHostEntry address = null;
				IPEndPoint Ep = null;

				
				string sIP = ConfigurationSettings.AppSettings["OCR_Server"].ToString();

				logger.AddLog("OpenBarrier command IP: " + sIP,LoggerSeverities.Debug);

				int iPort = int.Parse( ConfigurationSettings.AppSettings["OCR_Port"].ToString() );

				logger.AddLog("OpenBarrier command port: " + iPort,LoggerSeverities.Debug );

				address = Dns.Resolve(sIP);
				Ep = new IPEndPoint(address.AddressList[0], iPort);

				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				socket.Connect(Ep); //Conectamos

				
				Byte[] SendBytes = new byte[5]; 

				
				SendBytes[0] = 0x30;
				SendBytes[1] = 0x31;
				SendBytes[2] = 0x30;
				SendBytes[3] = 0x31;
				SendBytes[4] = 0x0A;

				int iSent = socket.Send(SendBytes,SendBytes.Length,SocketFlags.None);

				System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding(); 
				string str = enc.GetString(SendBytes); 

				logger.AddLog("Ready to send: " + str,LoggerSeverities.Debug );

				if(iSent == SendBytes.Length)
				{
					logger.AddLog("OpenBarrier command sent OK.",LoggerSeverities.Debug);
					bRslt = true;
				}
				else
				{
					logger.AddLog("OpenBarrier command sent ERROR.",LoggerSeverities.Debug);
				}
			
			}
			catch(Exception e)
			{
				bRslt = false;
			}

			if(socket != null)
			{
				socket.Close();
			}

			return bRslt;

		}


		public String ComputeReplacementFrmt1(Match matchResult)
		{
			string sMatchValue = matchResult.Value;

			// Take first four numbers of plate according to plate format 1234FFF
			string sNumbers = sMatchValue.Substring(0, 4);
			string sChars = sMatchValue.Substring(4, 3);
			sNumbers = sNumbers.Replace('O', '0');
			sNumbers = sNumbers.Replace('B', '8');
			sChars = sChars.Replace('8', 'B');
			sChars = sChars.Replace('0', 'O');
			return sNumbers + sChars;
		}

		public String ComputeReplacementFrmt2(Match matchResult)
		{
			string sMatchValue = matchResult.Value;

			// Take first four numbers of plate according to plate format 
			string sNumbers = sMatchValue.Substring(2, 4);
			sNumbers = sNumbers.Replace('O', '0');
			sNumbers = sNumbers.Replace('B', '8');
			return sNumbers;
		}

		public String ComputeReplacementFrmt3(Match matchResult)
		{
			string sMatchValue = matchResult.Value;

			// Take first four numbers of plate according to plate format 
			string sFirstChar = sMatchValue.Substring(0, 1);
			sFirstChar = sFirstChar.Replace('0', 'O');
			sFirstChar = sFirstChar.Replace('8', 'B');

			string sNumbers = sMatchValue.Substring(1, 4);
			sNumbers = sNumbers.Replace('B', '8');
			sNumbers = sNumbers.Replace('O', '0');

			string sLastChar = sMatchValue.Substring(5, 3);
			sLastChar = sLastChar.Replace('8', 'B');
			sLastChar = sLastChar.Replace('0', 'O');
			return sFirstChar + sNumbers + sLastChar;
		}

		public String ComputeReplacementFrmt4(Match matchResult)
		{
			string sMatchValue = matchResult.Value;

			// Take first four numbers of plate according to plate format 
			string sFirstChar = sMatchValue.Substring(0, 3);
			sFirstChar = sFirstChar.Replace('0', 'O');
			sFirstChar = sFirstChar.Replace('8', 'B');

			string sNumbers = sMatchValue.Substring(3, 4);
			sNumbers = sNumbers.Replace('B', '8');
			sNumbers = sNumbers.Replace('O', '0');
			
			return sFirstChar + sNumbers;
		}

		private void PlateFormatCorrection(ref string sPlate)
		{
			sPlate = sPlate.ToUpper();

			Regex regexObjFrmt1 = new Regex(@"^[0-9OB]{4}[B,C,D,F,G,H,J,K,L,M,N,P,R,S,T,V,W,X,Y,Z,8]{3}$");
			sPlate = regexObjFrmt1.Replace(sPlate,new MatchEvaluator(ComputeReplacementFrmt1));

			Regex regexObjFrmt2 = new Regex(@"^([C][D]|[O][I]|[C][C]|[T][A])\d\d\d\d$");
			sPlate = regexObjFrmt2.Replace(sPlate,new MatchEvaluator(ComputeReplacementFrmt2));

			//Regex regexObjFrmt3 = new Regex(@"^[A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,8,0]\d\d\d\d[A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,8,0]{3}$");
			Regex regexObjFrmt3 = new Regex(@"^[A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,8,0][0-9OB]{4}[A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,8,0]{3}$");
			sPlate = regexObjFrmt3.Replace(sPlate,new MatchEvaluator(ComputeReplacementFrmt3));

			Regex regexObjFrmt4 = new Regex(@"^[A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,8,0]{3}\d\d\d\d$");
			sPlate = regexObjFrmt4.Replace(sPlate,new MatchEvaluator(ComputeReplacementFrmt4));
		}

		/// <summary>
		/// Inserts a new register in the OPERATIONS table, and if everything is succesful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd  = null;
			OracleCommand oraCmd2 = null; 
			OracleTransaction transaction = null;
			ILogger logger = null;
			System.Configuration.AppSettingsReader appSettings2 = null;
			bool bUnitOk = false;

			try
			{
				AppSettingsReader appSettings = new AppSettingsReader();
				logger = logger = DatabaseFactory.Logger;

				logger.AddLog("processing message ...",LoggerSeverities.Debug);

				
                
				int num = 0;
				if(_plate != "NOOCR")
				{
					appSettings2 = new System.Configuration.AppSettingsReader();
		
					string sPlateFormatCorrection = (string)appSettings2.GetValue("PLATE_CORRECTION", typeof(string));

					if( sPlateFormatCorrection == "TRUE" )
					{
						PlateFormatCorrection(ref _plate);
					}

					int opInterval = (int)appSettings2.GetValue("OPERATION_INTERVAL", typeof(int));
					num = GetNumPlateOperations(_plate, opInterval);
				}
				

				if( num == 0 )
				{
					if(logger != null)
					{
						logger.AddLog("Ready to get unit group",LoggerSeverities.Debug);
					}
 
					string GrpID = GetUnitGroup( _unit.ToString());

					string TypeOfOp = GetTypeOfOperation(_unit.ToString());

					

					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					
					if(logger != null)
					{
						logger = DatabaseFactory.Logger;
					}

					System.Data.IDbConnection DBCon=d.GetNewConnection();
					
					
					if(GrpID == String.Empty || TypeOfOp == String.Empty )
					{
						if(logger != null)
							logger.AddLog("Error al insertar reconocimiento de matrícula: La unidad no tiene grupo o tipo de operación",LoggerSeverities.Error);
						bUnitOk = false;
					}
					else
					{
						if(logger != null)
						{
							logger.AddLog("Ready to open DB Connection",LoggerSeverities.Debug);
						}

						oraDBConn = (OracleConnection)DBCon;
						oraDBConn.Open();

						if (oraDBConn.State == System.Data.ConnectionState.Open)
						{
							if(logger != null)
							{
								logger.AddLog("DB Connection open OK",LoggerSeverities.Debug);
							}

							PedRuleEvaluation ruleEvaluator = new PedRuleEvaluation();

							// to_date('" + _date + " " + _time + "','DD/MM/YYYY HH24:mi:ss')
							//
							DateTime dt = DateTime.Parse(_date + " " + _time);
							int iGrpId = int.Parse(GrpID);
							int rule = ruleEvaluator.EvaluateRules( _plate, dt, _unit, iGrpId );
							
							string sBarrierMode = (string)appSettings2.GetValue("BARRIER_MODE", typeof(string));

							if( sBarrierMode == "TRUE" )
							{
								if( PayedOperation(_plate) )
								{
									OpenBarrier();
								}
							}

							int Finable = 0;
							if(rule == PedRuleEvaluation.NoRule )
							{
								Finable = 1;
							}
							
							Thread.CurrentThread.CurrentCulture = new CultureInfo( "es-ES", false );
							string Now = DateTime.Now.ToShortDateString() + " " +  DateTime.Now.ToLongTimeString();

							oraCmd= new OracleCommand();

							// Start a local transaction
							transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
							// Assign transaction object for a pending local transaction
							oraCmd.Transaction = transaction;

							_conf1 = _conf1.Replace(".", ",");
							string reliability = float.Parse(_conf1).ToString();
							reliability = reliability.Replace(",",".");

							oraCmd.CommandText = "insert into pedestrian_zone_operations ";
							oraCmd.CommandText += "(pzo_vehicleid, pzo_capture_date, pzo_reception_date, pzo_uni_id, pzo_grp_id, pzo_pzod_id,PZO_PLATE_REL, PZO_FINABLE, PZO_PZOS_ID) ";
							oraCmd.CommandText += " values ('" + _plate + "',to_date('" + _date + " " + _time + "','DD/MM/YYYY HH24:mi:ss'),";
							oraCmd.CommandText += "to_date('" + Now + "','DD/MM/YYYY HH24:mi:ss')," + _unit.ToString() + "," +  GrpID + "," + TypeOfOp + "," + reliability + "," + Finable.ToString() +  ", 1) "; // PZO_PZOS_ID = 1: Pendiente
							oraCmd.CommandText += " returning PZO_ID into :ID";

							OracleParameter oraParam = oraCmd.Parameters.Add(":ID", OracleDbType.Int32);
							oraParam.Direction = System.Data.ParameterDirection.Output;

							if (oraDBConn.State == System.Data.ConnectionState.Open)
							{
								oraCmd.Connection = oraDBConn;
								int iNumColsMod = oraCmd.ExecuteNonQuery();
						
								if(iNumColsMod == 1)
								{
									
									String param = oraParam.Value.ToString();

									oraCmd2= new OracleCommand();

									oraCmd2.Transaction = transaction;

									oraCmd2.CommandText = "insert into pedestrian_zone_op_photos ( pzop_pzo_id, pzop_path ) values (";
									oraCmd2.CommandText += param + ",'" + _jpeg_path + "'" + ")";

									oraCmd2.Connection = oraDBConn;
									iNumColsMod = oraCmd2.ExecuteNonQuery();

									if(iNumColsMod == 1)
									{
										transaction.Commit();
									}
									else
									{
										transaction.Rollback();
									}

									int i = 0;
								}
								else
								{
									if(logger != null)
									{
										logger.AddLog( "Unable to perform operation: " + oraCmd.CommandText, LoggerSeverities.Error);
									}	

									if(transaction != null)
									{
										transaction.Rollback();
									}
								}
							}
						}//if (oraDBConn.State == System.Data.ConnectionState.Open) 
					}// else de if(GrpID == String.Empty || TypeOfOp == String.Empty )
				}//if( num == 0 )
			}
			catch(Exception e)
			{
				if(transaction != null)
				{
					transaction.Rollback();
					transaction.Dispose();
					transaction = null;
				}
				if(logger != null)
					logger.AddLog("[Msg63:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{
				if (oraCmd!=null)
				{
					oraCmd.Dispose();
					oraCmd = null;
				}

				if (oraCmd2 != null)
				{
					oraCmd2.Dispose();
					oraCmd2 = null;
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


		#endregion
	}
}
