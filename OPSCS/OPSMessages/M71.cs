using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// m71 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg71 : MsgReceived, IRecvMessage
	{

		private int _unitId;
		private int _platformId;
		private int _typeId;
		private string	_date="";
		private int _userId;
		private string _bikeTag;

/*
 * 
 * <m71 id="12" ret="1"><u>103</u>
 *						<d>120145171310</d>
 *						<pi>15</pi>
 *						<t>4</t>
 *						<bi>XXXXXXXXX</bi>
 *						<us>152021090310</us>
 * </m71>
 */		



		/// <summary>
		/// Constructs a new m71 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg71(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "pi": _platformId = Convert.ToInt32(n.InnerText); break;
					case "t": _typeId = Convert.ToInt32(n.InnerText); break;
					case "d": _date = n.InnerText; break;
					case "us": _userId = Convert.ToInt32(n.InnerText); break;
					case "bi": _bikeTag = n.InnerText; break;
			
					

				}
			}
		}

		#region DefinedRootTag(m71)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m71"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m71 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr= null;
			ILogger logger = null;
			bool bRes=false;


			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
				

					oraCmd.CommandText =	string.Format("insert into INCIDENTS  (INCID_TYPE_ID,"+
																				  "INCID_UNI_ID,"+ 
																				  "INCID_PLAT_ID,"+
																				  "INCID_BYC_ID,"+
																				  "INCID_BUSER_ID,"+ 
																				  "INCID_DATE,INCID_EX_TRIGGER) "+
														  "values ({0}, {1}, {2}, null, {3}, to_date('{4}', 'HH24MISSDDMMYY'),1)",
														 _typeId,_unitId,_platformId,_userId,_date);



					if (oraCmd.ExecuteNonQuery()==1)
					{
						bRes=true;
					}
				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg71:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg71:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{

				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

				if (oraCmd!=null)
				{
					oraCmd.Dispose();
					oraCmd = null;
				}


				if (oraDBConn!=null)
				{
					oraDBConn.Close();
					oraDBConn.Dispose();
					oraDBConn = null;
				}

			}

			return res;
		}



		#endregion
	
	}	
}
