using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;
using WXLogic;
using System.Web.Routing;
using System.IO;
using RestSharp;

using System.Web.Services.Protocols;
using System.Net;
using TrackWebServiceClient.TrackServiceWebReference;

namespace WAT.Controllers
{
    public class MainController : Controller
    {
        private ActionResult Jump2Welcome(string url)
        {
            var dict = new RouteValueDictionary();
            dict.Add("url", url);
            return RedirectToAction("Welcome", "Main", dict);
        }

        private bool CheckName(string ip,string url)
        {
            var machinename = MachineUserMap.GetUserMachineName(ip);
            if (machinename.Count == 0)
            { return false; }
            else
            {
                MachineUserMap.LoginLog(machinename[0], machinename[1], url);
                return true;
            }
        }

        // GET: Main
        public ActionResult Index()
        {
            var url = "/Main/Index";
            if (!CheckName(Request.UserHostName,url))
            { return Jump2Welcome(url); }

            return View();
        }

        private void heartbeatlog(string msg)
        {
            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }


        public ActionResult HeartBeat()
        {
            try
            {
                var syscfg = CfgUtility.GetSysConfig(this);

                var heartbeatinprocess = Server.MapPath("~/userfiles") + "\\" + "InHeartBeatProcess";
                if (System.IO.File.Exists(heartbeatinprocess))
                {
                    var lastinprocesstime = System.IO.File.GetLastWriteTime(heartbeatinprocess);
                    if ((DateTime.Now - lastinprocesstime).Hours >= 6)
                    { System.IO.File.Delete(heartbeatinprocess); }
                    else
                    { return View(); }
                }
                System.IO.File.WriteAllText(heartbeatinprocess, "hello");

                heartbeatlog("start heartbeat");

                var dailyscan = Server.MapPath("~/userfiles") + "\\" + "dailyscan_"+ DateTime.Now.ToString("yyyy-MM-dd");
                if (!System.IO.File.Exists(dailyscan))
                {
                    System.IO.File.WriteAllText(dailyscan, "hello");

                    heartbeatlog("DieSortVM.ScanNewWafer");
                    try
                    {
                        DieSortVM.ScanNewWafer(this);
                    }
                    catch (Exception ex) { }

                    heartbeatlog("WuxiWATData4MG.RefreshWATStatusDaily");
                    try
                    {
                        WuxiWATData4MG.RefreshWATStatusDaily();
                    }
                    catch (Exception ex) { }

                    heartbeatlog("BinSubstitute.RefreshBinSubstituteFromAllen");
                    try
                    {
                        BinSubstitute.RefreshBinSubstituteFromAllen();
                    }
                    catch (Exception ex) { }

                    heartbeatlog("WXEvalPNRate.RefreshEVALPNRate();");
                    try
                    {
                        WXEvalPNRate.RefreshEVALPNRate();
                    }
                    catch (Exception ex) { }
                }

                try
                {

                    var fouram = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 04:00:00");
                    var fiveam = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 05:30:00");
                    if (DateTime.Now >= fouram && DateTime.Now <= fiveam)
                    {
                        heartbeatlog("WaferTrace.DailyUpdate(this)");
                        WaferTrace.DailyUpdate(this);
                    }

                    var onepm = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 13:00:00");
                    var threepm = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 14:30:00");
                    if (DateTime.Now >= onepm && DateTime.Now <= threepm)
                    {
                        heartbeatlog("WaferTrace.DailyUpdate(this)");
                        WaferTrace.DailyUpdate(this);
                    }
                }
                catch (Exception ex) { }

                try
                {
                    SHTOLvm.RefreshDailySHTOLData(this);
                }
                catch (Exception ex) { }

                try
                {
                    var onepm = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 13:00:00");
                    var threepm = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 15:00:00");
                    if (DateTime.Now >= onepm && DateTime.Now <= threepm)
                    {
                        if (!WATJobCheckVM.WATCheckDone())
                        {
                            var msg = "Todays WAT JOB CHECKING is not done:\r\n";
                            msg += "http://wuxinpi.chn.ii-vi.net:9090/DashBoard/WATJOBCheck";
                            var towho = syscfg["WATJOBCHECKLIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            EmailUtility.SendEmail(this, "WAT JOB CHECK IS NOT DONE", towho, msg);
                            new System.Threading.ManualResetEvent(false).WaitOne(500);
                        }
                    }
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog("SHTOLAnalyzer.AnalyzeData");
                    SHTOLAnalyzer.AnalyzeData(this, "", "");
                }
                catch (Exception ex) { }

                try
                {
                    heartbeatlog(" WATOven.RefreshDailyOvenData");
                    WATOven.RefreshDailyOvenData(this);
                }
                catch (Exception ex) { }

                //heartbeatlog("WaferQUALVM.LoadWUXIWaferQUAL");
                //try
                //{
                //    WaferQUALVM.LoadNewWaferFromMES();
                //    WaferQUALVM.LoadWUXIWaferQUAL();
                //}
                //catch (Exception ex) { }


                //heartbeatlog("AllenEVALData.LoadAllenData");
                //try
                //{
                //    AllenEVALData.LoadAllenData();
                //}
                //catch (Exception ex) { }

                heartbeatlog("end heartbeat");

                try
                { System.IO.File.Delete(heartbeatinprocess); }
                catch (Exception ex) { }
            }
            catch (Exception ex) { }

            return View();
        }

        //public ActionResult LoadWaferQUAL()
        //{
        //    WaferQUALVM.LoadNewWaferFromMES();
        //    WaferQUALVM.LoadWUXIWaferQUAL();
        //    return View("HeartBeat");
        //}

        public ActionResult LoadAllenData()
        {
            AllenEVALData.LoadAllenData();
            return View("HeartBeat");
        }

        public ActionResult AllenLogic()
        {
            //AllenWATLogic.PassFaile("184051-80E01", "Eval_50up_rp03");
            //var logicres1 = AllenWATLogic.PassFail("184051-80E01", "Eval_50up_rp03", true);
            //var logicres2 = AllenWATLogic.PassFail("184051-80E01", "Eval_50up_rp03");

            //var logicres1 = AllenWATLogic.PassFail("184637-30E01", "Eval_50up_rp04", true);
            //var logicres2 = AllenWATLogic.PassFail("184637-30E01", "Eval_50up_rp04");

            return View("HeartBeat");
        }

        public ActionResult MoveDB()
        {
            //var cfg = CfgUtility.GetSysConfig(this);
            //var FromConnect = cfg["FromConnect"];
            //var ToConnect = cfg["ToConnect"];
            ////var MOVETABLELIST = cfg["MOVETABLELIST"].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ////foreach (var tab in MOVETABLELIST)
            ////{
            //    var ret = MoveDataBase.MoveDB("[EngrData].[insite].[Eval_Specs_Bin_PassFail]", FromConnect,ToConnect, "[WAT].[dbo].[Eval_Specs_Bin_PassFail]");
            //    if (ret)
            //    {
            //        System.Windows.MessageBox.Show("Sucess to moved table: " + "");
            //    }
            //    else
            //    {
            //        System.Windows.MessageBox.Show("Fail to moved table: " + "");
            //    }
            ////}

            return View("HeartBeat");
        }

        //public ActionResult PrepareFakeData()
        //{
        //    WXProbeData.PrepareProbeData("184637-30");
        //    WXOriginalWATData.PrepareFakeDataFromAllen("184637-30E01");

        //    return View("HeartBeat");
        //}

        //public ActionResult WXLogic()
        //{
        //    //WXProbeData.PrepareProbeData("184637-30");
        //    //WXOriginalWATData.PrepareDataFromAllen("184637-30E01");
        //    var wxlogic = new WXWATLogic();
        //    wxlogic.WATPassFail("184637-30E01", "POSTHTOL2JUDGEMENT");

        //    return View("HeartBeat");
        //}

        //public ActionResult ScanNewWafer() {
        //    DieSortVM.ScanNewWafer(this);
        //    return View("HeartBeat");
        //}

        public ActionResult VerifyAllenLogic()
        {
            AllenLogicVerify.Verify();
            return View("HeartBeat");
        }


        public ActionResult ScanNewVCSEL()
        {
            DieSortVM.ScanNewWafer(this);
            return View("HeartBeat");
        }

        //public ActionResult PrepareData4WAT()
        //{
        //    var wlist = new List<string>();
        //    wlist.Add("191106-70");
        //    wlist.Add("191531-10");
        //    wlist.Add("191007-70");

        //    //wlist.Add("190601-20");
        //    //wlist.Add("190628-30");
        //    //wlist.Add("190717-30");
        //    foreach (var w in wlist)
        //    {
        //        DieSortVM.PrepareData4WAT(w);
        //    }

        //    return View("HeartBeat");
        //}

        public ActionResult LoadOGPData()
        {
            var ogpdata = WXLogic.WATSampleXY.GetSampleXYByCouponGroup("172015-30E08");
            return View("HeartBeat");
        }

        public ActionResult GetInspectWafer()
        {
            var inspectwafer = DieSortVM.GetInspectedWaferInPast5Days();
            return View("HeartBeat");
        }

        public ActionResult RefreshWATStatusDaily()
        {
            WuxiWATData4MG.RefreshWATStatusDaily();
            return View("HeartBeat");
        }

        public ActionResult RefreshBinSubstituteFromAllen() {
            BinSubstitute.RefreshBinSubstituteFromAllen();
            return View("HeartBeat");
        }

        //  /main/CovertMapFile?Wafer=183124-80&FromBin=54&ToBin=55
        public ActionResult CovertMapFile(string Wafer,string FromBin,string ToBin)
        {
            if (!String.IsNullOrEmpty(Wafer) && !String.IsNullOrEmpty(FromBin) && !String.IsNullOrEmpty(ToBin))
            { DieSortVM.ConvertBinMapFileData(Wafer, FromBin, ToBin, this); }
            
            return View("HeartBeat");
        }

        public ActionResult NeomapToAllen()
        { return View(); }

        public JsonResult NeomapToAllenData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            foreach (var wf in wflist)
            {
                if (Models.WXProbeData.AllenHasData(wf))
                {
                    wfdatalist.Add(new {
                        wf = wf,
                        stat = "OK"
                    });
                }
                else
                {
                    if (Models.WXProbeData.AddProbeTrigge2Allen(wf))
                    {
                        wfdatalist.Add(new
                        {
                            wf = wf,
                            stat = "OK"
                        });
                    }
                    else
                    {
                        wfdatalist.Add(new
                        {
                            wf = wf,
                            stat = "NG"
                        });
                    }
                }
            }

            foreach (var wf in wflist)
            {
                var maxtimes = 0;
                while (true)
                {
                    if (Models.WXProbeData.AllenHasData(wf))
                    { break; }
                    new System.Threading.ManualResetEvent(false).WaitOne(5000);
                    maxtimes++;
                    if (maxtimes > 24)
                    { break; }
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }


        public ActionResult Prepare4WAT()
        { return View(); }

        public JsonResult Prepare4WATData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            foreach (var wf in wflist)
            {
                if (DieSortVM.PrepareData4WAT(wf))
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "OK"
                    });
                }
                else
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "NG"
                    });
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }

        public ActionResult ModifyMapFile()
        {
            DieSortVM.ModifyMapFileAsExpect(this);
            return View("HeartBeat");

        }


        //public ActionResult LoadOvenData()
        //{
        //    var syscfg = CfgUtility.GetSysConfig(this);
        //    var oventesters = syscfg["WATOVEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

        //    var startdate = DateTime.Parse("2019-11-03 00:00:00");
        //    var enddate = DateTime.Parse("2019-11-19 00:00:00");

        //    while (startdate <= enddate)
        //    {
        //        foreach (var tester in oventesters)
        //        {
        //            WATOven.LoadOvenData(startdate.ToString("yyyy-MM-dd HH:mm:ss")
        //                , startdate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"), tester.ToUpper());
        //        }
        //        startdate = startdate.AddDays(1);
        //    }

        //    return View("HeartBeat");
        //}

        public ActionResult RefreshDailyOvenData()
        {
            WATOven.RefreshDailyOvenData(this);
            return View("HeartBeat");
        }

        public ActionResult RefreshDailySHTOLData()
        {
            SHTOLvm.RefreshDailySHTOLData(this);
            return View("HeartBeat");
        }

        public ActionResult RefreshDailySHTOLAnalyze()
        {
            //var startdate = WAT.Models.UT.O2T("2021-04-07 00:00:00");
            //for (var idx = 0; idx < 1; idx++) {
            //    SHTOLAnalyzer.AnalyzeData(this, startdate.ToString("yyyy-MM-dd HH:mm:ss"), startdate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
            //    startdate = startdate.AddDays(1);
            //}
            SHTOLAnalyzer.AnalyzeData(this, "", "");
            return View("HeartBeat");
        }

        public ActionResult Welcome(string url)
        {
            ViewBag.url = url;
            return View();
        }

        public JsonResult UpdateMachineUserName()
        {
            var username = Request.Form["username"].ToUpper().Trim();
            MachineUserMap.AddMachineUserMap(Request.UserHostName, username);
            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }


        public ActionResult RefreshEVALPNRate()
        {
            WXEvalPNRate.RefreshEVALPNRate();
            return View("HeartBeat");
        }

        public ActionResult SixInchMapFile()
        {
            var wafer = "61926-609-050";
            var desfile = Path.Combine(@"\\wux-engsys01.chn.ii-vi.net\DieSortingReview", wafer + ".xml");
            SixIMapFile.GenerateMapFile(wafer, desfile);
            return View("HeartBeat");
        }

        public ActionResult PrepareBinMap()
        {
            var url = "/Main/PrepareBinMap";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult PrepareBinMapFile()
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var wfdatalist = new List<object>();
            foreach (var wf in wflist)
            {
                var evalpn = WXLogic.WXEvalPN.GetProdFamByWaferNum(wf);
                if (string.IsNullOrEmpty(evalpn))
                {
                    DieSortVM.PrepareEvalPN(wf);
                    evalpn = WXLogic.WXEvalPN.GetProdFamByWaferNum(wf);
                }

                if (string.IsNullOrEmpty(evalpn))
                {
                    wfdatalist.Add(new
                    {
                        wf = wf,
                        stat = "NG"
                    });
                }
                else
                {
                    var folderuser = syscfg["SHAREFOLDERUSER"];
                    var folderdomin = syscfg["SHAREFOLDERDOMIN"];
                    var folderpwd = syscfg["SHAREFOLDERPWD"];

                    var ok = false;
                    try
                    {
                        using (WAT.Models.NativeMethods cv = new WAT.Models.NativeMethods(folderuser, folderdomin, folderpwd))
                        {
                            ok = MoveOriginalMapFile(wf, evalpn, syscfg["DIESORTFOLDER"], syscfg["DIESORT100PCT"]);
                        }
                    }
                    catch (Exception ex) { ok = false; }

                    if (ok)
                    {
                        wfdatalist.Add(new
                        {
                            wf = wf,
                            stat = "OK"
                        });
                    }
                    else
                    {
                        wfdatalist.Add(new
                        {
                            wf = wf,
                            stat = "NG"
                        });
                    }
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }

        private bool MoveOriginalMapFile(string wafer, string product, string orgfolder, string PCT100folder)
        {
            try
            {
                var existfiles = Directory.GetFiles(PCT100folder);
                foreach (var efs in existfiles)
                {
                    if (efs.ToUpper().Contains(wafer.ToUpper()))
                    { return true; }
                }

                var filedict = new Dictionary<string, string>();
                var nofolderfiles = Directory.GetFiles(orgfolder + "\\" + product);
                foreach (var f in nofolderfiles)
                {
                    var uf = f.ToUpper();
                    if (!filedict.ContainsKey(uf))
                    { filedict.Add(uf, f); }
                }

                foreach (var kv in filedict)
                {
                    if (kv.Key.Contains(wafer.ToUpper()))
                    {
                        var desfile = Path.Combine(PCT100folder, Path.GetFileName(kv.Value));
                        System.IO.File.Copy(kv.Value, desfile, true);
                        return true;
                    }
                }
            }
            catch (Exception ex) { }

            return false;
        }


        public ActionResult BinCheckWithOCR()
        { return View(); }

        public JsonResult BinCheckWithOCRData()
        {
            //wafer for probe bin number,lotnum for OCR
            var wrongbinlist = new List<List<string>>();

            var wf = Request.Form["wafer"].Trim();
            var lotnum = Request.Form["lotnum"].Trim();
            var wafer = wf.Split(new string[] { "E", "T", "R" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var ocrlist = new List<OGPSNXYVM>();

            var MSG = "";
            var ocrdata = OGPSNXYVM.GetLocalOGPXYSNDict(lotnum).Values.ToList();
            if (ocrdata.Count == 0)
            { MSG = "No OCR coordinate data!"; }

            var bindict = Models.WXProbeData.GetBinDictByWafer(wafer);

            //MSG = DieSortVM.GetAllBinFromMapFile(wafer,bindict, this);
            if (bindict.Count == 0)
            {  MSG = "Fail to get bin info from Sherman database"; }

            if (bindict.Count > 0)
            {

                foreach (var item in ocrdata)
                {
                    var templine = new List<string>();
                    var key = (Models.UT.O2I(item.X.Replace("X", "").Replace("x", "")) + ":::" + Models.UT.O2I(item.Y.Replace("Y", "").Replace("y", "")));
                    if (bindict.ContainsKey(key))
                    {
                        item.Bin = bindict[key];
                        var bin = Models.UT.O2I(item.Bin);
                        if (bin < 50 || bin > 59)
                        {
                            templine.Add(wafer);
                            templine.Add(lotnum);
                            templine.Add(item.X);
                            templine.Add(item.Y);
                            templine.Add(item.Bin);
                        }
                    }
                    else
                    {
                        templine.Add(wafer);
                        templine.Add(lotnum);
                        templine.Add(item.X);
                        templine.Add(item.Y);
                        templine.Add("-1");
                    }

                    item.WaferNum = wafer; 
                    item.Product = lotnum;
                    ocrlist.Add(item);

                    if (templine.Count > 0) {
                        wrongbinlist.Add(templine);
                    }

                }
            }

            if (wrongbinlist.Count > 0)
            {
                var syscfgdict = CfgUtility.GetSysConfig(this);
                var towho = syscfgdict["DIESORTWARINGLIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var alllines = new List<List<string>>();
                var title = new List<string>();
                title.Add("WAFER");
                title.Add("Lotnum");
                title.Add("X");
                title.Add("Y");
                title.Add("Bin");
                alllines.Add(title);
                alllines.AddRange(wrongbinlist);

                var content = EmailUtility.CreateTableHtml("Hi All", "this the wrong bin check email", "", alllines);
                EmailUtility.SendEmail(this,wafer +" BIN Check",towho,content);
                new System.Threading.ManualResetEvent(false).WaitOne(1000);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ocrlist = ocrlist,
                MSG = MSG
            };
            return ret;

        }

        public JsonResult WrongBinCommitData()
        {
            var die = Request.Form["die"];
            var waferxy = die.ToUpper().Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            var username = WXWATIgnoreDie.GetUserName(Request.UserHostName);
            if (waferxy.Length == 3) {
                WXWATIgnoreDie.UpdateIgnoreDie(waferxy[0], Models.UT.O2I(waferxy[1].ToUpper().Replace("X","").Replace("Y","")).ToString()
                    , Models.UT.O2I(waferxy[2].ToUpper().Replace("X", "").Replace("Y", "")).ToString(), "Wrong bin", username,"");
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                Sucess = "OK"
            };
            return ret;
        }

        public ActionResult SampleCheckWithOCR()
        {
            return View();
        }

        public JsonResult SampleXYCheckWithOCRData()
        {
            var wf = Request.Form["wafer"].Trim();
            var lotnum = Request.Form["lotnum"].Trim();
            var wafer = wf.Split(new string[] { "E", "T", "R" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var MSG = "";

            var sampledict = DieSortVM.GetSampleXYDict(wafer,this);
            if (sampledict.Count == 0)
            { MSG = "This is a sorted wafer!"; }

            var ocrdata = OGPSNXYVM.GetLocalOGPXYSNDict(lotnum).Values.ToList();
            if (ocrdata.Count == 0)
            { MSG = "No OCR coordinate data,so this wafer "+wf+" have not done WAT at WUXI!"; }

            var matched = 0;
            foreach (var item in ocrdata)
            {
                var templine = new List<string>();
                var key = (Models.UT.O2I(item.X.Replace("X", "").Replace("x", "")) + ":::" + Models.UT.O2I(item.Y.Replace("Y", "").Replace("y", "")));
                if (sampledict.ContainsKey(key))
                {
                    item.Bin = "MATCH";
                    matched++;
                }
            }

            var matchrate = (double)matched / (double)ocrdata.Count;
            if (matchrate > 0.7)
            { MSG = "This is a unsorted wafer!"; }
            else
            { MSG = "This is a sorted wafer!"; }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ocrlist = ocrdata,
                MSG = MSG
            };
            return ret;
        }

        public JsonResult GetWXWATWafer()
        {
            var couponidlistx = new List<string>();
            var dbret = WuxiWATData4MG.GetWUXIWATWaferStepData();
            foreach (var line in dbret)
            {
                var wafer = Models.UT.O2S(line[0]);
                if (wafer.Length > 3)
                { wafer = wafer.Substring(0, wafer.Length - 3); }
                else
                { continue; }
                if (!couponidlistx.Contains(wafer))
                {
                    couponidlistx.Add(wafer);
                }
            }
            var ret = new JsonResult();
            ret.Data = new
            {
                couponidlist = couponidlistx
            };
            return ret;
        }

        public JsonResult GetWXOCRWafer()
        {
            var couponidlistx = WATOGPVM.GetLocalOGPXYWafer();
            var ret = new JsonResult();
            ret.Data = new
            {
                couponidlist = couponidlistx
            };
            return ret;
        }

        public ActionResult AllenBinInfo()
        {
            return View();
        }

        public JsonResult ALLENBinData()
        {
            var datalist = new List<object>();

            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var w4list = new List<string>();
            var w6list = new List<string>();

            foreach (var w in wflist)
            {
                if (w.Length == 9)
                { w4list.Add(w); }
                if (w.Length == 13)
                { w6list.Add(w); }
            }

            if (w4list.Count > 0)
            {
                var famdict = Models.WXEvalPN.GetProdFamByWaferListAllen(w4list);

                var wfcond = "('" + string.Join("','", w4list) + "')";
                var sql = @" select b.wafer_id,b.prodfam,b.DS,b.[Good -50],b.[Good -51],b.[Good -52],b.[Good -53],b.[Good -54],b.[Good -55] ,b.[GOOD -56] ,b.[GOOD -57],b.[GOOD -58],b.[GOOD -59] from [Insite].[insite].[100PCT_BIN] b
                            left join (select wafer_id,max(ds) as maxtime from [Insite].[insite].[100PCT_BIN] where wafer_id in <wfcond>  and ([Good -55] is not null or [Good -54] is not null) group by wafer_id) c
                             on b.wafer_id = c.wafer_id and b.DS = c.maxtime
                             where b.wafer_id in  <wfcond>
                             and c.wafer_id is not null";
                sql = sql.Replace("<wfcond>", wfcond);
                var dbret = Models.DBUtility.ExeAllenSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var w = Models.UT.O2S(line[0]);
                    var p = Models.UT.O2S(line[1]);
                    if (!famdict.ContainsKey(w)) { continue; }
                    var fam = famdict[w];
                    if (!p.Contains(fam)) { continue; }

                    datalist.Add(new
                    {
                        wafer = Models.UT.O2S(line[0]),
                        product = Models.UT.O2S(line[1]),
                        bin50 = Models.UT.O2S(line[3]),
                        bin51 = Models.UT.O2S(line[4]),
                        bin52 = Models.UT.O2S(line[5]),
                        bin53 = Models.UT.O2S(line[6]),
                        bin54 = Models.UT.O2S(line[7]),
                        bin55 = Models.UT.O2S(line[8]),
                        bin56 = Models.UT.O2S(line[9]),
                        bin57 = Models.UT.O2S(line[10]),
                        bin58 = Models.UT.O2S(line[11]),
                        bin59 = Models.UT.O2S(line[12])
                    });
                }
            }

            if (w6list.Count > 0)
            {
                foreach (var w in w6list)
                {
                    var prodfam = Models.WXEvalPN.GetProductFamilyFromSherman(w);
                    var bindict = WXLogic.SixIMapFile.GetBinDistribution(w);
                    if (bindict.Count > 0)
                    {
                        int bin50 = 0, bin51 = 0, bin52 = 0, bin53 = 0
                            , bin54 = 0, bin55 = 0, bin56 = 0
                            , bin57 = 0, bin58 = 0, bin59 = 0;
                        if (bindict.ContainsKey("50")) { bin50 = bindict["50"]; }
                        if (bindict.ContainsKey("51")) { bin51 = bindict["51"]; }
                        if (bindict.ContainsKey("52")) { bin52 = bindict["52"]; }
                        if (bindict.ContainsKey("53")) { bin53 = bindict["53"]; }
                        if (bindict.ContainsKey("54")) { bin54 = bindict["54"]; }
                        if (bindict.ContainsKey("55")) { bin55 = bindict["55"]; }
                        if (bindict.ContainsKey("56")) { bin56 = bindict["56"]; }
                        if (bindict.ContainsKey("57")) { bin57 = bindict["57"]; }
                        if (bindict.ContainsKey("58")) { bin58 = bindict["58"]; }
                        if (bindict.ContainsKey("59")) { bin59 = bindict["59"]; }
                        datalist.Add(new
                        {
                            wafer = w,
                            product = prodfam,
                            bin50 = bin50,
                            bin51 = bin51,
                            bin52 = bin52,
                            bin53 = bin53,
                            bin54 = bin54,
                            bin55 = bin55,
                            bin56 = bin56,
                            bin57 = bin57,
                            bin58 = bin58,
                            bin59 = bin59
                        });
                    }//end if
                }//end foreach
            }//end if

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                datalist = datalist
            };
            return ret;
        }

        public ActionResult WuxiBinInfo()
        {
            return View();
        }

        public JsonResult WUXIBinData()
        {
            var marks = Request.Form["marks"];
            List<string> wflist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());

            var datalist = WUXIWATBIN.GetBinInfo(wflist, this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                datalist = datalist
            };
            return ret;
        }

        public ActionResult WATTestStep()
        {

            return View();
        }

        public JsonResult WATTestStepData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var wafernum = Request.Form["wafernum"].Trim().ToUpper();
            var normaltest = "";
            var retest = "";
            var status = "";
            var charlist = new List<string>(new string[] { "E", "R", "T" });
            var tclist = new List<string>(new string[] { "08", "09", "10" });
            var matchstr = "";
            var matcht = "";
            foreach (var c in charlist)
            {
                foreach (var t in tclist)
                {
                    if (wafernum.Contains(c + t))
                    {
                        matchstr = c + t;
                        matcht = t;
                    }
                }
            }

            if (!string.IsNullOrEmpty(matchstr))
            {
                var currenttest = WuxiWATData4MG.GetWATTestStep(wafernum);
                if (!string.IsNullOrEmpty(currenttest))
                {
                    var key = matcht + "_" + currenttest + "_NEXT";
                    if (syscfg.ContainsKey(key))
                    {
                        var nexttest = syscfg[key];
                        if (nexttest.Contains("END"))
                        {
                            status = "SN 测试完成,OK";
                            retest = currenttest;
                        }
                        else
                        {
                            status = "OK";
                            retest = currenttest;
                            normaltest = nexttest;
                        }
                    }
                    else
                    {
                        status = "错误步骤 "+matchstr +" "+currenttest;
                    }
                }
                else
                {
                    status = "SN 第一次测试,OK";
                    if (syscfg.ContainsKey(matcht + "_FIRSTTESTSTEP"))
                    {
                        normaltest = syscfg[matcht + "_FIRSTTESTSTEP"];
                    }
                }
            }
            else
            {
                status = "SN 格式不正确";
            }

            WebLog.Log(wafernum, "WATTEST", status);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                normaltest = normaltest,
                retest = retest,
                status = status
            };

            return ret;
        }

        public ActionResult OVENTestPlan()
        {
            return View();
        }

        public JsonResult OVENPlanData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var wafernum = Request.Form["wafernum"].Trim().ToUpper();
            var normaltest = "";
            var tested = "";
            var status = "";
            var charlist = new List<string>(new string[] { "E", "R", "T" });
            var tclist = new List<string>(new string[] { "08", "09", "10" });
            var matchstr = "";
            var matcht = "";
            foreach (var c in charlist)
            {
                foreach (var t in tclist)
                {
                    if (wafernum.Contains(c + t))
                    {
                        matchstr = c + t;
                        matcht = t;
                    }
                }
            }

            if (!string.IsNullOrEmpty(matchstr))
            {
                var currenttest = WuxiWATData4MG.GetWATTestStep(wafernum);
                if (!string.IsNullOrEmpty(currenttest))
                {
                    var key = "OVEN"+ matcht + "_" + currenttest + "_NEXT";
                    if (syscfg.ContainsKey(key))
                    {
                        var nexttest = syscfg[key];
                        if (nexttest.Contains("END"))
                        {
                            status = "SN测试完成,不再进炉,OK";
                            normaltest = "";
                            tested = currenttest;
                        }
                        else
                        {
                            status = "OK";
                            normaltest = nexttest;
                            tested = currenttest;
                        }
                    }
                    else
                    {
                        status = "错误步骤 " + matchstr + " " + currenttest;
                    }
                }
                else
                {
                    status = "先测 " + matchstr + "-PRE-BURNI,再进炉！";
                    normaltest = "先测 " + matchstr + "-PRE-BURNI,再进炉！";
                }
            }
            else
            {
                status = "SN 格式不正确";
            }

            WebLog.Log(wafernum, "WATTEST", status);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                normaltest = normaltest,
                tested = tested,
                status = status
            };

            return ret;
        }
        

        public ActionResult IIVIBin() {
            return View();
        }

        public JsonResult IIVIBinData()
        {
            var gh = Request.Form["gh"];
            var wafernum = Request.Form["wafernum"].Trim();
            var marks = Request.Form["marks"];
            List<string> allxylist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());

            if (wafernum.Length == 13 || wafernum.Length == 9)
            {
                return allenshermanbincheck_(wafernum, allxylist, gh);
            }
            else
            {
                return iivibincheck_(wafernum, allxylist, gh);
            }
        }

        private JsonResult allenshermanbincheck_(string wafernum, List<string> allxylist, string gh)
        {
            var wfdatalist = new List<object>();
            var MSG = "";

            var xydict = Models.WXProbeData.GetBinDictByWafer(wafernum);
            if (xydict.Count == 0)
            {
                MSG = "没有对应的WAFER BIN 信息!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            var loglist = new List<WebLog>();

            foreach (var xy in allxylist)
            {
                var xystr = xy.Trim().Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (xystr.Length == 2)
                {
                    var x = Models.UT.O2I(xystr[0]).ToString();
                    var y = Models.UT.O2I(xystr[1]).ToString();
                    var k = x + ":::" + y;
                    if (xydict.ContainsKey(k))
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = xydict[k]
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ",bin " + xydict[k];
                        loglist.Add(tempvm);
                    }
                    else
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = "no bin"
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ", no bin";
                        loglist.Add(tempvm);
                    }
                }//end if
            }

            if (wfdatalist.Count == 0)
            {
                MSG = "坐标输入格式有误!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            foreach (var item in loglist)
            { WebLog.LogIIVIQuery(item.IIVIWafer, item.Name, item.MSG); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist,
                MSG = MSG
            };
            return ret;
        }

        private JsonResult sixinchbincheck_(string wafernum, List<string> allxylist, string gh)
        {
            var wfdatalist = new List<object>();
            var MSG = "";

            var xydict = SixInchMapFileData.GetBinDict(wafernum);
            if (xydict.Count == 0)
            {
                MSG = "没有对应的WAFER BIN 信息!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            var loglist = new List<WebLog>();

            foreach (var xy in allxylist)
            {
                var xystr = xy.Trim().Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (xystr.Length == 2)
                {
                    var x = Models.UT.O2I(xystr[0]).ToString();
                    var y = Models.UT.O2I(xystr[1]).ToString();
                    var k = x + ":::" + y;
                    if (xydict.ContainsKey(k))
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = xydict[k]
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ",bin "+ xydict[k];
                        loglist.Add(tempvm);
                    }
                    else
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = "no bin"
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ", no bin";
                        loglist.Add(tempvm);
                    }
                }//end if
            }

            if (wfdatalist.Count == 0)
            {
                MSG = "坐标输入格式有误!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            foreach (var item in loglist)
            { WebLog.LogIIVIQuery(item.IIVIWafer, item.Name, item.MSG); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist,
                MSG = MSG
            };
            return ret;
        }

        private JsonResult iivibincheck_(string wafernum, List<string> allxylist, string gh)
        {
            var wfdatalist = new List<object>();
            var MSG = "";

            var xydict = IIVIVcselVM.GetIIVICoordDict(wafernum);
            if (xydict.Count == 0)
            {
                MSG = "没有对应的WAFER!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            var loglist = new List<WebLog>();

            foreach (var xy in allxylist)
            {
                var xystr = xy.Trim().Split(new string[] { ",", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (xystr.Length == 2)
                {
                    var x = Models.UT.O2I(xystr[0]).ToString();
                    var y = Models.UT.O2I(xystr[1]).ToString();
                    var k = x + ":::" + y;
                    if (xydict.ContainsKey(k))
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = "1"
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ",bin 1";
                        loglist.Add(tempvm);
                    }
                    else
                    {
                        wfdatalist.Add(new
                        {
                            wf = wafernum,
                            x = x,
                            y = y,
                            bin = "0"
                        });

                        var tempvm = new WebLog();
                        tempvm.IIVIWafer = wafernum;
                        tempvm.Name = gh;
                        tempvm.MSG = k + ",bin 0";
                        loglist.Add(tempvm);
                    }
                }//end if
            }

            if (wfdatalist.Count == 0)
            {
                MSG = "坐标输入格式有误!";
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    wfdatalist = wfdatalist,
                    MSG = MSG
                };
                return ret1;
            }

            foreach (var item in loglist)
            { WebLog.LogIIVIQuery(item.IIVIWafer, item.Name, item.MSG); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist,
                MSG = MSG
            };
            return ret;
        }


        public JsonResult IIVIBinHistory()
        {
            var wafernum = Request.Form["wf"].Trim();
            var wfdatalist = WebLog.GetIIVIQuery(wafernum);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wfdatalist = wfdatalist
            };
            return ret;
        }

        public ActionResult WATDataWatchDog()
        {
            return View();
        }

        public ActionResult WatchDogDemo()
        {
            return View();
        }


        public ActionResult UpdateIgnoreDie()
        {
            var waferlist = new List<string>();

            var sql = "select distinct wafer from [EngrData].[dbo].[WXWATIgnoreDie]";
            var dbret = Models.DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { waferlist.Add(Models.UT.O2S(line[0])); }

            foreach (var wf in waferlist)
            {
                var igwfdict = new Dictionary<string, bool>();
                sql = "select X,Y from [EngrData].[dbo].[WXWATIgnoreDie] where wafer = '" + wf + "'";
                dbret = Models.DBUtility.ExeLocalSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    igwfdict.Add(Models.UT.O2I(line[0]) + ":::" + Models.UT.O2I(line[1]), true);
                }//foreach

                var spdata = WXLogic.WATSampleXY.GetSampleXYByCouponGroup(wf);
                foreach (var sp in spdata)
                {
                    var key = sp.X + ":::" + sp.Y;
                    if (igwfdict.ContainsKey(key))
                    {
                        sql = "update[EngrData].[dbo].[WXWATIgnoreDie] set[CouponCH] = '" + sp.CouponID + ":::" + sp.ChannelInfo + "' where wafer = '" + wf
                            + "' and CONVERT(int, X) = " + sp.X + " and CONVERT(int, Y) = " + sp.Y;
                        Models.DBUtility.ExeLocalSqlNoRes(sql);
                    }
                }//end foreach
            }

            return View("HeartBeat");
        }

        public ActionResult CheckDataUniform()
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var zerolevel = WAT.Models.UT.O2D(syscfg["WATDATAWDOGZEROLEVEL"]);
            var zerocnt = WAT.Models.UT.O2I(syscfg["WDOGZEROCNT"]);

            var filterlevel = syscfg["WATDATAWDOGFILTERLEVEL"];
            var filtercnt = syscfg["WDOGFILTERCNT"];

            var dict = new Dictionary<string, bool>();

            var allcoupon = WuxiWATData4MG.GetRecentWATCouponID("2020-12-27 13:43:34", "2020-12-27 13:53:34");
            foreach (var cp in allcoupon)
            {
                var key = cp.CouponID.ToUpper() + "_" + cp.TestStep.ToUpper();
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, true);
                    var ret = WuxiWATData4MG.CheckWATDataUniform(cp.CouponID, cp.TestStep, cp.TestTime
                        , zerolevel, zerocnt, filterlevel, filtercnt);
                }
            }
            return View("HeartBeat");
        }


        //public ActionResult CheckTraceID()
        //{
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    TrackRequest request = CreateTrackRequest();
        //    TrackService service = new TrackService();
        //    service.Url = "https://wsbeta.fedex.com:443/web-services/track";
        //    try
        //    {
        //        var tracestatus = "";
        //        var latestevent = "";
        //        var latesteventtime = "";

        //        TrackReply reply = service.track(request);
        //        if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
        //        {
        //            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
        //            {
        //                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
        //                {
        //                    tracestatus = trackDetail.StatusDetail.Description;

        //                    if (trackDetail.Events != null)
        //                    {
        //                        //Console.WriteLine("Track Events:");
        //                        foreach (TrackEvent trackevent in trackDetail.Events)
        //                        {
        //                            if (trackevent.TimestampSpecified)
        //                            {
        //                                latesteventtime = trackevent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        //                            }
        //                            latestevent = trackevent.EventDescription;
        //                            break;
        //                            //Console.WriteLine("***");
        //                        }
        //                        //Console.WriteLine();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex) { }

        //    return View("HeartBeat");
        //}

        //private static TrackRequest CreateTrackRequest()
        //{
        //    // Build the TrackRequest
        //    TrackRequest request = new TrackRequest();
        //    //
        //    request.WebAuthenticationDetail = new WebAuthenticationDetail();
        //    request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
        //    request.WebAuthenticationDetail.UserCredential.Key = "3jbeGywUe3xc0Vo6"; // Replace "XXX" with the Key
        //    request.WebAuthenticationDetail.UserCredential.Password = "WJ1P0OVH4RuJylkC63Kxpq1QG "; // Replace "XXX" with the Password
        //    request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
        //    request.WebAuthenticationDetail.ParentCredential.Key = "3jbeGywUe3xc0Vo6"; // Replace "XXX" with the Key
        //    request.WebAuthenticationDetail.ParentCredential.Password = "WJ1P0OVH4RuJylkC63Kxpq1QG "; // Replace "XXX"
        //    //if (usePropertyFile()) //Set values from a file for testing purposes
        //    //{
        //    //    request.WebAuthenticationDetail.UserCredential.Key = getProperty("key");
        //    //    request.WebAuthenticationDetail.UserCredential.Password = getProperty("password");
        //    //    request.WebAuthenticationDetail.ParentCredential.Key = getProperty("parentkey");
        //    //    request.WebAuthenticationDetail.ParentCredential.Password = getProperty("parentpassword");
        //    //}
        //    //
        //    request.ClientDetail = new ClientDetail();
        //    request.ClientDetail.AccountNumber = "510087780"; // Replace "XXX" with the client's account number
        //    request.ClientDetail.MeterNumber = "100495072"; // Replace "XXX" with the client's meter number
        //    //if (usePropertyFile()) //Set values from a file for testing purposes
        //    //{
        //    //    request.ClientDetail.AccountNumber = getProperty("accountnumber");
        //    //    request.ClientDetail.MeterNumber = getProperty("meternumber");
        //    //}
        //    //
        //    request.TransactionDetail = new TransactionDetail();
        //    request.TransactionDetail.CustomerTransactionId = "II-VI";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
        //    //
        //    request.Version = new VersionId();
        //    //
        //    // Tracking information
        //    request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
        //    request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
        //    request.SelectionDetails[0].PackageIdentifier.Value = "772395610600 "; // Replace "XXX" with tracking number or door tag
        //    //if (usePropertyFile()) //Set values from a file for testing purposes
        //    //{
        //    //    request.SelectionDetails[0].PackageIdentifier.Value = getProperty("trackingnumber");
        //    //}
        //    request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
        //    //
        //    // Date range is optional.
        //    // If omitted, set to false
        //    request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("06/18/2012"); //MM/DD/YYYY
        //    request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
        //    request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
        //    request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
        //    //
        //    // Include detailed scans is optional.
        //    // If omitted, set to false
        //    request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
        //    request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
        //    return request;
        //}


        public ActionResult WaferTraceSYS()
        {
            return View();
        }

        public JsonResult CommitWaferTraceData()
        {
            var prodpndict = CfgUtility.GetSixInchProdPN(this);
            var wafer = Request.Form["wafer"].Trim().ToUpper();
            var traceid = Request.Form["traceid"].Trim().Replace(" ", "");
            var deliever = Request.Form["deliever"];
            var priority = Request.Form["priority"];

            if (!WaferTrace.CheckValidWafer(wafer))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    res = false,
                    msg = "wafer number already exist! try to use suffix R08 or T08 or E09."
                };
                return ret;
            }

            var prod = "";
            var wf = wafer.Split(new string[] { "E", "R", "T" }, StringSplitOptions.RemoveEmptyEntries)[0];
            if (wf.Length == 13)
            {
                prod = Models.WXEvalPN.GetProductFamilyFromSherman(wf);
            }
            else
            {
                prod = Models.WXEvalPN.GetProductFamilyFromAllen(wf);
                if (string.IsNullOrEmpty(prod))
                {
                    prod = Models.WXEvalPN.GetProductFamilyFromSherman(wf);
                }
            }

            if (string.IsNullOrEmpty(prod))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    res = false,
                    msg = "failed to get product family, check wafer number."
                };
                return ret;
            }

            var pn = "";
            if (prodpndict.ContainsKey(prod))
            { pn = prodpndict[prod]; }

            if (string.IsNullOrEmpty(prod))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    res = false,
                    msg = "failed to get part number from prod:"+prod+"."
                };
                return ret;
            }

            var traceinfo = WaferTrace.GetTraceStatus(traceid, this);
            if (string.IsNullOrEmpty(traceinfo[0]))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    res = false,
                    msg = "we failed to get deliever info from the trace id, please check the trace id."
                };
                return ret;
            }

            var wtrace = new WaferTrace();
            wtrace.WaferNum = wafer;
            wtrace.TraceID = traceid;
            wtrace.TraceCompany = deliever;
            wtrace.Product = prod;
            wtrace.PN = pn;
            wtrace.Priority = priority;

            var tracestatus = traceinfo[0];
            var traceevent = traceinfo[1];
            var tracedate = traceinfo[2];
            if (tracestatus.Contains("DELIVERED"))
            {
                wtrace.DeliverStatus = "DELIVERED";
                wtrace.ArriveDate = tracedate;
            }
            else
            {
                wtrace.DeliverStatus = (traceevent.Length > 100)?traceevent.Substring(0,100):traceevent;
                wtrace.ArriveDate = tracedate;
            }

            wtrace.StoreData();
            WaferTrace.UpdateTraceStatus(wtrace.TraceID, wtrace.DeliverStatus, wtrace.ArriveDate);

            if (tracestatus.Contains("DELIVERED"))
            {
                WaferTrace.SendJOEmail(wtrace, this);
            }

            var rets = new JsonResult();
            rets.MaxJsonLength = Int32.MaxValue;
            rets.Data = new
            {
                res = true,
                msg = ""
            };
            return rets;
        }

        public JsonResult LoadWaferTraceData()
        {
            var wafertracelist = WaferTrace.GetAllData(this);
            var rets = new JsonResult();
            rets.MaxJsonLength = Int32.MaxValue;
            rets.Data = new
            {
                wafertracelist = wafertracelist
            };
            return rets;
        }

        public JsonResult RefreshWaferLogis()
        {
            WaferTrace.RefreshDelievery(this);
            var rets = new JsonResult();
            rets.MaxJsonLength = Int32.MaxValue;
            rets.Data = new
            {
                res = true
            };
            return rets;
        }

        public JsonResult RefreshWaferTrace()
        {
            WaferTrace.DailyUpdate(this);
            var rets = new JsonResult();
            rets.MaxJsonLength = Int32.MaxValue;
            rets.Data = new
            {
                res = true
            };
            return rets;
        }

        public ActionResult StartWATJO(string wf)
        {
            if (!string.IsNullOrEmpty(wf))
            { WaferTrace.UpdateAssemblyStatus(wf); }
            return View();
        }

        public JsonResult WaferTracePriority()
        {
            var act = Request.Form["act"];
            var wf = Request.Form["wf"];

            var prt = WaferTrace.GetPriority(wf);
            var res = false;
            var py = "";

            if (!string.IsNullOrEmpty(prt))
            {
                var pt = Models.UT.O2I(prt.Replace("P", ""));
                if (act.Contains("UP") && pt > 1)
                {
                    py = "P" + (pt - 1);
                    res = true;
                    WaferTrace.UpdatePriority(wf, py);
                }

                if (act.Contains("DOWN") && pt < 5)
                {
                    py = "P" + (pt + 1);
                    res = true;
                    WaferTrace.UpdatePriority(wf, py);
                }
            }

            var rets = new JsonResult();
            rets.MaxJsonLength = Int32.MaxValue;
            rets.Data = new
            {
                res = res,
                py = py
            };
            return rets;
        }


    }
}