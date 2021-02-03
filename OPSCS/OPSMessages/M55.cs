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
	/// m54 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg55 : MsgReceived, IRecvMessage
	{



		private int	_streetStretch;
		private string	_StartDate="";
		private string	_EndDate="";
		private int _unitId;
		private int _userId;
		private int _nEnLineaPar;
		private int _nEnLineaImpar;
		private int _nEnBateriaPar;
		private int _nEnBateriaImpar;
		private int _nCDDiaEntero;
		private int _nCDMediodia;
		private int _nPMR;
		private int _nVadoDiaEntero;
		private int _nVadoMediodia;
		private int _nBasuras;
		private int _nMotos;

		/// <summary>
		/// Constructs a new m54 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg55(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{

					case "ss": _streetStretch = Convert.ToInt32(n.InnerText); break;
					case "d1": _StartDate = n.InnerText; break;
					case "d2": _EndDate = n.InnerText; break;
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "z": _userId = Convert.ToInt32(n.InnerText); break;
					case "lp": _nEnLineaPar = Convert.ToInt32(n.InnerText); break;
					case "li": _nEnLineaImpar = Convert.ToInt32(n.InnerText); break;
					case "bp": _nEnBateriaPar = Convert.ToInt32(n.InnerText); break;
					case "bi": _nEnBateriaImpar = Convert.ToInt32(n.InnerText); break;
					case "cde": _nCDDiaEntero = Convert.ToInt32(n.InnerText); break;
					case "cdm": _nCDMediodia = Convert.ToInt32(n.InnerText); break;
					case "pmr": _nPMR = Convert.ToInt32(n.InnerText); break;
					case "ve": _nVadoDiaEntero = Convert.ToInt32(n.InnerText); break;
					case "vm": _nVadoMediodia = Convert.ToInt32(n.InnerText); break;
					case "b": _nBasuras = Convert.ToInt32(n.InnerText); break;
					case "m": _nMotos = Convert.ToInt32(n.InnerText); break;
				}
			}
		}

		#region DefinedRootTag(m55)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m55"; } }
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
				
					if (!ExistInventory(ref bExist,ref oraDBConn,ref oraCmd))
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
						oraCmd.CommandText =	string.Format("insert into SS_INVENTORIES (SSI_SS_ID, SSI_INI_DATE, SSI_END_DATE, SSI_USR_ID, SSI_UNI_ID, SSI_P_EN_LINEA_PAR, SSI_P_EN_LINEA_IMPAR, SSI_P_EN_BATERIA_PAR, SSI_P_EN_BATERIA_IMPAR, SSI_P_C_D_DIA_ENTERO, SSI_P_C_D_MEDIODIA, SSI_P_PMR, SSI_P_VADO_DIA_ENTERO, SSI_P_VADO_MEDIODIA, SSI_P_BASURA, SSI_P_MOTOS) "+
																"values ({0}, to_date('{1}', 'HH24MISSDDMMYY'), to_date('{2}', 'HH24MISSDDMMYY'), {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})",															   
																_streetStretch,
																_StartDate,
																_EndDate,
																_userId,
																_unitId,
																_nEnLineaPar,
																_nEnLineaImpar,
																_nEnBateriaPar,
																_nEnBateriaImpar,
																_nCDDiaEntero,
																_nCDMediodia,
																_nPMR,
																_nVadoDiaEntero,
																_nVadoMediodia,
																_nBasuras,
																_nMotos);
							
							
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
						logger.AddLog("[Msg55:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg55:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistInventory(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd)
		{
			bool bRes=true;
			bExist=false;

			try
			{
				oraCmd.CommandText =	string.Format("select count(*) "+    
					"from   SS_INVENTORIES "+
					"where  SSI_SS_ID = {0} "+
					"  and  SSI_UNI_ID = {1} "+
					"  and  TO_CHAR(SSI_INI_DATE,'HH24MISSDDMMYY') = '{2}'",
					_streetStretch, _unitId, _StartDate );
				

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
