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

namespace OPS.Comm.Becs.Messages
{

	/// <summary>
	/// Implements the m50 message (sancionar estacionamiento)
	/// <m>Vehicle</m>
	/// <g>Group ID</g>
	/// <d>Date</d>
	/// <u>Unit (PDA) ID</p>
	/// </summary>
	internal sealed class Msg50 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m50)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m50"; } }
		#endregion

		#region Static stuff

		private static int M1_OP_OK			= 1;
		private static int M1_OP_TPERM		= -7;
		private static int DEF_OPERTYPE_PARK= 1;
		private static int DEF_OPERTYPE_AMP	= 2;
		private static int M50_RESULT_OK	= 1;
		private static int M50_RESULT_VIP	= 3;
		private static int M50_RESULT_MUST_FINE	= -1;
		private static int M50_DIAS_PARA_VEHICULO_REINCIDENTE;
		private static int M50_MULTAS_PARA_VEHICULO_REINCIDENTE;
		private static int _numOfFinesShown;			// Num of fines to show in the <f> tag of response
		private static double _nDifHour;
		private static bool _bIntraZonePark;
		static Msg50 ()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			
			CmpParametersDB cmpParam = new CmpParametersDB();
			string valueFINES = cmpParam.GetParameter("P_NUMFINESSHOWN");
			string valueIntraZonePark = cmpParam.GetParameter("P_M50_INTRAZONEPARK");
			_bIntraZonePark=false;
			if (valueIntraZonePark.Length>0)
			{
				_bIntraZonePark=(valueIntraZonePark!="0");
			}
			// Get the number of fines to show in <f> tag of r50
			_numOfFinesShown = Convert.ToInt32(valueFINES);
			
			M50_DIAS_PARA_VEHICULO_REINCIDENTE = (int)appSettings.GetValue ("M50.DIASPARAVEHICULOREINCIDENTE", typeof(int));
			M50_MULTAS_PARA_VEHICULO_REINCIDENTE = (int)appSettings.GetValue ("M50.MULTASPARAVEHICULOREINCIDENTE", typeof(int));

			try
			{
				_nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
			}
			catch
			{
				_nDifHour=0;
			}			


		}

		#endregion

		#region Variables, creation and parsing

		// Data from the message
		private string _vehicleId;
		private DateTime _date;
		private int _groupId;
		private int _unitId;
		private int _pedestrian = 0;
		
		private int _result;
		private int iResult=-1;
		private int iTipoOperacion=0;
		private int iPostPago=0;
		DateTime dtInitial=DateTime.Now.AddHours(_nDifHour);
		private int iArticleDef;
		private bool bM1AppliedHistory=true;
		private bool bIsResident=false;
		private bool bIsVIP=false;
		private bool bExistTicket=false;
		DateTime dtMax=DateTime.Now.AddHours(_nDifHour);
		private double _dLatitud=-999;
		private double _dLongitud=-999;

		/// <summary>
		/// Constructs a new Msg50 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg50 (XmlDocument msgXml) : base (msgXml)
		{
		}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		
		// <p>
		//	<m50>
		//		<m>0000BBB</m>
		//      <g>60001</g>
		//		<u>10000</u>
		//	</m50>
		//</p>
		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);
			

			ILogger logger = null;
			logger = DatabaseFactory.Logger;
			if(logger != null)
				logger.AddLog("[Msg50:DoParseMessage]",LoggerSeverities.Debug);

			try
			{
				foreach (XmlNode node in _root.ChildNodes)
				{
					switch (node.Name)
					{
						case "m": 
							_vehicleId = node.InnerText; 
							break;
						case "p":
							_pedestrian = Convert.ToInt32(node.InnerText); 
							break;
						case "g": 
							_groupId = Convert.ToInt32(node.InnerText); 
							break;
						case "d": 
							_date = OPS.Comm.Dtx.StringToDtx(node.InnerText); 
							break;
						case "u": 
							_unitId  = Convert.ToInt32(node.InnerText); 
							break;
						case "lt": 
							_dLatitud  =  Convert.ToDouble(node.InnerText, (IFormatProvider)culture.NumberFormat);
							break;				
						case "lg": 
							_dLongitud  =  Convert.ToDouble(node.InnerText, (IFormatProvider)culture.NumberFormat);
							break;				
					}
				}
			}
			catch (Exception ex)
			{
				logger.AddLog("[Msg50:DoParseMessage] - ERROR in parse",LoggerSeverities.Debug);
				logger.AddLog("[Msg50:DoParseMessage] " + ex.ToString(),LoggerSeverities.Debug);
				throw ex;
			}

		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m50 message. If all goes OK, returns an ACK_PROCESSED with the correct data
		/// </summary>
		/// <returns></returns>
		/*
		 * 
 
		<m50 id="XXX">
			<m>matricula</m>
			<g>grupo</g>
			<d>fecha</d>
			<u>PDA</u>
		</m50>


		  |
		  |
		  |
		  V
  
		<m1 id "XXX">
			<m>matricula</m>
			<g>grupo</g>
			<d>fecha</d>
			<u>PDA</u>
			<o>1</o>
		</m1>


		Respuestas posibles:
		<ap id="XXX">
		<Ar>1</Ar>
		<Aq1>40</Aq1>
		<Aq2>1000</Aq2>
		<At>138</At>
		<Ao>1</Ao> --> Tipo de operaci�n 1-> Estacionamiento nuevo; 2->Prolongaci�n
		<Ad>111800270706</Ad>
		<Adi>160504260706</Adi> --> Fecha de comienzo de la operaci�n ( si aparcamos en el descanso del mediod�a sera 15:00:00)
		<Ag>60004</Ag>
		<Aad>4</Aad> -->Tipo de art�culo
		<Aaq>0</Aaq>
		<Aat>0</Aat>
		<Aaqag>-999</Aaqag>
		<Aatag>-999</Aatag>
		<Ad0>160504260706</Ad0>
		<Aq>-999</Aq>
		<Adr0>160504260706</Adr0>   ---> Inicio del aparcamiento (sin tener en cuenta resets de tiempo)
		<Araq>0</Araq>  -----> Cantidad total acumulada de aparcamiento 
		<Arat>0</Arat>  -----> Tiempo total acumulado de aparcamiento
		<App>0</App>  -------> Es una operaci�n de postpago
		</ap>


		1) r!=1 && r!=M1_OP_TPERM(Se ha superado el tiempo de permanencia)  => Se ha de multar al veh�culo

		2) r==M1_OP_TPERM (Se ha superado el tiempo de permanencia)  =>  NO Se ha de multar al veh�culo. 
			Ya que este error implica que el veh�culo est� todav�a en tiempo
			de aparcamiento. Si no lo estuviera el error habr�a "No se ha superado el tiempo m�nimo de reentrada" o 
			bien hubiera permitido un aparcamiento nuevo o una prolongaci�n

		3) r==1

			3.1) (o==2) && (!pp) 
				Esto implica que estamos prolongando un aparcamiento => estamos en tiempo de cortes�a => 
				=> NO Se ha de multar al veh�culo. 

			3.2) (o==2) && (pp) 
				Esto implica que estamos prolongando un aparcamiento pero ya no es tiempo de cortes�a si no de postpago
				=> Se ha de multar al veh�culo. 
		
			3.3) (o==1). En principio se ha de multar a excepci�n de que no estemos en tiempo de tarifa (ejemplo al mediod�a,
				por la noche)
		   
				   3.3.1) m1(d)==ap(di) (la fecha de inicio del aparcamiento ser� ahora) => Se ha de multar al veh�culo
				   3.3.1) m1(d)<ap(di) (la fecha de inicio del aparcamiento m�s adelante) => No ha de multar al veh�culo


		4) El problema esta cuando en el calculo de la tarifa no se aplica la historia (zona verde para residentes y vips). 
		   La soluci�n es sencilla. Hemos de devolver hacia el programa llamante a la DLL del c�lculo de M1 si se utilizado 
		   la historia para el c�lculo o no.
			4.1) Se ha utilizado historia.El calculo se realiza con el algoritmo anterior.
			4.2) No se ha utilizado historia. Hemos de analizar igualmente la respuesta ya que en ella se nos dir� el tipo de art�culo.
				 Se ha de realizar la siguiente consulta
					select count(*)
					  from operations o
					 where o.ope_vehicleid='m1(m)'and
						   o.ope_grp_id=m1(g) and
						   m1(d) between o.ope_movdate and o.ope_enddate and
						   ope_dart_id=ap(ad)
			       
			
				 4.2.1) Respuesta==0  => 	     
				   4.2.1.1) m1(d)==ap(di) (la fecha de inicio del aparcamiento ser� ahora) => Se ha de multar al veh�culo
				   4.2.1.1) m1(d)<ap(di) (la fecha de inicio del aparcamiento m�s adelante) => No ha de multar al veh�culo
	     
	     
				 4.2.2) Respuesta>0  => NO Se ha de multar al veh�culo. 
       
		 */
		
		

		private bool GetLastOperationUnitID(string plate, ref int UnitID, ref int GrpID)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			bool bRslt = true;
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
					sqlQuery.Append("select PZO_UNI_ID,PZO_GRP_ID from pedestrian_zone_operations");
					sqlQuery.AppendFormat(" where pzo_vehicleid = '{0}'", plate);
					sqlQuery.Append(" order by pzo_capture_date desc");

					oraCmd.CommandText = sqlQuery.ToString();
					
					dr = oraCmd.ExecuteReader();	
				
					if (dr.Read())
					{				
						int iOrdinal = dr.GetOrdinal("PZO_UNI_ID");
						UnitID = (int)dr.GetOracleDecimal(iOrdinal);

						iOrdinal = dr.GetOrdinal("PZO_GRP_ID");
						GrpID = (int)dr.GetOracleDecimal(iOrdinal);
					}
				}
			}
			catch(Exception e)
			{
				bRslt = false;
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

			return bRslt;
		}

		
		private string  LastEntryOperation(string plate, ref double dReliability)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string	date = String.Empty;
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
					sqlQuery.Append("select pzo_capture_date,PZO_PLATE_REL from pedestrian_zone_operations");
					sqlQuery.AppendFormat(" where pzo_vehicleid = '{0}'", plate);
					sqlQuery.Append(" order by pzo_capture_date desc");

					oraCmd.CommandText = sqlQuery.ToString();
					
					dr = oraCmd.ExecuteReader();	
				
					if (dr.Read())
					{				

						int iOrdinal = dr.GetOrdinal("PZO_CAPTURE_DATE");
						date = dr.GetOracleDate(iOrdinal).ToString();
						
						iOrdinal = dr.GetOrdinal("PZO_PLATE_REL");
						dReliability = (double)dr.GetOracleDecimal(iOrdinal);
						
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

			return date;
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

					StringBuilder sqlQuery = new StringBuilder();
					sqlQuery.AppendFormat("select CGRP_ID from groups_childs where CGRP_CHILD = {0}", unit.ToString() );

					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("CGRP_ID");
						grp_id = dr.GetOracleDecimal(iOrdinal).ToString(); 
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


		private bool LastNOOCR(int iNumRegs)
		{
			//select DAY_DDAY_ID from days where DAY_DATE = to_date('28/07/2010','dd:mm:yyyy')
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			bool bAllNoOCR =false;
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
					sqlQuery.AppendFormat("select * from (select pzo_vehicleid, t.rowid from pedestrian_zone_operations t order by pzo_reception_date desc) where rownum <= {0}", iNumRegs.ToString());
					
					oraCmd.CommandText = sqlQuery.ToString();
					
					
					dr = oraCmd.ExecuteReader();	

					bAllNoOCR = true;

					if (dr.Read() && bAllNoOCR == true)
					{
						int iOrdinal = dr.GetOrdinal("PZO_VEHICLEID");
						string vehicleId = dr.GetString(iOrdinal); 
						
						if(vehicleId != "NOOCR")
						{
							bAllNoOCR = false;
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

			return bAllNoOCR;
		}


		
		

		private bool GetApplyEntryTimeForFine(int ruleId)
		{
			
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string	result = String.Empty;
			bool bApplyEntryTimeForFine = false;
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
					sqlQuery.AppendFormat("select PDR_APPLY_ENTRY_TIME_FOR_FINE from PEDESTRIAN_ZONE_RULES where PDR_ID = {0}", ruleId.ToString() );

					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("PDR_APPLY_ENTRY_TIME_FOR_FINE");
						result = dr.GetOracleDecimal(iOrdinal).ToString(); 
						if(result == "1")
						{
							bApplyEntryTimeForFine = true;
						}
						else if(result == "0")
						{
							bApplyEntryTimeForFine = false;
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

			return bApplyEntryTimeForFine;
		}


		private string GetDateTimeOfLastOperation()
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			string 	result = String.Empty;
			
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
					sqlQuery.Append("select max(pzo_capture_date) LAST_DATE from pedestrian_zone_operations where pzo_vehicleid != 'NOOCR'");

					oraCmd.CommandText = sqlQuery.ToString();
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("LAST_DATE");
						
						if( dr.GetOracleValue(iOrdinal) != null )
						{
							result = dr.GetOracleValue(iOrdinal).ToString(); 
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
			return result;
		}

		
		private int GetMaxStateTime(int ruleId)
		{
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			OracleDataReader dr = null;
			int	result = -1;
			
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
					sqlQuery.AppendFormat("select PDR_MAX_STAY_TIME from PEDESTRIAN_ZONE_RULES where PDR_ID = {0}", ruleId.ToString() );

					oraCmd.CommandText = sqlQuery.ToString();
					//oraCmd.CommandText = "select CGRP_ID from groups_childs where CGRP_CHILD = '" + unit +"'";
					
					dr = oraCmd.ExecuteReader();	

					
					if (dr.Read())
					{
						int iOrdinal = dr.GetOrdinal("PDR_MAX_STAY_TIME");
						
						if( dr.GetOracleValue(iOrdinal) != null )
						{
							result = dr.GetInt32(iOrdinal); 
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
			return result;
		}


		private bool EvaluateTimeLimit(int iMaxStateTime,DateTime dtOp, DateTime dtCons)
		{
			bool bRslt = false;

			TimeSpan result = dtCons - dtOp;

			if((int)result.TotalMinutes > iMaxStateTime)
			{
				bRslt = true;
			}
			else
			{
				bRslt = false;
			}

			return bRslt;
		}

		public StringCollection Process()
		{
			
			PedRuleEvaluation ruleEvaluator = new PedRuleEvaluation();

			bool bExistUnit = ExistUnit(_unitId);

			_pedestrian=0;

			if(_pedestrian == 1)
			{
				DateTime dtOp = DateTime.MinValue;
				double dReliabillity = -1;
				bool bLastEntryOperation = false;
				bool bMustFine = false;

				bool bTimeLimit = false;
				int iMaxStateTime = -1;

				// if <pedestrian>1</pedestrian>
				if(bExistUnit)
				{		
			
					

					string sDate = LastEntryOperation(_vehicleId, ref dReliabillity);

					DateTime dtCons =  _date ;
					int iUnitID = -1;
					int iGrpID  = -1;

					int ruleIDop   = PedRuleEvaluation.NoRule;
					int ruleIDCons = PedRuleEvaluation.NoRule;

					if( sDate != null && sDate != String.Empty )
					{	
						GetLastOperationUnitID(_vehicleId, ref iUnitID, ref iGrpID );

						dtOp = DateTime.Parse( sDate );
						bLastEntryOperation = true;

						

						ruleIDop   = ruleEvaluator.EvaluateRules(_vehicleId,dtOp,iUnitID, iGrpID);
						ruleIDCons = ruleEvaluator.EvaluateRules(_vehicleId,dtCons,iUnitID, iGrpID);

						if(ruleIDCons == PedRuleEvaluation.NoRule)
						{
							bMustFine = true;
						}
						else
						{
							if(ruleIDop == PedRuleEvaluation.NoRule)
							{
								bool bApplyEntryTimeForFine = GetApplyEntryTimeForFine(ruleIDCons);
								if(bApplyEntryTimeForFine == true)
								{
									bMustFine = true;
								}
								else
								{
									iMaxStateTime = GetMaxStateTime(ruleIDCons);
									bTimeLimit = EvaluateTimeLimit(iMaxStateTime, dtOp,dtCons);
									if(bTimeLimit == true)
									{
										bMustFine = true;
									}
								}
							}
							else
							{
								iMaxStateTime = GetMaxStateTime(ruleIDop);
								bTimeLimit = EvaluateTimeLimit(iMaxStateTime, dtOp,dtCons);
								if(bTimeLimit == true)
								{
									bMustFine = true;
								}
							}
						}

					}
					else
					{					
						//ruleIDCons = ruleEvaluator.EvaluateRules(_vehicleId,dtCons,iUnitID, iGrpID);
						string sGroup = GetUnitGroup(_unitId.ToString());
						int iGroup = int.Parse(sGroup);
						ruleIDCons = ruleEvaluator.EvaluateRules(_vehicleId,dtCons,_unitId, iGroup);
						if(ruleIDCons == PedRuleEvaluation.NoRule)
						{
							bMustFine = true;
						}
					}
				}


				if(bMustFine)
				{
					// Multar
					_result=M50_RESULT_MUST_FINE;
				}
				else
				{
					_result=M50_RESULT_OK;
				}

				string response=ToStringM50Pedestrian(bLastEntryOperation,dtOp,dReliabillity,bTimeLimit,iMaxStateTime);
				StringCollection sc = new StringCollection();								 
				sc.Add (response);
				return sc;

			}
			else if(_pedestrian == 0)
			{

			
				// if <p>0</p>
				if (bExistUnit)
				{
					DateTime dtMaxVIPResident=DateTime.Now.AddHours(_nDifHour);
					bool bApplyHistory=true;
					bool bUseDefaultArticleDef=false;

					iPostPago=0;
					if (!GetM1(bApplyHistory,bUseDefaultArticleDef,_date))
					{
						return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
					}

					if (((bIsResident)||(bIsVIP))&&(!TicketObligatorio()))
					{
						dtMaxVIPResident=dtMax;
					}

					if (bM1AppliedHistory)
					{
						if ((iResult==M1_OP_OK)&&(iTipoOperacion==DEF_OPERTYPE_PARK))
						{
							bApplyHistory=false;
							bUseDefaultArticleDef=true;

							if (!GetM1(bApplyHistory,bUseDefaultArticleDef,_date))
							{
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (dtInitial>_date)
							{
								_result=M50_RESULT_OK;
							}
							else
							{
								DateTime dtHoraFinUltimaOperacion=DateTime.Now.AddHours(_nDifHour);
								if (ObtenerFechaUltimaOperacionAmpliacion(ref dtHoraFinUltimaOperacion))
								{											
							
									if (dtInitial>dtHoraFinUltimaOperacion)
									{
										//Nos cargamos el concepto de cortes�a del M1 aplicado al M50
										//ya que en Gernika no nos funciona
										_result=M50_RESULT_MUST_FINE;	
									}
									else
									{
										bExistTicket=true;
										_result=M50_RESULT_OK;
									}
									dtInitial=dtHoraFinUltimaOperacion;
								}
								else
								{									
									_result=M50_RESULT_MUST_FINE;									
								}
							}
						}
						else
						{
							if (iResult==M1_OP_OK)
							{	
								//iTipoOperacion==DEF_OPERTYPE_AMP
								if (iPostPago==0)// prolongaci�n en tiempo cortes�a 
								{
									bExistTicket=true;
									
									// a eliminar tras actualizar las PDA's 
									if (dtInitial<_date)
									{
										dtInitial=_date;
									}


									bApplyHistory=false;
									bUseDefaultArticleDef=true;

									if (!GetM1(bApplyHistory,bUseDefaultArticleDef,_date))
									{
										return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
									}

									if (iResult==M1_OP_OK)
									{
										if (dtInitial>_date)
										{ 
											// tiempo fuera de tarifa
											_result=M50_RESULT_OK;
										}
										else
										{
											DateTime dtHoraFinUltimaOperacion=DateTime.Now.AddHours(_nDifHour);
											if (ObtenerFechaUltimaOperacionAmpliacion(ref dtHoraFinUltimaOperacion))
											{											

												if (dtInitial>dtHoraFinUltimaOperacion)
												{
													//Nos cargamos el concepto de cortes�a del M1 aplicado al M50
													//ya que en Gernika no nos funciona
													_result=M50_RESULT_MUST_FINE;	
												}
												else
												{
													_result=M50_RESULT_OK;
												}
												dtInitial=dtHoraFinUltimaOperacion;
											}
											else
											{									
												_result=M50_RESULT_MUST_FINE;									
											}

										}
									}

								}
								else // prolongaci�n en tiempo de postpago 
								{
									_result=M50_RESULT_MUST_FINE;
								}

							}
							else if (iResult==M1_OP_TPERM) 
								// estamos en tiempo de aparcamiento (el m�ximo)
							{
								bExistTicket=true;
								_result=M50_RESULT_OK;
							}
							else // el resto de posibles errores 
								// hemos de comprabar si estamos en periodo de tarifa o no
							{
								
								bApplyHistory=false;
								bUseDefaultArticleDef=true;

								if (!GetM1(bApplyHistory,bUseDefaultArticleDef,_date))
								{
									return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
								}

								if ((iResult==M1_OP_OK)&&(iTipoOperacion==DEF_OPERTYPE_PARK))
								{

									if (dtInitial>_date)
									{
										_result=M50_RESULT_OK;
									}
									else
									{
										_result=M50_RESULT_MUST_FINE;
									}
								}
							}
						}
					}
					else //!bM1AppliedHistory
					{
					
						DateTime dtHoraFinUltimaOperacion=DateTime.Now.AddHours(_nDifHour);
						if (ObtenerFechaUltimaOperacion(ref dtHoraFinUltimaOperacion))
						{	
							bExistTicket=true;
							_result=M50_RESULT_OK;
							
							bApplyHistory=false;
							bUseDefaultArticleDef=true;

							if (!GetM1(bApplyHistory,bUseDefaultArticleDef,dtHoraFinUltimaOperacion))
							{
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}
						}
						else
						{

							bApplyHistory=false;
							bUseDefaultArticleDef=true;

							if (!GetM1(bApplyHistory,bUseDefaultArticleDef,_date))
							{
								return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}

							if (dtInitial>_date)
							{
								_result=M50_RESULT_OK;
							}
							else
							{
								_result=M50_RESULT_MUST_FINE;
							}
						}
					}

					if (_result==M50_RESULT_MUST_FINE)
					{
						if (!TicketObligatorio())
						{
							_result=M50_RESULT_OK;
							dtInitial=dtMaxVIPResident;
						}
					}


					string response=ToStringM50(dtInitial);

					StringCollection sc = new StringCollection();
					sc.Add (response);
					return sc;
				}
				else
				{
					return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			else
			{
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			
		}


		private bool GetM1(bool bApplyHistory,bool bUseDefaultArticleDef,DateTime dtFecha)
		{
			bool bRdo=true;
			try
			{
				string m1Tel;

				m1Tel="<m1 id=\""+_msgId+"\">";
				m1Tel+="<m>"+_vehicleId+"</m>";
				m1Tel+="<g>"+_groupId.ToString()+"</g>";
				m1Tel+="<d>"+OPS.Comm.Dtx.DtxToString(dtFecha)+"</d>";
				m1Tel+="<u>"+_unitId.ToString()+"</u>";
				m1Tel+="<o>1</o></m1>";				

				
				CS_M1 pCS_M1 = new CS_M1();
				pCS_M1.StrIn = m1Tel;
				pCS_M1.ApplyHistory=bApplyHistory;
				pCS_M1.UseDefaultArticleDef=bUseDefaultArticleDef;


				if(pCS_M1.Exectue()!= CS_M1.C_RES_OK)
				{
					if(m_logger != null)
						m_logger.AddLog("[Msg50]:Process Parsing " +  "Error Execute",LoggerSeverities.Debug);
					bRdo=false;
					return bRdo;
				}

				string m1Res=pCS_M1.StrOutM50.ToString();
			
				if(m_logger != null)
					m_logger.AddLog("[Msg50]:Process Parsing : Result" +  m1Res,LoggerSeverities.Debug);
			
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
							if (act.InnerText.Length > 0)
								iResult = int.Parse(act.InnerText);
							break;
						case "o":
							if (act.InnerText.Length > 0)
								iTipoOperacion = int.Parse(act.InnerText);
							break;
						case "pp":
							if (act.InnerText.Length > 0)
								iPostPago = int.Parse(act.InnerText);
							break;
						case "ad":
							if (!bUseDefaultArticleDef)
							{
								if (act.InnerText.Length > 0)
									iArticleDef = int.Parse(act.InnerText);
							}
							break;
						case "d":
							if (act.InnerText.Length > 0)
								dtMax = OPS.Comm.Dtx.StringToDtx(act.InnerText);
							break;
						case "di":
							if (act.InnerText.Length > 0)
								dtInitial = OPS.Comm.Dtx.StringToDtx(act.InnerText);
							break;
						case "his":
							if (act.InnerText.Length > 0)
								bM1AppliedHistory = (int.Parse(act.InnerText) == 1);
							break;
						case "resi":
							if (act.InnerText.Length > 0)
								bIsResident = (int.Parse(act.InnerText) == 1);
							break;
						case "vip":
							if (act.InnerText.Length > 0)
								bIsVIP = (int.Parse(act.InnerText) == 1);
							break;
						default:
							break;

					}
				}
			}
			catch
			{
				bRdo=false;
			}
			return bRdo;

		}

		private string ToStringM50Pedestrian(bool bLastEntryOperation,DateTime dtOp, double dReliability,bool bTimeLimit, int iMaxStateTime)
		{
			System.Text.StringBuilder ret = new System.Text.StringBuilder();
			try
			{
				ret.Append("<r>" + _result.ToString() + "</r>");
				if(_pedestrian == 1)
				{
					TimeSpan timeSp = new TimeSpan(0);
					
					
					if( bLastEntryOperation )
					{
						ret.Append("<dOp>" + OPS.Comm.Dtx.DtxToString(dtOp) + "</dOp>");
						//strDate = OPS.Comm.Dtx.DtxToString(dtOp);
						string sReliability = dReliability.ToString("0.00");
						ret.Append("<rel>" + sReliability + "</rel>");
					}
					

					if (_result==M50_RESULT_MUST_FINE)
					{
											
						// Si le vamos a multar por l�mite de tiempo
						if(bTimeLimit == true)
						{
							//N�mero de minutos que puede estas
							ret.Append("<maxMi>" + iMaxStateTime.ToString() + "</maxMi>");
							//TimeSpan sp = new TimeSpan(0,0,(int)timeSp.TotalMinutes,0,0);
							
							
							if( bLastEntryOperation )
							{
								TimeSpan span = new TimeSpan(0,0,iMaxStateTime,0,0);
								DateTime dtEnd = dtOp + span;
								ret.Append("<dEnd>" + OPS.Comm.Dtx.DtxToString(dtEnd) + "</dEnd>");
							}	

							//N�mero de minutos que ya lleva
							
							TimeSpan dt = DateTime.Now - dtOp ;
							ret.Append("<curMi>" + dt.TotalMinutes.ToString() + "</curMi>");
						}	

					}


					
					string lastdate = GetDateTimeOfLastOperation();
					DateTime dtLastDate = DateTime.Parse(lastdate);

					timeSp = DateTime.Now - dtLastDate;
						
						
					System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
					int iMaxHours = (int)appSettings.GetValue("MaxHoursWithoutOps", typeof(int));

					if( timeSp.TotalHours > iMaxHours )
					{
						string totalHours = timeSp.TotalHours.ToString("0.00");
							
						ret.Append("<alrmOp>" + totalHours + "</alrmOp>");
					}

					int iNumRegs = (int)appSettings.GetValue("MaxNumNOOCR", typeof(int));

					if(LastNOOCR(iNumRegs))
					{
						ret.Append("<alrmNoOcr>" + iNumRegs.ToString() + "</alrmNoOcr>");
					}

				}
			}
			catch(Exception e)
			{
			}
			return (new AckMessage(_msgId, ret.ToString())).ToString();

		}

		private string ToStringM50(DateTime dtFinPark)
		{

			System.Text.StringBuilder ret = new System.Text.StringBuilder();

			string strDate="";
			bool bReincidente=false;
			string strVIPGroup = "";
			bool bBlackList=false;

			ret.Append("<r>" + _result.ToString() + "</r>");			
			
			
			if(_pedestrian == 0)
			{
			
				if (_result==M50_RESULT_OK)
				{
					ret.Append("<d>" + OPS.Comm.Dtx.DtxToString(dtFinPark) + "</d>");
					strDate = OPS.Comm.Dtx.DtxToString(dtFinPark);

				}
				else if (_result==M50_RESULT_MUST_FINE)
				{
					if (ObtenerFechaUltimaOperacionDelDia(ref dtFinPark))
					{
						ret.Append("<d>" + OPS.Comm.Dtx.DtxToString(dtFinPark) + "</d>");
						strDate =  OPS.Comm.Dtx.DtxToString(dtFinPark);
					}
					int iNumFines=ObtenerMultas();
					if ((!bIsVIP)&&(!bIsResident)&&(iNumFines>=M50_MULTAS_PARA_VEHICULO_REINCIDENTE))
					{
						ret.Append("<rein>1</rein>");
						bReincidente=true;
					}
				}

				if (bIsVIP)
				{
					string grupo="";
					if (ObtenerGrupoVIP(ref grupo))
					{
						ret.Append("<vip>"+grupo+"</vip>");
						strVIPGroup=grupo;
					}
				}

				if (bIsResident)
				{
					ret.Append("<resi>1</resi>");

				}

				string strFinesInfo="";
				CalculateInfoAboutPlateFines(ref strFinesInfo);
				ret.Append(strFinesInfo);

				if (bExistTicket)
					ret.Append("<tick>1</tick>");

				if (ListaNegra())
				{
					ret.Append("<bl>1</bl>");
					bBlackList=true;
				}

				int iResGroup=-1;
				
				if ((bIsResident)&&(ResidentGroup(ref iResGroup)))
				{
					InsertGPSPosResi(iResGroup,strDate,bReincidente,strVIPGroup,bBlackList);
				}
				else
				{
					InsertGPSPos(strDate,bReincidente,strVIPGroup,bBlackList);
				}


				
			}
				
			return (new AckMessage(_msgId, ret.ToString())).ToString();
		}

		/// <summary>
		/// Obtains the _numOfFinesShown last fines of the current vehicle
		/// </summary>
		/// <returns>DataTable with fines</returns>
		private int ObtenerMultas()
		{
			CmpFinesDB cmp=null;
			DataTable dt=null;
			int iNumFines=0;
			try
			{
				cmp = new CmpFinesDB();
				dt = cmp.GetData (null, 
					string.Format("(FIN_VEHICLEID=@FINES.FIN_VEHICLEID@ AND TRUNC(FIN_DATE)>=TRUNC(TO_DATE('{0}','HH24MISSDDMMYY')-{1}) AND TRUNC(FIN_DATE)<TRUNC(TO_DATE('{0}','HH24MISSDDMMYY')) AND FIN_STATUSADMON!={2})",
					OPS.Comm.Dtx.DtxToString(_date), M50_DIAS_PARA_VEHICULO_REINCIDENTE,
					OPS.FineLib.CFineManager.C_ADMON_STATUS_ANULADA), 
					"FIN_DATE DESC", new object[] {_vehicleId});
				iNumFines = dt.Rows.Count;
			}
			catch
			{
				
			}
			finally
			{
				dt=null;
				cmp=null;
			}
			return iNumFines;
		}

		private bool ObtenerFechaUltimaOperacionAmpliacion(ref DateTime dtFecha)
		{
			bool bRet=false;
			try
			{
				CmpOperationsDB cmp = new CmpOperationsDB();
				StringBuilder sb = new StringBuilder();
				sb.Append("SELECT TO_CHAR(OPE_ENDDATE,'HH24MISSDDMMYY') FROM OPERATIONS WHERE OPE_VEHICLEID=@OPERATIONS.OPE_VEHICLEID@");

				if (_bIntraZonePark)
				{
					sb.Append (" AND (OPE_GRP_ID, OPE_DART_ID) in (select m50izp_grp_id_out, m50izp_dart_id_out ");
					sb.Append ("                    from m50_intrazonepark_groups  ");
					sb.Append ("                    where m50izp_grp_id_in = @OPERATIONS.OPE_GRP_ID@  ");
					sb.Append ("                          and m50izp_dart_id_in = @OPERATIONS.OPE_DART_ID@)  ");
				}
				else
				{				
					sb.Append (" AND OPE_GRP_ID = @OPERATIONS.OPE_GRP_ID@ ");
					sb.Append (" AND OPE_DART_ID = @OPERATIONS.OPE_DART_ID@ ");
				}				
				
				sb.Append (" AND OPE_DOPE_ID  IN (1,2,3) ");
				sb.Append (" AND (OPE_INIDATE < @OPERATIONS.OPE_MOVDATE@) ");
				sb.Append (" ORDER BY ABS(@OPERATIONS.OPE_MOVDATE@-OPE_MOVDATE) ASC");

				DataTable dt=null;

				dt = cmp.GetData (sb.ToString(), 
					new object[] {_vehicleId, _groupId, iArticleDef, _date, _date } );

				if (dt.Rows.Count>0)
				{
					DataRow dr=dt.Rows[0];
					dtFecha=OPS.Comm.Dtx.StringToDtx(dr[0].ToString());
					bRet=true;
				}
			}
			catch(Exception e)
			{
				bRet=false;
			}
				
			return bRet;
		}

		private bool ObtenerFechaUltimaOperacion(ref DateTime dtFecha)
		{
			bool bRet=false;
			try
			{
				CmpOperationsDB cmp = new CmpOperationsDB();
				StringBuilder sb = new StringBuilder();
				sb.Append("SELECT TO_CHAR(OPE_ENDDATE,'HH24MISSDDMMYY') FROM OPERATIONS WHERE OPE_VEHICLEID=@OPERATIONS.OPE_VEHICLEID@");
				if (_bIntraZonePark)
				{
					sb.Append (" AND (OPE_GRP_ID, OPE_DART_ID) in (select m50izp_grp_id_out, m50izp_dart_id_out ");
					sb.Append ("                    from m50_intrazonepark_groups  ");
					sb.Append ("                    where m50izp_grp_id_in = @OPERATIONS.OPE_GRP_ID@  ");
					sb.Append ("                          and m50izp_dart_id_in = @OPERATIONS.OPE_DART_ID@)  ");
				}
				else
				{				
					sb.Append (" AND OPE_GRP_ID = @OPERATIONS.OPE_GRP_ID@ ");
					sb.Append (" AND OPE_DART_ID = @OPERATIONS.OPE_DART_ID@ ");

				}
				
				sb.Append (" AND OPE_DOPE_ID  IN (1,2,3) ");
				sb.Append (" AND @OPERATIONS.OPE_MOVDATE@ BETWEEN OPE_MOVDATE AND OPE_ENDDATE");
				sb.Append (" ORDER BY OPE_MOVDATE DESC");


				
				DataTable dt=null;

				dt = cmp.GetData (sb.ToString(), 
					new object[] {_vehicleId, _groupId, iArticleDef, _date } );


				if (dt.Rows.Count>0)
				{
					DataRow dr=dt.Rows[0];
					dtFecha=OPS.Comm.Dtx.StringToDtx(dr[0].ToString());
					bRet=true;
				}
			}
			catch
			{
				
			}
				
			return bRet;
		}

		private bool ObtenerFechaUltimaOperacionDelDia(ref DateTime dtFecha)
		{
			bool bRet=false;
			try
			{
				CmpOperationsDB cmp = new CmpOperationsDB();
				StringBuilder sb = new StringBuilder();
				sb.Append("SELECT TO_CHAR(OPE_ENDDATE,'HH24MISSDDMMYY') FROM OPERATIONS WHERE OPE_VEHICLEID=@OPERATIONS.OPE_VEHICLEID@");
				
				if (_bIntraZonePark)
				{
					sb.Append (" AND (OPE_GRP_ID, OPE_DART_ID) in (select m50izp_grp_id_out, m50izp_dart_id_out ");
					sb.Append ("                    from m50_intrazonepark_groups  ");
					sb.Append ("                    where m50izp_grp_id_in = @OPERATIONS.OPE_GRP_ID@  ");
					sb.Append ("                          and m50izp_dart_id_in = @OPERATIONS.OPE_DART_ID@)  ");

				}
				else
				{				
					sb.Append (" AND OPE_GRP_ID = @OPERATIONS.OPE_GRP_ID@ ");
					sb.Append (" AND OPE_DART_ID = @OPERATIONS.OPE_DART_ID@ ");
				}
				

				sb.Append (" AND OPE_DOPE_ID  IN (1,2,3) ");
				sb.Append (" AND (TRUNC(OPERATIONS.OPE_MOVDATE) = TRUNC(@OPERATIONS.OPE_MOVDATE@)");
				sb.Append (" OR TRUNC(OPERATIONS.OPE_INIDATE) = TRUNC(@OPERATIONS.OPE_INIDATE@)");
				sb.Append (" OR TRUNC(OPERATIONS.OPE_ENDDATE) = TRUNC(@OPERATIONS.OPE_ENDDATE@))");
				sb.Append (" ORDER BY OPE_MOVDATE DESC");

				DataTable dt=null;

				dt = cmp.GetData (sb.ToString(), 
					new object[] {_vehicleId, _groupId, iArticleDef, _date , _date, _date} );

				if (dt.Rows.Count>0)
				{
					DataRow dr=dt.Rows[0];
					dtFecha=OPS.Comm.Dtx.StringToDtx(dr[0].ToString());
					bRet=true;
				}
			}
			catch
			{
				
			}
				
			return bRet;
		}

		private bool ObtenerGrupoVIP(ref string grupo)
		{
			bool bRet=false;
			grupo="";
			try
			{
				CmpVIPSDB cmp = new CmpVIPSDB();
				StringBuilder sb = new StringBuilder();
				sb.Append("SELECT VIP_TEXT FROM VIPS WHERE VIP_VEHICLEID=@VIPS.VIP_VEHICLEID@");
				sb.Append (" ORDER BY NVL(VIP_TEXT,-1)");

				DataTable dt = cmp.GetData (sb.ToString(), 
					new object[] {_vehicleId } );

				if (dt.Rows.Count>0)
				{
					DataRow dr=dt.Rows[0];
					grupo = dr[0].ToString();
					bRet=true;
				}
			}
			catch
			{
				
			}
				
			return bRet;
		}

		private bool TicketObligatorio()
		{
			bool bRet=true;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("select mati_is_mandatory from mandatory_ticket where mati_dart_id={0} and mati_grp_id={1}",iArticleDef,_groupId );
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())==0)
					{
						bRet=false;
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


		private bool ListaNegra()
		{
			bool bRet=false;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("select count(*) "+
						"from black_lists b "+
						"where b.blis_value = '{0}' "+
						" and b.blis_dblis_id = 1 and blis_deleted=0", _vehicleId );
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
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

		private bool ResidentGroup(ref int iResGroup)
		{
			bool bRet=false;
			iResGroup=-1;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("select res_grp_id from residents where res_vehicleid='{0}'", _vehicleId);
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					iResGroup= Convert.ToInt32(cmd.ExecuteScalar());
					bRet=true;
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
				
			if (iResGroup==-1)
			{
				bRet=false;
			}
			return bRet;
		}




		private bool ExistUnit(int iUnit)
		{
			bool bRet=false;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("select count(*) "+
						"from units "+
						"where uni_id = {0} ", iUnit );
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
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
					sqlQuery.AppendFormat(	"select fin_id, fin_statusadmon, TO_CHAR(FIN_DATE,'HH24MISSDDMMYY') findate, TO_CHAR(OPE_MOVDATE,'HH24MISSDDMMYY') paydate "+
						"from fines t, operations o "+
						"WHERE TO_char(FIN_DATE,'DDMMYY')='{0:ddMMyy}' "+
						"AND o.ope_fin_id(+) = t.fin_id "+
						"AND fin_vehicleid='{1}' "+
						"AND t.fin_statusadmon in (0,1) and t.fin_status=30 "+ //pendiente de pago o anulada(pagada) y generada
						"ORDER BY FIN_DATE DESC", _date,_vehicleId);

					oraCmd.CommandText = sqlQuery.ToString();

					dr=oraCmd.ExecuteReader();

					if (dr.HasRows)
					{

						outxml="<fs>";

						while (dr.Read())
						{
							string fin_id= dr["fin_id"].ToString();
							string fin_status=dr["fin_statusadmon"].ToString();
							string fin_date=dr["findate"].ToString();
							string pay_date="";

							if (!dr.IsDBNull(dr.GetOrdinal("paydate")))
							{
								pay_date=dr["paydate"].ToString();
							}
							
							outxml+=string.Format("<f><id>{0}</id><st>{1}</st><fd>{2}</fd><pd>{3}</pd></f>",fin_id,fin_status,fin_date,pay_date);
						
							
						}	

						outxml+="</fs>";

						logger.AddLog(string.Format("[Msg50:CalculateInfoAboutPlateFines]: Info about Plate {0} in date {1:ddMMyy}}: {2}",_vehicleId,_date,outxml),LoggerSeverities.Debug);

					}
				}

			}
			catch(Exception e)
			{
				logger.AddLog("[Msg50:CalculateInfoAboutPlateFines]: Excepcion: "+e.Message,LoggerSeverities.Error);

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


		private bool InsertGPSPos(string strDate,bool bReincidente,string strVIPGroup,bool bBlackList)
		{
			bool bRet=true;
			try
			{

				if ((_dLatitud!=-999)&&(_dLongitud!=-999))
				{
					CultureInfo culture = new CultureInfo("", false);
					bRet=false;
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					DBCon.Open();
					try
					{

						String strSQL;
						
						if (strDate.Length>0)
						{

							strSQL = String.Format("insert into UNIT_GPS_POSITION (UGPS_UNI_ID,UGPS_DATE,UGPS_LATITUDE, UGPS_LONGITUD,"+
								"UGPS_GRP_ID,UGPS_VEHICLEID,UGPS_RESULT,UGPS_HAVE_TICKET,UGPS_DATEMAX,UGPS_RESIDENT,UGPS_VIPGROUP,UGPS_REINCIDENTE,UGPS_BLACKLIST) values "+
								"({0},to_date('{1}','hh24missddmmyy'),{2},{3},{4},'{5}',{6},{7},to_date('{8}','hh24missddmmyy'),{9},'{10}',{11},{12})", 
								_unitId, 
								OPS.Comm.Dtx.DtxToString(_date),
								Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat),
								Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat),
								_groupId,
								_vehicleId,
								_result,
								(bExistTicket?1:0),
								strDate,
								(bIsResident?1:0),
								strVIPGroup,
								(bReincidente?1:0),
								(bBlackList?1:0));
						}
						else
						{
							strSQL = String.Format("insert into UNIT_GPS_POSITION (UGPS_UNI_ID,UGPS_DATE,UGPS_LATITUDE, UGPS_LONGITUD,"+
								"UGPS_GRP_ID,UGPS_VEHICLEID,UGPS_RESULT,UGPS_HAVE_TICKET,UGPS_RESIDENT,UGPS_VIPGROUP,UGPS_REINCIDENTE,UGPS_BLACKLIST) values "+
								"({0},to_date('{1}','hh24missddmmyy'),{2},{3},{4},'{5}',{6},{7},{8},'{9}',{10},{11})", 
								_unitId, 
								OPS.Comm.Dtx.DtxToString(_date),
								Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat),
								Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat),
								_groupId,
								_vehicleId,
								_result,
								(bExistTicket?1:0),
								(bIsResident?1:0),
								strVIPGroup,
								(bReincidente?1:0),
								(bBlackList?1:0));

						}
						
						if(m_logger != null)
							m_logger.AddLog("[Msg50]:Process Executing: " + strSQL,LoggerSeverities.Debug);

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

			}
			catch
			{
					
			}
				
			return bRet;
		}


		private bool InsertGPSPosResi(int iResGroup, string strDate,bool bReincidente,string strVIPGroup,bool bBlackList)
		{
			bool bRet=true;
			try
			{
				if ((_dLatitud!=-999)&&(_dLongitud!=-999))
				{
					CultureInfo culture = new CultureInfo("", false);
					bRet=false;
					Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
					System.Data.IDbConnection DBCon=d.GetNewConnection();
					DBCon.Open();
					try
					{

						String strSQL;
						
						if (strDate.Length>0)
						{

							strSQL = String.Format("insert into UNIT_GPS_POSITION (UGPS_UNI_ID,UGPS_DATE,UGPS_LATITUDE, UGPS_LONGITUD,"+
								"UGPS_GRP_ID,UGPS_VEHICLEID,UGPS_RESULT,UGPS_HAVE_TICKET,UGPS_DATEMAX,UGPS_RESIDENT,UGPS_VIPGROUP,UGPS_REINCIDENTE,UGPS_BLACKLIST,UGPS_RESIDENT_GRP_ID) values "+
								"({0},to_date('{1}','hh24missddmmyy'),{2},{3},{4},'{5}',{6},{7},to_date('{8}','hh24missddmmyy'),{9},'{10}',{11},{12},{13})", 
								_unitId, 
								OPS.Comm.Dtx.DtxToString(_date),
								Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat),
								Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat),
								_groupId,
								_vehicleId,
								_result,
								(bExistTicket?1:0),
								strDate,
								(bIsResident?1:0),
								strVIPGroup,
								(bReincidente?1:0),
								(bBlackList?1:0),
								iResGroup);
						}
						else
						{
							strSQL = String.Format("insert into UNIT_GPS_POSITION (UGPS_UNI_ID,UGPS_DATE,UGPS_LATITUDE, UGPS_LONGITUD,"+
								"UGPS_GRP_ID,UGPS_VEHICLEID,UGPS_RESULT,UGPS_HAVE_TICKET,UGPS_RESIDENT,UGPS_VIPGROUP,UGPS_REINCIDENTE,UGPS_BLACKLIST,UGPS_RESIDENT_GRP_ID) values "+
								"({0},to_date('{1}','hh24missddmmyy'),{2},{3},{4},'{5}',{6},{7},{8},'{9}',{10},{11},{12})", 
								_unitId, 
								OPS.Comm.Dtx.DtxToString(_date),
								Convert.ToString(_dLatitud, (IFormatProvider)culture.NumberFormat),
								Convert.ToString(_dLongitud, (IFormatProvider)culture.NumberFormat),
								_groupId,
								_vehicleId,
								_result,
								(bExistTicket?1:0),
								(bIsResident?1:0),
								strVIPGroup,
								(bReincidente?1:0),
								(bBlackList?1:0),
								iResGroup);

						}
						
						if(m_logger != null)
							m_logger.AddLog("[Msg50]:Process Executing: " + strSQL,LoggerSeverities.Debug);

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

			}
			catch
			{
					
			}
				
			return bRet;
		}



		#endregion

	}	
/*
	#region Auxiliary Functions
	public sealed class Msg51 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m51)
		#region Static stuff



		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg51()
		{
			
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m51"; } }
		#endregion

		#region Variables, creation and parsing

		private string		_vehicleId		= null;
		private int			_operationDefId;
		private int			_operationId	= -1;
		private int			_articleId		= -1;
		private int			_articleDefId	= -1;
		private int			_groupId		= -1;
		private int			_unitId;
		private DateTime	_date;
		/// <summary>
		/// Constructs a new msg02 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg51(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

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
					case "d":
						_date = OPS.Comm.Dtx.StringToDtx(n.InnerText);
						break;

				}
			}
				
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Inserts a new register in the FINES_HIS table, and if everything is succesful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			ILogger logger = null;
			logger = DatabaseFactory.Logger;

			try
			{
				string strRes="";
				ManagePlate(logger, _vehicleId, ref strRes);
				AckMessage ret = new AckMessage (_msgId, "<r>" + strRes + "</r>");
				StringCollection sc = new StringCollection();
				sc.Add (ret.ToString());
				return sc;					
			}
			catch (Exception e)
			{
				if (logger != null)
					logger.AddLog("[Msg02:Process]" + e.Message ,LoggerSeverities.Debug);
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}				
		}





		private bool ManagePlate(ILogger logger,string strVehicleId, ref string strRes)
		{
			bool bRet=false;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("insert into FINES_HIS (HFIN_UNI_ID,HFIN_DATE,HFIN_NUMBER,HFIN_NAME,HFIN_XPRTN_DATE,HFIN_VEHICLEID) values "+
						"({0},to_date('{1}','hh24missddmmyy'),'{2}','{3}',to_date('{4}','hh24missddmmyy'),'{5}','{6}')", 
						_unitId, 
						OPS.Comm.Dtx.DtxToString(_date),
						strVehicleId,
						strVehicleId.Substring(1,5),
						OPS.Comm.Dtx.DtxToString(DateTime.Now),
						_vehicleId,
						_root.OuterXml);

					//logger.AddLog(string.Format("[Msg02:Process]: {0}", strSQL),LoggerSeverities.Debug);
					OracleCommand cmd = new OracleCommand(strVehicleId, (OracleConnection)DBCon);
					int iRes=cmd.ExecuteNonQuery();
					strRes=iRes.ToString();
					cmd.Dispose();
					bRet=true;
				}
				catch(Exception e)
				{
					strRes=e.Message;
				}
								
				DBCon.Close();
					
			}
			catch(Exception e)
			{
				strRes=e.Message;				
			}
				
			return bRet;
		}



		
		#endregion
		

	}
		

	#endregion
*/

}
