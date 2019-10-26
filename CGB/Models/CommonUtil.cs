using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.Models
{
    public static class CommonUtil
    {
        public static string Reverse(this string source)
        {
            char[] charArray = source.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
