using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EncryptRequest.Middleware
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly Appsettings _appsettings;
        public EncryptionMiddleware(RequestDelegate next)
        {
            _next = next;
            
        }
        public async Task Invoke(HttpContext context)
        {
            context.Response.Body = EncryptStream(context.Response.Body);
            context.Request.Body = DecryptStream(context.Request.Body);
            if (context.Request.QueryString.HasValue)
            {
                string decryptedString = DecryptString(context.Request.QueryString.Value.Substring(1));
                context.Request.QueryString = new QueryString($"?{decryptedString}");
            }
            await _next(context);
            await context.Request.Body.DisposeAsync();
            await context.Response.Body.DisposeAsync();
        }
        private static CryptoStream EncryptStream(Stream responseStream)
        {
            Aes aes = GetEncryptionAlgorithm();

            ToBase64Transform base64Transform = new ToBase64Transform();
            CryptoStream base64EncodedStream = new CryptoStream(responseStream, base64Transform, CryptoStreamMode.Write);
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, encryptor, CryptoStreamMode.Write);

            return cryptoStream;
        }

        private static Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            aes.Key = secret_key;
            aes.IV = initialization_vector;

            return aes;
        }
        private static Stream DecryptStream(Stream cipherStream)
        {
            Aes aes = GetEncryptionAlgorithm();

            FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);
            return decryptedStream;
        }
        private static string DecryptString(string cipherText)
        {
            Aes aes = GetEncryptionAlgorithm();
            byte[] buffer = Convert.FromBase64String(cipherText);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

     
       

        private List<string> GetExcludeURLList()
        {
            List<string> excludeURL = new List<string>();
            excludeURL.Add("/api/Common/commonFileuploaddata");
            excludeURL.Add("/api/Users/UploadProfilePicture");
            excludeURL.Add("/api/Common/downloadattachedfile");
            return excludeURL;
        }
    }
}
