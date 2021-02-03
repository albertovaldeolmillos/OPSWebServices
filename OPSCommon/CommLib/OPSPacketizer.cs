using System;
using System.Collections;

namespace OPS.Comm.Common
{
	/// <summary>
	/// Implementation of IPacketizer for messa
	/// </summary>
	public class OPSPacketizer : IPacketizer
	{
		protected string _packetTag;
		protected ArrayList _data;
		protected string _xml;
		protected PacketData _packetInfo;
		public OPSPacketizer(string xmlData)
		{
			_packetTag = "p";					// <p> is used for tag marking packets
			_xml = xmlData;
		}

		protected OPSPacketizer(string xmlData, string packetTag)
		{
			_packetTag = packetTag;
			_xml = xmlData;
		}

		/// <summary>
		/// Process the data and generates ONE string for each message enclosed in <_packetTag></_packetTag>
		/// </summary>
		/// <param name="xmlData">XML String to process</param>
		protected virtual ArrayList ProcessData (string xmlData)
		{
			ArrayList col = new ArrayList();
			// Do a search of nodes <_packetTag>
			// Format of a document is one root node of and 
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			doc.LoadXml(xmlData);
			_packetInfo = null;
			//System.Xml.XmlNode packetNode = doc.SelectSingleNode("//" + _packetTag);
			System.Xml.XmlNode packetNode = doc.FirstChild;
			//if (packetNode != null) 
			if (packetNode.LocalName.Equals(_packetTag))
			{
				// Gets the childs of the node
				foreach (System.Xml.XmlNode node in packetNode.ChildNodes)
				{
					col.Add(node.OuterXml);
				}
				// Gets the atributes of the packetNode
				System.Xml.XmlAttributeCollection attrs = packetNode.Attributes;
				if (attrs!=null) 
				{
					string dtx = null;
					string src = null;
					foreach (System.Xml.XmlAttribute attr in attrs)
					{
						if (attr.Name.Equals("dtx")) 
						{
							dtx = attr.Value;
						}
						else if (attr.Name.Equals("src"))
						{
							src = attr.Value;
						}
					}
					_packetInfo = new PacketData(dtx, src);
				}
			}
			return col;
		}

		//***************** Implementation of IPacketizer **************************
		public int PacketsCount
		{
			get 
			{
				if (_data == null) 
				{
					_data = ProcessData (_xml);
				}
				return _data.Count;
			}
		}
		public string this [int idx]
		{
			get 
			{
				if (_data == null)
				{
					_data = ProcessData (_xml);
				}
				return (String) _data[idx];
			}
		}
		public PacketData PacketInfo 
		{
			get 
			{
				if (_data == null)
				{
					_data = ProcessData(_xml);
				}
				return _packetInfo;
			}
		}
	}
}
