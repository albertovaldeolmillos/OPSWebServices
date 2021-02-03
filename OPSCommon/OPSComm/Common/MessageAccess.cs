using System;
using System.Collections;
using System.Xml;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Summary description for MessageAccess.
	/// </summary>
	public class MessageAccess
	{
		public class Element
		{
			public Element()
			{
			}
			public Element(string tagName, string tagValue)
			{
				_tagName = tagName;
				_tagValue = tagValue;
			}
			public string TagName
			{
				get { return _tagName; }
				set {_tagName = value; }
			}
			public string TagValue
			{
				get { return _tagValue; }
				set {_tagValue = value; }
			}
			private string _tagName;
			private string _tagValue;
		}

		/// <summary>
		/// Constructor that needs a message type, identifier, retry count, destination
		/// and priority
		/// </summary>
		/// <param name="msgType"></param>
		/// <param name="id"></param>
		/// <param name="retries"></param>
		/// <param name="destination"></param>
		/// <param name="priority"></param>
		public MessageAccess(string msgType, string id, string retries, string destination, string priority)
		{
			_xmlDoc = new XmlDocument();
			XmlElement msgElem =  _xmlDoc.CreateElement(msgType);
			_xmlDoc.AppendChild(msgElem);
			XmlAttribute attr = _xmlDoc.CreateAttribute(Tags.MsgIdAttr);
			attr.Value = id;
			msgElem.Attributes.Append(attr);
			if (retries != null)
			{
				attr = _xmlDoc.CreateAttribute(Tags.RetriesAttr);
				attr.Value = retries;
				msgElem.Attributes.Append(attr);
			}
			if (destination != null)
			{
				attr = _xmlDoc.CreateAttribute(Tags.DestinationAttr);
				attr.Value = destination;
				msgElem.Attributes.Append(attr);
			}
			if (priority != null)
			{
				attr = _xmlDoc.CreateAttribute(Tags.PriorityAttr);
				attr.Value = priority;
				msgElem.Attributes.Append(attr);
			}
		}
		/// <summary>
		/// Constructor of a MessageAccess object from an xml document as a string
		/// </summary>
		/// <param name="xmlData"></param>
		public MessageAccess(string xmlData)
		{
			try
			{
				_xmlDoc = new XmlDocument();
				_xmlDoc.LoadXml(xmlData);
			}
			catch (XmlException xex)
			{
				//Debug.WriteLine("Message access constructor - " + xex.Message);
				CommMain.Logger.AddLog(xex);
				//Debug.WriteLine("- data: " + xmlData);
				CommMain.Logger.AddLog("XmlException data: " + xmlData, LoggerSeverities.Info);
				_xmlDoc = null;
			}
		}
		/// <summary>
		/// Returns a string representing this object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string retVal = null;
			if (_xmlDoc != null)
			{
				return XmlDocAsString(_xmlDoc);
			}
			return retVal;
		}
		/// <summary>
		/// Returns the value of with the specified name. To obtain nested values
		/// the path to the node must be specified using '/'. For example:
		/// "ap/r50/f"
		/// </summary>
		public string this[string tagName]
		{
			get
			{
				string tagValue = null;
				string[] lst = tagName.Split(new char[]{'/'});
				ArrayList aList = new ArrayList(lst);
				string lastPart = (string) aList[aList.Count-1];
				tagValue = GetNodeValue(aList, lastPart, _xmlDoc.FirstChild);
				return tagValue;
			}
		}
		/// <summary>
		/// Adds a new child to the root element using the supplied name and
		/// value
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="tagValue"></param>
		public void AddNode(string tagName, string tagValue)
		{
			if (_xmlDoc != null)
			{
				XmlElement root = (XmlElement) _xmlDoc.FirstChild;
				XmlElement elem = _xmlDoc.CreateElement(tagName);
				elem.InnerText = tagValue;
				root.AppendChild(elem);
			}
		}
		/// <summary>
		/// Adds a series of elements to the node specified by tagName. To
		/// add children to elements different of the root node, tagName must
		/// contain the path to the desired element, for example: "ap/r50"
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="values"></param>
		public void AddNode(string tagName, Element[] values)
		{
			if (_xmlDoc != null)
			{
				XmlNode root =  _xmlDoc.FirstChild;
				string[] lst = tagName.Split(new char[]{'/'});
				ArrayList aList = new ArrayList(lst);
				string lastPart = (string) aList[aList.Count-1];
				XmlNode nodeAdd = GetNode(aList, lastPart, root);
				if (nodeAdd != null)
				{
					foreach (Element e in values)
					{
						XmlNode n = _xmlDoc.CreateElement(e.TagName);
						n.InnerText = e.TagValue;
						nodeAdd.AppendChild(n);
					}
				}
			}
		}
		/// <summary>
		/// Returns a string containing the message represented as a telegram
		/// </summary>
		/// <param name="date"></param>
		/// <param name="deviceId"></param>
		/// <returns></returns>
		public string GetTelegram(string date, string deviceId)
		{
			string retVal = null;

			XmlDocument tlgDoc = new XmlDocument();
			XmlElement tlgRoot = tlgDoc.CreateElement(Tags.Packet);
			tlgDoc.AppendChild(tlgRoot);
			XmlAttribute attr = tlgDoc.CreateAttribute(Tags.PacketDateAttr);
			attr.Value = date;
			tlgRoot.Attributes.Append(attr);
			attr = tlgDoc.CreateAttribute(Tags.PacketSrcAttr);
			attr.Value = deviceId;
			tlgRoot.Attributes.Append(attr);
			if (_xmlDoc != null)
			{
				XmlNode node = tlgDoc.ImportNode(_xmlDoc.FirstChild, true);
				tlgRoot.AppendChild(node);
			}
			retVal = XmlDocAsString(tlgDoc);
			return retVal;
		}
		/// <summary>
		/// Returns the name of the message
		/// </summary>
		/// <returns>The name of the message. String.Empty if the name is
		/// not found</returns>
		public string GetMessageName()
		{
			string retVal = string.Empty;
			if (_xmlDoc != null)
			{
				XmlElement root = (XmlElement) _xmlDoc.FirstChild;
				if (root != null)
					retVal = root.Name;
			}
			return retVal;
		}
		/// <summary>
		/// Returns the name of the message
		/// </summary>
		/// <returns></returns>
		public string GetMessageId()
		{
			string retVal = null;
			string msgName = GetMessageName();
			if (msgName != null)
			{
				string path = String.Format("{0}/@{1}", msgName, Tags.MsgIdAttr);
				retVal = GetNodeAttribute(path);
			}
			return retVal;
		}
		/// <summary>
		/// Returns the value of an element attribute.
		/// </summary>
		/// <param name="attrPath">The specification of the attribute to
		/// obtain. For example: "ap/r50/@id"</param>
		/// <returns></returns>
		public string GetNodeAttribute(string attrPath)
		{
			string retVal = string.Empty;
			string[] lst = attrPath.Split(new char[]{'/'});
			ArrayList aList = new ArrayList(lst);
			string attr = (string) aList[aList.Count-1];
			if (attr.StartsWith("@"))
			{
				aList.RemoveAt(aList.Count-1);
				XmlNode root =  _xmlDoc.FirstChild;
				XmlNode node = GetNode(aList, (string) aList[aList.Count-1], root);
				if (node != null)
				{
					XmlAttribute xmlAtt =  node.Attributes[attr.Substring(1)];
					if (xmlAtt != null)
					{
						retVal = xmlAtt.Value;
					}
				}
			}
			return retVal;
		}
		/// <summary>
		/// Returns the status corresponding to ACK responses.
		/// </summary>
		/// <returns>Sent:  The message is an ACK_PROCESSED
		/// NackServer:  The message is an nb.
		/// NackMessage:  The message is an ne.
		/// Jammed:  The message is ACK_OK, ACK_PROCESSED or ACK_DEFERRED.
		/// Error:  The message is not an ACK.</returns>
		public MessageStatus AckResponse()
		{
			MessageStatus status = MessageStatus.Sent;
			string msgName = GetMessageName();
			if (msgName != null)
			{
				switch (msgName)
				{
					case Tags.AckProcessed:
						status = MessageStatus.Sent;
						break;
					case Tags.NackServer:
						status = MessageStatus.NackServer;
						break;
					case Tags.NackMsg:
						status = MessageStatus.NackMessage;
						break;
					case Tags.AckOK:
					case Tags.AckJammed:
					case Tags.AckDeferred:
						status = MessageStatus.Jammed;
						break;
					default:
						status = MessageStatus.Error;
						break;
				}
			}
			return status;
		}
		/// <summary>
		/// Updates the values of the common message header attributes
		/// </summary>
		/// <param name="id">The new message id</param>
		/// <param name="destination">The new destination id</param>
		/// <param name="priority">The new priority</param>
		public void UpdateMessageHeader(string id, string destination, string priority)
		{
			string msgName = GetMessageName();
			if (msgName.Length > 0)
			{
				ArrayList aList = new ArrayList(1);
				aList.Add(msgName);
				XmlNode msgNode = GetNode(aList, msgName, _xmlDoc.FirstChild);
				if (msgNode != null)
				{
					XmlAttribute attr = msgNode.Attributes[Tags.MsgIdAttr];
					if (attr != null)
						attr.Value = id;
					attr = msgNode.Attributes[Tags.DestinationAttr];
					if (attr != null)
						attr.Value = destination;
					attr = msgNode.Attributes[Tags.PriorityAttr];
					if (attr != null)
						attr.Value = priority;
				}
			}
		}
		/// <summary>
		/// Changes the value of the message retry count attribute
		/// </summary>
		/// <param name="retries">The new number of retries</param>
		public void UpdateRetries(string retries)
		{
			string msgName = GetMessageName();
			if (msgName.Length > 0)
			{
				ArrayList aList = new ArrayList(1);
				aList.Add(msgName);
				XmlNode msgNode = GetNode(aList, msgName, _xmlDoc.FirstChild);
				if (msgNode != null)
				{
					XmlAttribute attr = msgNode.Attributes[Tags.RetriesAttr];
					if (attr != null)
						attr.Value = retries;
				}
			}
		}
		public XmlNodeList GetNodeChildren(string path)
		{
			XmlNodeList children = null;
			if (_xmlDoc != null)
			{
				XmlNode root =  _xmlDoc.FirstChild;
				string[] lst = path.Split(new char[]{'/'});
				ArrayList aList = new ArrayList(lst);
				string lastPart = (string) aList[aList.Count-1];
				XmlNode node = GetNode(aList, lastPart, root);
				if (node != null)
				{
					children = node.ChildNodes;
				}
			}
			return children;
		}

		/// <summary>
		/// Returns a string containing the xml document
		/// </summary>
		/// <param name="xmlDoc"></param>
		/// <returns></returns>
		private string XmlDocAsString(XmlDocument xmlDoc)
		{
			StringBuilder strBld = new StringBuilder();
			StringWriter writer = new StringWriter(strBld);
			xmlDoc.WriteTo(new XmlTextWriter(writer));
			return strBld.ToString();
		}
		/// <summary>
		/// Returns the value of a node. The path to the desired node is contained
		/// in the supplied string list
		/// </summary>
		/// <param name="aList"></param>
		/// <param name="tagName"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		private string GetNodeValue(ArrayList aList, string tagName, XmlNode node)
		{
			string tagValue = null;
			if (aList.Count > 0 && node != null)
			{
				string elemName = (string) aList[0];
				if (elemName.Equals(node.Name))
				{
					if (aList.Count == 1)
					{
						tagValue = node.InnerText;
					}
					else
					{
						ArrayList newList = new ArrayList(aList);
						newList.RemoveAt(0);
						if (node.HasChildNodes)
						{
							XmlNodeList nodeList = ((XmlElement)node).GetElementsByTagName((string)newList[0]);
							if (nodeList != null && nodeList.Count > 0)
							{
								return GetNodeValue(newList, tagName, nodeList[0]);
							}
						}
					}
				}
			}
			return tagValue;
		}
		/// <summary>
		/// Returns a node. The path to the desired node is contained
		/// in the supplied string list
		/// </summary>
		/// <param name="aList"></param>
		/// <param name="tagName"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		private XmlNode GetNode(ArrayList aList, string tagName, XmlNode node)
		{
			XmlNode nodeRet = null;
			if (aList.Count > 0 && node != null)
			{
				string elemName = (string) aList[0];
				if (elemName.Equals(node.Name))
				{
					if (aList.Count == 1)
					{
						nodeRet = node;
					}
					else
					{
						ArrayList newList = new ArrayList(aList);
						newList.RemoveAt(0);
						if (node.HasChildNodes)
						{
							XmlNodeList nodeList = ((XmlElement)node).GetElementsByTagName((string)newList[0]);
							if (nodeList != null && nodeList.Count > 0)
							{
								return GetNode(newList, tagName, nodeList[0]);
							}
						}
					}
				}
			}
			return nodeRet;
		}

		private XmlDocument _xmlDoc;
	}
}
