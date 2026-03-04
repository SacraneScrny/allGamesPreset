using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sackrany.Utils.Hash
{
    public static class XXHash32
    {
        private const uint PRIME32_1 = 2654435761U;
        private const uint PRIME32_2 = 2246822519U;
        private const uint PRIME32_3 = 3266489917U;
        private const uint PRIME32_4 = 668265263U;
        private const uint PRIME32_5 = 374761393U;
    
        public static uint XXHash(this object obj)
        {
            if (obj == null)
                return 0;

            string s = obj is string str ? str : obj.ToString();

            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return XXHash32.Hash(bytes);
        }
        public static uint XXHash(this string obj)
        {
            if (obj == null)
                return 0;

            byte[] bytes = Encoding.UTF8.GetBytes(obj);
            return XXHash32.Hash(bytes);
        }
    
        public static uint Hash(byte[] data, uint seed = 0)
        {
            int len = data.Length;
            int index = 0;
            uint hash;

            if (len >= 16)
            {
                uint v1 = seed + PRIME32_1 + PRIME32_2;
                uint v2 = seed + PRIME32_2;
                uint v3 = seed + 0;
                uint v4 = seed - PRIME32_1;

                int limit = len - 16;
                while (index <= limit)
                {
                    v1 = Round(v1, BitConverter.ToUInt32(data, index)); index += 4;
                    v2 = Round(v2, BitConverter.ToUInt32(data, index)); index += 4;
                    v3 = Round(v3, BitConverter.ToUInt32(data, index)); index += 4;
                    v4 = Round(v4, BitConverter.ToUInt32(data, index)); index += 4;
                }

                hash = Rotl(v1, 1) + Rotl(v2, 7) + Rotl(v3, 12) + Rotl(v4, 18);
            }
            else
            {
                hash = seed + PRIME32_5;
            }

            hash += (uint)len;

            while (index <= len - 4)
            {
                hash = Rotl(hash ^ (BitConverter.ToUInt32(data, index) * PRIME32_3), 17) * PRIME32_4;
                index += 4;
            }

            while (index < len)
            {
                hash = Rotl(hash ^ (data[index] * PRIME32_5), 11) * PRIME32_1;
                index++;
            }

            hash ^= hash >> 15;
            hash *= PRIME32_2;
            hash ^= hash >> 13;
            hash *= PRIME32_3;
            hash ^= hash >> 16;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Rotl(uint x, int r) => (x << r) | (x >> (32 - r));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Round(uint acc, uint input)
        {
            acc += input * PRIME32_2;
            acc = Rotl(acc, 13);
            acc *= PRIME32_1;
            return acc;
        }
    }
}
