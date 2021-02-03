using System;
using System.Xml;

namespace OPS.Comm
{
	/// <summary>
	/// Header of the messages sent through MSMQ
	/// </summary>
	public abstract class MSMQHeader
	{
		protected string _xml;

		public MSMQHeader()
		{
			_xml = null;
		}

		/// <summary>
		/// Builds a header by parsing the xml string.
		/// </summary>
		/// <param name="sxml">String in the same XML format that obtained using FecsBecsHeader::ToString()</param>
		public MSMQHeader(string xml)
		{
			_xml = xml;
		}

		/// <summary>
		/// Gets header stringified in XML format
		/// </summary>
		/// <returns>XML string.</returns>
		public override string ToString()
		{
			return _xml;
		}
	}

	/// <summary>
	/// Header of every message sended from BECS to FECS
	/// Format is as follows:
	/// <becsfecs sec="Number of message in the secuence" max="Number of messages in the sequence" 
	/// newmsg="1 if is new message or 0 if is a response">
	/// <dst>Unique ID of the destination (UNI_ID or other unique identifier)</dst>
	/// <fid>internal FECS id (passed by FECS)</fid>
	/// <ip>last known IP of the device dst</ip>
	/// </becsfecs>
	/// </summary>
	public class BecsFecsHeader : MSMQHeader
	{
		private string _id;
		private int _sec;
		private int _maxsec;
		private string _fid;
		private string _ip;
		private bool _newmsg;

		// UNI_ID of destination
		public string DstId { get { return _id; } }
		// Fecs UID of the Fecs Client object (only used on response messages)
		public string Fid { get { return _fid; } }
		// Last known IP of the destination
		public string IP { get { return _ip; } }
		// Ordinal secuence message
		public int Sec { get { return _sec; } }
		// Number of messages in the secuence
		public int MaxSec { get { return _maxsec; } }
		// If true is a new message, if false is a response to a previous FECS message
		public bool NewMsg { get { return _newmsg; } }

		/// <summary>
		/// Builds a BecsFecsHeader
		/// </summary>
		/// <param name="id">ID of the destination</param>
		/// <param name="ip">Last known destination IP</param>
		/// <param name="fid">GUID of the client of the FECS (passed by FECS).
		/// Can be null if is not a response message (a null fid forces FECS to search for a valid
		/// Client or create a new Client object to send data).</param>
		/// <param name="sec">Secuential index of the message</param>
		/// <param name="maxsec">Maximum messages in the response</param>
		/// <param name="newmsg">Sets if message is a NEW message (true) or response to previous message (false)</param>
		public BecsFecsHeader (string id,  string ip, string fid, int sec, int maxsec, bool newmsg)
		{
			_id = id;
			_sec = sec;
			_fid = fid;
			_ip = ip;
			_maxsec = maxsec;
			_newmsg = newmsg;
			// Builds the xml string
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append ("<becsfecs sec=\"");
			sb.Append (_sec);
			sb.Append ("\" max=\"");
			sb.Append (_maxsec);
			sb.Append ("\" newmsg=\"");
			sb.Append (_newmsg ? '1' : '0');
			sb.Append ("\"><dst>");
			sb.Append (_id);
			sb.Append ("</dst>");
			if (_fid!=null && _fid != string.Empty)
			{
				sb.Append ("<fid>");
				sb.Append (_fid);
				sb.Append ("</fid>");
			}
			sb.Append("<ip>");
			sb.Append (_ip);
			sb.Append ("</ip></becsfecs>");
			_xml = sb.ToString();
		}

		/// <summary>
		/// Builds a new BecsFecsHeader parsing the sxml string
		/// </summary>
		/// <param name="xml">XML string, in the same format of BecsFecsHeader::ToString()</param>
		public BecsFecsHeader (string sxml)
		{
			_id = null;
			_sec = -1;
			_maxsec = -1;
			_fid = null;
			_ip = null;
			_newmsg = false;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml (sxml);
			XmlNode root = doc.DocumentElement;
			XmlAttribute attr = root.Attributes["sec"];
			if (attr!=null) _sec = Convert.ToInt32 (attr.Value);
			attr = root.Attributes["max"];
			if (attr!=null) _maxsec = Convert.ToInt32(attr.Value);
			attr = root.Attributes["newmsg"];
			if (attr!=null)  _newmsg = Convert.ToInt32 (attr.Value) == 1;

			foreach (XmlNode n in root.ChildNodes)
			{
				if (n.Name == "dst")  { _id = n.InnerText; continue; }
				if (n.Name == "fid")  { _fid = n.InnerText; continue; }
				if (n.Name == "ip") { _ip = n.InnerText; continue; }
			}
			_xml = sxml;
		}
	}

