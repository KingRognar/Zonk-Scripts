using UnityEngine;

namespace Combinations
{
    static class Combinations
    {
        static Combination ones3 = new Combination(new int[] { 1, 1, 1 }, 1000);
        static Combination ones4 = new Combination(new int[] { 1, 1, 1, 1 }, 2000);
        static Combination ones5 = new Combination(new int[] { 1, 1, 1, 1, 1 }, 4000);
        static Combination ones6 = new Combination(new int[] { 1, 1, 1, 1, 1, 1 }, 8000);
        static Combination twos3 = new Combination(new int[] { 2, 2, 2 }, 200);
        static Combination twos4 = new Combination(new int[] { 2, 2, 2, 2 }, 400);
        static Combination twos5 = new Combination(new int[] { 2, 2, 2, 2, 2 }, 800);
        static Combination twos6 = new Combination(new int[] { 2, 2, 2, 2, 2, 2 }, 1600);
        static Combination tres3 = new Combination(new int[] { 3, 3, 3 }, 300);
        static Combination tres4 = new Combination(new int[] { 3, 3, 3, 3 }, 600);
    }

    struct Combination
    {
        int[] comb;
        int value;

        public Combination(int[] _comb, int _value)
        {
            comb = _comb;
            value = _value;
        }
    }
}
