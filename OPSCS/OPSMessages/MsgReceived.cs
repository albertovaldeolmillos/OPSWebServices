using System;
using System.Xml;
using System.Collections.Specialized;
using OPS.Comm;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to be the base class of all received messages.
	/// All received messgaes MUST derive of that class AND implement IRecvMessage interface
	/// </summary>
	public abstract class MsgReceived
	{
		//SYSTEM_IDENTIFIERS

		protected const int SYSTEM_IDENTIFIER_DURANGO	= 1;
		protected const int SYSTEM_IDENTIFIER_CASTROURDIALES	= 14;
		protected const int SYSTEM_IDENTIFIER_CORDOBA	= 22;
		protected const int SYSTEM_IDENTIFIER_ZARAGOZA= 25;
		protected const int SYSTEM_IDENTIFIER_DONOSTIA= 26;
		protected const int SYSTEM_IDENTIFIER_BANYOLES= 52;
		protected const int SYSTEM_IDENTIFIER_FIGUERES= 53;
		protected const int SYSTEM_IDENTIFIER_HONDARRIBIA= 10;
		protected const int SYSTEM_IDENTIFIER_GETXO= 4;


		// Xml document with the message
		protected XmlDocument _docXml;
		// ID of the message
		protected long _msgId;
		// # of retries (0 based)
		protected int  _msgRetry;
		// ID of the destination
		protected string _msgDest;
		// ID of the origin
		protected string _msgOrig;
		// root of the document
		protected XmlNode _root;
		// Messages session
		protected MessagesSession _session;
		// Logger
		protected ILogger m_logger = null;

		/// <summary>
		/// Constructs a new MsgReceived with the data of the string
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		/// <param name="session">The messages session</param>
		protected MsgReceived (XmlDocument msgXml)
		{
			_docXml = msgXml;
			// Gets some attributes from the message (ID, optional ret, optional dst) that are common to ALL messages
			_root = _docXml.DocumentElement;
			XmlAttributeCollection attrs = _root.Attributes;
			if (attrs!=null)
			{
				_msgId = Convert.ToInt64(attrs["id"].Value);
				XmlAttribute attr = attrs["ret"];
				_msgRetry = attr!= null ? Convert.ToInt32(attr.Value) : 0;
				attr = attrs["dst"];
				_msgDest = attr!=null ? attr.Value : null;
			}
			// Parses the message
			DoParseMessage();
		}
		
		protected MsgReceived()
		{
		}

		/// <summary>
		/// Constructs a new MsgCeceived with the data of the string and sets logger
		/// </summary>
		/// <param name="msgXml"></param>
		/// <param name="logger"></param>
		protected MsgReceived (XmlDocument msgXml, ILogger logger)
		{
			m_logger = logger;
			m_logger.AddLog("[MsgReceived]: Setting Message",LoggerSeverities.Debug);
			try
			{
				//
				_docXml = msgXml;
				// Gets some attributes from the message (ID, optional ret, optional dst) that are common to ALL messages
				_root = _docXml.DocumentElement;
				XmlAttributeCollection attrs = _root.Attributes;
				if (attrs!=null)
				{
					_msgId = Convert.ToInt64(attrs["id"].Value);
					XmlAttribute attr = attrs["ret"];
					_msgRetry = attr!= null ? Convert.ToInt32(attr.Value) : 0;
					attr = attrs["dst"];
					_msgDest = attr!=null ? attr.Value : null;
				}
				// Parses the message
				DoParseMessage();														
				//	
			}
			catch (Exception ex)
			{
				m_logger.AddLog("[MsgReceived]: EXCEPTION " + ex.ToString(),LoggerSeverities.Debug);
				throw ex;
			}
			
		}

		/// <summary>
		/// Parses the message and stores the values in the members of the class.
		/// That method is called by the constructor of MsgReceived, and MUST be reimplemented
		/// on each class.
		/// The derived class can assume that all protected members defined in MsgReceived are
		/// set when that method is called.
		/// Note that the DoParseMessage is called BEFORE the constructor of the inherited class, so
		/// the constructor of the inherited class SHOULD NOT to init vars that are set in DoParseMessage.
		/// The flow of the methods is 1) Base ctor -> 2) Inherited DoParseMessage -> 3) Inherited ctor
		/// </summary>
		protected abstract void DoParseMessage();

		#region Partial implementation of IRecvMessage (so the MsgReceived derived classes have not to implement MsgId)

		public long MsgId { get { return _msgId; } }

		#endregion

		#region Trace Region
		public void TraceMessage()
		{

			if(m_logger != null)
			{
				
				m_logger.AddLog("[MsgReceived]: ID " + _msgId.ToString(),LoggerSeverities.Debug);
		//		_docXml;
		// ID of the message
		//protected long _msgId;
		// # of retries (0 based)
		//protected int  _msgRetry;
		// ID of the destination
		//protected string _msgDest;
		// root of the document
		//protected XmlNode _root;
			}
			} // end if
		#endregion



		public MessagesSession Session
		{
			get { return _session; }
			set { _session = value; }
		}

		public string MsgOrig
		{
			get { return _msgOrig; }
			set { _msgOrig = value; }
		}

		#region Useful methods to return specific messages (ACK, NACK)

		protected StringCollection ReturnAck(AckMessage.AckTypes type)
		{
			StringCollection sc = new StringCollection();
			sc.Add(new AckMessage(_msgId, type).ToString());
			return sc;
		}

		protected StringCollection ReturnNack(NackMessage.NackTypes type)
		{
			StringCollection sc = new StringCollection();
			sc.Add(new NackMessage (_msgId, type).ToString());
			return sc;
		}

		#endregion
	}



}