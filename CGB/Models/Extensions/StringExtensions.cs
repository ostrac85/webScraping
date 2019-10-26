using System;

namespace CGB.Models.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static short ToInt16(this string value)
        {
            return Convert.ToInt16(value);
        }

        public static int ToInt32(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value);
        }

        public static long ToInt64(this string value)
        {
            return Convert.ToInt64(value);
        }

        public static bool ToBoolean(this string value)
        {
            switch (value.ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    throw new InvalidCastException("You can't cast that value to a bool!");
            }
        }
    }
}
