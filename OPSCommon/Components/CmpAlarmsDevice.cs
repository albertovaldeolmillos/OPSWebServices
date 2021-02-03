using System;
using System.Data;

using OPS.Components.Data;


namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpAlarms.
	/// </summary>
	public class CmpAlarmsDevice
	{

		public struct Alarm
		{
			public readonly int _uniId;
			public readonly int _id;
			public int _type;
			public DateTime _start;
			public DateTime _end;
			public bool _attended;
			public Alarm (int devid) { _uniId = devid; _id = -1; _type = 0; _start = DateTime.Now; _end = DateTime.Now; _attended = false; }
			public Alarm (int devid, int id) { _uniId = devid; _id = id; _type = 0; _start = DateTime.Now; _end = DateTime.Now; _attended = false;}
		}

		private int _device;
		/// <summary>
		/// Builds a new CmpAlarmsDevice associated to a device
		/// </summary>
		/// <param name="devid">ID of the device (ALA_UNI_ID field)</param>
		public CmpAlarmsDevice(int devid) 
		{
			_device = devid;
		}

		private Alarm[] GetActiveAlarms()
		{
			CmpAlarmsDB adb  =  new CmpAlarmsDB();
			DataTable dt = adb.GetData (new string[] {"ALA_ID","ALA_DALA_ID","ALA_INIDATE"},
				"ALA_UNI_ID = @ALARMS.ALA_UNI_ID@ AND ALA_ENDDATE IS NULL","ALA_DALA_ID ASC",new object[] {_device});

			if (dt.Rows.Count == 0) return null;
			Alarm[] alarms = new Alarm[dt.Rows.Count];
			int i = 0;
			foreach (DataRow dr in dt.Rows)
			{
				alarms[i] = new Alarm(_device, Convert.ToInt32 (dr["ALA_ID"]));
				alarms[i]._end = DateTime.MaxValue;
				alarms[i]._start = Convert.ToDateTime (dr["ALA_INIDATE"]);
				alarms[i]._type = Convert.ToInt32 (dr["ALA_DALA_ID"]);
			}
			return alarms;
		}
		
		/// <summary>
		/// Processes all alarms received by a device
		/// </summary>
		/// <param name="masksAlarms">Masks of alarms</param>
		public void ProcessAlarmsReceived (uint[] masksAlarms)
		{
			const int ALARMS_PER_MAX = 32;							// ALARMS_PER_MAX = sizeof(uint) = 32
			Alarm[] activeAlarms = GetActiveAlarms();
			CmpAlarmsDB adb = new CmpAlarmsDB();
			bool [] bAlarmsToActivate;
			bAlarmsToActivate = new bool[masksAlarms.Length * ALARMS_PER_MAX ];			// Each mask is 32 alarms

			// Iterates over all masks and fills an array of bools storing the status of each alarm.
			for (int indexMask=0; indexMask< ALARMS_PER_MAX ; indexMask++)				// each mask is 32 alarms... (bits)
			{
				uint mask = masksAlarms[indexMask];
				for (int bit = 0; bit < ALARMS_PER_MAX  ; bit++) 
				{
					// Check if bit-essim bit is activated ==> alarm ON
					bAlarmsToActivate [bit + (indexMask * ALARMS_PER_MAX )] =  ((mask & (uint)(1 << bit )) > 0); 
				} 
			}
			// Now we have to store in the DB everyalarm set to true and delete everyalarm set to false.
		}
	}
}
