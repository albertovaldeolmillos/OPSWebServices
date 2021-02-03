using System;
using System.Collections;

namespace OPS.Components.Data
{
	/// <summary>
	/// Mantains a collection of DataMap objects
	/// </summary>
	public class DataMapCollection : CollectionBase 
	{	
		public DataMapCollection()
		{
		}

		/// <summary>
		/// Allows access to an element using array-notation []
		/// </summary>
		public DataMap this [int index] 
		{
			get 
			{
				return (DataMap)(List[index]);
			}
			set 
			{
				List[index] = value;
			}
		}
		/// <summary>
		/// Allow access to an element indexed by logic name (read-only indexer)
		/// </summary>
		public DataMap this [string logicname]
		{
			get 
			{
				IEnumerator ie=List.GetEnumerator();
				while (ie.MoveNext()) 
				{
					if (((DataMap)ie.Current).LogicName.Equals (logicname)) 
					{
						return (DataMap)ie.Current;
					}
				}
				throw new ArgumentOutOfRangeException ("indexer",logicname,"DataMap not found");
			}
		}
		/// <summary>
		/// Adds a new DataMap to collection
		/// </summary>
		/// <param name="value">DataMap object to add</param>
		/// <returns></returns>
		public int Add (DataMap value) 
		{
			return List.Add (value);
		}

		/// <summary>
		/// Adds a new DataMap to collection
		/// </summary>
		/// <param name="physicalname">Physical table name</param>
		/// <param name="logicalname">Logical table name</param>
		/// <returns></returns>
		public int Add (string physicalname,string logicalname) 
		{
			return List.Add (new DataMap (physicalname,logicalname));
		}
		/// <summary>
		/// Retrieves the position of a element
		/// </summary>
		/// <param name="value">Element of which position is desired</param>
		/// <returns>Position of value (zero-based)</returns>
		public int IndexOf (DataMap value) 
		{
			return List.IndexOf (value);
		}
		/// <summary>
		/// Inserts an element at given position
		/// </summary>
		/// <param name="index">Position (zero-based) to insert new element</param>
		/// <param name="value">DataMap object to insert</param>
		public void Insert (int index, DataMap value) 
		{
			List.Insert (index, value);
		}
		/// <summary>
		/// Removes an element from collection
		/// </summary>
		/// <param name="value">DataMap object to remove</param>
		public void Remove (DataMap value)
		{
			List.Remove (value);
		}
		/// <summary>
		/// Checks if an object is contained whitin the collection
		/// </summary>
		/// <param name="value">DataMap object to check</param>
		/// <returns>Returns <code>true</code> if element is in the collection and <code>false</code> otherwise</returns>
		public bool Contains(DataMap value) 
		{
			return List.Contains (value);
		}
		//****************************************************************************************
		// Methods here are to force OPS.Components.Data.DataMap objects to be used in the collection
		//****************************************************************************************	
		protected override void OnRemove (int index, Object value) 
		{
			if (!(value is DataMap)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataMap object","value");
			}
		}
		protected override void OnInsert (int index, Object value) 
		{
			if (!(value is DataMap)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataMap object","value");
			}
		}
		protected override void OnSet (int index, Object oldValue, Object newValue) 
		{
			if (!(newValue is DataMap)) 
			{
				throw new ArgumentException ("newValue must be a OPS.Components.Data.DataMap object","newValue");
			}
		}
		protected override void OnValidate (Object value) 
		{
			if (!(value is DataMap)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataMap object","value");
			}
		}
	}
}
