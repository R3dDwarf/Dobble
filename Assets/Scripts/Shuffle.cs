using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    static class Shuffle 
    {
        private static Random rng = new Random();

        public static void ShuffleFunc<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                (list[i], list[k]) = (list[k], list[i]); 
            }
        }
    }
}
