using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiCore.Lib.Barcode
{
    internal static class BitHelper
    {
        public static uint GetHammingBitsCount(uint length)
        {
            var table = new Dictionary<uint[], uint>()
            {
                {new[] {1u, 1u}, 2u},
                {new[] {2u, 4u}, 3u},
                {new[] {5u, 11u}, 4u},
                {new[] {12u, 26u}, 5u},
                {new[] {27u, 57u}, 6u},
                {new[] {58u, 120u}, 7u},
                {new[] {121u, 245u}, 8u}

            };
            var key = table.Keys.FirstOrDefault(k => length >= k[0] && length <= k[1]);
            return key == null ? 8 : table[key];
        }

        public static bool[] GetMask(uint index, int length)
        {
            var period = index.Pow2();
            var result = new bool[length];
            for (var i = period - 1; i < length; i += 2*period)
            {
                for (var j = 0; j < period; j++)
                {
                    if (i + j < result.Length)
                        result[i + j] = true;
                }
            }
            return result;
        }

        private static uint Pow2(this uint n)
        {
            var result = 1u;
            for (var i = 0; i < n; i++)
            {
                result *= 2;
            }
            return result;
        }

        public static bool[] GetBits(this string data)
        {
            var bits = new[] {8, 4, 2, 1};
            return data
                .Select(c => byte.Parse(c.ToString(), NumberStyles.HexNumber))
                .SelectMany(c => bits.Select(b => (c & b) == b))
                .Reverse()
                .ToArray();
        }

        //public static bool[] ReverseOrder(this bool[] data)
        //{
        //    return data.Reverse().ToArray();
        //}

        private static bool IsPowerOfTwo(this int data)
        {
            return (data & (data - 1)) == 0;
        }

        public static bool[] InsertFakeBits(this bool[] data)
        {
            var fbCount = GetHammingBitsCount((uint) data.Length);
            var result = new bool[data.Length + fbCount];
            for (var i = 0; i < result.Length; i++)
            {
                var index = i + 1;
                if (index.IsPowerOfTwo())
                {
                    result[i] = false;
                    continue;
                }
                var shift = (int)Math.Ceiling(Math.Log(index, 2));
                result[i] = data[i - shift];
            }
            return result;
        }

        public static bool[] And(this bool[] data1, bool[] data2)
        {
            return data1.Zip(data2, (a, b) => a & b).ToArray();
        }

        public static bool XorBits(this IEnumerable<bool> data)
        {
            return data.Aggregate(false, (current, b) => current ^ b);
        }

        private static string ToHexPart(this bool[] data)
        {
            var val = 0u;
            for (uint i = 0; i < data.Length; i++)
            {
                val += data[i] ? i.Pow2() : 0;
            }
            return $"{val:X}";
        }

        public static string ToHex(this bool[] data)
        {
            var result = "";
            var tetrads = data.Length/4 + 1;
            for (var i = 0; i < tetrads; i++)
            {
                var item = data.Skip(i*4).Take(4).ToArray();
                result = item.ToHexPart() + result;
            }
            return result;
        }
    }
}