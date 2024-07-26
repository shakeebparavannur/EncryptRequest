using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EncryptRequest.Middleware
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly List<string> _excludeURLList;
        public EncryptionMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
            _excludeURLList = GetExcludeURLList();
        }
        public async Task Invoke(HttpContext context)
        {
            if (_excludeURLList.Contains(context.Request.Path.ToString()))
            {
                await _next(context);
                return;
            }
            context.Response.Body = EncryptStream(context.Response.Body);
            context.Request.Body = DecryptStream(context.Request.Body);
            if (context.Request.QueryString.HasValue)
            {
                string decryptedString = DecryptString(context.Request.QueryString.Value.Substring(1));
                context.Request.QueryString = new QueryString($"?{decryptedString}");
            }
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception and set appropriate status code
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
            finally
            {
                await context.Request.Body.DisposeAsync();
                await context.Response.Body.DisposeAsync();
            }
        }
        private static CryptoStream EncryptStream(Stream responseStream)
        {
            Aes aes = Aes.Create();

            ToBase64Transform base64Transform = new ToBase64Transform();
            CryptoStream base64EncodedStream = new CryptoStream(responseStream, base64Transform, CryptoStreamMode.Write);
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, encryptor, CryptoStreamMode.Write);

            return cryptoStream;
        }

        private Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_appSettings.EncryptionKey); // Ensure your key length matches AES requirements
            aes.IV = Encoding.UTF8.GetBytes(_appSettings.EncryptionIV);

            return aes;
        }
        private static Stream DecryptStream(Stream cipherStream)
        {
            Aes aes = Aes.Create();

            FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);
            return decryptedStream;
        }
        private static string DecryptString(string cipherText)
        {
            Aes aes = Aes.Create();
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
        public class AppSettings
        {
            public string EncryptionKey { get; set; }
            public string EncryptionIV { get; set; }
        }
    }
}
