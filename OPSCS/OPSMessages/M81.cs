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
	/// m81 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg81 : MsgReceived, IRecvMessage
	{

		private long	_unit=-1;
		private string	_movDate="";
		private long _user=-1;
		private long _chipCardId=-1;
		private long _NumberOfSlots=-1;
		private long _InSlotIni=-1;
		private long	_InSlotEnd=-1;

		private long _responseResult=1;
		private long _OutSlotIni=-1;
		private long	_OutSlotEnd=-1;
		private long _ReservationId=-1; 
		private string _DateIni="";
		private string _DateEnd=""; 						
		private string _FirstOpDateEnd=""; 						
		private long _TimeSlotIni=-1;
		private long _TimeSlotEnd=-1;



/*
 * 
 * <m81 id="17" ret="1">
 *					<u>103</u> --> unidad
 *					<d>152631170310</d> --> Fecha de consulta
 *					<us>1</us>  --> Usuario
 *					<ns>1</ns>  --> Number of slots
 *					<si>1</si>  --> Slot ini
 *					<se>1</se>  --> Slot End
 *					<chi>2</chi> --> Identificador de tarjeta smartcard
 * </m81>
 * 
 * <ap>
 *					<r> </r> -->Result
 *					<re> </re> --> Reservation
 *					<ns> </ns> --> Number of slots
 *					<si> </si> --> Slot ini
 *					<se> </se> --> Slot end
 *					<ts> </ts> --> Time slot
 *					<d1> </d1> --> Fecha de inicio 
 *					<d2> </d2> --> Fecha de fin 
 * 
 * </ap>
 * 
 * 
 * 
 */



		/// <summary>
		/// Constructs a new m81 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg81(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unit = Convert.ToInt64(n.InnerText); break;
					case "d": _movDate = n.InnerText; break;
					case "us": _user = Convert.ToInt64(n.InnerText); break;
					case "ns": _NumberOfSlots = Convert.ToInt64(n.InnerText); break;
					case "si": _InSlotIni = Convert.ToInt64(n.InnerText); break;
					case "se": _InSlotEnd = Convert.ToInt64(n.InnerText); break;
					case "chi": _chipCardId = Convert.ToInt64(n.InnerText); break;		
			
					

				}
			}
		}

		#region DefinedRootTag(m81)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m81"; } }
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
					CargaDescarga.DBComponent dbCargaDescarga = new CargaDescarga.DBComponent();

					_OutSlotIni=_InSlotIni;
					_OutSlotEnd=_InSlotEnd;

					dbCargaDescarga.UMDM_Query(ref oraDBConn,_user,_unit,_movDate,
											   _NumberOfSlots,ref _OutSlotIni, ref _OutSlotEnd,
											   ref _ReservationId, ref _DateIni,ref _DateEnd, ref _FirstOpDateEnd,						
											   ref _TimeSlotIni, ref _TimeSlotEnd);

					bRes=true;
					_responseResult=1;
																	
				}

				if (bRes)
				{
/*
 * <ap>
 *					<r> </r> -->Result
 *					<re> </re> --> Reservation
 *					<ns> </ns> --> Number of slots
 *					<si> </si> --> Slot ini
 *					<se> </se> --> Slot end
 *					<ts> </ts> --> Time slot
 *					<d1> </d1> --> Fecha de inicio 
 *					<d2> </d2> --> Fecha de fin 
 *					<do2> </do2> --> Fecha de fin de la primera operacion de la reserva
 * 
 * </ap>
 * */
					string response = "<r>" + Convert.ToString(_responseResult) + "</r>";
					response += "<re>" + _ReservationId.ToString() + "</re>";
					response += "<ns>" + _NumberOfSlots.ToString() + "</ns>";
					response += "<si>" + _OutSlotIni.ToString() + "</si>";
					response += "<se>" + _OutSlotEnd.ToString() + "</se>";
					response += "<ts>" + _TimeSlotIni.ToString() + "</ts>";
					response += "<d1>" + _DateIni + "</d1>";
					response += "<d2>" + _DateEnd + "</d2>";
					response += "<do2>" + _FirstOpDateEnd + "</do2>";

					res = new StringCollection();
					res.Add(new AckMessage(_msgId, response).ToString());
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg81:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg81:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{

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
