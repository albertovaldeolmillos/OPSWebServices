using System;
using System.Collections;

namespace OPS.Components.Data
{
	/// <summary>
	/// Mantains a collection of DataEntry objects
	/// </summary>
	public class DataEntryCollection : CollectionBase 
	{	
		public DataEntryCollection()
		{
		}

		/// <summary>
		/// Allows access to an element using array-notation []
		/// </summary>
		public DataEntry this [int index] 
		{
			get 
			{
				return (DataEntry)(List[index]);
			}
			set 
			{
				List[index] = value;
			}
		}
		/// <summary>
		/// Allow access to an element indexed by logic name (read-only indexer)
		/// </summary>
		public DataEntry this [string logicname]
		{
			get 
			{
				IEnumerator ie=List.GetEnumerator();
				while (ie.MoveNext()) 
				{
					if (((DataEntry)ie.Current).LogicName.Equals (logicname)) 
					{
						return (DataEntry)ie.Current;
					}
				}
				throw new ArgumentOutOfRangeException ("indexer",logicname,"DataEntry not found");
			}
		}

		/// <summary>
		/// Adds a new DataEntry to the collection
		/// </summary>
		/// <param name="colname">Physical Column name</param>
		/// <param name="logicname">Logical Column name</param>
		/// <param name="type">DbType of column</param>
		/// <returns></returns>
		public int Add (string colname, string logicname, System.Data.DbType type)
		{
			return List.Add (new DataEntry(colname, logicname, type));
		}
		/// <summary>
		/// Adds a new DataEntry to the collection
		/// </summary>
		/// <param name="colname">Physical Column name</param>
		/// <param name="logicname">Logical Column name</param>
		/// <param name="type">DbType of column</param>
		/// <param name="size">Size (in bytes) of the column type</param>
		/// <returns></returns>
		public int Add (string colname, string logicname, System.Data.DbType type, int size) 
		{
			return List.Add (new DataEntry(colname, logicname, type, size));
		}

		/// <summary>
		/// Adds a new DataEntry to collection
		/// </summary>
		/// <param name="value">DataEntry object to add</param>
		/// <returns></returns>
		public int Add (DataEntry value) 
		{
			return List.Add (value);
		}
		/// <summary>
		/// Retrieves the position of a element
		/// </summary>
		/// <param name="value">Element of which position is desired</param>
		/// <returns>Position of value (zero-based)</returns>
		public int IndexOf (DataEntry value) 
		{
			return List.IndexOf (value);
		}
		/// <summary>
		/// Inserts an element at given position
		/// </summary>
		/// <param name="index">Position (zero-based) to insert new element</param>
		/// <param name="value">DataEntry object to insert</param>
		public void Insert (int index, DataEntry value) 
		{
			List.Insert (index, value);
		}
		/// <summary>
		/// Removes an element from collection
		/// </summary>
		/// <param name="value">DataEntry object to remove</param>
		public void Remove (DataEntry value)
		{
			List.Remove (value);
		}
		/// <summary>
		/// Checks if an object is contained whitin the collection
		/// </summary>
		/// <param name="value">DataEntry object to check</param>
		/// <returns>Returns <code>true</code> if element is in the collection and <code>false</code> otherwise</returns>
		public bool Contains(DataEntry value) 
		{
			return List.Contains (value);
		}
		//****************************************************************************************
		// Methods here are to force OPS.Components.Data.DataEntry objects to be used in the collection
		//****************************************************************************************	
		protected override void OnRemove (int index, Object value) 
		{
			if (!(value is DataEntry)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataEntry object","value");
			}
		}
		protected override void OnInsert (int index, Object value) 
		{
			if (!(value is DataEntry)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataEntry object","value");
			}
		}
		protected override void OnSet (int index, Object oldValue, Object newValue) 
		{
			if (!(newValue is DataEntry)) 
			{
				throw new ArgumentException ("newValue must be a OPS.Components.Data.DataEntry object","newValue");
			}
		}
		protected override void OnValidate (Object value) 
		{
			if (!(value is DataEntry)) 
			{
				throw new ArgumentException ("value must be a OPS.Components.Data.DataEntry object","value");
			}
		}
	}
}
