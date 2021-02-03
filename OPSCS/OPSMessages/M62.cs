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


namespace OPS.Comm.Becs.Messages
{


	/// <summary>
	/// Class to handle de m9 message.
	/// </summary>
	public sealed class Msg62 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m62)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg62()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m62"; } }
		#endregion

		#region Variables, creation and parsing

		private int			_unit;    
		private string		_date;		



		/// <summary>
		/// Constructs a new msg06 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg62(XmlDocument msgXml) : base(msgXml) {}

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
					*/

					case "u":		 _unit  = Convert.ToInt32(n.InnerText); break;    
					case "d":		_date = n.InnerText; break;	

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
			try
			{				
				res=ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
			}
			catch(Exception e)
			{
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{
			}

			return res;
		}


		#endregion
	}
}
