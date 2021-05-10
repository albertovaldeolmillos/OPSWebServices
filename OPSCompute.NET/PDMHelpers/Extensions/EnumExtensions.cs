using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PDMHelpers.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return enumValue.ToString();
        }

        public static bool ExistValueFromString(this Type enumType, string input)
        {
            try
            {
                object o = Enum.Parse(enumType, input, true);
                return true;
            }
            catch (ArgumentException error)
            {
                return false;
            }
        }

        /// <exception cref="ArgumentOutOfRangeException">Si no se encuentra el valor devuelve exception</exception>
        public static T GetValueFromString<T>(this Type enumType, string input)
        {
            try
            {
                return (T)Enum.Parse(enumType, input, true);
            }
            catch (ArgumentException error)
            {
                throw new ArgumentOutOfRangeException(error.Message, error);
            }
        }

        public static bool IsIn<T>(this T @this, params T[] possibles) where T : struct, IConvertible
        {
            return possibles.Contains(@this);
        }
    }
}
