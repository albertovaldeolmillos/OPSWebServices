using System;

namespace PDMHelpers
{
    public static class Guard
    {
        public static void IsNull(object obj, string parameterName, string message = null)
        {
            if (obj == null)
                throw new ArgumentNullException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }

        public static void IsNullOrEmptyString(string obj, string parameterName, string message = null)
        {
            if (String.IsNullOrEmpty(obj))
                throw new ArgumentNullException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }

        public static void IsUndefined(long obj, string parameterName, string message = null)
        {
            if (obj == GlobalDefs.DEF_UNDEFINED_VALUE)
                throw new ArgumentNullException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }
        public static void IsUndefined(int obj, string parameterName, string message = null)
        {
            if (obj == GlobalDefs.DEF_UNDEFINED_VALUE)
                throw new ArgumentNullException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }

        public static void IsInRange(int obj, int from , int to, string parameterName, string message = null)
        {
            if (obj >= from && obj <= to )
                throw new ArgumentOutOfRangeException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }
        public static void IsLessThan(int obj, int value, string parameterName, string message = null)
        {
            if (obj <= value)
                throw new ArgumentOutOfRangeException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }
        public static void IsGreaterThan(int obj, int value, string parameterName, string message = null)
        {
            if (obj >= value)
                throw new ArgumentOutOfRangeException(parameterName, message ?? $"Invalid input parameter {parameterName}");
        }
    }
}
