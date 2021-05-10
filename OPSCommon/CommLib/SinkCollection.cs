using System;
using System.Collections;

namespace OPS.Comm.Common.Channel
{

	public interface IReadOnlySinkCollection
	{
		/// <summary>
		/// Gets the ISink object at index-th position, using [] notation 
		/// </summary>
		ISink this [int index] { get; }
		
		/// <summary>
		/// Retrieves the position of a element
		/// </summary>
		/// <param name="value">Element of which position is desired</param>
		/// <returns>Position of value (zero-based)</returns>
		int IndexOf (ISink value);
		
		/// <summary>
		/// Checks if an object is contained whitin the collection
		/// </summary>
		/// <param name="value">ISink object to check</param>
		/// <returns>true if element is in the collection and false otherwise</returns>
		bool Contains(ISink value);
	}
	/// <summary>
	/// A collection of ISink objects
	/// </summary>
	public class SinkCollection : CollectionBase, IReadOnlySinkCollection
	{
		internal SinkCollection() {}

		/// <summary>
		/// Allows access to an element using array-notation []
		/// </summary>
		public ISink this [int index] 
		{
			get 
			{
				return (ISink)(List[index]);
			}
			set 
			{
				List[index] = value;
			}
		}

		/// <summary>
		/// Adds a new ISink to collection
		/// </summary>
		/// <param name="value">ISink object to add</param>
		/// <returns>Index of ISink object</returns>
		internal int Add (ISink value) 
		{
			return List.Add (value);
		}

		/// <summary>
		/// Retrieves the position of a element
		/// </summary>
		/// <param name="value">Element of which position is desired</param>
		/// <returns>Position of value (zero-based)</returns>
		public int IndexOf (ISink value) 
		{
			return List.IndexOf (value);
		}

		/// <summary>
		/// Inserts an element at given position
		/// </summary>
		/// <param name="index">Position (zero-based) to insert new element</param>
		/// <param name="value">ISink object to insert</param>
		internal void Insert (int index, ISink value) 
		{
			List.Insert (index, value);
		}

		/// <summary>
		/// Removes an element from collection
		/// </summary>
		/// <param name="value">ISink object to remove</param>
		internal void Remove (ISink value)
		{
			List.Remove (value);
		}
		/// <summary>
		/// Checks if an object is contained whitin the collection
		/// </summary>
		/// <param name="value">ISink object to check</param>
		/// <returns>true if element is in the collection and false otherwise</returns>
		public bool Contains(ISink value) 
		{
			return List.Contains (value);
		}


		#region Methods here are to force ISink objects to be used in the collection
		
		protected override void OnRemove (int index, Object value) 
		{
			if (!(value is ISink)) 
			{
				throw new ArgumentException ("value must be a ISink object","value");
			}
		}
		protected override void OnInsert (int index, Object value) 
		{
			if (!(value is ISink)) 
			{
				throw new ArgumentException ("value must be a ISink object","value");
			}
		}
		protected override void OnSet (int index, Object oldValue, Object newValue) 
		{
			if (!(newValue is ISink)) 
			{
				throw new ArgumentException ("newValue must be a ISink object","newValue");
			}
		}
		protected override void OnValidate (Object value) 
		{
			if (!(value is ISink)) 
			{
				throw new ArgumentException ("value must be a ISink object","value");
			}
		}

		#endregion
	}
}
