using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamStreamPlayer.DataServices
{
    public class CustomDelegatingHandler : HttpClientHandler
    {
        ICryptographyService cryptService = DependencyService.Get<ICryptographyService>();

        

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            var authenticationValue = await AuthenticationHeaderValue(request);

            request.Headers.Authorization = authenticationValue;

            response = await base.SendAsync(request, cancellationToken);
            return response;
        }

        private async Task<AuthenticationHeaderValue> AuthenticationHeaderValue(HttpRequestMessage request)
        {
            var requestContentBase64String = string.Empty;
            var requestUri = request.RequestUri.AbsoluteUri.ToLower();
            var requestHttpMethod = request.Method.Method;

            // Calculate UNIX time
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - epochStart;
            var requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            // Create random nonce for each request
            var nonce = Guid.NewGuid().ToString("N");

            // Check if the request contains body, usually will be null with HTTP GET or DELETE
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsByteArrayAsync();
                // Hash the request body, any change in request body will result in the different hash
                var requestContentHash = cryptService.Md5ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            // Create the raw signature string
            var signatureRawData = string.Format("{0}{1}{2}{3}{4}{5}", GlobalSettings.AppId, requestHttpMethod, requestUri, requestTimeStamp,
                nonce, requestContentBase64String);
            var secretKeyByteArray = Convert.FromBase64String(GlobalSettings.AppKey);
            var signature = Encoding.UTF8.GetBytes(signatureRawData);

            // Apply hashing algorithm using the APP Key
            var signatureBytes = cryptService.HmacshaComputeHash(secretKeyByteArray, signature);
            var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

            // Set the values in the Authorization header using custom scheme (amx)
            var authenticationValue = new AuthenticationHeaderValue("amx",
                string.Format("{0}:{1}:{2}:{3}", GlobalSettings.AppId, requestSignatureBase64String, nonce, requestTimeStamp));
            return authenticationValue;
        }

        // Get Token for other requests
        public async Task<string> GetToken(string url)
        {
            try
            {
                var httpRequest = new HttpRequestMessage();
                httpRequest.RequestUri = new Uri(url);
                httpRequest.Method = HttpMethod.Get;
                var header = await AuthenticationHeaderValue(httpRequest);

                return $"{header.Scheme} {header.Parameter}";
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }

    public interface ICryptographyService
    {
        byte[] Md5ComputeHash(byte[] content);
        byte[] HmacshaComputeHash(byte[] secretKeyByteArray, byte[] content);
    }
}
