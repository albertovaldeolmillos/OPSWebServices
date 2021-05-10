using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
////using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// m54 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg56 : MsgReceived, IRecvMessage
	{
		private string _PDAID;
		private int	_streetStretch;
		private string	_StartDate="";
		private string	_EndDate="";
		private int _unitId;
		private int _userId;
		private string _Remarks="";
		private int _NumPlaces=0;
		private string _LicenseNumber="";
		private string _LicenseCompany="";


		/// <summary>
		/// Constructs a new m54 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg56(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{

					case "id": _PDAID = n.InnerText; break;
					case "ss": _streetStretch = Convert.ToInt32(n.InnerText); break;
					case "d1": _StartDate = n.InnerText; break;
					case "d2": _EndDate = n.InnerText; break;
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "z": _userId = Convert.ToInt32(n.InnerText); break;
					case "r": _Remarks = n.InnerText; break;
					case "p": _NumPlaces = Convert.ToInt32(n.InnerText); break;
					case "ln": _LicenseNumber = n.InnerText; break;
					case "le": _LicenseCompany = n.InnerText; break;
				}
			}
		}

		#region DefinedRootTag(m56)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m56"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m52 message.
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
			bool bExist=false;


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
				
					if (!ExistWork(ref bExist,ref oraDBConn,ref oraCmd))
					{
						bRes=false;
					}
					else
					{
						bRes=true;
					}

					string strEndDate;

					if (_EndDate=="")
						strEndDate="NULL";
					else
						strEndDate=string.Format("to_date('{0}', 'HH24MISSDDMMYY')",_EndDate);

					if ((bRes)&&(!bExist))
					{
						bRes=false;
						oraCmd.CommandText =	string.Format("insert into WORKS (WORK_SS_ID, WORK_PDA_ID, WORK_USR_ID, WORK_UNI_ID, WORK_NUM_PARK_SPACES, WORK_REMARKS, WORK_INI_DATE, WORK_END_DATE, WORK_LICEN_NUMBER, WORK_LICEN_CORP) "+
															"values ({0}, '{1}', {2} , {3}, {4}, '{5}', to_date('{6}', 'HH24MISSDDMMYY'), {7}, '{8}', '{9}')",															   							
															_streetStretch,
															_PDAID,
															_userId,
															_unitId,
															_NumPlaces,
															_Remarks,
															_StartDate,
															strEndDate,
															_LicenseNumber,
															_LicenseCompany);

							
						oraCmd.ExecuteNonQuery();
						bRes=true;
							

					}
					else if ((bRes)&&(bExist))
					{
						bRes=false;
						oraCmd.CommandText =	string.Format("update WORKS set WORK_SS_ID={0}, WORK_USR_ID={2}, WORK_UNI_ID={3}, WORK_NUM_PARK_SPACES={4}, WORK_REMARKS='{5}', WORK_INI_DATE= to_date('{6}', 'HH24MISSDDMMYY'), WORK_END_DATE={7}, WORK_LICEN_NUMBER='{8}', WORK_LICEN_CORP='{9}' "+
															  "where WORK_PDA_ID='{1}'",															   																				
																_streetStretch,
																_PDAID,
																_userId,
																_unitId,
																_NumPlaces,
																_Remarks,
																_StartDate,
																strEndDate,
																_LicenseNumber,
																_LicenseCompany);


							
						oraCmd.ExecuteNonQuery();
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
						logger.AddLog("[Msg56:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg56:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistWork(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bExist=false;

			try
			{
				oraCmd.CommandText =	string.Format("select count(*) "+    
					"from   WORKS "+
					"where  WORK_PDA_ID = '{0}'",
					_PDAID );
				

				if (Convert.ToInt32(oraCmd.ExecuteScalar())>0)
				{
					bExist=true;
				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		#endregion
	
	}	
}