	/// <summary>
	/// Header of EVERY message send from FECS to becs
	/// format is as follows:
	///		<fecsbecs>
	///			<fid>Unique Identifier of FECS thread</fid>
	///			<ip>Current IP of the device associated to that thread</ip>
	///			<p src="sourceId" dtx="DateTime" />
	///		</fecsbecs>
	/// </summary>
	public class FecsBecsHeader : MSMQHeader
	{
		private PacketData _data;
		private string _fid;
		private string _ip;

		public string ID { get { return _fid; } }
		public PacketData PacketInfo { get { return _data; } } 
		public string IP { get { return  _ip; } }

		/// <summary>
		/// Builds a FecsBecsHeader parsing the sxml string.
		/// </summary>
		/// <param name="sxml">String in the same XML format that obtained using FecsBecsHeader::ToString()</param>
		public FecsBecsHeader (string sxml)
		{
			string dtx = null;
			string src = null;
			_fid = null;
			_ip = null;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml (sxml);
			XmlNode root = doc.DocumentElement;
			foreach (XmlNode n in root.ChildNodes)
			{
				if (n.Name == "fid") { _fid= n.InnerText; continue; }
				if (n.Name == "p") 
				{
					XmlAttribute attr = n.Attributes["src"];
					if (attr!=null) src= attr.Value;
					attr = n.Attributes["dtx"];
					if (attr!=null) dtx = attr.Value;
					continue;
				}
				if (n.Name == "ip") { _ip = n.InnerText; continue; }
			}
			_data = new PacketData (dtx, src);
			_xml = sxml;
		}

		/// <summary>
		/// Constructs a new FecsBecsHeader
		/// </summary>
		/// <param name="fid">Unique identifier to be included in the header</param>
		/// <param name="data">PacketData with data of the pack (dtx and src)</param>
		public FecsBecsHeader(string fid, string ip, PacketData data)
		{
			_fid = fid;
			_data = data;
			_ip = ip;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append ("<fecsbecs><fid>");
			sb.Append (_fid);
			sb.Append ("</fid><p");
			if (data.SourceId!=null) 
			{
				sb.Append(" src=\"");
				sb.Append (_data.SourceId);
				sb.Append ('\"');
			}
			if (data.HasDtx)
			{
				sb.Append (" dtx=\"");
				sb.Append (data.DtxToString());
				sb.Append ('\"');
			}
			sb.Append (" /><ip>");
			sb.Append (_ip);
			sb.Append ("</ip></fecsbecs>");
			_xml = sb.ToString();
		}
	}

	/// <summary>
	/// Header of the messages sent from BECS to CC
	/// </summary>
	public class BecsCcHeader : MSMQHeader
	{
		private int _id;
		private bool _isNew;

		/// <summary>
		/// Builds a new BecsCcHeader with the data supplied.
		/// </summary>
		/// <param name="id">Id of the message</param>
		/// <param name="isNew">True if message is a NEW message, False if it is a response to a previous message</param>
		public BecsCcHeader (int id, bool isNew)
		{
			_id = id;
			_isNew = isNew;
			_xml = "<becscc id=\"" + id + "\" new=\"" + (isNew ? 1 : 0) + "\" />";
		}

		/// <summary>
		/// Builds a new BecsCcHeader parsing the xml string
		/// </summary>
		/// <param name="xml">String in the same format that the one obtained using ToString()</param>
		public BecsCcHeader (string xml)
		{
			_id = 0;
			_isNew = false;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode root = doc.DocumentElement;
			foreach (XmlNode n in root.ChildNodes)
			{
				switch (n.Name)
				{
					case "id": _id = Convert.ToInt32(n.InnerText); break;
					case "new": _isNew = Convert.ToBoolean(n.InnerText); break;
				}
			}
			_xml = xml;
		}

		/// <summary>Returns de id of the message</summary>
		public int Id { get { return _id; } }
		/// <summary>True if message is a NEW message, False if it is a response to a previous message</summary>
		public bool IsNew { get { return _isNew; } }
	}

	/// <summary>
	/// Header of the messages sent from CC to BECS
	/// </summary>
	public class CcBecsHeader : MSMQHeader
	{
		protected int _id;

		/// <summary>
		/// Builds a new CcBecsHeader with the data supplied.
		/// </summary>
		/// <param name="id">Identifier of the message</param>
		public CcBecsHeader(int id)
		{
			_id = id;
			_xml = "<ccbecs id=\"" + id + "\" />";
		}

		/// <summary>
		/// Builds a new CcBecsHeader parsing the xml string.
		/// </summary>
		/// <param name="xml">String in the same format that the one obtained using ToString()</param>
		public CcBecsHeader(string xml)
		{
			_id = 0;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlNode root = doc.DocumentElement;
			foreach (XmlNode n in root.ChildNodes)
			{
				switch (n.Name)
				{
					case "id": _id = Convert.ToInt32(n.InnerText); break;
				}
			}
			_xml = xml;
		}

		/// <summary>Returns de id of the message</summary>
		public int Id { get { return _id; } }
	}
}
