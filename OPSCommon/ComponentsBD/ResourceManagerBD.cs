using System;
using System.Data;
using System.Configuration;


namespace OPS.Components.Data
{
	/// <summary>
	/// Loads a subset of resources of a specific Culture and Logical Unit.
	/// </summary>
	public class ResourceManagerDB
	{
		private const int PDA_LIMIT = 1000000;

		public ResourceManagerDB() { }

		public DataSet LoadResources(string culture, int logicalUnit)
		{
			Database d = DatabaseFactory.GetDatabase();
			object[] values = {culture, "FIELD", "STATICS", "MESSAGES", logicalUnit};
			return d.FillDataSet("SELECT LIT_ID, LIT_DESCLONG FROM LITERALS "
				+ "WHERE LIT_LAN_ID = @LITERALS.LIT_LAN_ID@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_DLUNI_ID = @LITERALS.LIT_DLUNI_ID@", "Literales", values);
		}

		public DataSet LoadMenuResources(string culture, int logicalUnit)
		{
			Database d = DatabaseFactory.GetDatabase();
			object[] values = {culture, "FIELD", "STATICS", "MESSAGES", logicalUnit};
			return d.FillDataSet("SELECT LIT_ID, LIT_DESCSHORT, LIT_DESCLONG FROM LITERALS "
				+ "WHERE LIT_LAN_ID = @LITERALS.LIT_LAN_ID@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_CATEGORY <> @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_DLUNI_ID = @LITERALS.LIT_DLUNI_ID@", "Literales", values);
		}

		public DataSet LoadFieldResources(string culture, int logicalUnit)
		{
			Database d = DatabaseFactory.GetDatabase();
			object[] values = {culture, "FIELD", logicalUnit};
			return d.FillDataSet("SELECT UPPER(LIT_DESCSHORT), LIT_DESCLONG FROM LITERALS "
				+ "WHERE LIT_LAN_ID = @LITERALS.LIT_LAN_ID@ "
				+ "AND LIT_CATEGORY = @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_DLUNI_ID = @LITERALS.LIT_DLUNI_ID@", "LiteralesField", values);
		}

		public DataSet LoadStaticResources(string culture, int logicalUnit)
		{
			Database d = DatabaseFactory.GetDatabase();
			object[] values = {culture, "STATICS", logicalUnit};
			return d.FillDataSet("SELECT UPPER(LIT_DESCSHORT), LIT_DESCLONG FROM LITERALS "
				+ "WHERE LIT_LAN_ID = @LITERALS.LIT_LAN_ID@ "
				+ "AND LIT_CATEGORY = @LITERALS.LIT_CATEGORY@ "
				+ "AND LIT_DLUNI_ID = @LITERALS.LIT_DLUNI_ID@", "LiteralesField", values);
		}

		public DataSet LoadMessageBoxResources(string culture, int logicalUnit)
		{
			Database d = DatabaseFactory.GetDatabase();
			object[] values = {culture, "VIEWS", "MESSAGES", "MODULES", logicalUnit};
			return d.FillDataSet("SELECT LIT_ID, LIT_DESCSHORT, LIT_DESCLONG FROM LITERALS "
				+ "WHERE LIT_LAN_ID = @LITERALS.LIT_LAN_ID@ "
				+ "AND (LIT_CATEGORY = @LITERALS.LIT_CATEGORY@ "
					+ "OR LIT_CATEGORY = @LITERALS.LIT_CATEGORY@ "
					+ "OR LIT_CATEGORY = @LITERALS.LIT_CATEGORY@) "
				+ "AND LIT_DLUNI_ID = @LITERALS.LIT_DLUNI_ID@", "LiteralesField", values);
		}
	}
}
