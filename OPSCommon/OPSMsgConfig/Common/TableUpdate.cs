using System;
using System.Data;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// Provides methods for updating tables with the data received from
	/// replications
	/// </summary>
	public class TableUpdate
	{
		#region Public API

		/// <summary>
		/// Constructor taking the database connection needed in order
		/// to update a table
		/// </summary>
		/// <param name="connection">The connection to a database</param>
		public TableUpdate(IDbConnection connection)
		{
			_connection = connection;
		}
		public string TableName
		{
			get { return _tableName; }
			set { _tableName = value; }
		}
		/// <summary>
		/// The prefix added to the names of the table fields
		/// </summary>
		public string FieldPrefix
		{
			get { return _fieldPrefix; }
			set { _fieldPrefix = value; }
		}
		/// <summary>
		/// The comma separated list of the fields to update
		/// </summary>
		public string Fields
		{
			get { return _fieldList; }
			set { _fieldList = value; }
		}
		/// <summary>
		/// The comma separated list of the values to put into the table
		/// </summary>
		public string Values
		{
			get { return _valueList; }
			set { _valueList = value; }
		}
		/// <summary>
		/// Updates the table with the values set in the Values property
		/// </summary>
		public void Update()
		{
			UpdateWithCurrentValues();
		}
		/// <summary>
		/// Sets as valid the records inserted as non-valid
		/// </summary>
		public void Validate()
		{
			ValidateNewValues();
		}
		/// <summary>
		/// Removes from the table the records set as non-valid
		/// </summary>
		public void DeleteNonValid()
		{
			DeleteNonValidValues();
		}

		#endregion // Public API

		#region Overridables

		/// <summary>
		/// Performs the necessary operations to insert the current values
		/// </summary>
		/// <remarks>Override in derived classes</remarks>
		protected virtual void UpdateWithCurrentValues()
		{
		}
		/// <summary>
		/// Performs the necessary operations to setting as valid the 
		/// records inserted as non-valid
		/// </summary>
		/// <remarks>Override in derived classes</remarks>
		protected virtual void ValidateNewValues()
		{
		}
		/// <summary>
		/// Performs the necessary operations to removing from the 
		/// table the records set as non-valid
		/// </summary>
		/// <remarks>Override in derived classes</remarks>
		protected virtual void DeleteNonValidValues()
		{
		}

		#endregion // Overridables

		#region Private data members

		/// <summary>
		/// The database connection
		/// </summary>
		protected IDbConnection _connection;
		/// <summary>
		/// The name of the table to update
		/// </summary>
		protected string _tableName;
		/// <summary>
		/// The prefix added to the names of the table fields
		/// </summary>
		protected string _fieldPrefix;
		/// <summary>
		/// The comma separated list of the fields to update
		/// </summary>
		protected string _fieldList;
		/// <summary>
		/// The comma separated list of the values to put into the table
		/// </summary>
		protected string _valueList;

		#endregion // Private data members
	}
}
