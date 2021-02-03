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
	/// m102 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg102 : MsgReceived, IRecvMessage
	{

		
		private string	 _id;
		private int _unitId;
		private string	_date="";
		private int _userId;
		private string _Msg="";

		/*
		 * 
		 * <m102 id="12" ret="1"><mid>16251009011200000101</mid>
		 *						<mus>1</mus>
		 *						<md>151045090112</md> 
		 *						<mt>This is an example for Kike</mt>
		 *						<mu>101</mu>
		 * </m102>
		 */		



		/// <summary>
		/// Constructs a new m102 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg102(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "mid": _id = n.InnerText; break;
					case "mu": _unitId = Convert.ToInt32(n.InnerText); break;
					case "md": _date = n.InnerText; break;
					case "mus": _userId = Convert.ToInt32(n.InnerText); break;
					case "mt": _Msg = n.InnerText; break;
			
					

				}
			}
		}

		#region DefinedRootTag(m102)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m102"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m102 message.
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
				
					oraCmd.CommandText =	string.Format("insert into msgs_received(VMSG_ID, VMSG_USR_ID, VMSG_TEXT, VMSG_DATE, VMSG_UNI_ID, VMSG_STATE) "+
														  "values ('{0}',{1},'{2}',to_date('{3}','HH24MISSDDMMYY'),{4},{5})",_id,_userId,_Msg,_date,_unitId,Msg100.STATE_MSG_RECEIVED_RECEIVED);

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
						logger.AddLog("[Msg102:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg102:Process]: Error: "+e.Message,LoggerSeverities.Error);
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
