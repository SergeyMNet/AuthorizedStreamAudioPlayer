using System.Security.Cryptography;
using Xamarin.Forms;
using XamStreamPlayer.DataServices;
using XamStreamPlayer.Droid.Services;

[assembly: Dependency(typeof(CryptographyService))]
namespace XamStreamPlayer.Droid.Services
{
    public class CryptographyService : ICryptographyService
    {
        public byte[] Md5ComputeHash(byte[] content)
        {
            var hash = MD5.Create();
            var result = hash.ComputeHash(content);

            return result;
        }

        public byte[] HmacshaComputeHash(byte[] secretKeyByteArray, byte[] content)
        {
            HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray);
            var result = hmac.ComputeHash(content);

            return result;
        }
    }
}