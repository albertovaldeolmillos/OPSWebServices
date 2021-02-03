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
	/// m70 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg70 : MsgReceived, IRecvMessage
	{

		private int _unitId;
		private int _platformId;
		private int _statusId;
		private string	_statusDate="";
		private int _errorId;
		private string _errorDate;

/*
 * 
 * <m70 id="12" ret="1"><u>103</u>
 *						<d>120145170310</d>
 *						<pi>15</pi>
 *						<s>4</s>
 *						<e>0</e>
 *						<ed>152021090310</ed>
 * </m70>
 */		



		/// <summary>
		/// Constructs a new m70 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg70(XmlDocument msgXml) : base(msgXml) {}

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
					case "pi": _platformId = Convert.ToInt32(n.InnerText); break;
					case "s": _statusId = Convert.ToInt32(n.InnerText); break;
					case "d": _statusDate = n.InnerText; break;
					case "e": _errorId = Convert.ToInt32(n.InnerText); break;
					case "ed": _errorDate = n.InnerText; break;
			
					

				}
			}
		}

		#region DefinedRootTag(m70)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m70"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m70 message.
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
				

					oraCmd.CommandText =	string.Format("update platforms set PLAT_STATUS={0}, "+
																			   "PLAT_STATUS_DATE=to_date('{1}', 'HH24MISSDDMMYY'), "+
																			   "PLAT_ERROR={2}, "+
																			   "PLAT_ERROR_DATE=to_date('{3}', 'HH24MISSDDMMYY') "+
														  "where plat_id={4}", _statusId, _statusDate, _errorId, _errorDate, _platformId);
					if (oraCmd.ExecuteNonQuery()==1)
					{
						bRes=true;
					}
				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg70:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg70:Process]: Error: "+e.Message,LoggerSeverities.Error);
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
