using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using WG3000_COMM.Core;

namespace RfidFunctionsApp
{
    public static class GetRecords
    {
        [FunctionName("GetRecords")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info("Getting swipe records");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            var controller = new wgMjController
            {
                ControllerSN = (int)data?.serialNumber,
                IP = data?.ip,
                PORT = (int)data?.port
            };

            DataTable dtSwipeRecords = new DataTable("SwipeRecords");
            dtSwipeRecords.Columns.Add("f_Index", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_ReadDate", System.Type.GetType("System.DateTime"));
            dtSwipeRecords.Columns.Add("f_CardNO", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_DoorNO", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_InOut", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_ReaderNO", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_EventCategory", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_ReasonNo", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_ControllerSN", System.Type.GetType("System.UInt32"));
            dtSwipeRecords.Columns.Add("f_RecordAll", System.Type.GetType("System.String"));

            int num = -1;

            using (wgMjControllerSwipeOperate swipe4GetRecords = new wgMjControllerSwipeOperate())
            {
                swipe4GetRecords.Clear();
                num = swipe4GetRecords.GetSwipeRecords(controller.ControllerSN, controller.IP, controller.PORT, ref dtSwipeRecords);
            }

            if (num > 0)
            {
                wgMjControllerSwipeRecord mjrec = new wgMjControllerSwipeRecord();
                for (int i = 0; i < num; i++)
                {
                    mjrec.Update(dtSwipeRecords.Rows[i]["f_RecordAll"] as string);
                    log.Info(mjrec.CardID.ToString());
                }
            } else
            {
                log.Error(num.ToString());
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
