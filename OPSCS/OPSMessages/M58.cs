using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Comm;
using OPS.Components.Data;
using OPS.Components;
using System.Data;
using System.Collections;
using CS_OPS_TesM1;
//using Oracle.DataAccess.Client;
using System.Globalization;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// M58 - Replications version query.
	/// </summary>
	internal sealed class Msg58 : MsgReceived, IRecvMessage
	{
		private string _fineNumber;
		private DateTime _date;
		private int _unitId;

		/// <summary>
		/// Constructs a new M58 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg58(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "f": _fineNumber = n.InnerText; break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(n.InnerText); break;
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;

				}
			}
		}

		#region DefinedRootTag(m58)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m58"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m58 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public StringCollection Process()
		{

			bool bExistUnit = ExistUnit(_unitId);


			if(bExistUnit)
			{						
				string response=ToStringM58();
				StringCollection sc = new StringCollection();								 
				sc.Add (response);
				return sc;
			}
			else
			{
				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}

		}


		private string ToStringM58()
		{

			System.Text.StringBuilder ret = new System.Text.StringBuilder();
			OracleCommand BDCommand=null;
			ILogger logger = null;

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();

				BDCommand = new OracleCommand("SELECT FIN_ID,FIN_DFIN_ID,NVL(FIN_FIN_ID,-1) FIN_FIN_ID,FIN_VEHICLEID,FIN_MODEL,FIN_MANUFACTURER,FIN_COLOUR, "+
											  "FIN_GRP_ID_ZONE,FIN_GRP_ID_ROUTE,FIN_STR_ID,NVL(FIN_STRNUMBER,-1) FIN_STRNUMBER,NVL(FIN_LETTER,'-1') FIN_LETTER,FIN_POSITION, "+
											  "TO_CHAR(FIN_DATE,'HH24MISSDDMMYY') FIN_DATE,nvl(FIN_COMMENTS,'-1') FIN_COMMENTS,FIN_USR_ID,FIN_UNI_ID,FIN_STATUS,nvl(FIN_POLICENUMBER,'-1') FIN_POLICENUMBER, "+
											  "DECODE(FIN_CONFIRM_DATE,NULL,'-1',TO_CHAR(FIN_CONFIRM_DATE,'HH24MISSDDMMYY')) FIN_CONFIRM_DATE, "+
											  "nvl(FIN_LATITUDE,-1) FIN_LATITUDE ,nvl(FIN_LONGITUD,-1) FIN_LONGITUD,nvl(FIN_NUM_PHOTOS,0) FIN_NUM_PHOTOS FROM FINES WHERE fin_status=30 and fin_statusadmon in (0,1) AND FIN_ID = " + _fineNumber, (OracleConnection)DBCon);
				OracleDataReader myReader = BDCommand.ExecuteReader();
				if (myReader.Read())
				{
					ret.Append("<f>"+myReader.GetInt32(myReader.GetOrdinal("FIN_ID")).ToString()+"</f>");
					ret.Append("<y>"+myReader.GetInt32(myReader.GetOrdinal("FIN_DFIN_ID")).ToString()+"</y>");
					ret.Append("<rf>"+myReader.GetInt32(myReader.GetOrdinal("FIN_FIN_ID")).ToString()+"</rf>");
					ret.Append("<m>"+myReader.GetString(myReader.GetOrdinal("FIN_VEHICLEID"))+"</m>");
					ret.Append("<j>"+myReader.GetString(myReader.GetOrdinal("FIN_MODEL"))+"</j>");
					ret.Append("<k>"+myReader.GetString(myReader.GetOrdinal("FIN_MANUFACTURER"))+"</k>");
					ret.Append("<l>"+myReader.GetString(myReader.GetOrdinal("FIN_COLOUR"))+"</l>");
					ret.Append("<g>"+myReader.GetInt32(myReader.GetOrdinal("FIN_GRP_ID_ZONE")).ToString()+"</g>");
					ret.Append("<r>"+myReader.GetInt32(myReader.GetOrdinal("FIN_GRP_ID_ROUTE")).ToString()+"</r>");
					ret.Append("<w>"+myReader.GetInt32(myReader.GetOrdinal("FIN_STR_ID")).ToString()+"</w>");
					ret.Append("<n>"+myReader.GetInt32(myReader.GetOrdinal("FIN_STRNUMBER")).ToString()+"</n>");
					ret.Append("<nl>"+myReader.GetString(myReader.GetOrdinal("FIN_LETTER"))+"</nl>");
					ret.Append("<p>"+myReader.GetInt32(myReader.GetOrdinal("FIN_POSITION")).ToString()+"</p>");
					ret.Append("<d>"+myReader.GetString(myReader.GetOrdinal("FIN_DATE"))+"</d>");
					ret.Append("<e>"+myReader.GetString(myReader.GetOrdinal("FIN_COMMENTS"))+"</e>");
					ret.Append("<z>"+myReader.GetInt32(myReader.GetOrdinal("FIN_USR_ID")).ToString()+"</z>");
					ret.Append("<u>"+myReader.GetInt32(myReader.GetOrdinal("FIN_UNI_ID")).ToString()+"</u>");
					ret.Append("<s>"+myReader.GetInt32(myReader.GetOrdinal("FIN_STATUS")).ToString()+"</s>");
					ret.Append("<ag>"+myReader.GetString(myReader.GetOrdinal("FIN_POLICENUMBER"))+"</ag>");
					ret.Append("<cd>"+myReader.GetString(myReader.GetOrdinal("FIN_CONFIRM_DATE"))+"</cd>");
					ret.Append("<lt>"+myReader.GetDouble(myReader.GetOrdinal("FIN_LATITUDE")).ToString()+"</lt>");
					ret.Append("<lg>"+myReader.GetDouble(myReader.GetOrdinal("FIN_LONGITUD")).ToString()+"</lg>");
					ret.Append("<np>"+myReader.GetInt32(myReader.GetOrdinal("FIN_NUM_PHOTOS")).ToString()+"</np>");
				}
				else
				{
					ret.Append("<f>-1</f>");		
				}

				logger.AddLog("[Msg58:ToStringM58]: Result: "+ret.ToString(),LoggerSeverities.Info);

				myReader.Close();
				BDCommand.Dispose();
				BDCommand=null;
				return (new AckMessage(_msgId, ret.ToString()).ToString());

			}
			catch(Exception e)
			{

				logger.AddLog("[Msg58:ToStringM58]: Error: "+e.Message,LoggerSeverities.Error);

				if (BDCommand!=null)
				{
					BDCommand.Dispose();
					BDCommand=null;
				}

				return ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS).ToString();

			}
			
		}


		private bool ExistUnit(int iUnit)
		{
			bool bRet=false;
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				DBCon.Open();
				try
				{
					String strSQL = String.Format("select count(*) "+
						"from units "+
						"where uni_id = {0} ", iUnit );
					OracleCommand cmd = new OracleCommand(strSQL, (OracleConnection)DBCon);
					if (Convert.ToInt32(cmd.ExecuteScalar())>0)
					{
						bRet=true;
					}
					cmd.Dispose();
				}
				catch
				{
				}
								
				DBCon.Close();


			}
			catch
			{
				
			}
				
			return bRet;
		}

		#endregion
	}	
}
