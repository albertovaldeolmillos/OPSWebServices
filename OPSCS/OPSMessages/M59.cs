using System;
using System.Collections.Specialized;
using System.Xml;
using System.Configuration;
using OPS.Components.Data;


namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Summary description for M59.
	/// </summary>
	internal sealed class Msg59  : MsgReceived, IRecvMessage
	{		
		#region DefinedRootTag (m59)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m59"; } }
		#endregion

		public Msg59(XmlDocument msgXml) : base(msgXml) {}
		
		// Nothing to parse
		protected override void DoParseMessage() {}

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m59 message. If all goes OK, returns an ACK_PROCESSED with the current time
		/// </summary>
		/// <returns></returns>
		public StringCollection Process()
		{
			AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			double nDifHour=0;
			try
			{
				nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
			}
			catch
			{
				nDifHour=0;
			}
			
			string sResponse = "<t>" + OPS.Comm.Dtx.DtxToString(DateTime.Now.AddHours(nDifHour)) + "</t>";
			CmpParametersDB cmpParam = new CmpParametersDB();
			string strHourDifference = cmpParam.GetParameter("P_HOUR_DIFF");					
			if (strHourDifference!="")
			{
				sResponse= sResponse+"<hd>" + strHourDifference + "</hd>";

			}

			AckMessage ret = new AckMessage (_msgId, sResponse);
			StringCollection sc = new StringCollection();
			sc.Add (ret.ToString());
			return sc;
		}

		#endregion
	}
}
