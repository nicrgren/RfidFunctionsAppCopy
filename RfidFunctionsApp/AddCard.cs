using System;
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
    public static class AddCard
    {
        [FunctionName("AddCard")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

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

            MjRegisterCard card = new MjRegisterCard();

            dynamic data = await req.Content.ReadAsAsync<object>();

            card.CardID = data?.tag;
            card.Password = 123456;
            card.ymdStart = DateTime.Now;
            card.ymdEnd = card.ymdStart.AddYears(10);
            card.ControlSegIndexSet(1, 1);
            card.ControlSegIndexSet(2, 1);

            bool success = false;

            await Task.Run(() =>
            {
                wgMjControllerPrivilege privilege = new wgMjControllerPrivilege();
                success = privilege.AddPrivilegeOfOneCardIP(controller.ControllerSN, controller.IP, controller.PORT, card) > 0;
            });
            
            if (success)
            {
                return req.CreateResponse(HttpStatusCode.Created, new { success = true, message = $"Tag ({card.CardID}) added" });
            } else
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Unable to save card, please try again...");
            }
        }
    }
}
