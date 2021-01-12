using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrackWebServiceClient.TrackServiceWebReference;

namespace WAT.Models
{
    public class WaferTrace
    {
        public static bool CheckValidWafer(string wafernum)
        {
            var sql = "select top 1 containername from [Insite].[dbo].[ProductionResult] where Containername like '<wafernum>%'";
            sql = sql.Replace("<wafernum>", wafernum.Replace("'", ""));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            { return false; }

            sql = "select top 1 WaferNum from [WAT].[dbo].[WaferTrace] where WaferNum = '<wafernum>'";
            sql = sql.Replace("<wafernum>", wafernum.Replace("'", ""));
            dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            { return false; }

            return true;
        }

        public static List<string> GetTraceStatus(string TraceID,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var ret = new List<string>();
            var tracestatus = "";
            var latestevent = "";
            var latesteventtime = "";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            TrackRequest request = CreateTrackRequest(TraceID, syscfg);
            TrackService service = new TrackService();
            service.Url = syscfg["FEDEXURL"]; //"https://wsbeta.fedex.com:443/web-services/track"
            try
            {

                TrackReply reply = service.track(request);
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
                    {
                        foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                        {
                            if (trackDetail.StatusDetail != null)
                            {  tracestatus = trackDetail.StatusDetail.Description.ToUpper(); }

                            if (trackDetail.Events != null)
                            {
                                //Console.WriteLine("Track Events:");
                                foreach (TrackEvent trackevent in trackDetail.Events)
                                {
                                    if (trackevent.TimestampSpecified)
                                    {
                                        latesteventtime = trackevent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    latestevent = trackevent.EventDescription.ToUpper();

                                    if (!string.IsNullOrEmpty(tracestatus))
                                    {
                                        ret.Add(tracestatus);
                                        ret.Add(latestevent);
                                        ret.Add(latesteventtime);
                                        return ret;
                                    }
                                    //Console.WriteLine("***");
                                }
                                //Console.WriteLine();
                            }
                        }
                    }

                    foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
                    {
                        foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                        {
                            foreach (var evt in trackDetail.DatesOrTimes)
                            {
                                if (evt.Type == TrackingDateOrTimestampType.ACTUAL_DELIVERY)
                                {
                                    tracestatus = "DELIVERED";
                                    latestevent = "DELIVERED";
                                    latesteventtime = evt.DateOrTimestamp;

                                    ret.Add(tracestatus);
                                    ret.Add(latestevent);
                                    ret.Add(latesteventtime);
                                    return ret;
                                }
                            }
                        }
                    }

                    foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
                    {
                        foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                        {
                            foreach (var evt in trackDetail.DatesOrTimes)
                            {
                                tracestatus = "DELIVERING";
                                latestevent = "DELIVERING";
                                latesteventtime = evt.DateOrTimestamp;

                                ret.Add(tracestatus);
                                ret.Add(latestevent);
                                ret.Add(latesteventtime);
                                return ret;
                            }
                        }
                    }
                }//end if
            }
            catch (Exception ex) { }

            

            ret.Add(tracestatus);
            ret.Add(latestevent);
            ret.Add(latesteventtime);
            return ret;
        }

        private static TrackRequest CreateTrackRequest(string TraceID, Dictionary<string,string> syscfg)
        {
            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = syscfg["FEDEXKEY"];//"3jbeGywUe3xc0Vo6"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = syscfg["FEDEXPWD"];//"WJ1P0OVH4RuJylkC63Kxpq1QG "; // Replace "XXX" with the Password

            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = syscfg["FEDEXKEY"];//"3jbeGywUe3xc0Vo6"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.ParentCredential.Password = syscfg["FEDEXPWD"];//"WJ1P0OVH4RuJylkC63Kxpq1QG "; // Replace "XXX"

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = syscfg["FEDEXACCOUNT"];//"510087780"; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = syscfg["FEDEXMETER"];//"100495072"; // Replace "XXX" with the client's meter number

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = syscfg["FEDEXTRANSACT"];//"II-VI";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = TraceID; //"772395610600 "; // Replace "XXX" with tracking number or door tag

            request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
            //
            // Date range is optional.
            // If omitted, set to false
            request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("06/18/2012"); //MM/DD/YYYY
            request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
            request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
            request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
            //
            // Include detailed scans is optional.
            // If omitted, set to false
            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }

