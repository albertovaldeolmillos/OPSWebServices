using PDMHelpers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace PDMHelpers.UnitTests
{

    public class LoggerManager : ILoggerManager
    {
        public ITraceable CreateTracer(Type type)
        {
            return new TracerFake();
        }
        
    }
    public class TracerFake : ITraceable
    {
        public ILoggerManager Creator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Write(TraceLevel level, string trace)
        {
          
        }

        public void Write(TraceLevel level, string context, string trace)
        {
           
        }

        public void Write(TraceLevel level, string context, string trace, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
        }
    }

    public class ParamLoaderTest
    {
        private static ParamLoader MakeParamLoader()
        {
            return new ParamLoader(new LoggerManager());
        }

        [Fact]
        public void LoadParams_EmptyOrNullFilepath_ReturnFalse()
        {
            ParamLoader paramLoader = MakeParamLoader();

            bool result = paramLoader.LoadParams(null);

            Assert.False(result);
        }

        [Fact]
        public void LoadParams_NotExistFile_ReturnFalse()
        {
            ParamLoader paramLoader = MakeParamLoader();

            bool result = paramLoader.LoadParams(null);

            Assert.False(result);
        }

        [Fact]
        public void LoadParams_ValidFile_LoadParams()
        {
            ParamLoader paramLoader = MakeParamLoader();

            bool result = paramLoader.LoadParams(@"C:\ops\becs\ParamCompute.xml");

            Assert.True(result);
        }

        [Fact]
        public void GetParam_NotValidParamName_ReturnsNull()
        {
            ParamLoader paramLoader = MakeParamLoader();
            paramLoader.LoadParams(@"C:\ops\becs\ParamCompute.xml");
            var value = paramLoader.GetParam("MAIKEL");

            Assert.Null(value);
        }

        [Fact]
        public void GetParam_ValidParamName_ReturnsValue()
        {
            ParamLoader paramLoader = MakeParamLoader();
            paramLoader.LoadParams(@"C:\ops\becs\ParamCompute.xml");
            var value = paramLoader.GetParam("DB_USER");

            Assert.NotNull(value);
        }
    }
}
