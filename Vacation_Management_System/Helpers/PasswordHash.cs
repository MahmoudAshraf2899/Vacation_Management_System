using Microsoft.AspNet.Identity;
using System.Security.Cryptography;

namespace Vacation_Management_System.Helpers
{
    public class PasswordHash : IPasswordHasher
    {
        public const int SaltByteSize = 24;
        public const int HashByteSize = 24;
        public const int Pbkdf2Iterations = 1000;

        public const int IterationIndex = 0;
        public const int SaltIndex = 1;
        public const int Pbkdf2Index = 2;

        public string HashPassword(string password)
        {
            var csprng = new RSACryptoServiceProvider();
            var salt = new byte[SaltByteSize];             

            var hash = Pbkdf2(password, salt, Pbkdf2Iterations, HashByteSize);
            return Pbkdf2Iterations + ":" +
            Convert.ToBase64String(salt) + ":" +
            Convert.ToBase64String(hash);
        }

        public static string CreateHash(string password)
        {
            var csprng = new RNGCryptoServiceProvider();
            var salt = new byte[SaltByteSize];
            csprng.GetBytes(salt);

            var hash = Pbkdf2(password, salt, Pbkdf2Iterations, HashByteSize);
            return Pbkdf2Iterations + ":" +
            Convert.ToBase64String(salt) + ":" +
            Convert.ToBase64String(hash);
        }

        public PasswordVerificationResult VerifyHashedPassword(string correctHash, string password)
        {
            char[] delimiter = { ':' };
            var split = correctHash.Split(delimiter);
            var iterations = Int32.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);
            var hash = Convert.FromBase64String(split[Pbkdf2Index]);

            var testHash = Pbkdf2(password, salt, iterations, hash.Length);
            return HashEquals(hash, testHash);
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            char[] delimiter = { ':' };
            var split = correctHash.Split(delimiter);
            var iterations = Int32.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);
            var hash = Convert.FromBase64String(split[Pbkdf2Index]);

            var testHash = Pbkdf2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        private PasswordVerificationResult HashEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);

            if (diff == 0)
            {
                return PasswordVerificationResult.Success;
            }

            else
            {
                return PasswordVerificationResult.Failed;
            }
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations };
            return pbkdf2.GetBytes(outputBytes);
        }
    }
}
