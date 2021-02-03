using System;
using System.Collections.Specialized;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using OPS.Comm;

namespace OPS.Comm.Becs.Messages
{	
	/// <summary>
	/// This is the message that a PDM sends to CC
	/// to notify about its status and alarms
	/// </summary>
	internal sealed class Msg12 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m12)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m12"; } }
		private static int MeasuresNum = 5;
		#endregion

		#region Variables, creation and parsing

		/*
			<p src="102">
				<m12 id="1" dst="4">
					<u>102</u>
					<d>120230270904</d>
					<ms1>0.00</ms1>
					<ms2>0.00</ms2>
					<ms3>0.00</ms3>
					<ms4>0.00</ms4>
					<ms5>0.00</ms5>
				</m12>
			</p>
		*/

		private int			_unit;
		private DateTime	_date;
		private float[]		_measures = new float[MeasuresNum];
	
		/// <summary>
		/// Constructs a new msg12 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg12(XmlDocument msgXml) : base(msgXml) {}
		
		/// <summary>
		/// Overriding of the DoParseMessage to get the unit, 
		/// all the alarms and the status and to store them in private vars
		/// </summary>
		protected override void DoParseMessage()
		{	
			ILogger logger =null;
			logger = DatabaseFactory.Logger;

			for(int i= 0; i < MeasuresNum; i++)
				_measures[i] = 0;

			foreach (XmlNode node in _root.ChildNodes)
			{
				try
				{
					switch (node.Name)
					{
						case "u": _unit = Convert.ToInt32(node.InnerText); break;
						case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
							// CFE - Quick but not elegant
						case "ms1":	_measures[0] = Convert.ToSingle(node.InnerText); break;
						case "ms2": _measures[1] = Convert.ToSingle(node.InnerText); break;
						case "ms3": _measures[2] = Convert.ToSingle(node.InnerText); break;
						case "ms4": _measures[3] = Convert.ToSingle(node.InnerText); break;
						case "ms5": _measures[4] = Convert.ToSingle(node.InnerText); break;
					}
				}
				catch (Exception ex)
				{
					if(logger!=null)
						logger.AddLog("[Msg03:DoParseMessage] ERRROR: " + ex.ToString(),LoggerSeverities.Error);
					throw ex;
				}				
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Process the Message. The process of this message is specified in OPS_D_M12.doc
		/// </summary>
		/// <returns>Response for the message (1 string)</returns>
		public StringCollection Process()
		{
			ILogger logger =null;

			try
			{
				logger = DatabaseFactory.Logger;
				if(logger!=null)
					logger.AddLog("Msg12:Process",LoggerSeverities.Debug);

				
				CmpUnitsMeasuresDB unitmesdb = new CmpUnitsMeasuresDB();

				unitmesdb.InsertMeasures(	_unit,_date,
					_measures[0],_measures[1],
					_measures[2],_measures[3],
					_measures[4]);

				return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
			}
			catch (Exception ex)
			{
				if(logger!=null)
					logger.AddLog("[Msg12:Process] ERRROR: " + ex.ToString(),LoggerSeverities.Error);
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
		}

		#endregion
	}
}



/*
CREATE TABLE UNITS_MEASURES
(
  MUNI_ID       NUMBER                          NOT NULL,
  MUNI_DATE     DATE                            NOT NULL,
  MUNI_UNI_ID   NUMBER                          NOT NULL,
  MUNI_VALUE1   NUMBER,
  MUNI_VALUE2   NUMBER,
  MUNI_VALUE3   NUMBER,
  MUNI_SENT     NUMBER                          DEFAULT 0,
  MUNI_VALUE4   NUMBER,
  MUNI_VALUE5   NUMBER,
  MUNI_VALUE6   NUMBER,
  MUNI_VALUE7   NUMBER,
  MUNI_VALUE8   NUMBER,
  MUNI_VALUE9   NUMBER,
  MUNI_VALUE10  NUMBER
)

<p src="102">
	<m12 id="1" dst="4">
		<u>102</u>
		<d>120230270904</d>
		<ms1>0.00</ms1>
		<ms2>0.00</ms2>
		<ms3>0.00</ms3>
		<ms4>0.00</ms4>
		<ms5>0.00</ms5>
	</m12>
</p>

*/