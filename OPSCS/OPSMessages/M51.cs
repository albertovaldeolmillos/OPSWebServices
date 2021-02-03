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
	/// M51 - Request of Fine proposals created automatically from M50 messages.
	/// </summary>
	internal sealed class Msg51 : MsgReceived, IRecvMessage
	{
		private int _unitId;
		private DateTime _date;
		private int	_streetStretch;
		public const long MAX_TEL_LEN=10000000000000; 
		private const int PMSG_DPUNI_ID_PDA=4;


		/// <summary>
		/// Constructs a new M51 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg51(XmlDocument msgXml) : base(msgXml) {}

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
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "ss": _streetStretch = Convert.ToInt32(n.InnerText); break;
				}
			}
		}

		#region DefinedRootTag (m51)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m51"; } }
		#endregion
				

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m51 message.
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
			string dataXML="";


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

					bRes=GetProposals(ref dataXML,ref oraDBConn,ref oraCmd,ref dr, ref logger);
				}

				if (bRes)
				{
					res = new StringCollection();
					res.Add ((new AckMessage(_msgId, dataXML)).ToString());
				}
				else
				{
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg51:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool GetProposals(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr, ref ILogger logger)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format("select PVC_ID, "+    
													  "PVC_VEHICLEID, "+
													  "TO_CHAR(PVC_INSDATE,'hh24missddmmyy') AS PVC_INSDATE, "+         
													  "PVC_SS_ID, "+          
													  "REPLACE(TO_CHAR(PVC_LATITUDE),',','.') AS PVC_LATITUDE, "+
													  "REPLACE(TO_CHAR(PVC_LONGITUDE),',','.') AS PVC_LONGITUDE, "+
													  "PVC_FIN_ID, "+
													  "PVC_DFIN_ID, "+
													  "TO_CHAR(PVC_FINDATE,'hh24missddmmyy') AS PVC_FINDATE, "+
													  "PVC_STR_ID, "+
													  "PVC_STR_NUM "+
													  "from VC_PROPOSALS "+
													  "where PVC_DELETED=0 " +
													  "and PVC_SS_ID={0} " +
													  "and NVL(PVC_IGNORE,PVC_INSDATE)<TO_DATE({1},'hh24missddmmyy') " +
													  "order by PVC_ID asc",
													  _streetStretch,
													  OPS.Comm.Dtx.DtxToString(_date));
													  
				// "		  and PVC_UPDATE >= to_date('{0}','hh24missddmmyy') "+
				//TO_char(FIN_DATE,'DDMMYY')='{0:ddMMyy}
				// OPS.Comm.Dtx.DtxToString(_date)
				// to_date('{1}','hh24missddmmyy')
				
				//if(logger != null)
				//	logger.AddLog("[Msg51:Process] Query to VC_PROPOSALS " + oraCmd.CommandText.ToString() ,LoggerSeverities.Debug);

				dr = oraCmd.ExecuteReader();	
				
				int ord_PVC_ID			= dr.GetOrdinal("PVC_ID");
				int ord_PVC_VEHICLEID	= dr.GetOrdinal("PVC_VEHICLEID");
				int ord_PVC_SS_ID		= dr.GetOrdinal("PVC_SS_ID");
				int ord_PVC_LATITUDE	= dr.GetOrdinal("PVC_LATITUDE");
				int ord_PVC_LONGITUDE	= dr.GetOrdinal("PVC_LONGITUDE");
				int ord_PVC_INSDATE		= dr.GetOrdinal("PVC_INSDATE");
				int ord_PVC_FIN_ID		= dr.GetOrdinal("PVC_FIN_ID");
				int ord_PVC_DFIN_ID		= dr.GetOrdinal("PVC_DFIN_ID");
				int ord_PVC_FINDATE		= dr.GetOrdinal("PVC_FINDATE");
				int ord_PVC_STR_ID		= dr.GetOrdinal("PVC_STR_ID");
				int ord_PVC_STR_NUM		= dr.GetOrdinal("PVC_STR_NUM");

				while ((dr.Read())&&(!bEnd))
				{
					
					strXMLReg = "<r>";
					strXMLReg += "<p>"+dr.GetInt32(ord_PVC_ID).ToString()+"</p>";
					strXMLReg += "<m>"+dr.GetString(ord_PVC_VEHICLEID)+"</m>";
					strXMLReg += "<ss>"+dr.GetInt32(ord_PVC_SS_ID)+"</ss>";
					strXMLReg += "<lt>"+dr.GetString(ord_PVC_LATITUDE)+"</lt>";
					strXMLReg += "<ln>"+dr.GetString(ord_PVC_LONGITUDE)+"</ln>";
					strXMLReg += "<d>"+dr.GetString(ord_PVC_INSDATE)+"</d>";


					if (!dr.IsDBNull(ord_PVC_FIN_ID))
					{
						strXMLReg += "<fn>"+dr.GetInt32(ord_PVC_FIN_ID).ToString()+"</fn>";
					}

					if (!dr.IsDBNull(ord_PVC_DFIN_ID))
					{
						strXMLReg += "<ft>"+dr.GetInt32(ord_PVC_DFIN_ID).ToString()+"</ft>";
					}
					
					if (!dr.IsDBNull(ord_PVC_FINDATE))
					{
						strXMLReg += "<fd>"+dr.GetString(ord_PVC_FINDATE)+"</fd>";
					}

					if (!dr.IsDBNull(ord_PVC_STR_ID))
					{
						strXMLReg += "<d>"+dr.GetInt32(ord_PVC_STR_ID).ToString()+"</d>";
					}

					if (!dr.IsDBNull(ord_PVC_STR_NUM))
					{
						strXMLReg += "<d>"+dr.GetInt32(ord_PVC_STR_NUM).ToString()+"</d>";
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);

					if (!bEnd)
					{
						strXML += strXMLReg;
					}
				}
			}
			catch (Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg51:Process] Error Querying VC_PROPOSALS " + e.ToString()  ,LoggerSeverities.Error);
				bRes=false;
			}

			return bRes;
			
		}

		#endregion
	
	}	
}
