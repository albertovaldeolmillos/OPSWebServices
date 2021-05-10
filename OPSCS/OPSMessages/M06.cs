using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
using CS_OPS_TesM1;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;



namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to handle de m6 message.
	/// </summary>
	public sealed class Msg06 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m6)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg06()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m6"; } }
		#endregion

		#region Variables, creation and parsing

		private int			_unit;
		private DateTime	_date;
		private int _MessageNumber;
		private int _MessageType;
		private string _Message	= "";

		/// <summary>
		/// Constructs a new msg06 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg06(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "n": _MessageNumber = Convert.ToInt32(n.InnerText); break;
					case "t": _MessageType = Convert.ToInt32(n.InnerText); break;
					case "m":	_Message		= n.InnerText;									
						break;


				}
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Inserts a new register in the OPERATIONS table, and if everything is succesful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
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

					if(logger != null)
						logger.AddLog(string.Format("[Msg6:Process]: insert into MSGS_TEXT  "+
							"(MSTE_DATE, MSTE_UNIT, MSTE_NUMBER, MSTE_TYPE, MSTE_TEXT) values "+
							"(TO_DATE('{0}','HH24MISSDDMMYY'),{1},{2},{3},'{4}')",
							OPS.Comm.Dtx.DtxToString(_date),
							_unit,
							_MessageNumber,
							_MessageType,
							_Message),				
							LoggerSeverities.Debug);

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
					oraCmd.CommandText=string.Format("insert into MSGS_TEXT  "+
						"(MSTE_DATE, MSTE_UNIT, MSTE_NUMBER, MSTE_TYPE, MSTE_TEXT) values "+
						"(TO_DATE('{0}','HH24MISSDDMMYY'),{1},{2},{3},'{4}')",
						OPS.Comm.Dtx.DtxToString(_date),
						_unit,
						_MessageNumber,
						_MessageType,
						_Message);
				

					oraCmd.ExecuteNonQuery();

					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);

				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg6:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

			return res;
		}


		#endregion
	}
}
