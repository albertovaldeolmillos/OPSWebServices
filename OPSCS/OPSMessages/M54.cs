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
	/// m54 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg54 : MsgReceived, IRecvMessage
	{

		private const int ROTATION_ESTANDAR=0;
		private const int ROTATION_QUICK=1;


		private int	_streetStretch;
		private string	_StartDate="";
		private string	_EndDate="";
		private int _unitId;
		private int _userId;
		private int _nResNoTicket;
		private int _nResTicket;
		private int _nNoResNoTicket;
		private int _nNoResTicket;
		private int _nSpecialVehicle;
		private int _nMinusvalid;
		private int _nHoles;
		private int _nRotationType=0;

		



		/// <summary>
		/// Constructs a new m54 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg54(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "ss": _streetStretch = Convert.ToInt32(n.InnerText); break;
					case "d1": _StartDate = n.InnerText; break;
					case "d2": _EndDate = n.InnerText; break;
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "z": _userId = Convert.ToInt32(n.InnerText); break;
					case "rnt": _nResNoTicket = Convert.ToInt32(n.InnerText); break;
					case "rt": _nResTicket = Convert.ToInt32(n.InnerText); break;
					case "nrnt": _nNoResNoTicket = Convert.ToInt32(n.InnerText); break;
					case "nrt": _nNoResTicket = Convert.ToInt32(n.InnerText); break;
					case "sv": _nSpecialVehicle = Convert.ToInt32(n.InnerText); break;
					case "min": _nMinusvalid = Convert.ToInt32(n.InnerText); break;
					case "hl": _nHoles = Convert.ToInt32(n.InnerText); break;		
					case "ty": _nRotationType = Convert.ToInt32(n.InnerText); break;		
			
					

				}
			}
		}

		#region DefinedRootTag(m54)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m54"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m52 message.
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
			bool bExist=false;


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
				
					if (_nRotationType!=-1)
					{

						if (!ExistRotation(ref bExist,ref oraDBConn,ref oraCmd))
						{
							bRes=false;
						}
						else
						{
							bRes=true;
						}

						if ((bRes)&&(!bExist))
						{



							bRes=false;
							oraCmd.CommandText="";
							if (_nRotationType==ROTATION_ESTANDAR)
							{

								oraCmd.CommandText =	string.Format("insert into SS_ROTATIONS (SSR_SS_ID, SSR_INI_DATE, SSR_END_DATE, SSR_USR_ID, SSR_UNI_ID, SSR_RESI_NO_TICKET, SSR_RESI_TICKET, SSR_NO_RESI_NO_TICKET, SSR_NO_RESI_TICKET, SSR_SPECIAL_VEHICLE, SSR_MINUSVALID, SSR_ROT_TYPE) "+
									"values ({0}, to_date('{1}', 'HH24MISSDDMMYY'), to_date('{2}', 'HH24MISSDDMMYY'), {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})",															   
									_streetStretch,
									_StartDate,
									_EndDate,
									_userId,
									_unitId,
									_nResNoTicket,
									_nResTicket,
									_nNoResNoTicket,
									_nNoResTicket,
									_nSpecialVehicle,
									_nMinusvalid,
									_nRotationType);
							}
							else
							{

								oraCmd.CommandText =	string.Format("insert into SS_ROTATIONS (SSR_SS_ID, SSR_INI_DATE, SSR_END_DATE, SSR_USR_ID, SSR_UNI_ID, SSR_HOLES, SSR_ROT_TYPE) "+
									"values ({0}, to_date('{1}', 'HH24MISSDDMMYY'), to_date('{2}', 'HH24MISSDDMMYY'), {3}, {4}, {5}, {6})",															   
									_streetStretch,
									_StartDate,
									_EndDate,
									_userId,
									_unitId,
									_nHoles,
									_nRotationType);
							}
								
								
							if (oraCmd.CommandText!="")
							{								
								oraCmd.ExecuteNonQuery();
								bRes=true;
							}
							
						}

					}
				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg54:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg54:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistRotation(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bExist=false;

			try
			{
				oraCmd.CommandText =	string.Format("select count(*) "+    
													  "from   SS_ROTATIONS "+
													  "where  SSR_SS_ID = {0} "+
													  "  and  SSR_UNI_ID = {1} "+
													  "  and  TO_CHAR(SSR_INI_DATE,'HH24MISSDDMMYY') = '{2}'",
													  _streetStretch, _unitId, _StartDate );
				

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


		#endregion
	
	}	
}
