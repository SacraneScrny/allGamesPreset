using System.Security.Cryptography;
using System.Text;

namespace Sackrany.Utils.Hash
{
    public static class StableHash
    {
        public static int Hash(this string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return System.BitConverter.ToInt32(hashBytes, 0);
            }
        }
    }
}