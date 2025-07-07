using System;
using System.Linq;

namespace com.ktgame.services.iap
{
    public static class Obfuscator
    {
        public class InvalidOrderArray : Exception { }

        public static byte[] Obfuscate(byte[] data, int[] order, out int rkey)
        {
            var rnd = new Random();
            var key = rnd.Next(2, 255);
            var res = new byte[data.Length];
            var slices = data.Length / 20 + 1;

            if (order == null || order.Length < slices)
            {
                throw new InvalidOrderArray();
            }

            Array.Copy(data, res, data.Length);
            for (var i = 0; i < slices - 1; i++)
            {
                var j = rnd.Next(i, slices - 1);
                order[i] = j;
                var sliceSize = 20; // prob should be configurable
                var tmp = res.Skip(i * 20).Take(sliceSize).ToArray(); // tmp = res[i*20 .. slice]
                Array.Copy(res, j * 20, res, i * 20, sliceSize); // res[i] = res[j*20 .. slice]
                Array.Copy(tmp, 0, res, j * 20, sliceSize); // res[j] = tmp
            }

            order[slices - 1] = slices - 1;

            rkey = key;
            return res.Select(x => (byte)(x ^ key)).ToArray();
        }

        public static byte[] DeObfuscate(byte[] data, int[] order, int key)
        {
            var res = new byte[data.Length];
            int slices = data.Length / 20 + 1;
            bool hasRemainder = data.Length % 20 != 0;

            Array.Copy(data, res, data.Length);
            for (int i = order.Length - 1; i >= 0; i--)
            {
                var j = order[i];
                int sliceSize = (hasRemainder && j == slices - 1) ? (data.Length % 20) : 20;
                var tmp = res.Skip(i * 20).Take(sliceSize).ToArray(); // tmp = res[i*20 .. slice]
                Array.Copy(res, j * 20, res, i * 20, sliceSize); // res[i] = res[j*20 .. slice]
                Array.Copy(tmp, 0, res, j * 20, sliceSize); // res[j] = tmp
            }

            return res.Select(x => (byte)(x ^ key)).ToArray();
        }
    }
}