using System;
using System.Data;
using OPS.Components.Data;
using System.Collections;

namespace OPS.Comm.Becs.Components
{
	/// <summary>
	/// That class mantains on-memory status of ALL devices and its alarms.
	/// Is a buffer of table ALARMS in order to minimize the queries to the database
	/// and maximize the performance
	/// </summary>
	public class CmpAlarmsDevices
	{ 

		public class AlarmDeviceInfo
		{
			public enum AlarmAction {INSERT = 1, DELETE = 2, NOTHING = 3}

			private static int ALARMS_PER_BLOCK = 32;			// ALARMS_PER_BLOCK = sizeof(uint) ==> 32
			
			private static long _numAlarms;
			public readonly int UnitId;
			public DateTime Date;
			private uint[] _alarms;
			private bool[] _alarmInDatabase;
			private AlarmAction[] _actions;

			// Get the num of alarms defined in the system
			static AlarmDeviceInfo()
			{
				CmpAlarmsDefDB cmp = new CmpAlarmsDefDB();
				_numAlarms = cmp.LastPKValue + 1;					// +1 because the first DALA_ID valid is 0
			}

			// Constructs a new AlarmDeviceInfo assigned to specified unit
			// The object is empty, so it must be loaded from DataBase
			internal AlarmDeviceInfo (int unitId)
			{
				UnitId = unitId;
				_alarms = null;
				_alarmInDatabase = null;
				Date=DateTime.MinValue;
			}

			/// <summary>
			/// Read all the alarms of the current device from the DataBas
			/// and stores them in _alarms array
			/// </summary>
			internal void ReadAlarms()
			{

				CmpAlarmsDB cmp = new CmpAlarmsDB();
				DataTable dt = cmp.GetData(
					new string[] {"ALA_DALA_ID, TO_CHAR(ALA_INIDATE,'hh24missddmmyy') ALA_INIDATE"},
					"ALA_UNI_ID=@ALARMS.ALA_UNI_ID@",
					"ALA_DALA_ID ASC", 
					new object[] {UnitId});
				_alarms = new uint[(int)Math.Ceiling((double)_numAlarms / ALARMS_PER_BLOCK)];
				_alarmInDatabase = new bool[_numAlarms];
				_actions = new AlarmAction[_numAlarms];
				for (int i=0; i<_numAlarms; i++) _actions[i] = AlarmAction.NOTHING;
				foreach (DataRow dr in dt.Rows)
				{
					int bit = Convert.ToInt32(dr["ALA_DALA_ID"]);
					DateTime dtALA_INIDATE = OPS.Comm.Dtx.StringToDtx(dr["ALA_INIDATE"].ToString());
					// Activates the current bit.
					int idx = (int)Math.Floor ((double)bit / ALARMS_PER_BLOCK);
					_alarms[idx] |= ((uint)(1 << (bit % ALARMS_PER_BLOCK)));
					_alarmInDatabase[bit] = true;				// mark that the current alarm was found in the database

					if (dtALA_INIDATE>Date)
					{
						Date=dtALA_INIDATE;
					}
				}
			}

			/// <summary>
			/// Resync the status of the memory object with the status of the DataBase.
			/// Call Resync has the same result of calling ReadAlarms, assuming that the updates were done in the DataBase
			/// </summary>
			internal void Resync()
			{
				// Foreach action
				//		if action is DELETE alarm, set that alarm is not more in the database (since was deleted).
				//		if action is INSERT alarm, set that alarm is found in the database (since was added)
				//	after all, set that the action is NOTHING (because the action has really done, and we have to do
				//  nothing since the next update...)

				for (int i=0; i< _actions.Length; i++)
				{
					switch (_actions[i]) 
					{
						case AlarmAction.DELETE:
							_alarmInDatabase[i] = false;
							break;
						case AlarmAction.INSERT:
							_alarmInDatabase[i] = true;
						break;
						case AlarmAction.NOTHING:
							break;						// Nothing to do...
					}
					_actions[i] = AlarmAction.NOTHING;
				}
				// finally we have to rebuild _alarms member variables with the same info of _alarmInDatabase...
				for (int i=0; i< _alarms.Length;i++) { _alarms[i] = 0; }

				// For each bit activated we have to put them into the corresponding block of _alarms
				// (as always in each block of alarms 32 bits are stored).
				for (int bit=0; bit<_alarmInDatabase.Length; bit++)
				{
					if (_alarmInDatabase[bit])
					{
						int idx = (int)Math.Floor ((double)bit / ALARMS_PER_BLOCK);
						_alarms[idx] |= ((uint)(1 << (bit % ALARMS_PER_BLOCK)));
					}
				}
				// At that point we are exactly in the same status if we were read the database again (assuming, of course,
				// that all changes have been done to the database (so, in fact Resync has to be called, if and only if the
				// update to the database has been done succesfully.
				// The reason to use Resync instead of reading the database once again is only one: performance
			}

