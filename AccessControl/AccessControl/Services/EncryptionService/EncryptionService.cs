using System.Security.Cryptography;
using AccessControl.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(EncryptionService))]
namespace AccessControl.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

        public byte[] Encrypt(byte[] source)
        {
            if (source == null)
                return null;

            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.CBC;
                tdes.IV = new byte[tdes.BlockSize / 8];
                tdes.Padding = PaddingMode.None;

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(source, 0, source.Length);
                tdes.Clear();

                return resultArray;
            }
        }

        public byte[] Decrypt(byte[] encoded)
        {
            if (encoded == null)
                return null;

            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.CBC;
                tdes.IV = new byte[tdes.BlockSize / 8];
                tdes.Padding = PaddingMode.None;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(encoded, 0, encoded.Length);
                tdes.Clear();

                return resultArray;
            }
        }
    }
}
