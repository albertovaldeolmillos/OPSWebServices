using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// A pair of priority and destination values for messages in the
	/// MSGS table
	/// </summary>
	public class PriorityDestinationGroup
	{
		public PriorityDestinationGroup(decimal priority, decimal destination)
		{
			_priority = priority;
			_destination = destination;
		}
		public decimal Priority
		{
			get { return _priority; }
		}
		public decimal Destination
		{
			get { return _destination; }
		}

		private decimal _priority;
		private decimal _destination;
	}

	/// <summary>
	/// An ordered list of PriorityDestinationGroup objects
	/// </summary>
	public class PriorityDestinationGroupList
	{
		public PriorityDestinationGroupList()
		{
			_groups = new ArrayList(5);
		}
		public void Add(PriorityDestinationGroup group)
		{
			_groups.Add(group); 
		}
		public int Count
		{
			get { return _groups.Count; }
		}
		public IEnumerator GetEnumerator()
		{
			return _groups.GetEnumerator();
		}

		protected ArrayList _groups;
	}

}
