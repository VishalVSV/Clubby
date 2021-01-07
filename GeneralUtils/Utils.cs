using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.GeneralUtils
{

    public static class Utils
    {
        public delegate void Initializer<T>(ref T t);

        public static T Init<T>(Initializer<T> initializer)
            where T : struct
        {
            T t = new T();
            initializer(ref t);
            return t;
        }

        public static int Max(params int[] nums)
        {
            int max = int.MinValue;
            for (int i = 0; i < nums.Length; i++)
            {
                max = Math.Max(max, nums[i]);
            }

            return max;
        }
    }
}
