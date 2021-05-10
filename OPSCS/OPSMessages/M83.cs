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
	/// m83 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg83 : MsgReceived, IRecvMessage
	{

		private long	_unit=-1;
		private string	_movDate="";
		private long _user=-1;
		private long _chipCardId=-1;
		private long _NumberOfSlots=-1;
		private long _InSlotIni=-1;
		private long	_InSlotEnd=-1;
		private long _InReservationId=-1; 
		private string _DateIni="";
		private string _DateEnd=""; 						
		private long _TimeSlotIni=-1;
		private long _PDMOperation=-1;
		private long _PDMOperationStatus;
		private long _OnlineOperation;
		private long _TicketNumber;

		private long _OutReservationId=-1; 



		/*
		 * 
		 * <m83 id="17" ret="1">
		 *					<u>103</u> --> unidad
		 *					<d>152631170310</d> --> Fecha de consulta
		 *					<o>1</o>  --> PDM Internal operation 
		 *					<os>1</os>  --> Operation Status
		 *					<us>1</us>  --> Usuario
		 *					<re> </re> --> Reservation
		 *					<ns>1</ns>  --> Number of slots
		 *					<si>1</si>  --> Slot ini
		 *					<se>1</se>  --> Slot End
		 *					<ts> </ts> --> Time slot
		 *					<d1> </d1> --> Fecha de inicio 
		 *					<d2> </d2> --> Fecha de fin 
		 *					<chi>2</chi> --> Identificador de tarjeta smartcard
		 *					<om>1</om> --> Operation has been online
		 *					<tcn>1</tcn> --> Ticket number
		 * </m83>
		 * 
		 * <ap>
		 *					<re> </re> --> Reservation		 
		 * </ap>
		 * 
		 * 
		 * 
		 */



		/// <summary>
		/// Constructs a new m83 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg83(XmlDocument msgXml) : base(msgXml) {}

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
					case "o": _PDMOperation = Convert.ToInt64(n.InnerText); break;		
					case "os": _PDMOperationStatus = Convert.ToInt64(n.InnerText); break;		
					case "re": _InReservationId = Convert.ToInt64(n.InnerText); break;		
					case "ts": _TimeSlotIni = Convert.ToInt64(n.InnerText); break;		
					case "d1": _DateIni = n.InnerText; break;
					case "d2": _DateEnd = n.InnerText; break;
					case "om": _OnlineOperation = Convert.ToInt64(n.InnerText); break;		
					case "tcn": _TicketNumber = Convert.ToInt64(n.InnerText); break;		
					

				}
			}
		}

		#region DefinedRootTag(m83)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m83"; } }
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

					if (_InReservationId<0)
						_OutReservationId=-1;
					else
						_OutReservationId=_InReservationId;

					dbCargaDescarga.UMDM_Operation(ref oraDBConn,_user,_unit,_movDate,_PDMOperationStatus,
						_NumberOfSlots,ref _InSlotIni, ref _InSlotEnd,
						ref _OutReservationId, ref _DateIni,ref _DateEnd, 						
						ref _TimeSlotIni);

					bRes=true;
																	
				}

				if (bRes)
				{
					/*
					 * <ap>
					 *					<re> </re> --> Reservation
					 * </ap>
					 * */
					string response = "<re>" + _OutReservationId.ToString() + "</re>";

					res = new StringCollection();
					res.Add(new AckMessage(_msgId, response).ToString());
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg83:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg83:Process]: Error: "+e.Message,LoggerSeverities.Error);
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
