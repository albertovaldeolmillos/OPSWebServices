using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// m80 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg80 : MsgReceived, IRecvMessage
	{

		private int _unitId=-1;
		private int _spaceSensorId=-1;
		private int _statusId=-1;
		private string	_statusDate="";
		private int _errorId=-1;
		private string _errorDate="";
		private long _seqId=-1;
		private string	_sentDate="";
		private int _nodeType=-1;
		private int _firmwareVersion=-1;
		private long _lastMessageId=-1;
		private int _magneticStatus=-1;
		private int _opticStatus=-1;
		private int _ambientLightOverExposure=-1;
		private int _magneticUncalibrated=-1;
		private int _iceDetection=-1;
		private int _opticalFreeQuality=-1;
		private int _opticalOccupiedQuality=-1;
		private int _temperature=-1;
		private long _operationId=-1;
		private string	_operationDate="";
 

/*
 * 
 * <m80 id="315" ret="1">
 *	<u>260</u> _unitId
 *	<ss>1</ss> _spaceSensorId
 *	<seq>316</seq> _seqId
 *	<s>2</s> _statusId
 *	<e>0</e> _errorId
 *	<d>190413200911</d> _statusDate
 *	<ed>114930200911</ed> _errorDate
 *	<sd>190413200911</sd> _sentDate
 *	<nt>12297</nt> _nodeType
 *	<fv>32</fv> _firmwareVersion
 *	<lm>126480</lm> _lastMessageId
 *	<ms>0</ms> _magneticStatus
 *	<os>1</os> _opticStatus
 *	<alo>0</alo> _ambientLightOverExposure
 *	<mu>0</mu> _magneticUncalibrated
 *	<id>0</id> _iceDetection
 *	<ofq>0</ofq> _opticalFreeQuality
 *	<ooq>10</ooq> _opticalOccupiedQuality
 *	<t>207</t> _temperature
 *	<o>1465</o> _operationId
 *	<od>190413200911</od>  _operationDate
 * </m80>
 */		



		/// <summary>
		/// Constructs a new m80 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg80(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "ss": _spaceSensorId = Convert.ToInt32(n.InnerText); break;
					case "s": _statusId = Convert.ToInt32(n.InnerText); break;
					case "d": _statusDate = n.InnerText; break;
					case "e": _errorId = Convert.ToInt32(n.InnerText); break;
					case "ed": _errorDate = n.InnerText; break;
					case "seq": _seqId = Convert.ToInt32(n.InnerText); break;
					case "sd": _sentDate = n.InnerText; break;
					case "nt": _nodeType = Convert.ToInt32(n.InnerText); break;
					case "fv": _firmwareVersion = Convert.ToInt32(n.InnerText); break;
					case "lm": _lastMessageId = Convert.ToInt32(n.InnerText); break;
					case "ms": _magneticStatus = Convert.ToInt32(n.InnerText); break;
					case "os": _opticStatus = Convert.ToInt32(n.InnerText); break;
					case "alo": _ambientLightOverExposure = Convert.ToInt32(n.InnerText); break;
					case "mu": _magneticUncalibrated = Convert.ToInt32(n.InnerText); break;
					case "id": _iceDetection = Convert.ToInt32(n.InnerText); break;
					case "ofq": _opticalFreeQuality = Convert.ToInt32(n.InnerText); break;
					case "ooq": _opticalOccupiedQuality = Convert.ToInt32(n.InnerText); break;
					case "t": _temperature = Convert.ToInt32(n.InnerText); break;
					case "o": _operationId = Convert.ToInt32(n.InnerText); break;
					case "od": _operationDate = n.InnerText; break;




				}
			}
		}

		#region DefinedRootTag(m80)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m80"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m80 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr= null;
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
				

					oraCmd.CommandText =	string.Format("update SPACE_SENSORS set SSE_STATUS={2}, "+
																			   "SSE_STATUS_TIME=to_date('{3}', 'HH24MISSDDMMYY'), "+
																			   "SSE_ERROR={4}, "+
																			   "SSE_ERROR_TIME=to_date('{5}', 'HH24MISSDDMMYY'), "+
																			   "SSE_SEQ_ID={1} "+																			   
																				((_sentDate.Length>0)?",SSE_STATUS_SENT_TIME=to_date('"+_sentDate+"', 'HH24MISSDDMMYY') ":"")+
																				((_nodeType!=-1)?",SSE_SENSOR_TYPE="+_nodeType.ToString()+" ":"")+
																				((_firmwareVersion!=-1)?",SSE_FIRM_VERSION="+_firmwareVersion.ToString()+" ":"")+
																				((_lastMessageId!=-1)?",SSE_DEV_SEQ_ID="+_lastMessageId.ToString()+" ":"")+
																				((_magneticStatus!=-1)?",SSE_MAGNETIC_STATUS="+_magneticStatus.ToString()+" ":"")+
																				((_opticStatus!=-1)?",SSE_OPTICAL_STATUS="+_opticStatus.ToString()+" ":"")+
																				((_ambientLightOverExposure!=-1)?",SSE_AMBIENT_LIGHT_OVEREXPOSURE="+_ambientLightOverExposure.ToString()+" ":"")+
																				((_magneticUncalibrated!=-1)?",SSE_MAGNETIC_UNCALIBRATED="+_magneticUncalibrated.ToString()+" ":"")+
																				((_iceDetection!=-1)?",SSE_ICE_DETECTION="+_iceDetection.ToString()+" ":"")+
																				((_opticalFreeQuality!=-1)?",SSE_OPTICAL_FREE_QUALITY="+_opticalFreeQuality.ToString()+" ":"")+
																				((_opticalOccupiedQuality!=-1)?",SSE_OPTICAL_OCCUPIED_QUALITY="+_opticalOccupiedQuality.ToString()+" ":"")+
																				((_temperature!=-1)?",SSE_TEMPERATURE="+_temperature.ToString()+" ":"")+
																				((_operationId!=-1)?",SSE_OPE_ID="+_operationId.ToString()+" ":",SSE_OPE_ID=NULL ")+
																				((_operationDate.Length>0)?",SSE_OPE_MOVDATE=to_date('"+_operationDate+"', 'HH24MISSDDMMYY') ":",SSE_OPE_MOVDATE=NULL ")+
														" where SSE_ID={0} and	SSE_SEQ_ID<{1}", 
																				_spaceSensorId, _seqId,
																				_statusId, _statusDate, _errorId, _errorDate);

					
					int iNumRegs=oraCmd.ExecuteNonQuery();
					if (iNumRegs==1)
					{
						bRes=true;
					}
					else if (iNumRegs==0)
					{
						logger.AddLog("[Msg80:Process]: Sensor Not found, or Message older than current: SQL:"+oraCmd.CommandText,LoggerSeverities.Error);
						bRes=true;
					}
					else
					{
						bRes=false;
					}

				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg80:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg80:Process]: Error: "+e.Message,LoggerSeverities.Error);
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



		#endregion
	
	}	
}
