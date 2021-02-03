using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Data;
using OPS.Components;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// M57 - Replications version query.
	/// </summary>
	internal sealed class Msg57 : MsgReceived, IRecvMessage
	{
		private int _replicationDefId;
		private int _unitId;

		/// <summary>
		/// Constructs a new M57 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg57(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "rp": _replicationDefId = Convert.ToInt32(n.InnerText); break;
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
				}
			}
		}

		#region DefinedRootTag(m57)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m57"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m57 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public StringCollection Process()
		{
			CmpReplication replication = new CmpReplication();
			DataTable dt = replication.ReplicationVersionQuery(_replicationDefId);
			string result = null;
			foreach (object o in dt.Rows)
			{
				DataRow dr = (DataRow)o;
				result += "<x id=\"" + Convert.ToString(dr["VER_TABLE"]) 
					+ "\" v=\"" + Convert.ToString(dr["VER_VERSION"]) + "\"/>";
			}

			StringCollection sc = new StringCollection();
			sc.Add(new AckMessage(_msgId, result).ToString());
			return sc;
		}

		#endregion
	}	
}
