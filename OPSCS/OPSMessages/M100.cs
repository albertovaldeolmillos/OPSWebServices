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
	/// m100 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg100 : MsgReceived, IRecvMessage
	{

		private int _unitId;
		private string	_date="";
		private int _userId;

		public const int STATE_MSG_SENT_GENERATED=0;
		public const int STATE_MSG_SENT_SENT=1;
		public const int STATE_MSG_SENT_READ=2;

		public const int STATE_MSG_RECEIVED_RECEIVED=0;
		public const int STATE_MSG_RECEIVED_READ=1;
		public const int STATE_MSG_RECEIVED_NOTIFIED=2;


		private const int RESPONSE_TYPE_MSG_SENT_GENERATED=0;
		private const int RESPONSE_TYPE_MSG_RECEIVED_READ=1;


/*
 * 
 * <m100 id="12" ret="1"><mu>103</mu>
 *						<md>1201451100310</md>
 *						<mus>145</us>
 * </m100>
 */		



		/// <summary>
		/// Constructs a new m100 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg100(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "mu": _unitId = Convert.ToInt32(n.InnerText); break;
					case "md": _date = n.InnerText; break;
					case "mus": _userId = Convert.ToInt32(n.InnerText); break;
			
					

				}
			}
		}

		#region DefinedRootTag(m100)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m100"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m100 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleCommand oraCmd2=null;
			OracleDataReader dr= null;
			OracleTransaction transaction = null;
			ILogger logger = null;
			bool bRes=false;
			string strMsgId="-1";
			string strMsgText="";
			int iMsgType=-1;
			string response="";
			string responseLocal="";
			bool bEnd=false;


			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{
					transaction = oraDBConn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
					
					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
					oraCmd.Transaction = transaction;
				

					oraCmd.CommandText =	string.Format("select smsg_id,smsg_text "+ 
						"from msgs_sent t "+
						"where ((t.smsg_uni_id = {0} and smsg_usr_id is null) or (smsg_usr_id = {1})) "+
						"	and t.smsg_state = {2} "+
						"	order by smsg_date asc",
						_unitId,_userId,STATE_MSG_SENT_GENERATED);


					oraCmd2= new OracleCommand();
					oraCmd2.Connection=(OracleConnection)oraDBConn;
					oraCmd2.Transaction = transaction;

					dr=oraCmd.ExecuteReader();

					while ((dr.Read())&&(!bEnd))
					{
						strMsgId=dr["smsg_id"].ToString();
						strMsgText=dr["smsg_text"].ToString();
						iMsgType=RESPONSE_TYPE_MSG_SENT_GENERATED;

						// Build response
						responseLocal = "<r><mid>" + strMsgId+ "</mid>";
						responseLocal += "<mus>" + _userId.ToString() + "</mus>";
						responseLocal += "<mtp>" + iMsgType.ToString() + "</mtp>";						
						responseLocal += "<mt>" + strMsgText + "</mt></r>";
						bEnd=(response.Length+responseLocal.Length>Msg52.MAX_TEL_LEN);
						if (!bEnd)
						{
							response += responseLocal;
							oraCmd2.CommandText =	string.Format("update msgs_sent set smsg_state={0}, smsg_usr_id={1} where smsg_id={2}",STATE_MSG_SENT_SENT,_userId,strMsgId);
							oraCmd2.ExecuteNonQuery();
						}

					}


					if (!bEnd)
					{

						dr.Close();
						dr.Dispose();
						dr=null;


						oraCmd.CommandText =	string.Format("select vmsg_id "+
							"from msgs_received t "+
							"where vmsg_usr_id = {0} "+
							"and t.vmsg_state = {1} "+
							"order by vmsg_date asc",
							_userId,STATE_MSG_RECEIVED_READ);



						dr=oraCmd.ExecuteReader();


						while ((dr.Read())&&(!bEnd))
						{
							strMsgId=dr["vmsg_id"].ToString();
							strMsgText="";
							iMsgType=RESPONSE_TYPE_MSG_RECEIVED_READ;
						
							// Build response
							responseLocal = "<r><mid>" + strMsgId+ "</mid>";
							responseLocal += "<mus>" + _userId.ToString() + "</mus>";
							responseLocal += "<mtp>" + iMsgType.ToString() + "</mtp>";						
							responseLocal += "<mt>" + strMsgText + "</mt></r>";
							bEnd=(response.Length+responseLocal.Length>Msg52.MAX_TEL_LEN);
							if (!bEnd)
							{
								response += responseLocal;
								oraCmd2.CommandText =	string.Format("update msgs_received set vmsg_state={0} where vmsg_id='{1}'",STATE_MSG_RECEIVED_NOTIFIED,strMsgId);
								oraCmd2.ExecuteNonQuery();
							}

						}

					}


					bRes=true;


				}
				if (bRes)
				{
					if (response.Length>0)
					{
						
						transaction.Commit();
						transaction.Dispose();
						transaction=null;
						res = new StringCollection();
						res.Add(new AckMessage(_msgId, response).ToString());		

					}
					else
					{

						res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}
				}
				else
				{
					try
					{
						if (transaction!=null)
						{
							transaction.Rollback();
							transaction.Dispose();
							transaction=null;
						}
					}
					catch
					{
					}


					if(logger != null)
						logger.AddLog("[Msg100:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				try
				{
					if (transaction!=null)
					{
						transaction.Rollback();
						transaction.Dispose();
						transaction=null;
					}
				}
				catch
				{
				}

				if(logger != null)
					logger.AddLog("[Msg100:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{

				if (transaction!=null)
				{
					transaction.Rollback();
					transaction.Dispose();
					transaction=null;

				}

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

				if (oraCmd2!=null)
				{
					oraCmd2.Dispose();
					oraCmd2 = null;
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
