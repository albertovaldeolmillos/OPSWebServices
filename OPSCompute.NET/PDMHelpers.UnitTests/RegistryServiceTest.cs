using PDMHelpers;
using System;
using Xunit;

namespace PDMHelpers.UnitTests
{
    public class RegistryServiceTest
    {
        [Fact]
        public void ReadKeyValue_EmptyParams_ThrowsException()
        {
            RegistryService registry = new RegistryService();

            string keyName = string.Empty;
            string valueName = string.Empty;

            Action code = () => registry.ReadKeyValue(keyName, valueName);

            Assert.Throws<NullReferenceException>(code);
        }

        [Fact]
        public void ReadKeyValue_NotValidKey_ThrowsException()
        {
            RegistryService registry = new RegistryService();

            string keyName = @"SOFTWARE\OTS\OPSComputeBilbao2";
            string valueName = @"ConfigFile";

            Action code = () => registry.ReadKeyValue(keyName, valueName);

            Assert.Throws<NullReferenceException>(code);
        }

        [Fact]
        public void ReadKeyValue_NotValidValueName_ThrowsException()
        {
            RegistryService registry = new RegistryService();

            string keyName = @"SOFTWARE\OTS\OPSComputeBilbao";
            string valueName = @"InvalidValueName";

            Action code = () => registry.ReadKeyValue(keyName, valueName);
            Assert.Throws<NullReferenceException>(code);
        }

        [Fact]
        public void ReadKeyValue_WithValidParameters_ReturnStringPath() {
            string expected = @"c:\ops\becs\ParamCompute.xml";
            RegistryService registry = new RegistryService();

            string keyName = @"SOFTWARE\OTS\OPSComputeBilbao";
            string valueName = @"ConfigFile";

            string paramsFilePath = registry.ReadKeyValue(keyName, valueName);

            Assert.Equal(expected, paramsFilePath);
        }
    }
}