        public void StoreData()
        {
            var sql = @"insert into [WAT].[dbo].[WaferTrace](WaferNum,Priority,Product,PN,TraceID,TraceCompany,DeliverStatus,ArriveDate,Assemblyed,TestStuatus,UpdateTime)
                       values(@WaferNum,@Priority,@Product,@PN,@TraceID,@TraceCompany,@DeliverStatus,@ArriveDate,@Assemblyed,@TestStuatus,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@Priority", Priority);
            dict.Add("@Product", Product);
            dict.Add("@PN", PN);
            dict.Add("@TraceID", TraceID);
            dict.Add("@TraceCompany", TraceCompany);
            dict.Add("@DeliverStatus", DeliverStatus);
            dict.Add("@ArriveDate", ArriveDate);
            dict.Add("@Assemblyed", Assemblyed);
            dict.Add("@TestStuatus", TestStuatus);
            dict.Add("@UpdateTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateTraceStatus(string TraceID, string DeliverStatus, string ArriveDate)
        {
            var sql = "update [WAT].[dbo].[WaferTrace] set DeliverStatus = @DeliverStatus,ArriveDate = @ArriveDate where TraceID = @TraceID";
            var dict = new Dictionary<string, string>();
            dict.Add("@TraceID", TraceID);
            dict.Add("@DeliverStatus", DeliverStatus);
            dict.Add("@ArriveDate", ArriveDate);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateAssemblyStatus(string wafer)
        {
            var sql = "update [WAT].[dbo].[WaferTrace] set Assemblyed = 'YES' , Appv_7=@ASBDATE where WaferNum = @WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            dict.Add("@ASBDATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateTestStatus(string wafer,string status)
        {
            var sql = "update [WAT].[dbo].[WaferTrace] set TestStuatus = @TestStuatus where WaferNum = @WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            dict.Add("@TestStuatus", status);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void UpdateEMAILStatus(string wafer)
        {
            var sql = "update [WAT].[dbo].[WaferTrace] set Appv_5 = 'EMAILED' where WaferNum = @WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<WaferTrace> GetAllData(Controller ctrl)
        {
            var prodtype = CfgUtility.GetSixInchProdType(ctrl);

            var ret = new List<WaferTrace>();

            var sql = "select WaferNum,Priority,Product,PN,TraceID,TraceCompany,DeliverStatus,ArriveDate,Assemblyed,TestStuatus from  [WAT].[dbo].[WaferTrace] order by UpdateTime desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WaferTrace();
                tempvm.WaferNum = UT.O2S(line[0]);
                tempvm.Priority = UT.O2S(line[1]);
                tempvm.Product = UT.O2S(line[2]);
                tempvm.PN = UT.O2S(line[3]);
                tempvm.TraceID = UT.O2S(line[4]);
                tempvm.TraceCompany = UT.O2S(line[5]);
                tempvm.DeliverStatus = UT.O2S(line[6]);
                tempvm.ArriveDate = UT.O2T(line[7]).ToString("yyyy-MM-dd");
                tempvm.Assemblyed = UT.O2S(line[8]);
                tempvm.TestStuatus = UT.O2S(line[9]);
                if (prodtype.ContainsKey(tempvm.Product))
                { tempvm.VType = prodtype[tempvm.Product]; }
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<WaferTrace> GetNONEMAILEDData(Controller ctrl)
        {
            var prodtype = CfgUtility.GetSixInchProdType(ctrl);

            var ret = new List<WaferTrace>();

            var sql = "select WaferNum,Priority,Product,PN,TraceID,TraceCompany,DeliverStatus,ArriveDate,Assemblyed,TestStuatus from  [WAT].[dbo].[WaferTrace] where DeliverStatus = 'DELIVERED' and Appv_5= ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WaferTrace();
                tempvm.WaferNum = UT.O2S(line[0]);
                tempvm.Priority = UT.O2S(line[1]);
                tempvm.Product = UT.O2S(line[2]);
                tempvm.PN = UT.O2S(line[3]);
                tempvm.TraceID = UT.O2S(line[4]);
                tempvm.TraceCompany = UT.O2S(line[5]);
                tempvm.DeliverStatus = UT.O2S(line[6]);
                tempvm.ArriveDate = UT.O2T(line[7]).ToString("yyyy-MM-dd");
                tempvm.Assemblyed = UT.O2S(line[8]);
                tempvm.TestStuatus = UT.O2S(line[9]);
                if (prodtype.ContainsKey(tempvm.Product))
                { tempvm.VType = prodtype[tempvm.Product]; }
                ret.Add(tempvm);
            }

            return ret;
        }

        public static bool CheckAssemblyed(string wafer)
        {
            var sql = "select top 1 sn from [WAT].[dbo].[OGPFatherImg] where sn like '%<wafer>%'";
            sql = sql.Replace("<wafer>", wafer.Replace("'", ""));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        public static bool CheckTesting(string wafer)
        {
            var sql = "select top 1 containername from [Insite].[dbo].[ProductionResult] where Containername like '<wafernum>%'";
            sql = sql.Replace("<wafernum>", wafer.Replace("'", ""));
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        public static string GetTestStatus(string wafer)
        {
            var wfpassfaildict = WuxiWATData4MG.GetPassFailWaferDict();
            if (wfpassfaildict.ContainsKey(wafer))
            {
                var stat = wfpassfaildict[wafer];
                if (stat.Contains("PASS") || stat.Contains("SCRAP"))
                { return stat; }
            }
            return "TESTING";
        }

        public static void SendJOEmail(WaferTrace wtrace,Controller ctrl)
        {
            try
            {
                var syscfg = CfgUtility.GetSysConfig(ctrl);
                var title = new List<string>();
                title.Add("Wafer");title.Add("Priority"); title.Add("Product"); title.Add("PN"); title.Add("EXP-INC"); title.Add("TraceID"); title.Add("ArriveDate"); title.Add("Action");
                var line = new List<string>();
                line.Add(wtrace.WaferNum);line.Add(wtrace.Priority); line.Add(wtrace.Product);line.Add(wtrace.PN);line.Add(wtrace.TraceCompany); line.Add(wtrace.TraceID);line.Add(UT.O2T(wtrace.ArriveDate).ToString("yyyy-MM-dd"));
                line.Add("<a href='http://wuxinpi.china.ads.finisar.com:9090/Main/StartWATJO?wf="+WebUtility.UrlEncode(wtrace.WaferNum)+"' target='_blank'>DONE</a>");
                var cnt = new List<List<string>>();
                cnt.Add(title);cnt.Add(line);
                var cstr = EmailUtility.CreateTableHtml("Hi Planner", "Below are wafers which arrive in recent time, Would you mind to start JO for them?", "", cnt);
                var towho = syscfg["JONOTICEPEOPLE"].Split(new string[] { ";"},StringSplitOptions.RemoveEmptyEntries).ToList();
                EmailUtility.SendEmail(ctrl, "WAT JO Notice",towho, cstr);
                new System.Threading.ManualResetEvent(false).WaitOne(300);
                UpdateEMAILStatus(wtrace.WaferNum);
            }
            catch (Exception ex) { }
        }

        public static void RefreshDelievery(Controller ctrl, bool email = false)
        {
            var traceidlist = new List<string>();
            var sql = "select distinct TraceID from [WAT].[dbo].[WaferTrace] where DeliverStatus <> 'DELIVERED'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                traceidlist.Add(UT.O2S(line[0]));
            }

            //update delievery status
            foreach (var id in traceidlist)
            {
                var traceinfo = WaferTrace.GetTraceStatus(id, ctrl);
                if (string.IsNullOrEmpty(traceinfo[0]))
                {
                    var tracestatus = traceinfo[0];
                    var traceevent = traceinfo[1];
                    var tracedate = traceinfo[2];

                    var wtrace = new WaferTrace();
                    if (tracestatus.Contains("DELIVERED"))
                    {
                        wtrace.DeliverStatus = "DELIVERED";
                        wtrace.ArriveDate = tracedate;
                    }
                    else
                    {
                        wtrace.DeliverStatus = (traceevent.Length > 100) ? traceevent.Substring(0, 100) : traceevent;
                        wtrace.ArriveDate = tracedate;
                    }

                    WaferTrace.UpdateTraceStatus(wtrace.TraceID, wtrace.DeliverStatus, wtrace.ArriveDate);

                    if (tracestatus.Contains("DELIVERED") && email)
                    {
                        WaferTrace.SendJOEmail(wtrace, ctrl);
                    }
                }//end if
            }//end foreach

        }


        public static void DailyUpdate(Controller ctrl)
        {
            RefreshDelievery(ctrl, true);

            //update assembly status
            var wflist = new List<string>();
            var sql = "select distinct WaferNum from [WAT].[dbo].[WaferTrace] where Assemblyed = ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                wflist.Add(UT.O2S(line[0]));
            }
            foreach (var wf in wflist)
            {
                if (CheckAssemblyed(wf))
                { WaferTrace.UpdateAssemblyStatus(wf); }
            }

            //update test status
            wflist = new List<string>();
            sql = "select distinct WaferNum from [WAT].[dbo].[WaferTrace] where TestStuatus <> 'PASS' and TestStuatus <> 'SCRAP'";
            dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                wflist.Add(UT.O2S(line[0]));
            }

            foreach (var wf in wflist)
            {
                if (CheckTesting(wf))
                {
                    var stat = GetTestStatus(wf);
                    UpdateTestStatus(wf, stat);
                }
            }

            //send email
            var wtracelist = GetNONEMAILEDData(ctrl);
            foreach (var trace in wtracelist)
            {
                SendJOEmail(trace, ctrl);
            }
        }

        public WaferTrace()
        {
            WaferNum = "";
            Priority = "";
            Product = "";
            PN = "";
            TraceID = "";
            TraceCompany = "";
            DeliverStatus = "";
            ArriveDate = "";
            Assemblyed = "";
            TestStuatus = "";
            VType = "";
        }

        public string WaferNum { set; get; }
        public string Priority { set; get; }
        public string Product { set; get; }
        public string PN { set; get; }
        public string TraceID { set; get; }
        public string TraceCompany { set; get; }
        public string DeliverStatus { set; get; }
        public string ArriveDate { set; get; }
        public string Assemblyed { set; get; }
        public string TestStuatus { set; get; }

        public string VType { set; get; }
    }
}