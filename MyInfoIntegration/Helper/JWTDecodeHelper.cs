using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MyInfoIntegration.Helper
{
    public class JWTDecodeHelper
    {
        public static string Decode(string token, byte[] keyBytes)
        {
            return Decode(token, keyBytes, true);
        }

        public static string Decode(string token, byte[] keyBytes, bool verify)
        {
            var parts = token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            byte[] crypto = Base64UrlDecode(parts[2]);

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var payloadData = JObject.Parse(payloadJson);

            if (verify)
            {
                var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));

                var certificate = new X509Certificate2(keyBytes);
                var publicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

                if (publicKey.VerifyData(bytesToSign, "2.16.840.1.101.3.4.2.1", crypto))
                {
                    return payloadData.ToString();
                }

                throw new ApplicationException("Invalid signature.");
            }
            return payloadData.ToString();
        }

        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }
}