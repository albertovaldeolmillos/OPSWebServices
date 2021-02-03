using System;
using System.Xml;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Summary description for OPSPropertyManager.
	/// </summary>
	public class OPSPropertyManager : IPropertyManager
	{
		protected string _msg;
		protected TimeSpan _caducity;
		protected System.Messaging.MessagePriority _priority;
		public OPSPropertyManager(string sMsg)
		{
			_msg = sMsg;
			parseString();
		}
		#region IPropertyManager Members

		public string Message
		{
			set
			{
				_msg = value;
				parseString();
			}
		}

		public TimeSpan Caducity
		{
			get
			{
				return _caducity;	
			}
		}

		public System.Messaging.MessagePriority Priority
		{
			get
			{
				return _priority;
			}
		}

		#endregion

		/// <summary>
		/// Parses the string of the message and obtains caducity and priority
		/// </summary>
		protected virtual void parseString ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (_msg);
			_priority = System.Messaging.MessagePriority.Normal;
			_caducity = System.Messaging.Message.InfiniteTimeout;
			// Get atributes of root (message) node
			XmlAttributeCollection attrs =  doc.Attributes;
			if (attrs!=null) 
			{
				XmlAttribute attr = attrs["p"];
				if (attr!=null) 
				{
					int ipriority = Convert.ToInt32(attr.Value);
					switch (ipriority)
					{
						case 0:
							_priority = System.Messaging.MessagePriority.Lowest;
							break;
						case 1:
							_priority = System.Messaging.MessagePriority.VeryLow;
							break;
						case 2:
							_priority = System.Messaging.MessagePriority.Low;
							break;
						case 3:
							_priority = System.Messaging.MessagePriority.Normal;
							break;
						case 4:
							_priority = System.Messaging.MessagePriority.AboveNormal;
							break;
						case 5:
							_priority = System.Messaging.MessagePriority.High;
							break;
						case 6:
							_priority = System.Messaging.MessagePriority.VeryHigh;
							break;
						case 7:
							_priority = System.Messaging.MessagePriority.Highest;
							break;
					}
				}
				attr = attrs["c"];
				if (attr!=null) 
				{
					int seconds = Convert.ToInt32(attr.Value);
					_caducity = new TimeSpan (0,0,seconds);
				}
			}		// end if(attrs!=null)
		}
	}
}
