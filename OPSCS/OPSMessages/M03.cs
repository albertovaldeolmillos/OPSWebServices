using System;
using System.Collections.Specialized;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace OPS.Comm.Becs.Messages
{	
	/// <summary>
	/// This is the message that a PDM sends to CC
	/// to notify about its status and alarms
	/// </summary>
	internal sealed class Msg03 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m3)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m3"; } }
		#endregion

		#region Variables, creation and parsing

		private int _unit;
		private int _status;
		private int _loadUnloadStatus=-1;
		private uint[] _alarmsMasks;
		private DateTime _date;
		private int	_user=-999;
	
		/// <summary>
		/// Constructs a new msg03 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg03(XmlDocument msgXml) : base(msgXml) {}
		
		/// <summary>
		/// Overriding of the DoParseMessage to get the unit, 
		/// all the alarms and the status and to store them in private vars
		/// </summary>
		protected override void DoParseMessage()
		{	
			ILogger logger =null;
			logger = DatabaseFactory.Logger;

			try
			{
				foreach (XmlNode node in _root.ChildNodes)
				{
					switch (node.Name)
					{
						case "u": _unit = Convert.ToInt32(node.InnerText); break;
						case "s": _status = Convert.ToInt32(node.InnerText); break;
						case "a":		// Tag <a> has an hexa number (can be bigger than 32 bits). ==> Each 8 chars are a uint
							System.Text.StringBuilder sHexa = new System.Text.StringBuilder(node.InnerText.Trim());
							int sHexaLength = sHexa.Length;
							int nblocks = (int)Math.Ceiling (sHexaLength / 8.0);		// 8 chars are a uint...
							_alarmsMasks = new uint[nblocks];
							// get each one of the blocks (8 hexa digits each one)
							for (int i=0; i< nblocks; i++)
							{
								string sBlock = null;
								if (sHexa.Length <=8) { sBlock = sHexa.ToString(); }
								else 
								{
									sBlock = sHexa.ToString().Substring(sHexaLength - 8, 8);
									sHexa.Remove (sHexaLength - 8, 8);
									sHexaLength = sHexa.Length;
								}
								// The block is a hexa number (uint) that is stored in _maskAlarms[i]
								_alarmsMasks[i] = UInt32.Parse (sBlock, System.Globalization.NumberStyles.AllowHexSpecifier);
							}
							break;
						case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
						case "lus": _loadUnloadStatus= Convert.ToInt32(node.InnerText); break;
						case "us": _user = Convert.ToInt32(node.InnerText); break;
					}
				}
			}
			catch (Exception ex)
			{
				if(logger!=null)
					logger.AddLog("[Msg12:DoParseMessage] ERRROR: " + ex.ToString(),LoggerSeverities.Error);
				throw ex;
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Process the Message. The process of this message is specified in OPS_D_M3.doc
		/// </summary>
		/// <returns>Response for the message (1 string)</returns>
		public StringCollection Process()
		{
			StringCollection res=null;
			// Get ALL current alarms of that device
			OPS.Comm.Becs.Components.CmpAlarmsDevices ad = _session.AlarmsDevices;
			//OPS.Comm.Becs.Components.CmpAlarmsDevices ad = new OPS.Comm.Becs.Components.CmpAlarmsDevices();
			if (ad.UpdateAlarms(_unit, _alarmsMasks,_date))
			{

				// Update UNIT status
				CmpUnitsDB udb = new CmpUnitsDB();
				udb.UpdateStatus(_unit, _status);


				if (_loadUnloadStatus>=0)
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
							oraCmd.CommandText="";

							int iMask=1;
							int iStatus;
							int iSlot;
							for(int i=0; i<3;i++)
							{
								
								iSlot=i+1;
								iStatus = Convert.ToInt32((_loadUnloadStatus&iMask)>0);
								iMask=iMask*2;

								oraCmd.CommandText =	string.Format("update umdm_stations_slots  set "+
									"USS_CURRENT_STATE={0}, "+
									"USS_UPDDATE=to_date('{1}', 'HH24MISSDDMMYY') "+
									"where  USS_ORDER_STATION = {2} "+
									"  and  USS_US_ID = {3}",
									iStatus,OPS.Comm.Dtx.DtxToString(_date),iSlot,_unit);

								oraCmd.ExecuteNonQuery();
							}

							res=ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
						}
						else
						{
							if(logger != null)
								logger.AddLog("[Msg3:Process]: Error: BD is not opened",LoggerSeverities.Error);
							res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}
					}
					catch(Exception e)
					{
						if(logger != null)
							logger.AddLog("[Msg3:Process]: Error: "+e.Message,LoggerSeverities.Error);
						res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
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
				else
				{
					

					if (_user>=-1)
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
								oraCmd.CommandText="";


								oraCmd.CommandText =	string.Format("update units  set "+
									"uni_usr_id={0} "+								
									"where uni_id = {1}",
									_user==-1?"NULL":_user.ToString(),_unit);

								oraCmd.ExecuteNonQuery();

								res=ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
							}
							else
							{
								if(logger != null)
									logger.AddLog("[Msg3:Process]: Error: BD is not opened",LoggerSeverities.Error);
								res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
							}
						}
						catch(Exception e)
						{
							if(logger != null)
								logger.AddLog("[Msg3:Process]: Error: "+e.Message,LoggerSeverities.Error);
							res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
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
					else
					{
						res=ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}

				}
			}
			else
			{
				res=ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
			}

			
			return res;
		}

		#endregion
	}
}