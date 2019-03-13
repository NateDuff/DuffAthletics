using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace SimpleAuthFunctions
{
    public static class SimpleAuth
    {
        [FunctionName("IsAuthenticated")]
        public static HttpResponseMessage IsAuthenticated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestMessage request,
            ILogger log)
        {
            //bool authenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;
            //bool authenticated = false;
            var headers = request.Headers;

            if (headers.Contains("X-MS-TOKEN-FACEBOOK-ACCESS-TOKEN"))
            {
                return request.CreateResponse(HttpStatusCode.OK, "Facebook", "application/json");
            }
            else if (headers.Contains("X-MS-TOKEN-GOOGLE-ACCESS-TOKEN"))
            {
                return request.CreateResponse(HttpStatusCode.OK, "Google", "application/json");
            }
            else if (headers.Contains("X-MS-TOKEN-MICROSOFT-ACCESS-TOKEN"))
            {
                return request.CreateResponse(HttpStatusCode.OK, "Microsoft", "application/json");
            }
            else
            {
                //return request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(headers), "application/json");
                 return request.CreateResponse(HttpStatusCode.OK, "None", "application/json");
            }

            
        }
    }
}
