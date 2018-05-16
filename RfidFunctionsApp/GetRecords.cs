using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using WG3000_COMM.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net.Http.Formatting;

namespace RfidFunctionsApp
{

    public class Pass {
        public DateTime readDate;
        public int id;
        public string direction;
        public int readerNumber;
    }

    public static class GetRecords
    {
        [FunctionName("GetRecords")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info("Getting swipe records");

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

            var passes = new List<Pass>();

            int num = -1;

            await Task.Run(() =>
            {
                
                using (wgMjControllerSwipeOperate swipe4GetRecords = new wgMjControllerSwipeOperate())
                {
                    swipe4GetRecords.Clear();
                    num = swipe4GetRecords.GetSwipeRecords(controller.ControllerSN, controller.IP, controller.PORT, ref dtSwipeRecords);
                }
                Console.WriteLine($"Got {num} records");

                if (num > 0)
                {
                    wgMjControllerSwipeRecord mjrec = new wgMjControllerSwipeRecord();
                    for (int i = 0; i < num; i++)
                    {
                        mjrec.Update(dtSwipeRecords.Rows[i]["f_RecordAll"] as string);
                        log.Info(mjrec.CardID.ToString());
                        var pass = new Pass
                        {
                            id = (int)mjrec.CardID,
                            readerNumber = mjrec.DoorNo,
                            readDate = mjrec.ReadDate,
                            direction = mjrec.IsEnterIn ? "in" : "out"
                        };
                        passes.Add(pass);
                    }
                }
            });

            if (num < 0)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Unable to get records");
            }

            return req.CreateResponse(HttpStatusCode.OK, passes, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
