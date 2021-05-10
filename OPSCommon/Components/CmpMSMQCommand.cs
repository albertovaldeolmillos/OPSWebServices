using System;
using System.Collections;
using OPS.Comm; 

namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpMSMQCommand.
	/// </summary>
	public class CmpMSMQCommand
	{

		public CmpMSMQCommand()
		{
		}

		public void SendPDMCommandCcBecsOutputWrapper(string path, ArrayList arrayPDMId, int commandId, string message)
		{
			CcBecsOutputWrapper wrapper = new CcBecsOutputWrapper(path);

			ArrayList sendMessage = new ArrayList();
			int count = 0;
			foreach(string pdmId in arrayPDMId)
			{
				if (!sendMessage.Contains(pdmId))
				{
					sendMessage.Add(pdmId);
					wrapper.Send(new CcBecsHeader(0),CreateBodyPDMMessage(count++,Int32.Parse(pdmId), commandId, message));
				}
			}
		}

		private string CreateBodyPDMMessage(int id, int pdmId, int messageId, string message)
		{
			string sbody =	"<p id=\"" + id.ToString() + "\">" + "<mc" + messageId.ToString() + ">";
			sbody = sbody + "<u>" + pdmId + "</u>";
			if (messageId == 24) { sbody = sbody + "<e>" + message + "</e>";  }
			sbody = sbody + "</mc" + messageId.ToString()+ ">" +  "</p>";

			return sbody;
		}

		public void SendPDACommandCcBecsOutputWrapper(string path, ArrayList arrayPDMId, int commandId, string message)
		{
			CcBecsOutputWrapper wrapper = new CcBecsOutputWrapper(path);

			ArrayList sendMessage = new ArrayList();
			int count = 0;
			foreach(string pdmId in arrayPDMId)
			{
				if (!sendMessage.Contains(pdmId))
				{
					sendMessage.Add(pdmId);
					wrapper.Send(new CcBecsHeader(0),CreateBodyPDAMessage(count++,Int32.Parse(pdmId), commandId, message));
				}
			}
		}
		private string CreateBodyPDAMessage(int id, int pdmId, int messageId, string message)
		{
			string sbody =	"<p id=\"" + id.ToString() + "\">" + "<mc" + messageId.ToString() + ">";
			sbody = sbody + "<u>" + pdmId + "</u>";
			if (messageId == 31) { sbody = sbody + "<e>" + message + "</e>";  }
			sbody = sbody + "</mc" + messageId.ToString()+ ">" +  "</p>";

			return sbody;
		}
		
	}
}
