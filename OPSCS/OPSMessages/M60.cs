using System;
using System.Data;
using System.Xml;
using System.Collections.Specialized;
using OPS.Components;
using OPS.Components.Data;
using OPS.Comm;
using System.Globalization;
using OPS.FineLib;
//using Oracle.DataAccess.Client;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Handles the M060 message: Delete proposal as rejected.
	/// </summary>
	internal sealed class Msg60 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m60)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m60"; } }
		#endregion

		#region Static stuff



		/// <summary>
		/// Static constructor. Initializes values global to all Msg60.
		/// </summary>
		static Msg60()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
		}

		#endregion

		#region Variables, creation and parsing

		private int _proposalId;
		private DateTime _date;
		private int _ignoreTime;
		private int _unitId;
		private int _userId;
		private string _comment;


		/// <summary>
		/// Constructs a new Msg60 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg60(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode node in _root.ChildNodes)
			{
				switch (node.Name)
				{
					case "p": _proposalId = Convert.ToInt32(node.InnerText); break;
					case "d": _date = OPS.Comm.Dtx.StringToDtx(node.InnerText); break;
					case "t": _ignoreTime= Convert.ToInt32(node.InnerText); break;
					case "u": _unitId = Convert.ToInt32(node.InnerText); break;
					case "z": _userId = Convert.ToInt32(node.InnerText); break;
					case "c": _comment = node.InnerText; break;
				}
			}
		}

		#endregion

		#region IRecvMessage Members

		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;

			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();
				oraCmd= new OracleCommand();
				oraCmd.Connection=(OracleConnection)oraDBConn;


				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					if(logger != null)
					{
						/**
						string procCall = string.Format("CALL VC_MANAGEMENT.VCM_PROPOSAL_REJECT(VCP_ID => {0}, REJDATE => to_date('{1}','hh24missddmmyy'), UNI_ID => {2});",
														_proposalId, OPS.Comm.Dtx.DtxToString(_date), _unitId);
 
						logger.AddLog("[Msg60:Process]: SQL COMMAND > " + procCall,LoggerSeverities.Debug);
						oraCmd.CommandText = procCall;
						oraCmd.ExecuteNonQuery;
						
						**/
						oraCmd.CommandText = "VC_MANAGEMENT.VCM_PROPOSAL_REJECT_BY_TIME";
						oraCmd.CommandType = CommandType.StoredProcedure;

						OracleParameter toDateParameter = new OracleParameter();
						toDateParameter.OracleDbType= OracleDbType.Date;
						toDateParameter.Value = _date;
						toDateParameter.ParameterName="REJDATE";

						oraCmd.Parameters.Add("VCP_ID", OracleDbType.Decimal).Value = _proposalId;
						logger.AddLog("[Msg60:Process]: PROPOSAL ID: " + _proposalId.ToString(),LoggerSeverities.Debug);
						oraCmd.Parameters.Add(toDateParameter);
						oraCmd.Parameters.Add("IGNOREMINS", OracleDbType.Decimal).Value = _ignoreTime;
						oraCmd.Parameters.Add("UNI_ID", OracleDbType.Decimal).Value = _unitId;

						oraCmd.ExecuteNonQuery();
						if(logger != null)
							logger.AddLog("[Msg60:Process]: RESULT OK",LoggerSeverities.Debug);
						return ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
					}
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg60:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{		
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