			/// <summary>
			/// Updates the alarms of the current AlarmDeviceInfo object
			/// Foreach alarm set if is a new alarm and has to be INSERTed, or is alarm that no longer
			/// has to be in the DataBase and has to be DELETEd
			/// </summary>
			/// <param name="alarms">array of uints with the new alarm status</param>
			internal void UpdateAlarms (uint[] alarms,DateTime date)
			{
				bool bChange=false;
				int nblocks = _alarms.Length;
				for (int bit = 0; bit<ALARMS_PER_BLOCK * nblocks; bit++)
				{
					if (bit == _actions.Length) break;
					int block = (int)Math.Floor ((double)bit / ALARMS_PER_BLOCK);
					// Compare the new status of the alarm with the stored status
					// The new status is given reading alarms parameter at bit-level. But alarms parameter can have
					// less elements that the total of elements needed to store all the alarms (because 0s are not sent).
					// So, if we find that te current element (aka block) is not in the alarms, we assume that is a 0,
					// and alarm is not on.
					bool  alarmOn =   (block > alarms.Length - 1) ? false : ((alarms[block] & (1 << (bit % ALARMS_PER_BLOCK))) > 0);
					// The old status is given reading _alarms member at bit-level. _alarms paramter always have the
					// # elements needed to store all alarms (in each element [uint] of _alarms are stored 32 alarms).
					bool alarmOldOn = ((_alarms[block] & (1 << (bit % ALARMS_PER_BLOCK))) > 0);
					if (alarmOn && !alarmOldOn)
					{
						// Is a NEW ALARM
						_actions[bit] = AlarmAction.INSERT;
						bChange=true;
					}
					else if (alarmOldOn && !alarmOn)
					{
						// Is a ALARM TURNED OFF!
						_actions[bit] = AlarmAction.DELETE;
						bChange=true;
					}
				}
				if (bChange)
				{
					this.Date=date;
				}
			}
			internal AlarmDeviceInfo.AlarmAction this [int idx] { get { return _actions[idx]; } }

			internal long AlarmsCount { get { return _numAlarms;} } 
		}

		private Hashtable _data;			// Hashtable of AlarmDeviceInfo
		public CmpAlarmsDevices()
		{
			_data = new Hashtable ();
		}

		/// <summary>
		/// Updates the alarms of the unit specified
		/// </summary>
		/// <param name="uniid">ID of the unit</param>
		/// <param name="alarms">new array of uints with new status</param>
		public bool UpdateAlarms(int uniid, uint[] alarms,DateTime date)
		{
			ArrayList alarmsAdd = new ArrayList ();
			ArrayList alarmsDelete = new ArrayList();
			AlarmDeviceInfo ai = (AlarmDeviceInfo) _data[uniid];
			if (ai==null) 
			{
				ai = new AlarmDeviceInfo(uniid);
				ai.ReadAlarms ();
				_data.Add (uniid, ai);
			}
			bool bRet=false;
			if (ai.Date<date)
			{
				ai.UpdateAlarms (alarms,date);
				int []alarmsToAct = new int[ai.AlarmsCount];
				// Alarms are updated in memory, now we have to update the database for the current alarm.
				// We have to:
				//	"Delete" ALL alarms with status DELETE
				//  Insert ALL alarms with status INSERT
				
				bool bChange=false;
				for (int i=0; i< ai.AlarmsCount; i++)
				{
					AlarmDeviceInfo.AlarmAction action = ai[i];
					if (action == AlarmDeviceInfo.AlarmAction.INSERT) 
					{
						alarmsAdd.Add (i);
						bChange=true;
					}
					else if (action == AlarmDeviceInfo.AlarmAction.DELETE) 
					{
						alarmsDelete.Add (i); 
						bChange=true;
					}
				}

				if (bChange)
				{
					CmpAlarmsDB cmp = new CmpAlarmsDB();
					bRet = cmp.ProcessAlarms (alarmsAdd, alarmsDelete, uniid,ai.Date );
					if (bRet) ai.ReadAlarms();				// Update the status, assuming that the update of the BD was correct
				}
				else
				{
					bRet=true;
				}
			}
			else
			{
				//Disordered alarm from unit
				bRet=false;
			}
			return bRet;

		}
	}
}
