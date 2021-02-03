using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDMHelpers.Extensions;
using Xunit;

namespace PDMHelpers.UnitTests
{
    public class EnumExtensionsTest
    {
        internal enum Prueba{
            Tipo1 = 1,
            Tipo2,
            Tipo3,
            Tipo4,
            Tipo5,
            Tipo6
        }

        [Fact]
        public void GetDescription_ReturnsEnumStringName() {
            Prueba tipo1 = Prueba.Tipo1;

            string actual = tipo1.GetDescription();
            string expected = "Tipo1";

            Assert.Equal(expected, actual);

        }

        [Fact]
        public void GetValueFromString_WhenValid_ReturnsEnumValue()
        {
            Type type = typeof(Prueba);

            int actual = type.GetValueFromString<int>("Tipo1");
            int expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetValueFromString_WhenNotValid_ThrowsArgumentOutOfRangeException()
        {
            Type type = typeof(Prueba);
            Action code = () => type.GetValueFromString<int>("NotValidValue");

            Assert.Throws<ArgumentOutOfRangeException>(code);
        }


        [Fact]
        public void ExistValueFromString_WhenValid_ReturnsTrue()
        {
            bool actual = typeof(Prueba).ExistValueFromString("Tipo1");

            Assert.True(actual);
        }

        [Fact]
        public void ExistValueFromString_WhenInvalid_ReturnsFalse()
        {
            bool actual = typeof(Prueba).ExistValueFromString("Tipo11");

            Assert.False(actual);
        }

    }
}
