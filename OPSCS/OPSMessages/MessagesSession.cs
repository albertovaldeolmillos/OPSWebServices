using System;
using OPS.Comm;

namespace OPS.Comm.Becs.Messages
{
	public class MessagesSession
	{
		// In-memory data structures
		protected Components.CmpAlarmsDevices cmpAlarmsDevices;

		public MessagesSession() 
		{
			cmpAlarmsDevices = new Components.CmpAlarmsDevices();
		}

		/// <summary>
		/// The alarms of the devices
		/// </summary>
		public Components.CmpAlarmsDevices AlarmsDevices
		{
			get { return cmpAlarmsDevices; }
		}
	}
}
