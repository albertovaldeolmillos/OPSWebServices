using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace CargaDescarga
{
	public class DBComponent
	{




		public void UMDM_Query(ref OracleConnection conn,
			long userId,
			long stationId,
			string date,               
			long numSlots,
			ref long slotIni,
			ref long slotEnd,
			ref long reservationId,
			ref string dateIni,
			ref string dateEnd,
			ref string firstOpDateEnd,
			ref long timeSlotIni,
			ref long timeSlotEnd)
		{

			OracleCommand cmd = null;

			try
			{
				cmd = new OracleCommand();
				cmd.Connection = conn;
				cmd.CommandText = "UMDM_ALGORITHM";
				cmd.CommandType = CommandType.StoredProcedure;



				OracleParameter pUserId = new OracleParameter("PUserId", OracleDbType.Decimal);
				pUserId.Direction = ParameterDirection.Input;
				pUserId.Value = userId;
				cmd.Parameters.Add(pUserId);

				OracleParameter pStationId = new OracleParameter("PStationId", OracleDbType.Decimal);
				pStationId.Direction = ParameterDirection.Input;
				pStationId.Value = stationId;
				cmd.Parameters.Add(pStationId);

				OracleParameter pDate = new OracleParameter("PDate", OracleDbType.Varchar2, 20);
				pDate.Direction = ParameterDirection.Input;
				pDate.Value = date;
				cmd.Parameters.Add(pDate);

				OracleParameter pNumSlots = new OracleParameter("PNumSlots", OracleDbType.Decimal);
				pNumSlots.Direction = ParameterDirection.InputOutput;
				pNumSlots.Value = numSlots;
				cmd.Parameters.Add(pNumSlots);

				OracleParameter pSlotIni = new OracleParameter("PSlotIni", OracleDbType.Decimal);
				pSlotIni.Direction = ParameterDirection.InputOutput;
				pSlotIni.Value = slotIni;
				cmd.Parameters.Add(pSlotIni);

				OracleParameter pSlotEnd = new OracleParameter("PSlotEnd", OracleDbType.Decimal);
				pSlotEnd.Direction = ParameterDirection.InputOutput;
				pSlotEnd.Value = slotEnd;
				cmd.Parameters.Add(pSlotEnd);

				OracleParameter pReservationId = new OracleParameter("PReservationId", OracleDbType.Decimal);
				pReservationId.Direction = ParameterDirection.InputOutput;
				pReservationId.Value = reservationId;
				cmd.Parameters.Add(pReservationId);

				OracleParameter pDateIni = new OracleParameter("PDateIni", OracleDbType.Varchar2, 20);
				pDateIni.Direction = ParameterDirection.Output;
				pDateIni.Value = dateIni;
				cmd.Parameters.Add(pDateIni);

				OracleParameter pDateEnd = new OracleParameter("PDateEnd", OracleDbType.Varchar2, 20);
				pDateEnd.Direction = ParameterDirection.Output;
				pDateEnd.Value = dateEnd;
				cmd.Parameters.Add(pDateEnd);

				OracleParameter PDateFirstOpEnd = new OracleParameter("PDateFirstOpEnd", OracleDbType.Varchar2, 20);
				PDateFirstOpEnd.Direction = ParameterDirection.Output;
				PDateFirstOpEnd.Value = firstOpDateEnd;
				cmd.Parameters.Add(PDateFirstOpEnd);

				OracleParameter pTimeSlotIni = new OracleParameter("PTimeSlotIni", OracleDbType.Decimal);
				pTimeSlotIni.Direction = ParameterDirection.Output;
				pTimeSlotIni.Value = timeSlotIni;
				cmd.Parameters.Add(pTimeSlotIni);

				OracleParameter pTimeSlotEnd = new OracleParameter("PTimeSlotEnd", OracleDbType.Decimal);
				pTimeSlotEnd.Direction = ParameterDirection.Output;
				pTimeSlotEnd.Value = timeSlotEnd;
				cmd.Parameters.Add(pTimeSlotEnd);

				cmd.ExecuteNonQuery();

				numSlots = long.Parse(cmd.Parameters["PNumSlots"].Value.ToString());
				slotIni = long.Parse(cmd.Parameters["PSlotIni"].Value.ToString());
				slotEnd = long.Parse(cmd.Parameters["PSlotEnd"].Value.ToString());
				reservationId = long.Parse(cmd.Parameters["PReservationId"].Value.ToString());                    
				dateIni = cmd.Parameters["PDateIni"].Value.ToString();
				dateEnd = cmd.Parameters["PDateEnd"].Value.ToString();
				firstOpDateEnd = cmd.Parameters["PDateFirstOpEnd"].Value.ToString();
				timeSlotIni = long.Parse(cmd.Parameters["PTimeSlotIni"].Value.ToString());
				timeSlotEnd = long.Parse(cmd.Parameters["PTimeSlotEnd"].Value.ToString());
			}
			catch (Exception)
			{
				throw;
			}

        
        
        
		}

		public void UMDM_Operation(ref OracleConnection conn,
			long userId,
			long stationId,
			string date,
			long status,
			long numSlots,
			ref long slotIni,
			ref long slotEnd,
			ref long reservationId,
			ref string dateIni,
			ref string dateEnd,
			ref long timeSlotIni)
		{

			OracleCommand cmd = null;

			try
			{
				cmd = new OracleCommand();
				cmd.Connection = conn;
				cmd.CommandText = "UMDM_OPERATION";
				cmd.CommandType = CommandType.StoredProcedure;



				OracleParameter pUserId = new OracleParameter("PUserId", OracleDbType.Decimal);
				pUserId.Direction = ParameterDirection.Input;
				pUserId.Value = userId;
				cmd.Parameters.Add(pUserId);

				OracleParameter pStationId = new OracleParameter("PStationId", OracleDbType.Decimal);
				pStationId.Direction = ParameterDirection.Input;
				pStationId.Value = stationId;
				cmd.Parameters.Add(pStationId);

				OracleParameter pDate = new OracleParameter("PDate", OracleDbType.Varchar2, 20);
				pDate.Direction = ParameterDirection.Input;
				pDate.Value = date;
				cmd.Parameters.Add(pDate);

				OracleParameter pStatus = new OracleParameter("PStatus", OracleDbType.Decimal);
				pStatus.Direction = ParameterDirection.Input;
				pStatus.Value = status;
				cmd.Parameters.Add(pStatus);

				OracleParameter pNumSlots = new OracleParameter("PNumSlots", OracleDbType.Decimal);
				pNumSlots.Direction = ParameterDirection.InputOutput;
				pNumSlots.Value = numSlots;
				cmd.Parameters.Add(pNumSlots);

				OracleParameter pSlotIni = new OracleParameter("PSlotIni", OracleDbType.Decimal);
				pSlotIni.Direction = ParameterDirection.InputOutput;
				pSlotIni.Value = slotIni;
				cmd.Parameters.Add(pSlotIni);

				OracleParameter pSlotEnd = new OracleParameter("PSlotEnd", OracleDbType.Decimal);
				pSlotEnd.Direction = ParameterDirection.InputOutput;
				pSlotEnd.Value = slotEnd;
				cmd.Parameters.Add(pSlotEnd);

				OracleParameter pReservationId = new OracleParameter("PReservationId", OracleDbType.Decimal);
				pReservationId.Direction = ParameterDirection.InputOutput;
				pReservationId.Value = reservationId;
				cmd.Parameters.Add(pReservationId);

				OracleParameter pDateIni = new OracleParameter("PDateIni", OracleDbType.Varchar2, 20);
				pDateIni.Direction = ParameterDirection.InputOutput;
				pDateIni.Value = dateIni;
				cmd.Parameters.Add(pDateIni);

				OracleParameter pDateEnd = new OracleParameter("PDateEnd", OracleDbType.Varchar2, 20);
				pDateEnd.Direction = ParameterDirection.InputOutput;
				pDateEnd.Value = dateEnd;
				cmd.Parameters.Add(pDateEnd);

				OracleParameter pTimeSlotIni = new OracleParameter("PTimeSlotIni", OracleDbType.Decimal);
				pTimeSlotIni.Direction = ParameterDirection.InputOutput;
				pTimeSlotIni.Value = timeSlotIni;
				cmd.Parameters.Add(pTimeSlotIni);

                

				cmd.ExecuteNonQuery();

				status = long.Parse(cmd.Parameters["PStatus"].Value.ToString());
				numSlots = long.Parse(cmd.Parameters["PNumSlots"].Value.ToString());
				slotIni = long.Parse(cmd.Parameters["PSlotEnd"].Value.ToString());
				reservationId = long.Parse(cmd.Parameters["PReservationId"].Value.ToString());
				dateIni = cmd.Parameters["PDateIni"].Value.ToString();
				dateEnd = cmd.Parameters["PDateEnd"].Value.ToString();
				timeSlotIni = long.Parse(cmd.Parameters["PTimeSlotIni"].Value.ToString());
                
			}
			catch (Exception)
			{
				throw;
			}




		}



	}
}
