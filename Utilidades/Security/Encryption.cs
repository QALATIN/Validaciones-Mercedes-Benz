using System;
using System.Text;
using System.Security.Cryptography;
namespace LatinID.Utilidades.Security
{

    /// <summary>
    /// Algoritmo de encripción compatible con encripción en java
    /// </summary>
    public class Encryption
    {

        private static string password = string.Empty;
        private static string iv = string.Empty;
        private static string salt = string.Empty;

        public Encryption(string keyPass,string keySalt)
        {
            salt = keySalt;
            password = keyPass;
            iv = keyPass;
        }


        public static string Encrypt(string raw)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                ICryptoTransform e = GetCryptoTransform(csp, true);
                byte[] inputBuffer = Encoding.UTF8.GetBytes(raw);
                byte[] output = e.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);

                string encrypted = Convert.ToBase64String(output);

                return encrypted;
            }
        }

        public static string Decrypt(string encrypted)
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                var d = GetCryptoTransform(csp, false);
                byte[] output = Convert.FromBase64String(encrypted);
                byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);

                string decypted = Encoding.UTF8.GetString(decryptedOutput);
                return decypted;
            }
        }

        private static ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypting)
        {
            csp.Mode = CipherMode.CBC;
            csp.Padding = PaddingMode.PKCS7;
            // var passWord = System.Configuration.ConfigurationManager.AppSettings["psw_value"];
        //    var salt = "L@t1n1D@";
            //String iv = System.Configuration.ConfigurationManager.AppSettings["psw_value"];//"e675f725e675f725";
            var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), 65536);
            byte[] key = spec.GetBytes(16);

            csp.IV = Encoding.UTF8.GetBytes(iv);
            csp.Key = key;
            if (encrypting)
            {
                return csp.CreateEncryptor();
            }
            return csp.CreateDecryptor();
        }
    }
}