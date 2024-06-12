using System.Security.Cryptography;
using System.Text;

namespace ApiRequestMarket.Controllers;

internal class Hash
{
    private static readonly string salt = "8f4gasf6dg5ada@J*D@F!sajfn1!*591gas";
    /*public static string GenerateSalt()
    {
        var randomBytes = new byte[24];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }*/
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = string.Concat(password, salt); 
            var salterPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = sha256.ComputeHash(salterPasswordBytes);
            return Convert.ToBase64String(hash);
        }
    }
}