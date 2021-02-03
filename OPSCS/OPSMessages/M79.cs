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
	/// m79 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg79 : MsgReceived, IRecvMessage
	{

		private int _unitId=-1;
		private int _elecChargerId=-1;
		private string	_statusDate="";
		private int _errorId=-1;
		private string _errorDate="";
		private long _seqId=-1;
		int				_pylonCoverState=-1;
		string			_lastChangeCoverStateDate="";	
		int				_plugState0=-1; 
		long			_totalEnergy0=-1;
		long			_partialPower0=-1;
		long			_partialEnergy0=-1;
		long			_currentChargeTime0=-1;
		long			_operationID0=-1; 
		string			_operationEndDate0="";
		string			_vehicleId0=""; 
		string			_unlockCode0="";	

		int				_plugState1=-1; 
		long			_totalEnergy1=-1;
		long			_partialPower1=-1;
		long			_partialEnergy1=-1;
		long			_currentChargeTime1=-1;
		long			_operationID1=-1; 
		string			_operationEndDate1="";
		string			_vehicleId1=""; 
		string			_unlockCode1="";	
		
		



/*
<m79 id="83" ret="1">
		<u>102</u>
		<ec>1</ec>
		<seq>4</seq>
		<e>2</e>
		<d>171937200312</d>
		<ed>000000010100</ed>
		<ps>1</ps>
		<psd>171946200312</psd>
		<pls0>1</pls0>
		<ten0>1768</ten0>
		<ppw0>0</ppw0>
		<pen0>0</pen0>
		<cct0>0</cct0>
		<o0>1</o0>
		<od0>174800200312</od0>
		<opl0>134343</opl0>
		<ouc0>JCJNN</ouc0>
		<pls1>1</pls1>
		<ten1>149</ten1>
		<ppw1>0</ppw1>
		<pen1>0</pen1>
		<cct1>0</cct1>
		<o1>2</o1>
		<od1>174800200312</od1>
		<opl1>142342</opl1>
		<ouc1>XMUW4</ouc1>
</m79>
*/

		/// <summary>
		/// Constructs a new m79 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg79(XmlDocument msgXml) : base(msgXml) {}

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
					case "ec": _elecChargerId = Convert.ToInt32(n.InnerText); break;
					case "d": _statusDate = n.InnerText; break;
					case "e": _errorId = Convert.ToInt32(n.InnerText); break;
					case "ed": _errorDate = n.InnerText; break;
					case "seq": _seqId = Convert.ToInt32(n.InnerText); break;
					case "ps": _pylonCoverState = Convert.ToInt32(n.InnerText); break;
					case "psd": _lastChangeCoverStateDate = n.InnerText; break;

					case "pls0": _plugState0 = Convert.ToInt32(n.InnerText); break;
					case "ten0": _totalEnergy0 = Convert.ToInt32(n.InnerText); break;
					case "ppw0": _partialPower0 = Convert.ToInt32(n.InnerText); break;
					case "pen0": _partialEnergy0 = Convert.ToInt32(n.InnerText); break;
					case "cct0": _currentChargeTime0 = Convert.ToInt32(n.InnerText); break;
					case "o0": _operationID0 = Convert.ToInt32(n.InnerText); break;
					case "od0": _operationEndDate0 = n.InnerText; break;
					case "opl0": _vehicleId0 = n.InnerText; break;
					case "ouc0": _unlockCode0 = n.InnerText; break;

					case "pls1": _plugState1 = Convert.ToInt32(n.InnerText); break;
					case "ten1": _totalEnergy1 = Convert.ToInt32(n.InnerText); break;
					case "ppw1": _partialPower1 = Convert.ToInt32(n.InnerText); break;
					case "pen1": _partialEnergy1 = Convert.ToInt32(n.InnerText); break;
					case "cct1": _currentChargeTime1 = Convert.ToInt32(n.InnerText); break;
					case "o1": _operationID1 = Convert.ToInt32(n.InnerText); break;
					case "od1": _operationEndDate1 = n.InnerText; break;
					case "opl1": _vehicleId1 = n.InnerText; break;
					case "ouc1": _unlockCode1 = n.InnerText; break;

				}
			}
		}

		#region DefinedRootTag(m79)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m79"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m79 message.
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
			


					oraCmd.CommandText =	string.Format("update ELEC_CHARGERS set ECH_STATE_SINCE=to_date('{2}', 'HH24MISSDDMMYY'), "+
																			   "ECH_IERROR={3}, "+
																			   "ECH_ERR_SINCE=to_date('{4}', 'HH24MISSDDMMYY'), "+
																			   "ECH_SEQ={1}, "+
																			   "ECH_PYLON_COVER_STATE={5}, "+													   
																			   "ECH_LAST_COVER_CHANGE_DATE=to_date('{6}', 'HH24MISSDDMMYY') "+
																				((_plugState0!=-1)?",ECH_PLUG_STATE_1="+_plugState0.ToString()+" ":",ECH_PLUG_STATE_1=NULL ")+
																				((_totalEnergy0!=-1)?",ECH_TOTAL_ENERGY_1="+_totalEnergy0.ToString()+" ":",ECH_TOTAL_ENERGY_1=NULL ")+
																				((_partialPower0!=-1)?",ECH_PARTIAL_POWER_1="+_partialPower0.ToString()+" ":",ECH_PARTIAL_POWER_1=NULL ")+
																				((_partialEnergy0!=-1)?",ECH_PARTIAL_ENERGY_1="+_partialEnergy0.ToString()+" ":",ECH_PARTIAL_ENERGY_1=NULL ")+
																				((_currentChargeTime0!=-1)?",ECH_CURRENT_CHARGE_TIME_1="+_currentChargeTime0.ToString()+" ":",ECH_CURRENT_CHARGE_TIME_1=NULL ")+
																				((_operationID0!=-1)?",ECH_OPE_ID_1="+_operationID0.ToString()+" ":",ECH_OPE_ID_1=NULL ")+
																				(((_operationID0!=-1)&&(_operationEndDate0.Length>0))?",ECH_OPE_ENDDATE_1=to_date('"+_operationEndDate0+"', 'HH24MISSDDMMYY') ":",ECH_OPE_ENDDATE_1=NULL ")+
																				(((_operationID0!=-1)&&(_vehicleId0.Length>0))?",ECH_VEHICLEID_1='"+_vehicleId0+"' ":",ECH_VEHICLEID_1=NULL ")+
																				(((_operationID0!=-1)&&(_unlockCode0.Length>0))?",ECH_UNLOCK_CODE_1='"+_unlockCode0+"' ":",ECH_UNLOCK_CODE_1=NULL ")+
																				((_plugState1!=-1)?",ECH_PLUG_STATE_2="+_plugState1.ToString()+" ":",ECH_PLUG_STATE_2=NULL ")+
																				((_totalEnergy1!=-1)?",ECH_TOTAL_ENERGY_2="+_totalEnergy1.ToString()+" ":",ECH_TOTAL_ENERGY_2=NULL ")+
																				((_partialPower1!=-1)?",ECH_PARTIAL_POWER_2="+_partialPower1.ToString()+" ":",ECH_PARTIAL_POWER_2=NULL ")+
																				((_partialEnergy1!=-1)?",ECH_PARTIAL_ENERGY_2="+_partialEnergy1.ToString()+" ":",ECH_PARTIAL_ENERGY_2=NULL ")+
																				((_currentChargeTime1!=-1)?",ECH_CURRENT_CHARGE_TIME_2="+_currentChargeTime1.ToString()+" ":",ECH_CURRENT_CHARGE_TIME_2=NULL ")+
																				((_operationID1!=-1)?",ECH_OPE_ID_2="+_operationID1.ToString()+" ":",ECH_OPE_ID_2=NULL ")+
																				(((_operationID1!=-1)&&(_operationEndDate1.Length>0))?",ECH_OPE_ENDDATE_2=to_date('"+_operationEndDate1+"', 'HH24MISSDDMMYY') ":",ECH_OPE_ENDDATE_2=NULL ")+
																				(((_operationID1!=-1)&&(_vehicleId1.Length>0))?",ECH_VEHICLEID_2='"+_vehicleId1+"' ":",ECH_VEHICLEID_2=NULL ")+
																				(((_operationID1!=-1)&&(_unlockCode1.Length>0))?",ECH_UNLOCK_CODE_2='"+_unlockCode1+"' ":",ECH_UNLOCK_CODE_2=NULL ")+
																			" where ECH_ID={0} and	ECH_SEQ<{1}", 
																				_elecChargerId, _seqId,
																				 _statusDate, _errorId, _errorDate,_pylonCoverState,_lastChangeCoverStateDate);



					
					int iNumRegs=oraCmd.ExecuteNonQuery();
					if (iNumRegs==1)
					{
						bRes=true;
					}
					else if (iNumRegs==0)
					{
						logger.AddLog("[Msg79:Process]: Sensor Not found, or Message older than current: SQL:"+oraCmd.CommandText,LoggerSeverities.Error);
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
						logger.AddLog("[Msg79:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg79:Process]: Error: "+e.Message,LoggerSeverities.Error);
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
