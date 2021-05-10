using System;
using System.Xml;
using OPS.Comm.Messaging;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// Summary description for SystemMessages.
	/// </summary>
	public class SystemMessages
	{
		public void Process(string body)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(body);
			switch (doc.DocumentElement.Name)
			{
				case "s1":
					string uri = doc.DocumentElement.ChildNodes[0].InnerXml;
					MessageProcessorManager.CloseProcessor(uri);
					BecsEngine.MainEngine.CommObjects.ChannelManager.CloseChannel(uri);
					break;
				default:
					break;
			}
		}
	}
}
