using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PDMHelpers.UnitTests
{
    public class COPSDateTest
    {
        [Fact]
        public void EmptyCtor_SetStatusToNull()
        {
            COPSDate date = new COPSDate();

            COPSDateStatus expected = COPSDateStatus.Null;
            COPSDateStatus actual = date.GetStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AnyCtor_WhithValidValues_SetStatusToValid()
        {
            COPSDate date = new COPSDate(00, 00, 00, 06, 01, 1985);

            COPSDateStatus expected = COPSDateStatus.Valid;
            COPSDateStatus actual = date.GetStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValueProperty_WhithInvalidValues_SetStatusToInvalid()
        {
            COPSDate date = new COPSDate(DateTime.MaxValue.ToOADate() + 1);

            COPSDateStatus expected = COPSDateStatus.Invalid;
            COPSDateStatus actual = date.GetStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Set_WhithInvalidValues_SetStatusToInvalid()
        {
            COPSDate date = new COPSDate();

            COPSDateStatus expected = COPSDateStatus.Invalid;

            date.Set("000000000000");
            COPSDateStatus actual = date.GetStatus();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CopyToChar_IfValueIsSet_ReturnStringDate() {
            COPSDate date = new COPSDate(00, 00, 00, 06, 01, 1985);

            string expected = "000000060185";
            string actual = date.CopyToChar();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("010101060185", true)]
        [InlineData("010101900185", false)]
        public void IsValid_ReturnExpectedValues(string dateString, bool expected)
        {
            COPSDate date = new COPSDate();
            date.Set(dateString);

            bool actual = date.IsValid();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OperatorIsLessThan_WorksWell() {
            COPSDate data1 = new COPSDate(0, 0, 0, 1, 1, 2018);
            COPSDate data2 = new COPSDate(0, 0, 0, 1, 2, 2018);

            Assert.True(data1 < data2);
        }

        [Fact]
        public void OperatorIsGreaterThan_WorksWell()
        {
            COPSDate data1 = new COPSDate(0, 0, 0, 1, 2, 2018);
            COPSDate data2 = new COPSDate(0, 0, 0, 1, 1, 2018);

            Assert.True(data1 > data2);
        }

        [Fact]
        public void OperatorIsGreaterOrEqualThan_WorksWell()
        {
            COPSDate data1 = new COPSDate(0, 0, 0, 1, 2, 2018);
            COPSDate data2 = new COPSDate(0, 0, 0, 1, 1, 2018);

            Assert.True(data1 >= data2);
        }

        [Fact]
        public void OperatorIsLessOrEqualThan_WorksWell()
        {
            COPSDate data1 = new COPSDate(0, 0, 0, 1, 1, 2018);
            COPSDate data2 = new COPSDate(0, 0, 0, 1, 2, 2018);

            Assert.True(data1 <= data2);

        }
    }
}
