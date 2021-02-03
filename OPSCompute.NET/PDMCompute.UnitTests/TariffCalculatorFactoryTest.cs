using Xunit;
using PDMCompute;
using PDMHelpers;
using NSubstitute;

namespace PDMCompute.UnitTests
{
    public class TariffCalculatorFactoryTest
    {
        [Theory]
        [InlineData("", null)]
        [InlineData("INVALID_TYPE", null)]
        public void CreateTariffCalculator_InvalidString_ReturnNull(string input, object expected)
        {
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            var current = TariffCalculatorFactory.CreateTariffCalculator(loggerManager, input);

            Assert.True(expected == current);
        }

        [Fact]
        public void CreateTariffCalculator_ValidStringParameter_ReturnTariffCalculator()
        {
            int expected = 0;
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            var current = TariffCalculatorFactory.CreateTariffCalculator(loggerManager, "OTS");

            Assert.Equal(expected, current.Type);
        }

        [Fact]
        public void CreateTariffCalculator_InvalidTypeParameter_ReturnNull()
        {
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            var current = TariffCalculatorFactory.CreateTariffCalculator(loggerManager, null);

            Assert.Null(current);
        }

        [Theory]
        [InlineData("OTS", typeof(M1ComputeEx0))]
        [InlineData("OTS_0", typeof(M1ComputeEx0))]
        [InlineData("OTS_1", typeof(M1ComputeEx1))]
        public void CreateTariffCalculator_Always_ReturnCorrectType(string tariffType, System.Type type)
        {
            ILoggerManager loggerManager = Substitute.For<ILoggerManager>();
            var current = TariffCalculatorFactory.CreateTariffCalculator(loggerManager, tariffType);

            Assert.IsType(type, current);
        }
    }
}
