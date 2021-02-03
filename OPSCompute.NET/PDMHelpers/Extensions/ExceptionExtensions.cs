using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMHelpers
{
    public static class ExceptionExtensions
    {
        public static string ToLogString(this Exception error) {
            return $"{error.GetType().FullName} :: {error.Message}{Environment.NewLine}{error.StackTrace.ToString()}";
        }
    }
}
