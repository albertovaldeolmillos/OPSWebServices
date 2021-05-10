using System;
using System.Text;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Utility class for handling IP addresses and ports
	/// </summary>
	public class TcpIpUri
	{
		public static char Separator
		{
			get { return _separator; }
			set { _separator = value; }
		}

		public static String AddressPortAsUri(String ipAddress, int port)
		{
			StringBuilder bld = new StringBuilder();
			bld.AppendFormat(null, "{0}{1}{2}", new object[] {ipAddress, Separator, port.ToString()});
			return bld.ToString();
		}
		public static void UriAsAddressPort(String uri, out String ipAddress, out int port)
		{
			ipAddress = null;
			port = 0;
			if (uri != null)
			{
				char[] separator = {Separator};
				string[] parts = uri.Split(separator);
				ipAddress = parts[0];
				port = Int32.Parse(parts[1]);
			}
        }

		private static char _separator = ':';
	}
}
