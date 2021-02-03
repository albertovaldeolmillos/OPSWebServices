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
	/// to notify message statistics
	/// </summary>
	internal sealed class Msg14 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m14)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m14"; } }
		private static int LongStatisticsNum = 11;
		private static int FloatStatisticsNum = 2;
		#endregion

		#region Variables, creation and parsing

		/*
			<p src="102">
				<m14 id="1" dst="4">
					<u>102</u>
					<d>120230270904</d>
					<st>
						<n>TOTAL</n>
						<o>6</o>
						<c>0</c>
						<t>0</t>
						<l>0</l>
						<e>0</e>
						<b>0</b>
						<s>0</s>
						<r>0</r>
						<un>0</un>
						<crc>0</crc>
						<k>0</k>
					</st>
					<st>
						<n>m59</n>
						<o>6</o>
						<c>0</c>
						<t>0</t>
						<l>0</l>
						<e>0</e>
						<b>0</b>
						<s>0</s>
						<r>0</r>
						<un>0</un>
						<crc>0</crc>
						<k>0</k>
					</st>
				</m14>
			</p>
		*/

		private int			_unit;
		private DateTime	_date;
		private string[]	_statisticsNames;
		private long[][]	_statisticslong;
		private float[][]	_statisticsfloat;
	
		/// <summary>
		/// Constructs a new msg14 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg14(XmlDocument msgXml) : base(msgXml) {}
		
		/// <summary>
		/// Overriding of the DoParseMessage to get the unit, 
		/// all the alarms and the status and to store them in private vars
		/// </summary>
		protected override void DoParseMessage()
		{	
			ILogger logger =null;

			logger = DatabaseFactory.Logger;
			if(logger!=null)
				logger.AddLog("Msg14:DoParseMessage",LoggerSeverities.Debug);
			XmlDocument	xmlDoc = _root.OwnerDocument;
			XmlNodeList	StatisticsList = xmlDoc.GetElementsByTagName( "st" );

			logger.AddLog( "Msg14:DoParseMessage: Statistics=" + StatisticsList.Count.ToString(), 
							LoggerSeverities.Info ); 

			int iNames = 0;

			if( StatisticsList.Count > 0 )
			{
				_statisticsNames = new string[ StatisticsList.Count ];
				_statisticslong = new long[ StatisticsList.Count ][];
				_statisticsfloat = new float[ StatisticsList.Count ][];

				for( int i = 0; i < StatisticsList.Count; i++ ) 
				{
					_statisticslong[ i ] = new long[ LongStatisticsNum ];
					_statisticsfloat[ i ] = new float[ FloatStatisticsNum ];
				}

				foreach( XmlNode node in StatisticsList )
				{
					XmlNodeList childs = node.ChildNodes;

					// Es poco elegante, pero, de momento...
					foreach( XmlNode nodeChild in childs )
					{
						switch (nodeChild.Name)
						{
							case "n": 
								logger.AddLog( "Found n tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticsNames[ iNames ] = nodeChild.InnerText;
								break;
							case "o":
								_statisticslong[ iNames ][ 0 ] = Convert.ToInt32( nodeChild.InnerText );
								logger.AddLog( "Found o tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								break;
							case "c":
								_statisticslong[ iNames ][ 1 ] = Convert.ToInt32( nodeChild.InnerText );
								logger.AddLog( "Found c tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								break;
							case "t": 
								_statisticslong[ iNames ][ 2 ] = Convert.ToInt32( nodeChild.InnerText );
								logger.AddLog( "Found t tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 

								break;
							case "l":
								logger.AddLog( "Found l tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 3 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "b":
								logger.AddLog( "Found b tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 4 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "e": 
								logger.AddLog( "Found e tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 5 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "s":
								logger.AddLog( "Found s tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 6 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "r":
								logger.AddLog( "Found r tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 7 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "un":
								logger.AddLog( "Found un tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 8 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "crc":
								logger.AddLog( "Found crc tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 9 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "k":
								logger.AddLog( "Found k tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticslong[ iNames ][ 10 ] = Convert.ToInt32( nodeChild.InnerText );
								break;
							case "mst":
								logger.AddLog( "Found mst tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticsfloat[ iNames ][ 0 ] = ( float )Convert.ToDouble( nodeChild.InnerText );
								break;
							case "mrt":
								logger.AddLog( "Found mrt tag: Value=" + nodeChild.InnerText, 
									LoggerSeverities.Info ); 
								_statisticsfloat[ iNames ][ 1 ] = ( float )Convert.ToDouble( nodeChild.InnerText );
								break;
						}
					}

					iNames++;
				}
			}
			foreach (XmlNode node in _root.ChildNodes)
			{
				switch (node.Name)
				{
						case "u": 
								logger.AddLog( "Found u tag: Value=" + node.InnerText, 
									LoggerSeverities.Info ); 
					_unit = Convert.ToInt32( node.InnerText ); break;
					case "d": 
						logger.AddLog( "Found d tag: Value=" + node.InnerText, 
							LoggerSeverities.Info ); 
						_date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
				}
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Process the Message. The process of this message is specified in OPS_D_M14.doc
		/// </summary>
		/// <returns>Response for the message (1 string)</returns>
		public StringCollection Process()
		{
			ILogger logger =null;

			logger = DatabaseFactory.Logger;
			if(logger!=null)
				logger.AddLog("Msg14:Process",LoggerSeverities.Debug);

			CmpPDMMessagesStatDB pmsDB = new CmpPDMMessagesStatDB();

			int j=0;
			for( int i = 0; i < _statisticsNames.Length; i++ )
			{
				logger.AddLog( "Inserting: "+_unit.ToString()+" "+_date.ToString( "F" )+_statisticsNames[i],
								LoggerSeverities.Info );
				for( j = 0; j < _statisticslong[i].Length; j++ )
					logger.AddLog( "Long Statistic["+j.ToString()+"]="+_statisticslong[i][j].ToString(),
									LoggerSeverities.Info );
				for( j = 0; j < _statisticsfloat[i].Length; j++ )
					logger.AddLog( "Float Statistic["+j.ToString()+"]="+_statisticsfloat[i][j].ToString(),
						LoggerSeverities.Info );
				pmsDB.Insert(	_unit,	_date, _statisticsNames[ i ],
								_statisticslong[ i ], _statisticsfloat[ i ]  );
			}

			return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
		}

		#endregion
	}
}



