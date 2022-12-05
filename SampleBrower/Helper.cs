using Mogre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleBrowser
{
    public static class Helper
    {
        public static StringVector ToStringVector<T>(this IEnumerable<T> list)
        {
            StringVector stringVector = new StringVector();

            foreach(var item in list)
            {
                stringVector.Add(item.ToString());
            }

            return stringVector;
        }

        public static T Clamp<T>(T val, T minval, T maxval) where T : IComparable
        {
            T temp;
            if (val.CompareTo(maxval) < 0)
            {
                temp = val;
            }
            else
            {
                temp = maxval;
            }
            if (temp.CompareTo(minval) > 0)
            {
                return temp;
            }
            else
            {
                return minval;
            }
        }
    }
}
