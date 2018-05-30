using System;
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Getting version...");

            // Serial Number
            int serialNumber = 0;
            foreach (string s in req.Headers.GetValues("X-M46-RFID-SN"))
            {
                serialNumber = int.Parse(s);
            }

            // IP
            string ip = "";
            foreach (string i in req.Headers.GetValues("X-M46-RFID-IP"))
            {
                ip = i;
            }

            // Port
            int port = 60000;
            foreach (string p in req.Headers.GetValues("X-M46-RFID-PORT"))
            {
                port = int.Parse(p);
            }

            var controller = new wgMjController
            {
                ControllerSN = serialNumber,
                IP = ip,
                PORT = port
            };

            bool success = false;

            await Task.Run(() =>
            {
                success = controller.GetMjControllerRunInformationIP() > 0;
            });

            if (success)
            {
                return req.CreateResponse(HttpStatusCode.OK, new { version = controller.RunInfo.driverVersion });
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Unable to get remote information");
            }
        }
    }
}
