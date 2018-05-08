using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using WG3000_COMM.Core;

namespace RfidFunctionsApp
{
    public static class RestoreRecords
    {
        [FunctionName("RestoreRecords")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            var controller = new wgMjController
            {
                ControllerSN = (int)data?.serialNumber,
                IP = data?.ip,
                PORT = (int)data?.port
            };

            bool restored = await Task.Run(() =>
            {
                return controller.RestoreAllSwipeInTheControllersIP() > 0;
            });

            if (restored)
            {
                return req.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Unable to restore swipe records");
            }
        }
        }
}
