using PDMDatabase.MemoryDatabase;
using PDMHelpers;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using Xunit;

namespace IMT_DaysDefTest
{
    public class GetListTypeOfDaysShould
    {
        [Fact]
        public void ReturnNotEmptyListWithValidDate()
        {
            IMT_DaysDef daysDef = InitializeDayDefTable();
            daysDef.LoadData();

            List<long> tiposDeDia = daysDef.GetListTypeOfDays(new COPSDate(0, 0, 0, 09, 11, 2018));
            Assert.NotEmpty(tiposDeDia);
        }

        private IMT_DaysDef InitializeDayDefTable()
        {
            IDbConnection connection = Substitute.For<IDbConnection>();
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            FakeDaysDefRepository fakeDaysDefRepository = new FakeDaysDefRepository(connection);

            return  new IMT_DaysDef(loggerManager, fakeDaysDefRepository);
        }
    }
}
