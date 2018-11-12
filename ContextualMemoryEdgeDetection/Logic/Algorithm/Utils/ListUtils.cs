using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.Utils
{
    public static class ListUtils
    {
        // remove seed for randomize
        public static Random rnd = new Random(123);

        public static void Shuffle<T>(IList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                swap(list, i, rnd.Next(i, list.Count));
            }
        }

        public static void swap<T>(IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static bool listNotContained<T>(List<T> currentList, List<List<T>> allLists)
        {
            foreach (List<T> list in allLists)
            {
                if (currentList.SequenceEqual(list))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
