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
	/// m73 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg73 : MsgReceived, IRecvMessage
	{

		private int	_unit=-1;
		private string	_movDate="";
		private int _user=-1;
		private int _chipCardId=-1;
		private int _cardStatus=-1;
		private int _LastOperSourcePlat=-1;
		private string	_LastOperDate="";


		private int _responseResult=1;
		private int _operatationStatus=-1;



/*
 * 
 * <m73 id="17" ret="1">
 *					<u>103</u> --> unidad
 *					<d>152631170310</d> --> Fecha de consulta
 *					<us>1</us>  --> Usuario
 *					<cs>0</cs>  --> Estado de la ultima operación en tarjeta 
 *					<ps>8</ps>  --> En caso de operación abierta plataforma de origen
 *					<lod>152319170310</lod> --> En caso de operación abierta fecha de inicio de la operación
 *					<chi>2</chi> --> Identificador de tarjeta smartcard
 * </m73>
 * 
 * <ap>
 *					<r> </r> -->Result
 *					<pt> </pt> --> Tipo de medio de pago
 *					<uss> </uss> --> Status del usuario
 *					<ad> </ad> --> Article Def
 *					<st> </st> --> Subtariff
 *					<ot> </ot> --> Operation Type
 *					<os> </os> --> Operation Status
 *					<bi> </bi> --> bike tag
 *					<d0> </d0> --> Fecha de inicio de la operación completa
 *					<d1> </d1> --> Fecha de inicio de la suboperación actual
 *					<d2> </d2> --> Fecha de fin de la suboperación actual
 *					<dm> </dm> --> Fecha máxima de la operación actual
 *					<q> </q> --> Cantidad a cobrar
 *					<t> </t> --> Tiempo efectivo consumido
 * 
 * </ap>
 * 
 * 
 * 
 */



		/// <summary>
		/// Constructs a new m73 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg73(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _movDate = n.InnerText; break;
					case "us": _user = Convert.ToInt32(n.InnerText); break;
					case "cs": _cardStatus = Convert.ToInt32(n.InnerText); break;
					case "ps": _LastOperSourcePlat = Convert.ToInt32(n.InnerText); break;
					case "lod": _LastOperDate = n.InnerText; break;
					case "chi": _chipCardId = Convert.ToInt32(n.InnerText); break;		
			
					

				}
			}
		}

		#region DefinedRootTag(m73)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m73"; } }
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
			bool bUserExist=false;
			int iUserStatus=Msg72.DEF_USER_STATUS_BLOCKED;
			int iChipCardId=-1;


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
					oraCmd.CommandText="";
					

					if (GetUserData(ref bUserExist,ref oraDBConn,ref oraCmd,ref iUserStatus,ref iChipCardId))
					{
						if (!bUserExist)
						{
							iUserStatus=Msg72.DEF_USER_STATUS_BLOCKED;

						}


						if (iUserStatus==Msg72.DEF_USER_STATUS_ACTIVE)
						{
							if (_cardStatus==Msg72.DEF_CARD_OPERSTATE_BYCING_OPEN)
							{
								/*  Consultamos el estado de la operacion abierta. Existen dos
								*	opciones: que encontramos la operación -> 
								*				debemos comprobar que efectivamente está abierta ->
								*						si está abierta -> calculamos la tarifa  y devolvemos los datos de cierre
								*						si está cerrada o cancelada -> devolvemos el estado para actualizar la tarjeta
								*													   del usuario
								*			  que no encontremos la operación -> supondremos que la tarjeta contiene la información
								*												 correcta y la cerreremos en local
								* 
								 
									

								*/

								int iOpStatus=-1;
								if (ExistOperation(ref iOpStatus,ref oraDBConn,ref oraCmd, ref dr))
								{
									_operatationStatus=	iOpStatus;															
								}
								
							}
						}
						bRes=true;
					}
																	
				}

				if (bRes)
				{

					string response = "<r>" + Convert.ToString(_responseResult) + "</r>";
					response += "<os>" + _operatationStatus.ToString() + "</os>";
					response += "<uss>" + iUserStatus.ToString() + "</uss>";

					res = new StringCollection();
					res.Add(new AckMessage(_msgId, response).ToString());
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg73:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg73:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistOperation(ref int iStatus, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			iStatus=-1;
			

			try
			{
				oraCmd.CommandText =	string.Format("select BYCOP_STATUS "+    
													  "from   BYCYCLES_OPERATIONS "+
													  "where  BYCOP_BUSER_ID = {0} "+
													  "  and  TO_CHAR(BYCOP_INIDATE0,'HH24MISSDDMMYY') = '{1}'"+
													  "  and  BYCOP_SRC_PLAT_ID={2}",
													  _user, _LastOperDate,_LastOperSourcePlat );
				

				

				dr=oraCmd.ExecuteReader();

				if (dr.Read())
				{
					iStatus=Convert.ToInt32(dr[0]);
				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetUserData(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,
						 ref int iUserStatus, ref int iChipCardId)
		{
			bool bRes=true;
			bExist=false;
			OracleDataReader dr= null;

			try
			{
				oraCmd.CommandText =	string.Format("select buser_status,nvl(buser_chipcard_id,-1) buser_chipcard_id "+    
														"from   BICYCLES_USERS "+
														"where  BUSER_ID = {0} ",
														_user );
				


				dr=oraCmd.ExecuteReader();


				if (dr.Read())
				{
					iUserStatus=dr.GetInt32(dr.GetOrdinal("buser_status"));
					iChipCardId=dr.GetInt32(dr.GetOrdinal("buser_chipcard_id"));
					bExist=true;

				}

			}
			catch
			{
				bRes=false;
			}
			finally
			{
				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

			}
			return bRes;
			
		}




		#endregion
	
	}	
}
