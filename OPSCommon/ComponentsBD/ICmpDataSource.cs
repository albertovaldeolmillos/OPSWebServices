using System;
using System.Data;
using OPS.Components.Globalization;

namespace OPS.Components.Data
{
	/// <summary>
	/// Base Interface for all components
	/// Defines a set of functions that each Component who acts of a DataSource of a Infragistics' grid
	/// must define
	/// That allows the use of the components with the Helper classes of namespace OTS.Infragistics.* 
	/// Note that each class that implements IcmpDataSource ALSO implements IExecutant
	/// </summary>
	public interface ICmpDataSource : IExecutant
	{

		/// <summary>
		/// Gets the foreign data of the current Component (that is, data related by FK)
		/// That method creates DataTables and adds them to the DataSet passed. Also creates
		/// the DataRelation objects between the DataTables.
		/// The DataSet can contain each number of tables, but that method supposes that the
		/// table containing the data is Tables[0] (Tables(0) in VB.NET)
		/// </summary>
		/// <param name="ds">DataSet in which the tables will be added</param>
		void GetForeignData(DataSet ds);

		/// <summary>
		/// Gets the foreign data of the current Component (that is, data related by FK)
		/// That method creates DataTables and adds them to the DataSet passed. Also creates
		/// the DataRelation objects between the DataTables.
		/// </summary>
		/// <param name="ds">DataSet in which the tables will be added</param>
		/// <param name="sTable">Name of the table which contains the data to be related</param>
		void GetForeignData(DataSet ds, string sTable);

		/// <summary>
		/// Saves the data to the DataSource. The DataSet MUST contain a table named
		/// with the same name as property MainTable returns.
		/// </summary>
		/// <param name="ds">DataSet with a table named as property MainTable specifies, with data</param>
		void SaveData (DataSet ds);

		/// <summary>
		/// Saves the data to the DataSource.
		/// </summary>
		/// <param name="ds">DataSet with the data</param>
		/// <param name="table">Name of the DataTable which contain the data</param>
		void SaveData (DataSet ds, string table);

		/// <summary>
		/// Gets the name of the table who contains the main data (not the foreign data)
		/// </summary>
		string MainTable {get;}

		/// <summary>
		/// Gets the TOTAL number of registers
		/// </summary>
		long GetCount();

		/// <summary>
		/// Gets the total number of registers. That overload allows to apply a WHERE clausule
		/// </summary>
		/// <param name="where">Where clausule to apply</param>
		/// <param name="values">Values of the parameters referenced in the where clausule (if any)</param>
		/// <returns>The number of registers</returns>
		long GetCount(string where, params object[] values);

		/// <summary>
		/// Returns the value of the last PK (saved in the DataSource).
		/// Only applies if:
		///		1. PK is only one column
		///		2. PK is a numeric value
		///	If PK is more than one column or PK is a alphanumeric value that method must return -2.
		///	If PK is only one column, numeric but is autoincremental that method must return -1
		/// </summary>
		/// <returns>A Int64 (aka long) with the max value of PK, or -2 if the PK has to be set by the user or -1 if PK is autoinc</returns>
		Int64 LastPKValue {get;}


		/// <summary>
		/// Return a bool indicating if you can edit the primary key
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		bool GetPKReadOnly{get;}
	}

	/// <summary>
	/// That class implements some (but not all) methods of ICmpDataSource. The methods
	/// implemented allow a easy creation of ICmpDataSource implementing classes: all
	/// overloads are implemented in this class, and the derived class only have
	/// to implement the real-working methods (one overload per method) 
	/// </summary>
	public abstract class CmpDataSourceAdapter : CmpExecutantBase, ICmpDataSource
	{

				
		public void GetForeignData(DataSet ds)
		{
			GetForeignData (ds, ds.Tables[0].TableName);
		}


		public void SaveData (DataSet ds)
		{
			SaveData (ds.Tables[this.MainTable]);
		}

		public void SaveData (DataSet ds, string table)
		{
			SaveData (ds.Tables[table]);
		}

		public long GetCount()
		{
			return GetCount(null);

		}

