using System;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using OPS.Components.Data;
using OPS.Comm;
using OPS.FineLib;
using System.Collections;
//using Oracle.DataAccess.Client;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{


	/// <summary>
	/// Class to handle de m9 message.
	/// </summary>
	public sealed class Msg61 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m61)
		#region Static stuff

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static Msg61()
		{
		}

		#endregion

		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m61"; } }
		#endregion

		#region Variables, creation and parsing

		private int			_unit=-1;	   
		private string		_date="";		
		private int			_user=-1;	   
		private int			_valid=9;	   
	
		private double _dLatitude=-999;
		private double _dLongitude=-999;
		private double _dAltitude=-999;
		private double _dSpeed=-999;
		private double _dDistance=-999;
/*
 * <m61 id="55" dst="4">
 *		<d>165451220512</d>
 *		<u>2049</u>
 *		<ur>1</ur>
 *		<v>0</v>
 *		<la>0.000000</la>
 *		<lo>0.000000</lo>
 *		<a>0.000000</a>
 *		<s>0.000000</s>
 *		<di>0.000000</di>
 *</m61>
 * 
 * 
 */


		/// <summary>
		/// Constructs a new msg06 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg61(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			CultureInfo culture = new CultureInfo("", false);

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _date = n.InnerText; break;			
					case "ur": _user = Convert.ToInt32(n.InnerText); break;
					case "v": _valid = Convert.ToInt32(n.InnerText); break;
					case "la": 
						_dLatitude  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;				
					case "lo": 
						_dLongitude  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;				
					case "a": 
						_dAltitude  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;				
					case "s": 
						_dSpeed  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;				
					case "di": 
						_dDistance  =  Convert.ToDouble(n.InnerText, (IFormatProvider)culture.NumberFormat);
						break;				


				}
			}
		}

		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Inserts a new register in the OPERATIONS table, and if everything is succesful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
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
				

					if (!ExistPosition(ref bExist,ref oraDBConn,ref oraCmd))
					{
						bRes=false;
					}
					else
					{
						bRes=true;
					}

					if ((bRes)&&(!bExist))
					{



						bRes=false;
						oraCmd.CommandText="";
			
						CultureInfo culture = new CultureInfo("", false);
						oraCmd.CommandText =	string.Format((IFormatProvider)culture.NumberFormat,
							"insert into MAP_GUARDS_POSITIONS (GUAP_UNI_ID, GUAP_USR_ID, GUAP_DATE, GUAP_VALID_POS, GUAP_POSLAT, GUAP_POSLON, GUAP_POSALT, GUAP_POSSPEED, GUAP_POSDIST) "+
							"values ({0}, {1}, to_date('{2}', 'HH24MISSDDMMYY'), {3}, {4}, {5}, {6}, {7}, {8})",															   
							_unit,
							_user,
							_date,
							_valid,
							_dLatitude,
							_dLongitude,
							_dAltitude,
							_dSpeed,
							_dDistance);
						
							
						if (oraCmd.CommandText!="")
						{								
							oraCmd.ExecuteNonQuery();
							bRes=true;
						}
						
					}

				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg54:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg54:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistPosition(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bExist=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select count(*) "+
														"from map_guards_positions t "+
														"where t.guap_uni_id = {0} "+
														" and t.guap_usr_id = {1} "+
														" and TO_CHAR(t.guap_date, 'HH24MISSDDMMYY') = '{2}'",
													_unit, _user, _date );
				

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
