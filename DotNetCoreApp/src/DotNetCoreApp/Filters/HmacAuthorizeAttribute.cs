using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCoreApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace DotNetCoreApp.Filters
{
    public class HmacAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private static Dictionary<string, string> allowedApps = new Dictionary<string, string>();
        private readonly ulong requestMaxAgeInSeconds = 300; //5 mins
        private readonly string authenticationScheme = "amx";
        private MemoryStream ms;

        static HmacAuthorizeAttribute()
        {
            if (allowedApps.Count == 0)
            {
                // TODO: use your secret Keys
                allowedApps.Add("12b901a3-fb5c-457c-8d55-eb26748df1a2", "UAszSt1DJyAjoKg2VjZBjWOyzlKTp33v5QkMLhwxRp7=");
            }
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            return AuthenticateAsync(context, CancellationToken.None);
        }

        public Task AuthenticateAsync(AuthorizationFilterContext context, CancellationToken cancellationToken)
        {
            if (CheckUser(context))
                return Task.FromResult(0);

            var req = context.HttpContext.Request;
            var authzHeader = req.Headers["Authorization"];
            LogService.SaveLogs("Authorization: " + authzHeader);

            if (authzHeader.Count != 0 && authenticationScheme.Equals(authzHeader[0].Split(' ')[0], StringComparison.OrdinalIgnoreCase))
            {
                var rawAuthzHeader = authzHeader[0].Split(' ')[1];
                var authzHeaderArray = GetAuthorizationHeaderValues(rawAuthzHeader);

                if (authzHeaderArray != null)
                {
                    var AppId = authzHeaderArray[0];
                    var incomingBase64Signature = authzHeaderArray[1];
                    var nonce = authzHeaderArray[2];
                    var requestTimeStamp = authzHeaderArray[3];
                    var isValid = IsValidRequest(req, AppId, incomingBase64Signature, nonce, requestTimeStamp);

                    if (isValid.Result)
                    {
                        // Original request stream can not re-read itself
                        ms.Position = 0;
                        req.Body = ms;
                    }
                    else
                    {
                        context.Result = new UnauthorizedResult();
                    }
                }
                else
                {
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }

            return Task.FromResult(0);
        }

        // check user Authentication
        private bool CheckUser(AuthorizationFilterContext context)
        {
            try
            {
                return
                    context.HttpContext.Request.HttpContext.Authentication.HttpContext.User.Identity.IsAuthenticated;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string[] GetAuthorizationHeaderValues(string rawAuthzHeader)
        {
            var array = rawAuthzHeader.Split(':');

            if (array.Length == 4)
            {
                return array;
            }
            else
            {
                return null;
            }
        }

        private async Task<bool> IsValidRequest(HttpRequest req, string AppId, string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            // If appId is absent - invalid request
            if (!allowedApps.ContainsKey(AppId))
            {
                LogService.SaveLogs("Authorization IsContainsKey");
                return false;
            }

            // If request is reply - invalid request
            if (IsReplyRequest(nonce, requestTimeStamp))
            {
                LogService.SaveLogs("Authorization IsReplyRequest");
                return false;
            }

            var requestContentBase64String = string.Empty;
            var requestUri = string.Format("{0}://{1}{2}{3}", req.Scheme, req.Host.ToUriComponent(), req.Path.ToUriComponent(), req.QueryString.ToUriComponent()).ToLower();
            var requestHttpMethod = req.Method;
            var sharedKey = allowedApps[AppId];
            byte[] hash = await ComputeHash(req.Body);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            var data = string.Format("{0}{1}{2}{3}{4}{5}", AppId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);
            var secretKeyByteArray = Convert.FromBase64String(sharedKey);
            var signature = Encoding.UTF8.GetBytes(data);

            // If the sugnatures are equals - valid request, otherwise - invalid
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                var signatureBytes = hmac.ComputeHash(signature);
                var converted = Convert.ToBase64String(signatureBytes);

                var result = (incomingBase64Signature.Equals(converted, StringComparison.Ordinal));
                LogService.SaveLogs("Authorization final result = " + result);
                return result;
            }
        }

        private bool IsReplyRequest(string nonce, string requestTimeStamp)
        {
            // todo fix (ios player send the same token)
            var nonceInMemory = AuthHelper.MemoryCache.Get(nonce);
            nonceInMemory = null;

            // If nonce is already in memory - reply request
            if (nonceInMemory != null)
            {
                //LogService.SaveLogs("nonceInMemory");
                return true;
            }

            // Calculate UNIX time
            var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = DateTime.UtcNow - epochStart;
            var serverTS = Convert.ToUInt64(timeSpan.TotalSeconds);
            var requestTS = Convert.ToUInt64(requestTimeStamp);

            // If request is too old - reply request
            if (serverTS > requestTS)
                if (serverTS - requestTS > requestMaxAgeInSeconds)
                {
                    //LogService.SaveLogs($" {serverTS} - {requestTS} > {requestMaxAgeInSeconds}");
                    return true;
                }

            // Otherwise add nonce to MemoryCache, and request is not reply
            AuthHelper.MemoryCache.Set(nonce, requestTimeStamp, DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));

            return false;
        }

        private async Task<byte[]> ComputeHash(Stream body)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = null;
                byte[] content = null;

                if (body is MemoryStream)
                {
                    ms = ((MemoryStream)body);
                }
                else
                {
                    ms = new MemoryStream();
                    await body.CopyToAsync(ms);
                    body.Dispose();
                }

                content = ms.ToArray();

                if (content.Length != 0)
                {
                    hash = md5.ComputeHash(content);
                }

                return hash;
            }
        }
    }
}
