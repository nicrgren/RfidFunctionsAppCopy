using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using WG3000_COMM.Core;

namespace RfidFunctionsApp
{
    public static class GetVersion
    {
        [FunctionName("GetVersion")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            var controller = new wgMjController
            {
                ControllerSN = (int)data?.serialNumber,
                IP = data?.ip,
                PORT = (int)data?.port
            };

            if (controller.GetMjControllerRunInformationIP() > 0)
            {
                return req.CreateResponse(HttpStatusCode.OK, controller.RunInfo.driverVersion);
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Unable to get remote information");
            }
        }
    }
}
