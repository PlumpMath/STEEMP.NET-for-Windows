﻿using System;
using System.Linq;
using System.Numerics;

namespace STEEM.Helpers
{
    public class Base58
    {
        public const int CheckSumSizeInBytes = 4;
        protected const string Hexdigits = "0123456789abcdefABCDEF";
        private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static byte[] GetBytes(string data)
        {
            var rezult = TryGetBytes(data);
            if (rezult.Length > 0)
                return rezult;
            throw new NotImplementedException();
        }

        public static byte[] TryGetBytes(string data)
        {
            if (data.All(Hexdigits.Contains))
                return Hex.HexToBytes(data);

            switch (data[0])
            {
                case '5':
                case '6':
                    return Base58CheckDecode(data);
                case 'K':
                case 'L':
                    return CutLastBytes(Base58CheckDecode(data), 1);
            }
            return new byte[0];
        }

        private static byte[] CutLastBytes(byte[] source, int cutCount)
        {
            var rez = new byte[source.Length - cutCount];
            Array.Copy(source, rez, rez.Length);
            return rez;
        }

        private static byte[] CutFirstBytes(byte[] source, int cutCount)
        {
            var rez = new byte[source.Length - cutCount];
            Array.Copy(source, cutCount, rez, 0, rez.Length);
            return rez;
        }

        private static byte[] Base58CheckDecode(string data)
        {
            var s = Decode(data);
            var dec = CutLastBytes(s, 4);

            //var checksum = SHA256.Instance.DoubleHash(dec);
            //for (int i = 0; i < 4; i++)
            //    Assert.IsTrue(checksum[i] == s[s.Length - 4 + i]);

            return CutFirstBytes(dec, 1);
        }

        private static byte[] RemoveCheckSum(byte[] data)
        {
            var result = new byte[data.Length - CheckSumSizeInBytes];
            Buffer.BlockCopy(data, 0, result, 0, data.Length - CheckSumSizeInBytes);

            return result;
        }

        private static string Encode(byte[] data)
        {
            // Decode byte[] to BigInteger
            BigInteger intData = 0;
            for (var i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            // Encode BigInteger to Base58 string
            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(intData % 58);
                intData /= 58;
                result = Digits[remainder] + result;
            }

            // Append `1` for each leading 0 byte
            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }

        private static byte[] Decode(string base58)
        {
            // Decode Base58 string to BigInteger 
            BigInteger intData = 0;
            for (var i = 0; i < base58.Length; i++)
            {
                var digit = Digits.IndexOf(base58[i]); //Slow
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character `{base58[i]}` at position {i}");
                intData = intData * 58 + digit;
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading `1` characters
            var leadingZeroCount = base58.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros =
                intData.ToByteArray()
                .Reverse()// to big endian
                .SkipWhile(b => b == 0);//strip sign byte
            var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return result;
        }
    }
}