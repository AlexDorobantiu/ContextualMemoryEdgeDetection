using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.Utils
{
    public static class NumberUtils
    {
        // fnv hashing
        public const int FNV_offset_basis = unchecked((int)2166136261);
        public const int FNV_prime = 16777619;

        public static int fnvOneAtATimeHash(byte[] key, int len)
        {
            int hash = FNV_offset_basis;
            for (int i = 0; i < len; i++)
            {
                hash ^= key[i];
                hash *= FNV_prime;
            }
            return hash;
        }
        public static void fnvOneAtATimeHash(byte[] key, int len, int[] output)
        {
            int hash = FNV_offset_basis;
            for (int i = 0; i < len; i++)
            {
                hash ^= key[i];
                hash *= FNV_prime;
                output[i] = hash;
            }
        }

        public static int fnvOneAtATimeWithMaskHash(byte[] key, byte[] mask, int len)
        {
            int hash = FNV_offset_basis;
            for (int i = 0; i < len; i++)
            {
                hash ^= (key[i] & mask[i]);
                hash *= FNV_prime;
            }
            return hash;
        }

        public static void fnvOneAtATimeWithMaskHash(byte[] key, byte[] mask, int len, int[] output)
        {
            int hash = FNV_offset_basis;
            for (int i = 0; i < len; i++)
            {
                hash ^= (key[i] & mask[i]);
                hash *= FNV_prime;
                output[i] = hash;
            }
        }

        public static uint jenkinsOneAtATimeHash(byte[] key, int len)
        {
            uint hash, i;
            for (hash = i = 0; i < len; ++i)
            {
                hash += key[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        public static void jenkinsOneAtATimeHash(byte[] key, int len, int[] output)
        {
            uint hash, i;
            for (hash = i = 0; i < len; ++i)
            {
                hash += key[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);

                uint outputValue = (hash << 3);
                outputValue ^= (outputValue >> 11);
                outputValue += (outputValue << 15);
                output[i] = (int)outputValue;
            }
        }

        public static uint jenkinsOneAtATimeWithMaskHash(byte[] key, byte[] mask, int len)
        {
            uint hash = 0;
            for (int i = 0; i < len; i++)
            {
                hash += (uint)(key[i] & mask[i]);
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        public static int hash2(int a, int b)
        {
            int h =  a * 200002979 + b * 30005491;
            return (h ^ h >> 9 ^ a >> 2 ^ b >> 3);
        }
        public static int hash3(int a, int b, int c)
        {
            int h = a * 200002979 + b * 30005491 + c * 50004239;
            return h ^ h >> 9 ^ a >> 2 ^ b >> 3 ^ c >> 4;
        }

        const int moduloHashConstant = (int)(0.5 * (2.236067 - 1) * ((uint)1 << 31));
        public static uint moduloHash(int value, int bits)
        {
            uint temp = (uint)(value * moduloHashConstant);
            return (temp >> (32 - bits));
        }

        public static void rotate2dPointAroundOrigin(ref double x, ref double y, double angle)
        {
            double tempX = x;
            double tempY = y;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            x = cos * tempX - sin * tempY;
            y = sin * tempX + cos * tempY;
        }
    }
}
