using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using NSubstitute;
using PDMDatabase.Repositories;
using System.Data;
using PDMDatabase.UnitTests.Fakes;
using PDMDatabase.MemoryDatabase;
using PDMHelpers;

namespace PDMDatabase.UnitTests.IMT_ConstraintsUT
{
    public class GetConstraintsShould
    {
        [Fact]
        public void ReturnEmptyList_WhenConstraintIdNotFoundInDatabase()
        {
            IMT_Constraints constraintsTable = BuildConstraintsTable();
            constraintsTable.LoadData();

            var constraintsList = constraintsTable.GetConstraints(-888);

            Assert.Empty(constraintsList);
        }

        [Fact]
        public void ReturnList_WhenConstraintIdNotFoundInDatabase()
        {
            IMT_Constraints constraintsTable = BuildConstraintsTable();
            constraintsTable.LoadData();

            var constraintsList = constraintsTable.GetConstraints(60101);

            Assert.NotEmpty(constraintsList);
        }

        [Fact(Skip = "true")]
        public void ThrowAnException_WhenConstraintIdNotFoundInDatabase()
        {
            IMT_Constraints constraintsTable = BuildConstraintsTable();
            constraintsTable.LoadData();

            Action codeToTest = () => constraintsTable.GetConstraints(-888);

            Assert.Throws<InvalidOperationException>(codeToTest);
        }


        private IMT_Constraints BuildConstraintsTable()
        {
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            IDbConnection connection = Substitute.For<IDbConnection>();
            FakeConstraintsRepository repository = new FakeConstraintsRepository(connection);
            IMT_Constraints constraintsTable = new IMT_Constraints(loggerManager, repository);
            return constraintsTable;
        }
    }
}
