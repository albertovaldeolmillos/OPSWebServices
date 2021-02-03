using System;
using System.Reflection;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Summary description for CommMain.
	/// </summary>
	public class CommMain
	{
		protected static ILogger _logger;

		static CommMain()
		{
			_logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		public static ILogger Logger
		{
			get { return _logger; }
		}
	}
}
