using System;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

using System.Text;
using System.Configuration;

namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for MSGS_LOG.
	/// </summary>
	public class CmpMsgsLogDB : CmpGenericBase
	{
		//private static System.Threading.Mutex m = new System.Threading.Mutex();

		public CmpMsgsLogDB()
		{          
			_standardFields		= new string[] { "LMSG_ID", "LMSG_SRC_UNI_ID", "LMSG_DST_UNI_ID", "LMSG_DATE",
												 "LMSG_TYPE", "LMSG_XML_IN", "LMSG_XML_OUT"};
			_standardPks		= new string[] { "LMSG_ID" };
			_standardTableName	= "MSGS_LOG";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		
			_standardRelationFileds	= new string[0];
			_standardRelationTables	= new string[0];
			_stValidDeleted			= new string[0];
		}

		
		#region Extra methods accessible only through a CmpMsgsLogDB reference
		/// <summary>
		/// Inserts a new Alarm into the alarms table
		/// </summary>
		/// <param name="iSrcId">Source Unit Id</param>
		/// <param name="iDstId">Destine Unit Id</param>
		/// <param name="sType">Message Type Name (m1, m2, ...)</param>
		/// <param name="sIn">Input message body</param>
		/// <param name="sOut">Answer message body</param>
		public void InsertMsg (int iSrcId, int iDstId, string sType, string sIn, string sOut)
		{

			Database d = DatabaseFactory.GetDatabase();
			long id = GetNewMsgsLogPk();
			AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			double nDifHour=0;
			try
			{
				nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
			}
			catch
			{
				nDifHour=0;
			}			
			
			string ssql = "INSERT INTO MSGS_LOG (LMSG_ID, LMSG_SRC_UNI_ID, LMSG_DST_UNI_ID, LMSG_DATE, " +
						  "LMSG_TYPE, LMSG_XML_IN, LMSG_XML_OUT) VALUES (";
			ssql = ssql + "@MSGS_LOG.LMSG_ID@, @MSGS_LOG.LMSG_SRC_UNI_ID@, @MSGS_LOG.LMSG_DST_UNI_ID@, " +
						  "@MSGS_LOG.LMSG_DATE@, @MSGS_LOG.LMSG_TYPE@, @MSGS_LOG.LMSG_XML_IN@, @MSGS_LOG.LMSG_XML_OUT@)";
			d.ExecuteNonQuery (ssql, id, iSrcId , iDstId, DateTime.Now.AddHours(nDifHour), sType, sIn, sOut);
		}

		
	
		#endregion

		#region Protected helper methods
		
		/// <summary>
		/// Retrieves a new PK for ALARMS_HIS.
		/// That method is not intrinsic thread-safe, so should be called in a thread-safe scenario...
		/// </summary>
		/// <returns>A new valid PK for ALARMS_HIS</returns>
		protected int GetNewMsgsLogPk()
		{
			Database d = DatabaseFactory.GetDatabase();
			int id = Convert.ToInt32(d.ExecuteScalar("select SEQ_MSGS_LOG.NEXTVAL FROM DUAL"));
			return id;
		}

		
		#endregion
	}
}