		//********************************* Not implemented overloads
		//********************************* (must be implemented on derived-classes
		public abstract void GetForeignData(DataSet ds, string sTable);
		public abstract string MainTable  {get;} 
		public abstract long GetCount(string where, params object[] values);
		public abstract Int64 LastPKValue {get;}
		public abstract bool GetPKReadOnly{get;}
	}

	/// <summary>
	/// Means a component with data that can be globalized (that is, that uses a ResourceManager to
	/// translate some of its contents to different languages).
	/// </summary>
	public interface ICmpLocalizedDataSource : ICmpDataSource
	{
		/// <summary>
		/// Gets the foreign data of the current Component (that is, data related by FK)
		/// That method creates DataTables and adds them to the DataSet passed. Also creates
		/// the DataRelation objects between the DataTables.
		/// That method corresponds to ICmpDataSource::GetForeignData
		/// </summary>
		/// <param name="ds">DataSet in which the tables will be added</param>
		/// <param name="sTable">Name of the table which contains the data to be related</param>
		/// <param name="rm">Resource Manager to use (to globalize language-dependent data)</param>
		void GetForeignData(DataSet ds, string sTable, IResourceManager rm);

		/// <summary>
		/// Localizes the DataTable containing the MainData.
		/// For allowing easy implementation, the methods inherited from IExecutant (GetData and
		/// GetPagedData) are not overloaded. Instead this new method appears.
		/// The DataTable parameter is the DataTable obtained with GetData or GetPagedData.
		/// If method is called with any other DataTable unpredicable errors may (and will) occur.
		/// </summary>
		/// <param name="dt">DataTable obtained with a call to GetData or GetPagedData</param>
		/// <param name="rm">ResourceManager to use</param>
		void LocalizeMainData (DataTable dt, IResourceManager rm);

		
	}

	/// <summary>
	/// That class provides a base point of derivation for implementing ICmpLocalizedDataSource
	/// That class fully-implements the ICmpLocalizedDataSourceAdapter, but really DOES NOT 
	/// localize anything. That allows to implement non-localized components as localized (and
	/// thus all components are implementing ICmpLocalizedDataSource).
	/// </summary>
	public abstract class CmpLocalizedDataSourceAdapter : CmpDataSourceAdapter,ICmpLocalizedDataSource	
	{
		/// <summary>
		/// Gets the foreign data of the current Component (that is, data related by FK)
		/// That method creates DataTables and adds them to the DataSet passed. Also creates
		/// the DataRelation objects between the DataTables.
		/// That method simply calls ICmpDataSource::GetForeignData passing the DataSet and
		/// the table name to it, and discarding the ResourceManager object
		/// So, each class deriving of CmpLocalizedDataSourceAdapter, and NOT overriding
		/// that method will implement ICmpLocalizedDataSource interface, but calling that method
		/// will be EXACTLY the same, that call GetForeignData with only 2 firsts parameters. 
		/// </summary>
		/// <param name="ds">DataSet in which the tables will be added</param>
		/// <param name="sTable">Name of the table which contains the data to be related</param>		
		/// /// <param name="rm">IResourceManager object to use (not used in this implementation)</param>
		public virtual void GetForeignData(DataSet ds, string sTable, IResourceManager rm)
		{
			GetForeignData (ds, sTable);
		}

		/// <summary>
		/// Localizes the DataTable containing the MainData.
		/// For allowing easy implementation, the methods inherited from IExecutant (GetData and
		/// GetPagedData) are not overloaded. Instead this new method appears.
		/// The DataTable parameter is the DataTable obtained with GetData or GetPagedData.
		/// If method is called with any other DataTable unpredicable errors may (and will) occur.
		/// That implementation DOES NOTHING. So any class inherited from CmpLocalizedDataSourceAdapter
		/// that does not override that method will fully implement the ICmpLocalizedDataSource interface
		/// but calling that method will do nothing.
		/// </summary>
		/// <param name="dt">DataTable obtained with a call to GetData or GetPagedData</param>
		/// <param name="rm">ResourceManager to use</param>
		public virtual void LocalizeMainData (DataTable dt, IResourceManager rm)
		{
			// Nothing to do...
			return;
		}
		/*public  bool GetPKReadOnly
		{
			get
			{
				return false;
			}
		}*/

	}
}
